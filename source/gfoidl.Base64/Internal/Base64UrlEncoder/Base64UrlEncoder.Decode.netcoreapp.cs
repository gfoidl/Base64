using System;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

// Scalar based on https://github.com/dotnet/corefx/tree/ec34e99b876ea1119f37986ead894f4eded1a19a/src/System.Memory/src/System/Buffers/Text
// SSE2   based on https://github.com/aklomp/base64/tree/a27c565d1b6c676beaf297fe503c4518185666f7/lib/arch/ssse3
// AVX2   based on https://github.com/aklomp/base64/tree/a27c565d1b6c676beaf297fe503c4518185666f7/lib/arch/avx2
// Lookup and validation for SSE2 and AVX2 based on http://0x80.pl/notesen/2016-01-17-sse-base64-decoding.html#vector-lookup-pshufb

namespace gfoidl.Base64.Internal
{
    public partial class Base64UrlEncoder
    {
        private OperationStatus DecodeImpl<T>(
            ref T src,
            int inputLength,
            Span<byte> data,
            out int consumed,
            out int written,
            bool isFinalBlock = true)
            where T : unmanaged
        {
            uint sourceIndex = 0;
            uint destIndex   = 0;

            if (!TryGetDataLength(inputLength, out int base64Len, out int decodedLength, isFinalBlock))
            {
                goto InvalidDataExit;
            }

            int srcLength    = base64Len & ~0x3;       // only decode input up to the closest multiple of 4.
            int maxSrcLength = srcLength;
            int destLength   = data.Length;

            // max. 2 padding chars
            if (destLength < decodedLength - 2)
            {
                // For overflow see comment below
                Debug.Assert(destIndex < int.MaxValue);
                maxSrcLength = (int)((uint)destLength / 3 * 4);
            }

            ref byte destBytes = ref MemoryMarshal.GetReference(data);

            if (Ssse3.IsSupported && maxSrcLength >= 24)
            {
                if (Avx2.IsSupported && maxSrcLength >= 45)
                {
                    this.Avx2Decode(ref src, ref destBytes, maxSrcLength, destLength, ref sourceIndex, ref destIndex);

                    if (sourceIndex == srcLength)
                        goto DoneExit;
                }

                if (Ssse3.IsSupported && (maxSrcLength >= (int)sourceIndex + 24))
                {
                    this.Ssse3Decode(ref src, ref destBytes, maxSrcLength, destLength, ref sourceIndex, ref destIndex);

                    if (sourceIndex == srcLength)
                        goto DoneExit;
                }
            }

            // Last bytes could have padding characters, so process them separately and treat them as valid only if isFinalBlock is true
            // if isFinalBlock is false, padding characters are considered invalid
            int skipLastChunk = isFinalBlock ? 4 : 0;

            if (destLength >= decodedLength)
            {
                maxSrcLength = srcLength - skipLastChunk;
            }
            else
            {
                // This should never overflow since destLength here is less than int.MaxValue / 4 * 3 (i.e. 1610612733)
                // Therefore, (destLength / 3) * 4 will always be less than 2147483641
                Debug.Assert(destLength < (int.MaxValue / 4 * 3));
                maxSrcLength = (int)((uint)destLength / 3 * 4);
            }

            ref sbyte decodingMap = ref MemoryMarshal.GetReference(DecodingMap);

            // In order to elide the movsxd in the loop
            if (sourceIndex < maxSrcLength)
            {
                do
                {
#if DEBUG
                    ScalarDecodingIteration?.Invoke();
#endif
                    int result = DecodeFour(ref Unsafe.Add(ref src, (IntPtr)sourceIndex), ref decodingMap);

                    if (result < 0)
                        goto InvalidDataExit;

                    WriteThreeLowOrderBytes(ref destBytes, destIndex, result);
                    destIndex   += 3;
                    sourceIndex += 4;
                }
                while (sourceIndex < (uint)maxSrcLength);
            }

            if (maxSrcLength != srcLength - skipLastChunk)
                goto DestinationTooSmallExit;

            // If input is less than 4 bytes, srcLength == sourceIndex == 0
            // If input is not a multiple of 4, sourceIndex == srcLength != 0
            if (sourceIndex == srcLength)
            {
                if (isFinalBlock)
                    goto InvalidDataExit;

                if (sourceIndex == inputLength)
                    goto DoneExit;

                goto NeedMoreDataExit;
            }

            // if isFinalBlock is false, we will never reach this point

            // Handle last four bytes. There are 0, 1, 2 padding chars.
            int numPaddingChars = base64Len - inputLength;
            ref T lastFourStart = ref Unsafe.Add(ref src, srcLength - 4);

            if (numPaddingChars == 0)
            {
                int result = DecodeFour(ref lastFourStart, ref decodingMap);

                if (result < 0) goto InvalidDataExit;
                if (destIndex > destLength - 3) goto DestinationTooSmallExit;

                WriteThreeLowOrderBytes(ref destBytes, destIndex, result);
                sourceIndex += 4;
                destIndex   += 3;
            }
            else if (numPaddingChars == 1)
            {
                int result = DecodeThree(ref lastFourStart, ref decodingMap);

                if (result < 0)
                    goto InvalidDataExit;

                if (destIndex > destLength - 2)
                    goto DestinationTooSmallExit;

                WriteTwoLowOrderBytes(ref destBytes, destIndex, result);
                sourceIndex += 3;
                destIndex   += 2;
            }
            else
            {
                int result = DecodeTwo(ref lastFourStart, ref decodingMap);

                if (result < 0)
                    goto InvalidDataExit;

                if (destIndex > destLength - 1)
                    goto DestinationTooSmallExit;

                WriteOneLowOrderByte(ref destBytes, destIndex, result);
                sourceIndex += 2;
                destIndex   += 1;
            }

            if (srcLength != base64Len)
                goto InvalidDataExit;

        DoneExit:
            consumed = (int)sourceIndex;
            written  = (int)destIndex;
            return OperationStatus.Done;

        DestinationTooSmallExit:
            if (srcLength != inputLength && isFinalBlock)
                goto InvalidDataExit; // if input is not a multiple of 4, and there is no more data, return invalid data instead

            consumed = (int)sourceIndex;
            written  = (int)destIndex;
            return OperationStatus.DestinationTooSmall;

        NeedMoreDataExit:
            consumed = (int)sourceIndex;
            written  = (int)destIndex;
            return OperationStatus.NeedMoreData;

        InvalidDataExit:
            consumed = (int)sourceIndex;
            written  = (int)destIndex;
            return OperationStatus.InvalidData;
        }
        //---------------------------------------------------------------------
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Avx2Decode<T>(ref T src, ref byte dest, int sourceLength, int destLength, ref uint sourceIndex, ref uint destIndex)
            where T : unmanaged
        {
            ref T srcStart     = ref src;
            ref byte destStart = ref dest;
            ref T simdSrcEnd   = ref Unsafe.Add(ref srcStart, (IntPtr)((uint)sourceLength - 45 + 1));    //  +1 for <=

            // The JIT won't hoist these "constants", so help it
            //Vector256<sbyte> allOnes = Vector256.Create((sbyte)-1);                // -1 = 0xFF = true in simd
            Vector256<sbyte> zero    = Vector256<sbyte>.Zero;
            Vector256<sbyte> allOnes = Avx2.CompareEqual(zero, zero);

            Vector256<sbyte> lutHi            = AvxDecodeLutHi.ReadVector256();
            Vector256<sbyte> lutLo            = AvxDecodeLutLo.ReadVector256();
            Vector256<sbyte> lutShift         = AvxDecodeLutShift.ReadVector256();
            Vector256<sbyte> mask5F           = Vector256.Create((sbyte)0x5F);              // ASCII: _
            Vector256<sbyte> shift5F          = Vector256.Create((sbyte)33);                // high nibble is 0x5 -> range 'P' .. 'Z' for shift, not '+' (0x2)
            Vector256<sbyte> shuffleConstant0 = Vector256.Create(0x01400140).AsSByte();
            Vector256<short> shuffleConstant1 = Vector256.Create(0x00011000).AsInt16();
            Vector256<sbyte> shuffleVec       = AvxDecodeShuffleVec.ReadVector256();
            Vector256<int> permuteVec         = AvxDecodePermuteVec.ReadVector256().AsInt32();

            //while (remaining >= 45)
            do
            {
                src.AssertRead<Vector256<sbyte>, T>(ref srcStart, sourceLength);
                Vector256<sbyte> str = src.ReadVector256();

                Vector256<sbyte> hiNibbles  = Avx2.And(Avx2.ShiftRightLogical(str.AsInt32(), 4).AsSByte(), mask5F);
                Vector256<sbyte> lowerBound = Avx2.Shuffle(lutLo, hiNibbles);
                Vector256<sbyte> upperBound = Avx2.Shuffle(lutHi, hiNibbles);

                Vector256<sbyte> below   = Avx2Helper.LessThan(str, lowerBound, allOnes);
                Vector256<sbyte> above   = Avx2.CompareGreaterThan(str, upperBound);
                Vector256<sbyte> eq5F    = Avx2.CompareEqual(str, mask5F);
                Vector256<sbyte> outside = Avx2.AndNot(eq5F, Avx2.Or(below, above));

                // https://github.com/dotnet/coreclr/issues/21247
                if (Avx2.MoveMask(outside) != 0)
                    break;
#if DEBUG
                Avx2DecodingIteration?.Invoke();
#endif
                Vector256<sbyte> shift = Avx2.Shuffle(lutShift, hiNibbles);
                str                    = Avx2.Add(str, shift);
                str                    = Avx2.Add(str, Avx2.And(eq5F, shift5F));

                Vector256<short> merge_ab_and_bc = Avx2.MultiplyAddAdjacent(str.AsByte(), shuffleConstant0);
                Vector256<int> @out              = Avx2.MultiplyAddAdjacent(merge_ab_and_bc, shuffleConstant1);
                @out                             = Avx2.Shuffle(@out.AsSByte(), shuffleVec).AsInt32();
                str                              = Avx2.PermuteVar8x32(@out, permuteVec).AsSByte();

                dest.AssertWrite<Vector256<sbyte>, byte>(ref destStart, destLength);
                dest.WriteVector256(str);

                src  = ref Unsafe.Add(ref src , 32);
                dest = ref Unsafe.Add(ref dest, 24);
            }
            while (Unsafe.IsAddressLessThan(ref src, ref simdSrcEnd));

            // Cast to ulong to avoid the overflow-check. Codegen for x86 is still good.
            sourceIndex = (uint)(ulong)Unsafe.ByteOffset(ref srcStart , ref src) / (uint)Unsafe.SizeOf<T>();
            destIndex   = (uint)(ulong)Unsafe.ByteOffset(ref destStart, ref dest);

            src  = ref srcStart;
            dest = ref destStart;
#if DEBUG
            Avx2Decoded?.Invoke();
#endif
        }
        //---------------------------------------------------------------------
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Ssse3Decode<T>(ref T src, ref byte dest, int sourceLength, int destLength, ref uint sourceIndex, ref uint destIndex)
            where T : unmanaged
        {
            ref T srcStart     = ref src;
            ref byte destStart = ref dest;
            ref T simdSrcEnd   = ref Unsafe.Add(ref srcStart, (IntPtr)((uint)sourceLength - 24 + 1));    //  +1 for <=

            // Shift to workspace
            src  = ref Unsafe.Add(ref src , (IntPtr)sourceIndex);
            dest = ref Unsafe.Add(ref dest, (IntPtr)destIndex);

            // The JIT won't hoist these "constants", so help it
            Vector128<sbyte> lutHi            = SseDecodeLutHi.ReadVector128();
            Vector128<sbyte> lutLo            = SseDecodeLutLo.ReadVector128();
            Vector128<sbyte> lutShift         = SseDecodeLutShift.ReadVector128();
            Vector128<sbyte> mask5F           = Vector128.Create((sbyte)0x5F);              // ASCII: _
            Vector128<sbyte> shift5F          = Vector128.Create((sbyte)33);                // high nibble is 0x5 -> range 'P' .. 'Z' for shift, not '+' (0x2)
            Vector128<sbyte> shuffleConstant0 = Vector128.Create(0x01400140).AsSByte();
            Vector128<short> shuffleConstant1 = Vector128.Create(0x00011000).AsInt16();
            Vector128<sbyte> shuffleVec       = SseDecodeShuffleVec.ReadVector128();

            //while (remaining >= 24)
            do
            {
                src.AssertRead<Vector128<sbyte>, T>(ref srcStart, sourceLength);
                Vector128<sbyte> str = src.ReadVector128();

                Vector128<sbyte> hiNibbles  = Sse2.And(Sse2.ShiftRightLogical(str.AsInt32(), 4).AsSByte(), mask5F);
                Vector128<sbyte> lowerBound = Ssse3.Shuffle(lutLo, hiNibbles);
                Vector128<sbyte> upperBound = Ssse3.Shuffle(lutHi, hiNibbles);

                Vector128<sbyte> below   = Sse2.CompareLessThan(str, lowerBound);
                Vector128<sbyte> above   = Sse2.CompareGreaterThan(str, upperBound);
                Vector128<sbyte> eq5F    = Sse2.CompareEqual(str, mask5F);
                Vector128<sbyte> outside = Sse2.AndNot(eq5F, Sse2.Or(below, above));

                if (Sse2.MoveMask(outside) != 0)
                    break;
#if DEBUG
                Ssse3DecodingIteration?.Invoke();
#endif
                Vector128<sbyte> shift = Ssse3.Shuffle(lutShift, hiNibbles);
                str                    = Sse2.Add(str, shift);
                str                    = Sse2.Add(str, Sse2.And(eq5F, shift5F));

                Vector128<short> merge_ab_and_bc = Ssse3.MultiplyAddAdjacent(str.AsByte(), shuffleConstant0);
                Vector128<int> @out              = Sse2.MultiplyAddAdjacent(merge_ab_and_bc, shuffleConstant1);
                str                              = Ssse3.Shuffle(@out.AsSByte(), shuffleVec);

                dest.AssertWrite<Vector128<sbyte>, byte>(ref destStart, destLength);
                dest.WriteVector128(str);

                src  = ref Unsafe.Add(ref src , 16);
                dest = ref Unsafe.Add(ref dest, 12);
            }
            while (Unsafe.IsAddressLessThan(ref src, ref simdSrcEnd));

            // Cast to ulong to avoid the overflow-check. Codegen for x86 is still good.
            sourceIndex = (uint)(ulong)Unsafe.ByteOffset(ref srcStart , ref src) / (uint)Unsafe.SizeOf<T>();
            destIndex   = (uint)(ulong)Unsafe.ByteOffset(ref destStart, ref dest);

            src  = ref srcStart;
            dest = ref destStart;
#if DEBUG
            Ssse3Decoded?.Invoke();
#endif
        }
        //---------------------------------------------------------------------
#pragma warning disable IDE1006 // Naming Styles
        private const sbyte lInv = 1;       // any value so that a comparison < results in true for invalid values
        private const sbyte hInv = 0x00;
#pragma warning restore IDE1006 // Naming Styles

