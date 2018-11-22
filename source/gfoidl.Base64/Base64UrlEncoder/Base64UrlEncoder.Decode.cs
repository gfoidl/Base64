using System;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#if NETCOREAPP
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
#endif

// Sequential based on https://github.com/dotnet/corefx/tree/master/src/System.Memory/src/System/Buffers/Text
// SSE2 based on https://github.com/aklomp/base64/tree/master/lib/arch/ssse3

namespace gfoidl.Base64
{
    partial class Base64UrlEncoder
    {
        public override int GetDecodedLength(ReadOnlySpan<byte> encoded) => this.GetDecodedLength(encoded.Length);
        public override int GetDecodedLength(ReadOnlySpan<char> encoded) => this.GetDecodedLength(encoded.Length);
        //---------------------------------------------------------------------
        internal int GetDecodedLength(int encodedLength)
        {
            if ((uint)encodedLength >= int.MaxValue)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.length);

            return GetDataLen(encodedLength, out int _);
        }
        //---------------------------------------------------------------------
#if NETCOREAPP3_0
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
#endif
        protected override OperationStatus DecodeCore<T>(ref T src, int inputLength, Span<byte> data, out int consumed, out int written, bool isFinalBlock = true)
        {
            uint sourceIndex = 0;
            uint destIndex   = 0;

            int dataLength = GetDataLen(inputLength, out int base64Len, isFinalBlock);
            int srcLength  = base64Len & ~0x3;      // only decode input up to the closest multiple of 4.

            ref byte destBytes  = ref MemoryMarshal.GetReference(data);

#if NETCOREAPP3_0
            if (Avx2.IsSupported && srcLength >= 45 && !s_isMac)
            {
                if (!Avx2Decode(ref src, ref destBytes, srcLength, ref sourceIndex, ref destIndex))
                    goto Sequential;

                if (sourceIndex == srcLength)
                    goto DoneExit;
            }
#endif

#if NETCOREAPP
#if NETCOREAPP3_0
            if (Ssse3.IsSupported && (srcLength - sourceIndex >= 24))
#else
            if (Sse2.IsSupported && Ssse3.IsSupported && (srcLength - sourceIndex >= 24))
#endif
            {
                if (!Sse2Decode(ref src, ref destBytes, srcLength, ref sourceIndex, ref destIndex))
                    goto Sequential;

                if (sourceIndex == srcLength)
                    goto DoneExit;
            }
        Sequential:
#endif
            ref sbyte decodingMap = ref s_decodingMap[0];

            // Last bytes could have padding characters, so process them separately and treat them as valid only if isFinalBlock is true
            // if isFinalBlock is false, padding characters are considered invalid
            int skipLastChunk = isFinalBlock ? 4 : 0;

            int maxSrcLength = 0;
            int destLength   = data.Length;

            if (destLength >= dataLength)
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
#if NETCOREAPP
        DoneExit:
#endif
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
            written = (int)destIndex;
            return OperationStatus.NeedMoreData;

        InvalidExit:
            consumed = (int)sourceIndex;
            written  = (int)destIndex;
            return OperationStatus.InvalidData;
        }
        //---------------------------------------------------------------------
#if NETCOREAPP3_0
#if DEBUG
        public static event EventHandler<EventArgs> Avx2Decoded;
#endif
        //---------------------------------------------------------------------
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool Avx2Decode<T>(ref T src, ref byte destBytes, int sourceLength, ref uint sourceIndex, ref uint destIndex)
        {
            bool success       = true;
            ref T srcStart     = ref src;
            ref byte destStart = ref destBytes;
            ref T simdSrcEnd   = ref Unsafe.Add(ref src, (IntPtr)(sourceLength - 45 + 1));

            // The JIT won't hoist these "constants", so help him
            Vector256<sbyte> lutHi            = s_avx_decodeLutHi;
            Vector256<sbyte> lutLo            = s_avx_decodeLutLo;
            Vector256<sbyte> lutRoll          = s_avx_decodeLutRoll;
            Vector256<sbyte> mask5F           = s_avx_decodeMask5F;
            Vector256<sbyte> shuffleConstant0 = Avx.StaticCast<int, sbyte>(Avx.SetAllVector256(0x01400140));
            Vector256<short> shuffleConstant1 = Avx.StaticCast<int, short>(Avx.SetAllVector256(0x00011000));
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

                Vector256<sbyte> hiNibbles = Avx2.And(Avx.StaticCast<int, sbyte>(Avx2.ShiftRightLogical(Avx.StaticCast<sbyte, int>(str), 4)), mask5F);
                Vector256<sbyte> loNibbles = Avx2.And(str, mask5F);
                Vector256<sbyte> hi        = Avx2.Shuffle(lutHi, hiNibbles);
                Vector256<sbyte> lo        = Avx2.Shuffle(lutLo, loNibbles);

                if (!Avx.TestZ(lo, hi))
                {
                    success = false;
                    break;
                }

                Vector256<sbyte> eq2F = Avx2.CompareEqual(str, mask5F);
                Vector256<sbyte> roll = Avx2.Shuffle(lutRoll, Avx2.Add(eq2F, hiNibbles));
                str                   = Avx2.Add(str, roll);

                Vector256<short> merge_ab_and_bc = Avx2.MultiplyAddAdjacent(Avx.StaticCast<sbyte, byte>(str), shuffleConstant0);
                Vector256<int> @out              = Avx2.MultiplyAddAdjacent(merge_ab_and_bc, shuffleConstant1);
                @out                             = Avx.StaticCast<sbyte, int>(Avx2.Shuffle(Avx.StaticCast<int, sbyte>(@out), shuffleVec));
                str                              = Avx.StaticCast<int, sbyte>(Avx2.PermuteVar8x32(@out, permuteVec));

                Unsafe.As<byte, Vector256<sbyte>>(ref destBytes) = str;

                src       = ref Unsafe.Add(ref src, 32);
                destBytes = ref Unsafe.Add(ref destBytes, 24);
            }

