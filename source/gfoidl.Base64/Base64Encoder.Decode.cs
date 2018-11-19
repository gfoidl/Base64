using System;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#if NETCOREAPP2_1
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
#endif

// Sequential based on https://github.com/dotnet/corefx/tree/master/src/System.Memory/src/System/Buffers/Text
// SSE2 based on https://github.com/aklomp/base64/tree/master/lib/arch/ssse3

namespace gfoidl.Base64
{
    partial class Base64Encoder
    {
        public int GetDecodedLength(ReadOnlySpan<byte> encoded)
        {
            int maxLen = this.GetDecodedLength(encoded.Length);

            ref byte tmp = ref MemoryMarshal.GetReference(encoded);
            ref byte end = ref Unsafe.Add(ref tmp, encoded.Length - 1);

            int padding = 0;

            if (end == EncodingPad) padding++;
            if (Unsafe.Subtract(ref end, 1) == EncodingPad) padding++;

            return maxLen - padding;
        }
        //---------------------------------------------------------------------
        public int GetDecodedLength(ReadOnlySpan<char> encoded)
        {
            int maxLen = this.GetDecodedLength(encoded.Length);

            ref char tmp = ref MemoryMarshal.GetReference(encoded);
            ref char end = ref Unsafe.Add(ref tmp, encoded.Length - 1);

            int padding = 0;

            if (end == EncodingPad) padding++;
            if (Unsafe.Subtract(ref end, 1) == EncodingPad) padding++;

            return maxLen - padding;
        }
        //---------------------------------------------------------------------
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal int GetDecodedLength(int encodedLength)
        {
            if (encodedLength < 0)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.length);

            // TODO: check CQ for this shortcut
            //return encodedLength == 0 ? 0 : (encodedLength >> 2) * 3;
            return (encodedLength >> 2) * 3;
        }
        //---------------------------------------------------------------------
        public OperationStatus Decode(ReadOnlySpan<byte> encoded, Span<byte> data, out int consumed, out int written) => this.DecodeCore(encoded, data, out consumed, out written);
        public OperationStatus Decode(ReadOnlySpan<char> encoded, Span<byte> data, out int consumed, out int written) => this.DecodeCore(encoded, data, out consumed, out written);
        //---------------------------------------------------------------------
        public byte[] Decode(ReadOnlySpan<char> encoded)
        {
            if (encoded.IsEmpty) return Array.Empty<byte>();

            int dataLength         = this.GetDecodedLength(encoded);
            byte[] data            = new byte[dataLength];
            ref char src           = ref MemoryMarshal.GetReference(encoded);
            OperationStatus status = this.DecodeCore(ref src, encoded.Length, data, out int consumed, out int written);

            if (status == OperationStatus.InvalidData)
                ThrowHelper.ThrowForOperationNotDone(status);

            Debug.Assert(status         == OperationStatus.Done);
            Debug.Assert(encoded.Length == consumed);
            Debug.Assert(data.Length    == written);

            return data;
        }
        //---------------------------------------------------------------------
        internal OperationStatus DecodeCore<T>(ReadOnlySpan<T> encoded, Span<byte> data, out int consumed, out int written)
        {
            if (encoded.IsEmpty)
            {
                consumed = 0;
                written  = 0;
                return OperationStatus.Done;
            }

            ref T src     = ref MemoryMarshal.GetReference(encoded);
            int srcLength = encoded.Length;

            return this.DecodeCore(ref src, srcLength, data, out consumed, out written);
        }
        //---------------------------------------------------------------------
        private OperationStatus DecodeCore<T>(ref T src, int inputLength, Span<byte> data, out int consumed, out int written)
        {
            int sourceIndex = 0;
            int destIndex   = 0;
            int srcLength   = inputLength & ~0x3;   // only decode input up to the closest multiple of 4.

            ref byte destBytes  = ref MemoryMarshal.GetReference(data);
#if NETCOREAPP2_1
            if (Sse2.IsSupported && Ssse3.IsSupported && srcLength >= 24)
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
            const bool isFinalBlock = true;
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
                int result = Decode(ref Unsafe.Add(ref src, sourceIndex), ref decodingMap);

                if (result < 0)
                    goto InvalidExit;

                WriteThreeLowOrderBytes(ref Unsafe.Add(ref destBytes, destIndex), result);
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
            }

            // if isFinalBlock is false, we will never reach this point

            // Handle last four bytes. There are 0, 1, 2 padding chars.
            int i0 = 0, i1 = 0, i2 = 0, i3 = 0;

