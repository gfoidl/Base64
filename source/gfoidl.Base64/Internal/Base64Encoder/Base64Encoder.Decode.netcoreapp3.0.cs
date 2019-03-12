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
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
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
            int srcLength    = inputLength & ~0x3;      // only decode input up to the closest multiple of 4.
            int maxSrcLength = srcLength;
            int destLength   = data.Length;

            // max. 2 padding chars
            if (destLength < decodedLength - 2)
            {
                // For overflow see comment below
                maxSrcLength = destLength / 3 * 4;
            }

            ref byte destBytes = ref MemoryMarshal.GetReference(data);

            if (Avx2.IsSupported && maxSrcLength >= 45)
            {
                Avx2Decode(ref src, ref destBytes, maxSrcLength, destLength, ref sourceIndex, ref destIndex);

                if (sourceIndex == srcLength)
                    goto DoneExit;
            }
            else if (Ssse3.IsSupported && maxSrcLength >= 24)
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
                maxSrcLength = destLength / 3 * 4;
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
            uint t0, t1, t2, t3;

            if (typeof(T) == typeof(byte))
            {
                ref byte tmp = ref Unsafe.As<T, byte>(ref src);
                t0 = Unsafe.Add(ref tmp, (IntPtr)(uint)(srcLength - 4));
                t1 = Unsafe.Add(ref tmp, (IntPtr)(uint)(srcLength - 3));
                t2 = Unsafe.Add(ref tmp, (IntPtr)(uint)(srcLength - 2));
                t3 = Unsafe.Add(ref tmp, (IntPtr)(uint)(srcLength - 1));
            }
            else if (typeof(T) == typeof(char))
            {
                ref char tmp = ref Unsafe.As<T, char>(ref src);
                t0 = Unsafe.Add(ref tmp, (IntPtr)(uint)(srcLength - 4));
                t1 = Unsafe.Add(ref tmp, (IntPtr)(uint)(srcLength - 3));
                t2 = Unsafe.Add(ref tmp, (IntPtr)(uint)(srcLength - 2));
                t3 = Unsafe.Add(ref tmp, (IntPtr)(uint)(srcLength - 1));
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
                    goto InvalidDataExit;

                if (destIndex > destLength - 3)
                    goto DestinationTooSmallExit;

                WriteThreeLowOrderBytes(ref destBytes, destIndex, i0);
                destIndex += 3;
            }
            else if (t2 != EncodingPad)
            {
                int i2 = Unsafe.Add(ref decodingMap, (IntPtr)t2);

                i2 <<= 6;

                i0 |= i2;

                if (i0 < 0)
                    goto InvalidDataExit;

                if (destIndex > destLength - 2)
                    goto DestinationTooSmallExit;

                Unsafe.Add(ref destBytes, (IntPtr)destIndex)       = (byte)(i0 >> 16);
                Unsafe.Add(ref destBytes, (IntPtr)(destIndex + 1)) = (byte)(i0 >> 8);
                destIndex += 2;
            }
            else
            {
                if (i0 < 0)
                    goto InvalidDataExit;

                if (destIndex > destLength - 1)
                    goto DestinationTooSmallExit;

                Unsafe.Add(ref destBytes, (IntPtr)destIndex) = (byte)(i0 >> 16);
                destIndex += 1;
            }

            sourceIndex += 4;

            if (srcLength != inputLength)
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
        public static event EventHandler<EventArgs> Avx2Decoded;
        public static event EventHandler<EventArgs> Ssse3Decoded;