            // Cast to ulong to avoid the overflow-check. Codegen for x86 is still good.
            sourceIndex = (uint)(ulong)Unsafe.ByteOffset(ref srcStart, ref src) / (uint)Unsafe.SizeOf<T>();
            destIndex   = (uint)(ulong)Unsafe.ByteOffset(ref destStart, ref destBytes);

            src       = ref srcStart;
            destBytes = ref destStart;
#if DEBUG
            if (success)
                Avx2Decoded?.Invoke(null, EventArgs.Empty);
#endif
            return success;
        }
#endif
        //---------------------------------------------------------------------
#if NETCOREAPP
#if DEBUG
        public static event EventHandler<EventArgs> Sse2Decoded;
#endif
        //---------------------------------------------------------------------
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool Sse2Decode<T>(ref T src, ref byte destBytes, int sourceLength, ref uint sourceIndex, ref uint destIndex)
        {
            bool success       = true;
            ref T srcStart     = ref src;
            ref byte destStart = ref destBytes;
            ref T simdSrcEnd   = ref Unsafe.Add(ref src, (IntPtr)(sourceLength - 24 + 1));

            // The JIT won't hoist these "constants", so help him
            Vector128<sbyte> lutHi            = s_sse_decodeLutHi;
            Vector128<sbyte> lutLo            = s_sse_decodeLutLo;
            Vector128<sbyte> lutRoll          = s_sse_decodeLutRoll;
            Vector128<sbyte> mask5F           = s_sse_decodeMask5F;
            Vector128<sbyte> shift5F          = Sse2.SetAllVector128((sbyte)33);     // high nibble is 0x5 -> range 'P' .. 'Z' for shift, not '+'
            Vector128<sbyte> shuffleConstant0 = Sse.StaticCast<int, sbyte>(Sse2.SetAllVector128(0x01400140));
            Vector128<short> shuffleConstant1 = Sse.StaticCast<int, short>(Sse2.SetAllVector128(0x00011000));
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

                    str = Sse.StaticCast<byte, sbyte>(Sse2.PackUnsignedSaturate(c0, c1));
                }
                else
                {
                    throw new NotSupportedException(); // just in case new types are introduced in the future
                }

                Vector128<sbyte> hiNibbles = Sse2.And(Sse.StaticCast<int, sbyte>(Sse2.ShiftRightLogical(Sse.StaticCast<sbyte, int>(str), 4)), mask5F);
                Vector128<sbyte> lowerBound = Ssse3.Shuffle(lutLo, hiNibbles);
                Vector128<sbyte> upperBound = Ssse3.Shuffle(lutHi, hiNibbles);

                Vector128<sbyte> below   = Sse2.CompareLessThan(str, lowerBound);
                Vector128<sbyte> above   = Sse2.CompareGreaterThan(str, upperBound);
                Vector128<sbyte> eq5F    = Sse2.CompareEqual(str, mask5F);
                Vector128<sbyte> outside = Sse2.AndNot(eq5F, Sse2.Or(below, above));

                if (Sse2.MoveMask(outside) != 0)
                    break;

                Vector128<sbyte> roll = Ssse3.Shuffle(lutRoll, hiNibbles);
                str                   = Sse2.Add(str, roll);
                str                   = Sse2.Add(str, Sse2.And(eq5F, shift5F));

                Vector128<short> merge_ab_and_bc = Ssse3.MultiplyAddAdjacent(Sse.StaticCast<sbyte, byte>(str), shuffleConstant0);