            if (typeof(T) == typeof(byte))
            {
                ref byte tmp = ref Unsafe.As<T, byte>(ref src);
                i0 = Unsafe.Add(ref tmp, srcLength - 4);
                i1 = Unsafe.Add(ref tmp, srcLength - 3);
                i2 = Unsafe.Add(ref tmp, srcLength - 2);
                i3 = Unsafe.Add(ref tmp, srcLength - 1);
            }
            else if (typeof(T) == typeof(char))
            {
                ref char tmp = ref Unsafe.As<T, char>(ref src);
                i0 = Unsafe.Add(ref tmp, srcLength - 4);
                i1 = Unsafe.Add(ref tmp, srcLength - 3);
                i2 = Unsafe.Add(ref tmp, srcLength - 2);
                i3 = Unsafe.Add(ref tmp, srcLength - 1);
            }
            else
            {
                throw new NotSupportedException();  // just in case new types are introduced in the future
            }

            i0 = Unsafe.Add(ref decodingMap, i0);
            i1 = Unsafe.Add(ref decodingMap, i1);

            i0 <<= 18;
            i1 <<= 12;

            i0 |= i1;

            if (i3 != EncodingPad)
            {
                i2 = Unsafe.Add(ref decodingMap, i2);
                i3 = Unsafe.Add(ref decodingMap, i3);

                i2 <<= 6;

                i0 |= i3;
                i0 |= i2;

                if (i0 < 0)
                    goto InvalidExit;

                if (destIndex > destLength - 3)
                    goto DestinationSmallExit;

                WriteThreeLowOrderBytes(ref Unsafe.Add(ref destBytes, destIndex), i0);
                destIndex += 3;
            }
            else if (i2 != EncodingPad)
            {
                i2 = Unsafe.Add(ref decodingMap, i2);

                i2 <<= 6;

                i0 |= i2;

                if (i0 < 0)
                    goto InvalidExit;

                if (destIndex > destLength - 2)
                    goto DestinationSmallExit;

                Unsafe.Add(ref destBytes, destIndex) = (byte)(i0 >> 16);
                Unsafe.Add(ref destBytes, destIndex + 1) = (byte)(i0 >> 8);
                destIndex += 2;
            }
            else
            {
                if (i0 < 0)
                    goto InvalidExit;

                if (destIndex > destLength - 1)
                    goto DestinationSmallExit;

                Unsafe.Add(ref destBytes, destIndex) = (byte)(i0 >> 16);
                destIndex += 1;
            }

            sourceIndex += 4;

            if (srcLength != inputLength)
                goto InvalidExit;
#if NETCOREAPP2_1
        DoneExit:
#endif
            consumed = sourceIndex;
            written  = destIndex;
            return OperationStatus.Done;

        DestinationSmallExit:
            if (srcLength != inputLength && isFinalBlock)
                goto InvalidExit; // if input is not a multiple of 4, and there is no more data, return invalid data instead

            consumed = sourceIndex;
            written  = destIndex;
            return OperationStatus.DestinationTooSmall;

        InvalidExit:
            consumed = sourceIndex;
            written  = destIndex;
            return OperationStatus.InvalidData;
        }
        //---------------------------------------------------------------------