        private static ReadOnlySpan<sbyte> SseDecodeLutLo => new sbyte[16]
        {
            lInv, lInv, 0x2D, 0x30,
            0x41, 0x50, 0x61, 0x70,
            lInv, lInv, lInv, lInv,
            lInv, lInv, lInv, lInv
        };

        private static ReadOnlySpan<sbyte> SseDecodeLutHi => new sbyte[16]
        {
            hInv, hInv, 0x2D, 0x39,
            0x4F, 0x5A, 0x6F, 0x7A,
            hInv, hInv, hInv, hInv,
            hInv, hInv, hInv, hInv
        };

        private static ReadOnlySpan<sbyte> SseDecodeLutShift => new sbyte[16]
        {
              0,   0,  17,   4,
            -65, -65, -71, -71,
              0,   0,   0,   0,
              0,   0,   0,   0
        };

        private static ReadOnlySpan<sbyte> AvxDecodeLutLo => new sbyte[32]
        {
            lInv, lInv, 0x2D, 0x30,
            0x41, 0x50, 0x61, 0x70,
            lInv, lInv, lInv, lInv,
            lInv, lInv, lInv, lInv,
            lInv, lInv, 0x2D, 0x30,
            0x41, 0x50, 0x61, 0x70,
            lInv, lInv, lInv, lInv,
            lInv, lInv, lInv, lInv
        };

        private static ReadOnlySpan<sbyte> AvxDecodeLutHi => new sbyte[32]
        {
            hInv, hInv, 0x2D, 0x39,
            0x4F, 0x5A, 0x6F, 0x7A,
            hInv, hInv, hInv, hInv,
            hInv, hInv, hInv, hInv,
            hInv, hInv, 0x2D, 0x39,
            0x4F, 0x5A, 0x6F, 0x7A,
            hInv, hInv, hInv, hInv,
            hInv, hInv, hInv, hInv
        };

        private static ReadOnlySpan<sbyte> AvxDecodeLutShift => new sbyte[32]
        {
              0,   0,  17,   4,
            -65, -65, -71, -71,
              0,   0,   0,   0,
              0,   0,   0,   0,
              0,   0,  17,   4,
            -65, -65, -71, -71,
              0,   0,   0,   0,
              0,   0,   0,   0
        };
    }
}