#if NETCOREAPP3_0
                Vector128<int> @out = Sse2.MultiplyAddAdjacent(merge_ab_and_bc, shuffleConstant1);
#else
                Vector128<int> @out = Sse2.MultiplyHorizontalAdd(merge_ab_and_bc, shuffleConstant1);
#endif
                str = Ssse3.Shuffle(Sse.StaticCast<int, sbyte>(@out), shuffleVec);

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
#if DEBUG
            if (success)
                Sse2Decoded?.Invoke(null, EventArgs.Empty);
#endif
            return success;
        }
#endif
        //---------------------------------------------------------------------
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int DecodeThree<T>(ref T encoded, ref sbyte decodingMap)
        {
            uint t0, t1, t2;

            if (typeof(T) == typeof(byte))
            {
                ref byte tmp = ref Unsafe.As<T, byte>(ref encoded);
                t0 = Unsafe.Add(ref tmp, 0);
                t1 = Unsafe.Add(ref tmp, 1);
                t2 = Unsafe.Add(ref tmp, 2);
            }
            else if (typeof(T) == typeof(char))
            {
                ref char tmp = ref Unsafe.As<T, char>(ref encoded);
                t0 = Unsafe.Add(ref tmp, 0);
                t1 = Unsafe.Add(ref tmp, 1);
                t2 = Unsafe.Add(ref tmp, 2);
            }
            else
            {
                throw new NotSupportedException();  // just in case new types are introduced in the future
            }

            int i0 = Unsafe.Add(ref decodingMap, (IntPtr)t0);
            int i1 = Unsafe.Add(ref decodingMap, (IntPtr)t1);
            int i2 = Unsafe.Add(ref decodingMap, (IntPtr)t2);

            return i0 << 18 | i1 << 12 | i2 << 6;
        }
        //---------------------------------------------------------------------
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int DecodeTwo<T>(ref T encoded, ref sbyte decodingMap)
        {
            uint t0, t1;

            if (typeof(T) == typeof(byte))
            {
                ref byte tmp = ref Unsafe.As<T, byte>(ref encoded);
                t0           = Unsafe.Add(ref tmp, 0);
                t1           = Unsafe.Add(ref tmp, 1);
            }
            else if (typeof(T) == typeof(char))
            {
                ref char tmp = ref Unsafe.As<T, char>(ref encoded);
                t0 = Unsafe.Add(ref tmp, 0);
                t1 = Unsafe.Add(ref tmp, 1);
            }
            else
            {
                throw new NotSupportedException();  // just in case new types are introduced in the future
            }

            int i0 = Unsafe.Add(ref decodingMap, (IntPtr)t0);
            int i1 = Unsafe.Add(ref decodingMap, (IntPtr)t1);

            return i0 << 18 | i1 << 12;
        }
        //---------------------------------------------------------------------
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void WriteTwoLowOrderBytes(ref byte destination, uint destIndex, int value)
        {
            Unsafe.Add(ref destination, (IntPtr)(destIndex + 0)) = (byte)(value >> 16);
            Unsafe.Add(ref destination, (IntPtr)(destIndex + 1)) = (byte)(value >> 8);
        }
        //---------------------------------------------------------------------
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void WriteOneLowOrderByte(ref byte destination, uint destIndex, int value)
        {
            Unsafe.Add(ref destination, (IntPtr)destIndex) = (byte)(value >> 16);
        }
        //---------------------------------------------------------------------
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetDataLen(int urlEncodedLen, out int base64Len, bool isFinalBlock = true)
        {
            if (isFinalBlock)
            {
                // Shortcut for Guid and other 16 byte data
                if (urlEncodedLen == 22)
                {
                    base64Len = 24;
                    return 16;
                }

                int numPaddingChars = GetNumBase64PaddingCharsToAddForDecode(urlEncodedLen);
                base64Len           = urlEncodedLen + numPaddingChars;

                Debug.Assert(base64Len % 4 == 0, "Invariant: Array length must be a multiple of 4.");

                int dataLength = (base64Len >> 2) * 3 - numPaddingChars;
                return dataLength;
            }
            else
            {
                base64Len = urlEncodedLen;
                return (urlEncodedLen >> 2) * 3;
            }
        }
        //---------------------------------------------------------------------
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetNumBase64PaddingCharsToAddForDecode(int urlEncodedLen)
        {
            // Calculation is:
            // switch (inputLength % 4)
            // 0 -> 0
            // 2 -> 2
            // 3 -> 1
            // default -> format exception

            int result = (4 - urlEncodedLen) & 3;

            if (result == 3)
                ThrowHelper.ThrowMalformedInputException(urlEncodedLen);

            return result;
        }
        //---------------------------------------------------------------------