#endif
        //---------------------------------------------------------------------
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Avx2Decode<T>(ref T src, ref byte destBytes, int sourceLength, int destLength, ref uint sourceIndex, ref uint destIndex)
            where T : unmanaged
        {
            ref T srcStart     = ref src;
            ref byte destStart = ref destBytes;
            ref T simdSrcEnd   = ref Unsafe.Add(ref src, (IntPtr)((uint)sourceLength - 45 + 1));   //  +1 for <=

            // The JIT won't hoist these "constants", so help it
            Vector256<sbyte> lutHi            = s_avxDecodeLutHi.ReadVector256();
            Vector256<sbyte> lutLo            = s_avxDecodeLutLo.ReadVector256();
            Vector256<sbyte> lutShift         = s_avxDecodeLutShift.ReadVector256();
            Vector256<sbyte> mask2F           = Vector256.Create((sbyte)0x2F);     // ASCII: /
            Vector256<sbyte> shuffleConstant0 = Vector256.Create(0x01400140).AsSByte();
            Vector256<short> shuffleConstant1 = Vector256.Create(0x00011000).AsInt16();
            Vector256<sbyte> shuffleVec       = s_avxDecodeShuffleVec.ReadVector256();
            Vector256<int>   permuteVec       = s_avxDecodePermuteVec.ReadVector256().AsInt32();

            //while (remaining >= 45)
            do
            {
                src.AssertRead<Vector256<sbyte>, T>(ref srcStart, sourceLength);
                Vector256<sbyte> str = src.ReadVector256();

                Vector256<sbyte> hiNibbles = Avx2.And(Avx2.ShiftRightLogical(str.AsInt32(), 4).AsSByte(), mask2F);
                Vector256<sbyte> loNibbles = Avx2.And(str, mask2F);
                Vector256<sbyte> hi        = Avx2.Shuffle(lutHi, hiNibbles);
                Vector256<sbyte> lo        = Avx2.Shuffle(lutLo, loNibbles);
                Vector256<sbyte> zero      = Vector256<sbyte>.Zero;

                // https://github.com/dotnet/coreclr/issues/21247
                if (Avx2.MoveMask(Avx2.CompareGreaterThan(Avx2.And(lo, hi), zero)) != 0)
                    break;
#if DEBUG
                Avx2Decoded?.Invoke(null, EventArgs.Empty);
#endif
                Vector256<sbyte> eq2F  = Avx2.CompareEqual(str, mask2F);
                Vector256<sbyte> shift = Avx2.Shuffle(lutShift, Avx2.Add(eq2F, hiNibbles));
                str                    = Avx2.Add(str, shift);

                Vector256<short> merge_ab_and_bc = Avx2.MultiplyAddAdjacent(str.AsByte(), shuffleConstant0);
                Vector256<int> @out              = Avx2.MultiplyAddAdjacent(merge_ab_and_bc, shuffleConstant1);
                @out                             = Avx2.Shuffle(@out.AsSByte(), shuffleVec).AsInt32();
                str                              = Avx2.PermuteVar8x32(@out, permuteVec).AsSByte();

                destBytes.AssertWrite<Vector256<sbyte>, byte>(ref destStart, destLength);
                destBytes.WriteVector256(str);

                src       = ref Unsafe.Add(ref src, 32);
                destBytes = ref Unsafe.Add(ref destBytes, 24);
            }
            while (Unsafe.IsAddressLessThan(ref src, ref simdSrcEnd));

            // Cast to ulong to avoid the overflow-check. Codegen for x86 is still good.
            sourceIndex = (uint)(ulong)Unsafe.ByteOffset(ref srcStart, ref src) / (uint)Unsafe.SizeOf<T>();
            destIndex   = (uint)(ulong)Unsafe.ByteOffset(ref destStart, ref destBytes);

            src       = ref srcStart;
            destBytes = ref destStart;
        }
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
            Vector128<sbyte> mask2F           = Vector128.Create((sbyte)0x2F);      // ASCII: /
            Vector128<sbyte> shuffleConstant0 = Vector128.Create(0x01400140).AsSByte();
            Vector128<short> shuffleConstant1 = Vector128.Create(0x00011000).AsInt16();
            Vector128<sbyte> shuffleVec       = s_sseDecodeShuffleVec.ReadVector128();

            //while (remaining >= 24)
            do
            {
                src.AssertRead<Vector128<sbyte>, T>(ref srcStart, sourceLength);
                Vector128<sbyte> str = src.ReadVector128();

                Vector128<sbyte> hiNibbles = Sse2.And(Sse2.ShiftRightLogical(str.AsInt32(), 4).AsSByte(), mask2F);
                Vector128<sbyte> loNibbles = Sse2.And(str, mask2F);
                Vector128<sbyte> hi        = Ssse3.Shuffle(lutHi, hiNibbles);
                Vector128<sbyte> lo        = Ssse3.Shuffle(lutLo, loNibbles);
                Vector128<sbyte> zero      = Vector128<sbyte>.Zero;

                if (Sse2.MoveMask(Sse2.CompareGreaterThan(Sse2.And(lo, hi), zero)) != 0)
                    break;
#if DEBUG
                Ssse3Decoded?.Invoke(null, EventArgs.Empty);
#endif
                Vector128<sbyte> eq2F  = Sse2.CompareEqual(str, mask2F);
                Vector128<sbyte> shift = Ssse3.Shuffle(lutShift, Sse2.Add(eq2F, hiNibbles));
                str                    = Sse2.Add(str, shift);

                Vector128<short> merge_ab_and_bc = Ssse3.MultiplyAddAdjacent(str.AsByte(), shuffleConstant0);
                Vector128<int> @out              = Sse2.MultiplyAddAdjacent(merge_ab_and_bc, shuffleConstant1);
                str                              = Ssse3.Shuffle(@out.AsSByte(), shuffleVec);

                destBytes.AssertWrite<Vector128<sbyte>, byte>(ref destStart, destLength);
                destBytes.WriteVector128(str);

                src       = ref Unsafe.Add(ref src,  16);
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
        private static ReadOnlySpan<sbyte> s_sseDecodeLutLo => new sbyte[]
        {
            0x15, 0x11, 0x11, 0x11,
            0x11, 0x11, 0x11, 0x11,
            0x11, 0x11, 0x13, 0x1A,
            0x1B, 0x1B, 0x1B, 0x1A
        };

        private static ReadOnlySpan<sbyte> s_sseDecodeLutHi => new sbyte[]
        {
            0x10, 0x10, 0x01, 0x02,
            0x04, 0x08, 0x04, 0x08,
            0x10, 0x10, 0x10, 0x10,
            0x10, 0x10, 0x10, 0x10
        };

        private static ReadOnlySpan<sbyte> s_sseDecodeLutShift => new sbyte[]
        {
              0,  16,  19,   4,
            -65, -65, -71, -71,
              0,   0,   0,   0,
              0,   0,   0,   0
        };

        private static ReadOnlySpan<sbyte> s_avxDecodeLutLo => new sbyte[]
        {
            0x15, 0x11, 0x11, 0x11,
            0x11, 0x11, 0x11, 0x11,
            0x11, 0x11, 0x13, 0x1A,
            0x1B, 0x1B, 0x1B, 0x1A,
            0x15, 0x11, 0x11, 0x11,
            0x11, 0x11, 0x11, 0x11,
            0x11, 0x11, 0x13, 0x1A,
            0x1B, 0x1B, 0x1B, 0x1A
        };

        private static ReadOnlySpan<sbyte> s_avxDecodeLutHi => new sbyte[]
        {
            0x10, 0x10, 0x01, 0x02,
            0x04, 0x08, 0x04, 0x08,
            0x10, 0x10, 0x10, 0x10,
            0x10, 0x10, 0x10, 0x10,
            0x10, 0x10, 0x01, 0x02,
            0x04, 0x08, 0x04, 0x08,
            0x10, 0x10, 0x10, 0x10,
            0x10, 0x10, 0x10, 0x10
        };

        private static ReadOnlySpan<sbyte> s_avxDecodeLutShift => new sbyte[]
        {
             0,  16,  19,   4,
           -65, -65, -71, -71,
             0,   0,   0,   0,
             0,   0,   0,   0,
             0,  16,  19,   4,
           -65, -65, -71, -71,
             0,   0,   0,   0,
             0,   0,   0,   0
        };
    }
}
