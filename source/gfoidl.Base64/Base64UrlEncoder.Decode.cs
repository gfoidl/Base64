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
            int srcLength    = inputLength & ~0x3;      // only decode input up to the closest multiple of 4.

            ref byte destBytes  = ref MemoryMarshal.GetReference(data);

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

            if (destLength >= this.GetDecodedLength(srcLength))
            {
                maxSrcLength = srcLength - skipLastChunk;
            }
            else
            {
                // This should never overflow since destLength here is less than int.MaxValue / 4 * 3 (i.e. 1610612733)
                // Therefore, (destLength / 3) * 4 will always be less than 2147483641
                maxSrcLength = (destLength / 3) * 4;
            }

            while (sourceIndex < maxSrcLength)
            {
                int result = Decode(ref Unsafe.Add(ref src, (IntPtr)sourceIndex), ref decodingMap);

                if (result < 0)
                    goto InvalidExit;

                WriteThreeLowOrderBytes(ref Unsafe.Add(ref destBytes, (IntPtr)destIndex), result);
                destIndex   += 3;
                sourceIndex += 4;
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
            uint t0, t1, t2, t3;

            if (typeof(T) == typeof(byte))
            {
                ref byte tmp = ref Unsafe.As<T, byte>(ref src);
                t0 = Unsafe.Add(ref tmp, (IntPtr)(srcLength - 4));
                t1 = Unsafe.Add(ref tmp, (IntPtr)(srcLength - 3));
                t2 = Unsafe.Add(ref tmp, (IntPtr)(srcLength - 2));
                t3 = Unsafe.Add(ref tmp, (IntPtr)(srcLength - 1));
            }
            else if (typeof(T) == typeof(char))
            {
                ref char tmp = ref Unsafe.As<T, char>(ref src);
                t0 = Unsafe.Add(ref tmp, (IntPtr)(srcLength - 4));
                t1 = Unsafe.Add(ref tmp, (IntPtr)(srcLength - 3));
                t2 = Unsafe.Add(ref tmp, (IntPtr)(srcLength - 2));
                t3 = Unsafe.Add(ref tmp, (IntPtr)(srcLength - 1));
            }
            else
            {
                throw new NotSupportedException();  // just in case new types are introduced in the future
            }

            int i0 = Unsafe.Add(ref decodingMap, (IntPtr)t0);
            int i1 = Unsafe.Add(ref decodingMap, (IntPtr)t1);

            i0 <<= 18;
            i1 <<= 12;

            i0 |= i1;

            if (t3 != EncodingPad)
            {
                int i2 = Unsafe.Add(ref decodingMap, (IntPtr)t2);
                int i3 = Unsafe.Add(ref decodingMap, (IntPtr)t3);

                i2 <<= 6;

                i0 |= i3;
                i0 |= i2;

                if (i0 < 0)
                    goto InvalidExit;

                if (destIndex > destLength - 3)
                    goto DestinationSmallExit;

                WriteThreeLowOrderBytes(ref Unsafe.Add(ref destBytes, (IntPtr)destIndex), i0);
                destIndex += 3;
            }
            else if (t2 != EncodingPad)
            {
                int i2 = Unsafe.Add(ref decodingMap, (IntPtr)t2);

                i2 <<= 6;

                i0 |= i2;

                if (i0 < 0)
                    goto InvalidExit;

                if (destIndex > destLength - 2)
                    goto DestinationSmallExit;

                Unsafe.Add(ref destBytes, (IntPtr)destIndex)       = (byte)(i0 >> 16);
                Unsafe.Add(ref destBytes, (IntPtr)(destIndex + 1)) = (byte)(i0 >> 8);
                destIndex += 2;
            }
            else
            {
                if (i0 < 0)
                    goto InvalidExit;

                if (destIndex > destLength - 1)
                    goto DestinationSmallExit;

                Unsafe.Add(ref destBytes, (IntPtr)destIndex) = (byte)(i0 >> 16);
                destIndex += 1;
            }

            sourceIndex += 4;

            if (srcLength != inputLength)
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
#if NETCOREAPP
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
            Vector128<sbyte> mask2F           = s_sse_decodeMask2F;
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

                Vector128<sbyte> hiNibbles = Sse2.And(Sse.StaticCast<int, sbyte>(Sse2.ShiftRightLogical(Sse.StaticCast<sbyte, int>(str), 4)), mask2F);
                Vector128<sbyte> loNibbles = Sse2.And(str, mask2F);
                Vector128<sbyte> hi        = Ssse3.Shuffle(lutHi, hiNibbles);
                Vector128<sbyte> lo        = Ssse3.Shuffle(lutLo, loNibbles);
#if NETCOREAPP3_0
                // https://github.com/dotnet/coreclr/issues/21130
                //Vector128<sbyte> zero = Vector128<sbyte>.Zero;
                Vector128<sbyte> zero = Sse2.SetZeroVector128<sbyte>();
#elif NETCOREAPP2_1
                Vector128<sbyte> zero = Sse2.SetZeroVector128<sbyte>();
#endif
                if (Sse2.MoveMask(Sse2.CompareGreaterThan(Sse2.And(lo, hi), zero)) != 0)
                {
                    success = false;
                    break;
                }

                Vector128<sbyte> eq2F = Sse2.CompareEqual(str, mask2F);
                Vector128<sbyte> roll = Ssse3.Shuffle(lutRoll, Sse2.Add(eq2F, hiNibbles));
                str                   = Sse2.Add(str, roll);

                Vector128<short> merge_ab_and_bc = Ssse3.MultiplyAddAdjacent(Sse.StaticCast<sbyte, byte>(str), shuffleConstant0);
#if NETCOREAPP3_0
                Vector128<int> @out = Sse2.MultiplyAddAdjacent(merge_ab_and_bc, shuffleConstant1);
#elif NETCOREAPP2_1
                Vector128<int> @out = Sse2.MultiplyHorizontalAdd(merge_ab_and_bc, shuffleConstant1);
#endif
                str = Ssse3.Shuffle(Sse.StaticCast<int, sbyte>(@out), shuffleVec);

                // As has better CQ than WriteUnaligned
                // https://github.com/dotnet/coreclr/issues/21132
                Unsafe.As<byte, Vector128<sbyte>>(ref destBytes) = str;

                src       = ref Unsafe.Add(ref src,  16);
                destBytes = ref Unsafe.Add(ref destBytes, 12);
            }

            // Cast to ulong to avoid the overflow-check. Codegen for x86 is still good.
            sourceIndex = (uint)(ulong)Unsafe.ByteOffset(ref srcStart, ref src) / (uint)Unsafe.SizeOf<T>();
            destIndex   = (uint)(ulong)Unsafe.ByteOffset(ref destStart, ref destBytes);

            src       = ref srcStart;
            destBytes = ref destStart;

            return success;
        }