#if NETCOREAPP2_1
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool Sse2Decode<T>(ref T src, ref byte destBytes, int sourceLength, ref int sourceIndex, ref int destIndex)
        {
            bool success       = true;
            ref T srcStart     = ref src;
            ref byte destStart = ref destBytes;
            ref T simdSrcEnd   = ref Unsafe.Add(ref src, sourceLength - 24 + 1);

            // The JIT won't hoist these "constants", so help him
            Vector128<sbyte> lutHi            = s_decodeLutHi;
            Vector128<sbyte> lutLo            = s_decodeLutLo;
            Vector128<sbyte> lutRoll          = s_decodeLutRoll;
            Vector128<sbyte> mask2F           = s_decodeMask2F;
            Vector128<sbyte> shuffleConstant0 = Sse.StaticCast<int, sbyte>(Sse2.SetAllVector128(0x01400140));
            Vector128<short> shuffleConstant1 = Sse.StaticCast<int, short>(Sse2.SetAllVector128(0x00011000));
            Vector128<sbyte> shuffleVec       = s_decodeShuffleVec;

            //while (remaining >= 24)
            while (Unsafe.IsAddressLessThan(ref src, ref simdSrcEnd))
            {
                //Vector128<sbyte> str = Unsafe.ReadUnaligned<Vector128<sbyte>>(ref srcBytes);
                Vector128<sbyte> str;

                if (typeof(T) == typeof(byte))
                {
                    ref byte srcBytes = ref Unsafe.As<T, byte>(ref src);
                    str = Unsafe.ReadUnaligned<Vector128<sbyte>>(ref srcBytes);
                }
                else if (typeof(T) == typeof(char))
                {
                    ref byte srcBytes = ref Unsafe.As<T, byte>(ref src);
                    Vector128<short> c0 = Unsafe.ReadUnaligned<Vector128<short>>(ref srcBytes);
                    Vector128<short> c1 = Unsafe.ReadUnaligned<Vector128<short>>(ref Unsafe.Add(ref srcBytes, 16));

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

                if (Sse2.MoveMask(Sse2.CompareGreaterThan(Sse2.And(lo, hi), Sse2.SetZeroVector128<sbyte>())) != 0)
                {
                    success = false;
                    break;
                }

                Vector128<sbyte> eq2F = Sse2.CompareEqual(str, mask2F);
                Vector128<sbyte> roll = Ssse3.Shuffle(lutRoll, Sse2.Add(eq2F, hiNibbles));
                str                   = Sse2.Add(str, roll);

                Vector128<short> merge_ab_and_bc = Ssse3.MultiplyAddAdjacent(Sse.StaticCast<sbyte, byte>(str), shuffleConstant0);
                Vector128<int> @out              = Sse2.MultiplyHorizontalAdd(merge_ab_and_bc, shuffleConstant1);
                str                              = Ssse3.Shuffle(Sse.StaticCast<int, sbyte>(@out), shuffleVec);

                Unsafe.WriteUnaligned(ref destBytes, str);

                src       = ref Unsafe.Add(ref src,  16);
                destBytes = ref Unsafe.Add(ref destBytes, 12);
            }

            // Cast to long to avoid the overflow-check
            sourceIndex = ((int)(long)Unsafe.ByteOffset(ref srcStart, ref src)) / Unsafe.SizeOf<T>();
            destIndex   = (int)(long)Unsafe.ByteOffset(ref destStart, ref destBytes);

            src       = ref srcStart;
            destBytes = ref destStart;

            return success;
        }
#endif
        //---------------------------------------------------------------------
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Decode<T>(ref T encoded, ref sbyte decodingMap)
        {
            int i0, i1, i2, i3;

            if (typeof(T) == typeof(byte))
            {
                ref byte tmp = ref Unsafe.As<T, byte>(ref encoded);
                i0 = Unsafe.Add(ref tmp, 0);
                i1 = Unsafe.Add(ref tmp, 1);
                i2 = Unsafe.Add(ref tmp, 2);
                i3 = Unsafe.Add(ref tmp, 3);
            }
            else if (typeof(T) == typeof(char))
            {
                ref char tmp = ref Unsafe.As<T, char>(ref encoded);
                i0 = Unsafe.Add(ref tmp, 0);
                i1 = Unsafe.Add(ref tmp, 1);
                i2 = Unsafe.Add(ref tmp, 2);
                i3 = Unsafe.Add(ref tmp, 3);
            }
            else
            {
                throw new NotSupportedException();  // just in case new types are introduced in the future
            }

            i0 = Unsafe.Add(ref decodingMap, i0);
            i1 = Unsafe.Add(ref decodingMap, i1);
            i2 = Unsafe.Add(ref decodingMap, i2);
            i3 = Unsafe.Add(ref decodingMap, i3);

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
#if NETCOREAPP2_1
        private static readonly Vector128<sbyte> s_decodeShuffleVec;
        private static readonly Vector128<sbyte> s_decodeLutLo;
        private static readonly Vector128<sbyte> s_decodeLutHi;
        private static readonly Vector128<sbyte> s_decodeLutRoll;
        private static readonly Vector128<sbyte> s_decodeMask2F;
#endif
        // internal because tests use this map too
        internal static readonly sbyte[] s_decodingMap = {
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 62, -1, -1, -1, 63,         //62 is placed at index 43 (for +), 63 at index 47 (for /)
            52, 53, 54, 55, 56, 57, 58, 59, 60, 61, -1, -1, -1, -1, -1, -1,         //52-61 are placed at index 48-57 (for 0-9), 64 at index 61 (for =)
            -1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14,
            15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, -1, -1, -1, -1, -1,         //0-25 are placed at index 65-90 (for A-Z)
            -1, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40,
            41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, -1, -1, -1, -1, -1,         //26-51 are placed at index 97-122 (for a-z)
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,         // Bytes over 122 ('z') are invalid and cannot be decoded
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,         // Hence, padding the map with 255, which indicates invalid input
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
        };
    }
}