#if NETCOREAPP
        private static readonly Vector128<sbyte> s_sse_decodeShuffleVec;
        private static readonly Vector128<sbyte> s_sse_decodeLutLo;
        private static readonly Vector128<sbyte> s_sse_decodeLutHi;
        private static readonly Vector128<sbyte> s_sse_decodeLutRoll;
        private static readonly Vector128<sbyte> s_sse_decodeMask5F;

        private static readonly Vector256<sbyte> s_avx_decodeShuffleVec;
        private static readonly Vector256<sbyte> s_avx_decodeLutLo;
        private static readonly Vector256<sbyte> s_avx_decodeLutHi;
        private static readonly Vector256<sbyte> s_avx_decodeLutRoll;
        private static readonly Vector256<sbyte> s_avx_decodeMask5F;
        private static readonly Vector256<int>   s_avx_decodePermuteVec;
#endif
        // internal because tests use this map too
        internal static readonly sbyte[] s_decodingMap = {
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 62, -1, -1,
            52, 53, 54, 55, 56, 57, 58, 59, 60, 61, -1, -1, -1, -1, -1, -1,
            -1,  0,  1,  2,  3,  4,  5,  6,  7,  8,  9, 10, 11, 12, 13, 14,
            15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, -1, -1, -1, -1, 63,
            -1, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40,
            41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1
        };
    }
}