#endif
        //---------------------------------------------------------------------
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Decode<T>(ref T encoded, ref sbyte decodingMap)
        {
            uint t0, t1, t2, t3;

            if (typeof(T) == typeof(byte))
            {
                ref byte tmp = ref Unsafe.As<T, byte>(ref encoded);
                t0 = Unsafe.Add(ref tmp, 0);
                t1 = Unsafe.Add(ref tmp, 1);
                t2 = Unsafe.Add(ref tmp, 2);
                t3 = Unsafe.Add(ref tmp, 3);
            }
            else if (typeof(T) == typeof(char))
            {
                ref char tmp = ref Unsafe.As<T, char>(ref encoded);
                t0 = Unsafe.Add(ref tmp, 0);
                t1 = Unsafe.Add(ref tmp, 1);
                t2 = Unsafe.Add(ref tmp, 2);
                t3 = Unsafe.Add(ref tmp, 3);
            }
            else
            {
                throw new NotSupportedException();  // just in case new types are introduced in the future
            }

            int i0 = Unsafe.Add(ref decodingMap, (IntPtr)t0);
            int i1 = Unsafe.Add(ref decodingMap, (IntPtr)t1);
            int i2 = Unsafe.Add(ref decodingMap, (IntPtr)t2);
            int i3 = Unsafe.Add(ref decodingMap, (IntPtr)t3);

            i0 <<= 18;
            i1 <<= 12;
            i2 <<= 6;

            i0 |= i3;
            i1 |= i2;

            i0 |= i1;
            return i0;
        }
        //---------------------------------------------------------------------
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void WriteThreeLowOrderBytes(ref byte destination, int value)
        {
            Unsafe.Add(ref destination, 0) = (byte)(value >> 16);
            Unsafe.Add(ref destination, 1) = (byte)(value >> 8);
            Unsafe.Add(ref destination, 2) = (byte)value;
        }
        //---------------------------------------------------------------------
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
        private static readonly Vector128<sbyte> s_sse_decodeMask2F;
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
