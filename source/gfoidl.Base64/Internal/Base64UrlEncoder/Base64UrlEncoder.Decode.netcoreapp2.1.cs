using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

// Scalar based on https://github.com/dotnet/corefx/tree/ec34e99b876ea1119f37986ead894f4eded1a19a/src/System.Memory/src/System/Buffers/Text
// SSE2 based on https://github.com/aklomp/base64/tree/a27c565d1b6c676beaf297fe503c4518185666f7/lib/arch/ssse3
// Lookup and validation for SSE2 and AVX2 based on http://0x80.pl/notesen/2016-01-17-sse-base64-decoding.html#vector-lookup-pshufb

namespace gfoidl.Base64.Internal
{
    partial class Base64UrlEncoder
    {
        private OperationStatus DecodeImpl<T>(
            ref T src,
            int inputLength,
            Span<byte> data,
            int decodedLength,
            out int consumed,
            out int written,
            bool isFinalBlock = true)
            where T : unmanaged
        {
            uint sourceIndex = 0;
            uint destIndex   = 0;

            decodedLength    = GetDataLen(inputLength, out int base64Len, isFinalBlock);
            int srcLength    = base64Len & ~0x3;       // only decode input up to the closest multiple of 4.
            int maxSrcLength = srcLength;
            int destLength   = data.Length;

            // max. 2 padding chars
            if (destLength < decodedLength - 2)
            {
                // For overflow see comment below
                maxSrcLength = destLength / 3 * 4;
            }

            ref byte destBytes = ref MemoryMarshal.GetReference(data);

            if (Sse2.IsSupported && Ssse3.IsSupported && maxSrcLength >= 24)
            {
                Ssse3Decode(ref src, ref destBytes, maxSrcLength, destLength, ref sourceIndex, ref destIndex);

                if (sourceIndex == srcLength)
                    goto DoneExit;
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
                maxSrcLength = (destLength / 3) * 4;
            }

            // https://github.com/dotnet/coreclr/issues/23194
            // Slicing is necessary to "unlink" the ref and let the JIT keep it in a register
            ref sbyte decodingMap = ref MemoryMarshal.GetReference(s_decodingMap.Slice(1));

            // In order to elide the movsxd in the loop
            if (sourceIndex < maxSrcLength)
            {
                do
                {
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
#if DEBUG
        public static event EventHandler<EventArgs> Ssse3Decoded;
#endif
        //---------------------------------------------------------------------
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Ssse3Decode<T>(ref T src, ref byte destBytes, int sourceLength, int destLength, ref uint sourceIndex, ref uint destIndex)
            where T : unmanaged
        {
            ref T srcStart     = ref src;
            ref byte destStart = ref destBytes;
            ref T simdSrcEnd   = ref Unsafe.Add(ref src, (IntPtr)((uint)sourceLength - 24 + 1));   //  +1 for <=

            // The JIT won't hoist these "constants", so help it
            Vector128<sbyte> lutHi            = s_sseDecodeLutHi.ReadVector128();
            Vector128<sbyte> lutLo            = s_sseDecodeLutLo.ReadVector128();
            Vector128<sbyte> lutShift         = s_sseDecodeLutShift.ReadVector128();
            Vector128<sbyte> mask5F           = Sse2.SetAllVector128((sbyte)0x5F);      // ASCII: _
            Vector128<sbyte> shift5F          = Sse2.SetAllVector128((sbyte)33);        // high nibble is 0x5 -> range 'P' .. 'Z' for shift, not '+' (0x2)
            Vector128<sbyte> shuffleConstant0 = Sse.StaticCast<int, sbyte>(Sse2.SetAllVector128(0x01400140));
            Vector128<short> shuffleConstant1 = Sse.StaticCast<int, short>(Sse2.SetAllVector128(0x00011000));
            Vector128<sbyte> shuffleVec       = s_sseDecodeShuffleVec.ReadVector128();

            //while (remaining >= 24)
            do
            {
                src.AssertRead<Vector128<sbyte>, T>(ref srcStart, sourceLength);
                Vector128<sbyte> str = src.ReadVector128();

                Vector128<sbyte> hiNibbles  = Sse2.And(Sse.StaticCast<int, sbyte>(Sse2.ShiftRightLogical(Sse.StaticCast<sbyte, int>(str), 4)), mask5F);
                Vector128<sbyte> lowerBound = Ssse3.Shuffle(lutLo, hiNibbles);
                Vector128<sbyte> upperBound = Ssse3.Shuffle(lutHi, hiNibbles);

                Vector128<sbyte> below   = Sse2.CompareLessThan(str, lowerBound);
                Vector128<sbyte> above   = Sse2.CompareGreaterThan(str, upperBound);
                Vector128<sbyte> eq5F    = Sse2.CompareEqual(str, mask5F);
                Vector128<sbyte> outside = Sse2.AndNot(eq5F, Sse2.Or(below, above));

                if (Sse2.MoveMask(outside) != 0)
                    break;
#if DEBUG
                Ssse3Decoded?.Invoke(null, EventArgs.Empty);
#endif
                Vector128<sbyte> shift = Ssse3.Shuffle(lutShift, hiNibbles);
                str                    = Sse2.Add(str, shift);
                str                    = Sse2.Add(str, Sse2.And(eq5F, shift5F));

                Vector128<short> merge_ab_and_bc = Ssse3.MultiplyAddAdjacent(Sse.StaticCast<sbyte, byte>(str), shuffleConstant0);
                Vector128<int> @out              = Sse2.MultiplyHorizontalAdd(merge_ab_and_bc, shuffleConstant1);
                str                              = Ssse3.Shuffle(Sse.StaticCast<int, sbyte>(@out), shuffleVec);

                destBytes.AssertWrite<Vector128<sbyte>, byte>(ref destStart, destLength);
                destBytes.WriteVector128(str);

                src       = ref Unsafe.Add(ref src, 16);
                destBytes = ref Unsafe.Add(ref destBytes, 12);
            }
            while (Unsafe.IsAddressLessThan(ref src, ref simdSrcEnd));

            // Cast to ulong to avoid the overflow-check. Codegen for x86 is still good.
            sourceIndex = (uint)(ulong)Unsafe.ByteOffset(ref srcStart, ref src) / (uint)Unsafe.SizeOf<T>();
            destIndex   = (uint)(ulong)Unsafe.ByteOffset(ref destStart, ref destBytes);

            src       = ref srcStart;
            destBytes = ref destStart;
        }
        //---------------------------------------------------------------------
        private const sbyte lInv = -1;      // 0xFF: Constant value '255' cannot be converted to a 'sbyte'
        private const sbyte hInv = 0x00;

        private static ReadOnlySpan<sbyte> s_sseDecodeLutLo => new sbyte[]
        {
            lInv, lInv, 0x2D, 0x30,
            0x41, 0x50, 0x61, 0x70,
            lInv, lInv, lInv, lInv,
            lInv, lInv, lInv, lInv
        };

        private static ReadOnlySpan<sbyte> s_sseDecodeLutHi => new sbyte[]
        {
            hInv, hInv, 0x2D, 0x39,
            0x4F, 0x5A, 0x6F, 0x7A,
            hInv, hInv, hInv, hInv,
            hInv, hInv, hInv, hInv
        };

        private static ReadOnlySpan<sbyte> s_sseDecodeLutShift => new sbyte[]
        {
              0,   0,  17,   4,
            -65, -65, -71, -71,
              0,   0,   0,   0,
              0,   0,   0,   0
        };
    }
}
