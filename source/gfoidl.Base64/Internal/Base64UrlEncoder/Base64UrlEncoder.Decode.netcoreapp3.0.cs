using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

// Scalar based on https://github.com/dotnet/corefx/tree/master/src/System.Memory/src/System/Buffers/Text
// SSE2 based on https://github.com/aklomp/base64/tree/master/lib/arch/ssse3
// AVX2 based on https://github.com/aklomp/base64/tree/master/lib/arch/avx2
// Lookup and validation for SSE2 and AVX2 based on http://0x80.pl/notesen/2016-01-17-sse-base64-decoding.html#vector-lookup-pshufb

namespace gfoidl.Base64.Internal
{
    partial class Base64UrlEncoder
    {
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private OperationStatus DecodeImpl<T>(
            ref T src,
            int inputLength,
            Span<byte> data,
            int decodedLength,
            out int consumed,
            out int written,
            bool isFinalBlock = true)
        {
            uint sourceIndex = 0;
            uint destIndex   = 0;

            decodedLength = GetDataLen(inputLength, out int base64Len, isFinalBlock);
            int srcLength = base64Len & ~0x3;       // only decode input up to the closest multiple of 4.

            ref byte destBytes = ref MemoryMarshal.GetReference(data);

            // s - 45 >= 0 used 'lea' as opposed to s >= 45
            if (Avx2.IsSupported && srcLength - 45 >= 0 && !s_isMac)
            {
                Avx2Decode(ref src, ref destBytes, srcLength, ref sourceIndex, ref destIndex);

                if (sourceIndex == srcLength)
                    goto DoneExit;
            }
            else if (Ssse3.IsSupported && srcLength - 24 >= 0)
            {
                Sse2Decode(ref src, ref destBytes, srcLength, ref sourceIndex, ref destIndex);

                if (sourceIndex == srcLength)
                    goto DoneExit;
            }

            ref sbyte decodingMap = ref s_decodingMap[0];

            // Last bytes could have padding characters, so process them separately and treat them as valid only if isFinalBlock is true
            // if isFinalBlock is false, padding characters are considered invalid
            int skipLastChunk = isFinalBlock ? 4 : 0;

            int maxSrcLength = 0;
            int destLength   = data.Length;

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

            // In order to elide the movsxd in the loop
            if (sourceIndex < maxSrcLength)
            {
                do
                {
                    int result = DecodeFour(ref Unsafe.Add(ref src, (IntPtr)sourceIndex), ref decodingMap);

                    if (result < 0)
                        goto InvalidExit;

                    WriteThreeLowOrderBytes(ref destBytes, destIndex, result);
                    destIndex   += 3;
                    sourceIndex += 4;
                }
                while (sourceIndex < (uint)maxSrcLength);
            }

            if (maxSrcLength != srcLength - skipLastChunk)
                goto DestinationSmallExit;

            // If input is less than 4 bytes, srcLength == sourceIndex == 0
            // If input is not a multiple of 4, sourceIndex == srcLength != 0
            if (sourceIndex == srcLength)
            {
                if (isFinalBlock)
                    goto InvalidExit;

                goto NeedMoreDataExit;
            }

            // if isFinalBlock is false, we will never reach this point

            // Handle last four bytes. There are 0, 1, 2 padding chars.
            int numPaddingChars = base64Len - inputLength;
            ref T lastFourStart = ref Unsafe.Add(ref src, srcLength - 4);

            if (numPaddingChars == 0)
            {
                int result = DecodeFour(ref lastFourStart, ref decodingMap);

                if (result < 0) goto InvalidExit;
                if (destIndex > destLength - 3) goto DestinationSmallExit;

                WriteThreeLowOrderBytes(ref destBytes, destIndex, result);
                sourceIndex += 4;
                destIndex   += 3;
            }
            else if (numPaddingChars == 1)
            {
                int result = DecodeThree(ref lastFourStart, ref decodingMap);

                if (result < 0)
                    goto InvalidExit;

                if (destIndex > destLength - 2)
                    goto DestinationSmallExit;

                WriteTwoLowOrderBytes(ref destBytes, destIndex, result);
                sourceIndex += 3;
                destIndex   += 2;
            }
            else
            {
                int result = DecodeTwo(ref lastFourStart, ref decodingMap);

                if (result < 0)
                    goto InvalidExit;

                if (destIndex > destLength - 1)
                    goto DestinationSmallExit;

                WriteOneLowOrderByte(ref destBytes, destIndex, result);
                sourceIndex += 2;
                destIndex   += 1;
            }

            if (srcLength != base64Len)
                goto InvalidExit;

        DoneExit:
            consumed = (int)sourceIndex;
            written  = (int)destIndex;
            return OperationStatus.Done;

        DestinationSmallExit:
            if (srcLength != inputLength && isFinalBlock)
                goto InvalidExit; // if input is not a multiple of 4, and there is no more data, return invalid data instead

            consumed = (int)sourceIndex;
            written  = (int)destIndex;
            return OperationStatus.DestinationTooSmall;

        NeedMoreDataExit:
            consumed = (int)sourceIndex;
            written  = (int)destIndex;
            return OperationStatus.NeedMoreData;

        InvalidExit:
            consumed = (int)sourceIndex;
            written  = (int)destIndex;
            return OperationStatus.InvalidData;
        }
        //---------------------------------------------------------------------
#if DEBUG
        public static event EventHandler<EventArgs> Avx2Decoded;
        public static event EventHandler<EventArgs> Sse2Decoded;
#endif
        //---------------------------------------------------------------------
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Avx2Decode<T>(ref T src, ref byte destBytes, int sourceLength, ref uint sourceIndex, ref uint destIndex)
        {
            ref T srcStart     = ref src;
            ref byte destStart = ref destBytes;
            ref T simdSrcEnd   = ref Unsafe.Add(ref src, (IntPtr)((uint)sourceLength - 45 + 1));

            // The JIT won't hoist these "constants", so help him
            Vector256<sbyte> allOnes          = Vector256.Create((sbyte)-1);        // -1 = 0xFF = true in simd
            Vector256<sbyte> lutHi            = s_avx_decodeLutHi;
            Vector256<sbyte> lutLo            = s_avx_decodeLutLo;
            Vector256<sbyte> lutShift         = s_avx_decodeLutShift;
            Vector256<sbyte> mask5F           = s_avx_decodeMask5F;
            Vector256<sbyte> shift5F          = Vector256.Create((sbyte)33);        // high nibble is 0x5 -> range 'P' .. 'Z' for shift, not '+' (0x2)
            Vector256<sbyte> shuffleConstant0 = Vector256.Create(0x01400140).AsSByte();
            Vector256<short> shuffleConstant1 = Vector256.Create(0x00011000).AsInt16();
            Vector256<sbyte> shuffleVec       = s_avx_decodeShuffleVec;
            Vector256<int> permuteVec         = s_avx_decodePermuteVec;

            //while (remaining >= 45)
            while (Unsafe.IsAddressLessThan(ref src, ref simdSrcEnd))
            {
                Vector256<sbyte> str;

                if (typeof(T) == typeof(byte))
                {
                    str = Unsafe.As<T, Vector256<sbyte>>(ref src);
                }
                else if (typeof(T) == typeof(char))
                {
                    str = Avx2Helper.Read(ref Unsafe.As<T, char>(ref src));
                }
                else
                {
                    throw new NotSupportedException(); // just in case new types are introduced in the future
                }

                Vector256<sbyte> hiNibbles = Avx2.And(Avx2.ShiftRightLogical(str.AsInt32(), 4).AsSByte(), mask5F);
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
                Avx2Decoded?.Invoke(null, EventArgs.Empty);
#endif
                Vector256<sbyte> shift = Avx2.Shuffle(lutShift, hiNibbles);
                str                    = Avx2.Add(str, shift);
                str                    = Avx2.Add(str, Avx2.And(eq5F, shift5F));

                Vector256<short> merge_ab_and_bc = Avx2.MultiplyAddAdjacent(str.AsByte(), shuffleConstant0);
                Vector256<int> @out              = Avx2.MultiplyAddAdjacent(merge_ab_and_bc, shuffleConstant1);
                @out                             = Avx2.Shuffle(@out.AsSByte(), shuffleVec).AsInt32();
                str                              = Avx2.PermuteVar8x32(@out, permuteVec).AsSByte();

                // As has better CQ than WriteUnaligned
                // https://github.com/dotnet/coreclr/issues/21132
                Unsafe.As<byte, Vector256<sbyte>>(ref destBytes) = str;

                src       = ref Unsafe.Add(ref src, 32);
                destBytes = ref Unsafe.Add(ref destBytes, 24);
            }

            // Cast to ulong to avoid the overflow-check. Codegen for x86 is still good.
            sourceIndex = (uint)(ulong)Unsafe.ByteOffset(ref srcStart, ref src) / (uint)Unsafe.SizeOf<T>();
            destIndex   = (uint)(ulong)Unsafe.ByteOffset(ref destStart, ref destBytes);

            src       = ref srcStart;
            destBytes = ref destStart;
        }
        //---------------------------------------------------------------------
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Sse2Decode<T>(ref T src, ref byte destBytes, int sourceLength, ref uint sourceIndex, ref uint destIndex)
        {
            ref T srcStart     = ref src;
            ref byte destStart = ref destBytes;
            ref T simdSrcEnd   = ref Unsafe.Add(ref src, (IntPtr)((uint)sourceLength - 24 + 1));

            // The JIT won't hoist these "constants", so help him
            Vector128<sbyte> lutHi            = s_sse_decodeLutHi;
            Vector128<sbyte> lutLo            = s_sse_decodeLutLo;
            Vector128<sbyte> lutShift         = s_sse_decodeLutShift;
            Vector128<sbyte> mask5F           = s_sse_decodeMask5F;
            Vector128<sbyte> shift5F          = Vector128.Create((sbyte)33); // high nibble is 0x5 -> range 'P' .. 'Z' for shift, not '+' (0x2)
            Vector128<sbyte> shuffleConstant0 = Vector128.Create(0x01400140).AsSByte();
            Vector128<short> shuffleConstant1 = Vector128.Create(0x00011000).AsInt16();
            Vector128<sbyte> shuffleVec       = s_sse_decodeShuffleVec;

            //while (remaining >= 24)
            while (Unsafe.IsAddressLessThan(ref src, ref simdSrcEnd))
            {
                Vector128<sbyte> str;

                if (typeof(T) == typeof(byte))
                {
                    str = Unsafe.As<T, Vector128<sbyte>>(ref src);
                }
                else if (typeof(T) == typeof(char))
                {
                    Vector128<short> c0 = Unsafe.As<T, Vector128<short>>(ref src);
                    Vector128<short> c1 = Unsafe.As<T, Vector128<short>>(ref Unsafe.Add(ref src, 8));
                    str                 = Sse2.PackUnsignedSaturate(c0, c1).AsSByte();
                }
                else
                {
                    throw new NotSupportedException(); // just in case new types are introduced in the future
                }

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
                Sse2Decoded?.Invoke(null, EventArgs.Empty);
#endif
                Vector128<sbyte> shift = Ssse3.Shuffle(lutShift, hiNibbles);
                str                    = Sse2.Add(str, shift);
                str                    = Sse2.Add(str, Sse2.And(eq5F, shift5F));

                Vector128<short> merge_ab_and_bc = Ssse3.MultiplyAddAdjacent(str.AsByte(), shuffleConstant0);
                Vector128<int> @out              = Sse2.MultiplyAddAdjacent(merge_ab_and_bc, shuffleConstant1);
                str                              = Ssse3.Shuffle(@out.AsSByte(), shuffleVec);

                // As has better CQ than WriteUnaligned
                // https://github.com/dotnet/coreclr/issues/21132
                Unsafe.As<byte, Vector128<sbyte>>(ref destBytes) = str;

                src       = ref Unsafe.Add(ref src, 16);
                destBytes = ref Unsafe.Add(ref destBytes, 12);
            }

            // Cast to ulong to avoid the overflow-check. Codegen for x86 is still good.
            sourceIndex = (uint)(ulong)Unsafe.ByteOffset(ref srcStart, ref src) / (uint)Unsafe.SizeOf<T>();
            destIndex   = (uint)(ulong)Unsafe.ByteOffset(ref destStart, ref destBytes);

            src       = ref srcStart;
            destBytes = ref destStart;
        }
        //---------------------------------------------------------------------
        private static readonly Vector128<sbyte> s_sse_decodeLutLo;
        private static readonly Vector128<sbyte> s_sse_decodeLutHi;
        private static readonly Vector128<sbyte> s_sse_decodeLutShift;
        private static readonly Vector128<sbyte> s_sse_decodeMask5F;

        private static readonly Vector256<sbyte> s_avx_decodeLutLo;
        private static readonly Vector256<sbyte> s_avx_decodeLutHi;
        private static readonly Vector256<sbyte> s_avx_decodeLutShift;
        private static readonly Vector256<sbyte> s_avx_decodeMask5F;
    }
}
