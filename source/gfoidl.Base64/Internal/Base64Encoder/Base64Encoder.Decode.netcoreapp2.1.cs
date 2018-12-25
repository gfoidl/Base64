using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

// Scalar based on https://github.com/dotnet/corefx/tree/master/src/System.Memory/src/System/Buffers/Text
// SSE2 based on https://github.com/aklomp/base64/tree/master/lib/arch/ssse3
// AVX2 based on https://github.com/aklomp/base64/tree/master/lib/arch/avx2

namespace gfoidl.Base64.Internal
{
    partial class Base64Encoder
    {
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
            int srcLength    = inputLength & ~0x3;      // only decode input up to the closest multiple of 4.

            ref byte destBytes  = ref MemoryMarshal.GetReference(data);

            if (Sse2.IsSupported && Ssse3.IsSupported && srcLength - 24 >= 0)
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

                WriteThreeLowOrderBytes(ref destBytes, destIndex, i0);
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
            written = (int)destIndex;
            return OperationStatus.NeedMoreData;

        InvalidExit:
            consumed = (int)sourceIndex;
            written  = (int)destIndex;
            return OperationStatus.InvalidData;
        }
        //---------------------------------------------------------------------
#if DEBUG
        public static event EventHandler<EventArgs> Sse2Decoded;
#endif
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
                    str                 = Sse.StaticCast<byte, sbyte>(Sse2.PackUnsignedSaturate(c0, c1));
                }
                else
                {
                    throw new NotSupportedException(); // just in case new types are introduced in the future
                }

                Vector128<sbyte> hiNibbles = Sse2.And(Sse.StaticCast<int, sbyte>(Sse2.ShiftRightLogical(Sse.StaticCast<sbyte, int>(str), 4)), mask2F);
                Vector128<sbyte> loNibbles = Sse2.And(str, mask2F);
                Vector128<sbyte> hi        = Ssse3.Shuffle(lutHi, hiNibbles);
                Vector128<sbyte> lo        = Ssse3.Shuffle(lutLo, loNibbles);
                Vector128<sbyte> zero = Sse2.SetZeroVector128<sbyte>();
                if (Sse2.MoveMask(Sse2.CompareGreaterThan(Sse2.And(lo, hi), zero)) != 0)
                    break;
#if DEBUG
                Sse2Decoded?.Invoke(null, EventArgs.Empty);
#endif
                Vector128<sbyte> eq2F  = Sse2.CompareEqual(str, mask2F);
                Vector128<sbyte> shift = Ssse3.Shuffle(lutShift, Sse2.Add(eq2F, hiNibbles));
                str                    = Sse2.Add(str, shift);

                Vector128<short> merge_ab_and_bc = Ssse3.MultiplyAddAdjacent(Sse.StaticCast<sbyte, byte>(str), shuffleConstant0);
                Vector128<int> @out              = Sse2.MultiplyHorizontalAdd(merge_ab_and_bc, shuffleConstant1);
                str                              = Ssse3.Shuffle(Sse.StaticCast<int, sbyte>(@out), shuffleVec);

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
        }
        //---------------------------------------------------------------------
        private static readonly Vector128<sbyte> s_sse_decodeLutLo;
        private static readonly Vector128<sbyte> s_sse_decodeLutHi;
        private static readonly Vector128<sbyte> s_sse_decodeLutShift;
        private static readonly Vector128<sbyte> s_sse_decodeMask2F;
    }
}
