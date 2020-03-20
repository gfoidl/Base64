using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

// Scalar based on https://github.com/dotnet/corefx/tree/ec34e99b876ea1119f37986ead894f4eded1a19a/src/System.Memory/src/System/Buffers/Text
// SSE2   based on https://github.com/aklomp/base64/tree/a27c565d1b6c676beaf297fe503c4518185666f7/lib/arch/ssse3
// AVX2   based on https://github.com/aklomp/base64/tree/a27c565d1b6c676beaf297fe503c4518185666f7/lib/arch/avx2

namespace gfoidl.Base64.Internal
{
    public partial class Base64Encoder
    {
        // internal for benchmarks
        internal OperationStatus EncodeImpl<T>(
            ref byte srcBytes,
            int srcLength,
            ref T dest,
            int destLength,
            int encodedLength,
            out int consumed,
            out int written,
            bool isFinalBlock = true)
            where T : unmanaged
        {
            uint sourceIndex = 0;
            uint destIndex   = 0;
            int maxSrcLength;

            if (srcLength <= MaximumEncodeLength && destLength >= encodedLength)
            {
                maxSrcLength = srcLength;
            }
            else
            {
                maxSrcLength = (destLength >> 2) * 3;
            }

            if (Avx2.IsSupported)
            {
                // AVX will eat as much as possible, then SSE will eat as much as possible.
                // In order to reuse AVX vectors in SSE the SSE-path is "duplicated" in Avx2Encode.
                if (maxSrcLength >= 16)
                {
                    this.Avx2Encode(ref srcBytes, ref dest, maxSrcLength, destLength, ref sourceIndex, ref destIndex);

                    if (sourceIndex == srcLength)
                        goto DoneExit;
                }
            }
            else if (Ssse3.IsSupported)
            {
                if (maxSrcLength >= 16)
                {
                    this.Ssse3Encode(ref srcBytes, ref dest, maxSrcLength, destLength, ref sourceIndex, ref destIndex);

                    if (sourceIndex == srcLength)
                        goto DoneExit;
                }
            }

            maxSrcLength -= 2;

            ref byte encodingMap = ref MemoryMarshal.GetReference(EncodingMap);

            // In order to elide the movsxd in the loop
            if (sourceIndex < maxSrcLength)
            {
                do
                {
#if DEBUG
                    ScalarEncodingIteration?.Invoke();
#endif
                    EncodeThreeBytes(ref Unsafe.Add(ref srcBytes, (IntPtr)sourceIndex), ref Unsafe.Add(ref dest, (IntPtr)destIndex), ref encodingMap);
                    destIndex   += 4;
                    sourceIndex += 3;
                }
                while (sourceIndex < (uint)maxSrcLength);
            }

            if (maxSrcLength != srcLength - 2)
                goto DestinationTooSmallExit;

            if (!isFinalBlock)
            {
                if (sourceIndex == srcLength)
                    goto DoneExit;

                goto NeedMoreDataExit;
            }

            if (sourceIndex == srcLength - 1)
            {
                EncodeOneByte(ref Unsafe.Add(ref srcBytes, (IntPtr)sourceIndex), ref Unsafe.Add(ref dest, (IntPtr)destIndex), ref encodingMap);
                destIndex   += 4;
                sourceIndex += 1;
            }
            else if (sourceIndex == srcLength - 2)
            {
                EncodeTwoBytes(ref Unsafe.Add(ref srcBytes, (IntPtr)sourceIndex), ref Unsafe.Add(ref dest, (IntPtr)destIndex), ref encodingMap);
                destIndex   += 4;
                sourceIndex += 2;
            }

        DoneExit:
            consumed = (int)sourceIndex;
            written  = (int)destIndex;
            return OperationStatus.Done;

        NeedMoreDataExit:
            consumed = (int)sourceIndex;
            written  = (int)destIndex;
            return OperationStatus.NeedMoreData;

        DestinationTooSmallExit:
            consumed = (int)sourceIndex;
            written  = (int)destIndex;
            return OperationStatus.DestinationTooSmall;
        }
        //---------------------------------------------------------------------
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Avx2Encode<T>(ref byte src, ref T dest, int sourceLength, int destLength, ref uint sourceIndex, ref uint destIndex)
            where T : unmanaged
        {
            ref byte srcStart = ref src;
            ref T destStart   = ref dest;

            // The JIT won't hoist these "constants", so help it
            Vector256<sbyte>  avxShuffleVec          = AvxEncodeShuffleVec.ReadVector256();
            Vector256<sbyte>  avxShuffleConstant0    = Vector256.Create(0x0fc0fc00).AsSByte();
            Vector256<sbyte>  avxShuffleConstant2    = Vector256.Create(0x003f03f0).AsSByte();
            Vector256<ushort> avxShuffleConstant1    = Vector256.Create(0x04000040).AsUInt16();
            Vector256<short>  avxShuffleConstant3    = Vector256.Create(0x01000010).AsInt16();
            Vector256<byte>   avxTranslationContant0 = Vector256.Create((byte)51);
            Vector256<sbyte>  avxTranslationContant1 = Vector256.Create((sbyte)25);
            Vector256<sbyte>  avxLut                 = AvxEncodeLut.ReadVector256();

            if (sourceLength < 32)
                goto Sse;

            // first load is done at c-0 not to get a segfault
            src.AssertRead<Vector256<sbyte>, byte>(ref srcStart, sourceLength);
            Vector256<sbyte> avxStr = src.ReadVector256();

            // shift by 4 bytes, as required by enc_reshuffle
            avxStr = Avx2.PermuteVar8x32(avxStr.AsInt32(), AvxEncodePermuteVec.ReadVector256().AsInt32()).AsSByte();

            // Next loads are at c-4, so shift it once
            src = ref Unsafe.Subtract(ref src, 4);

            ref byte simdSrcEnd = ref Unsafe.Add(ref srcStart, (IntPtr)((uint)sourceLength - 32));   // no +1 as the comparison is >

            while (true)
            {
#if DEBUG
                Avx2EncodingIteration?.Invoke();
#endif
                // Reshuffle
                avxStr               = Avx2.Shuffle(avxStr, avxShuffleVec);
                Vector256<sbyte>  t0 = Avx2.And(avxStr, avxShuffleConstant0);
                Vector256<sbyte>  t2 = Avx2.And(avxStr, avxShuffleConstant2);
                Vector256<ushort> t1 = Avx2.MultiplyHigh(t0.AsUInt16(), avxShuffleConstant1);
                Vector256<short>  t3 = Avx2.MultiplyLow(t2.AsInt16(), avxShuffleConstant3);
                avxStr               = Avx2.Or(t1.AsSByte(), t3.AsSByte());

                // Translation
                Vector256<byte>  indices = Avx2.SubtractSaturate(avxStr.AsByte(), avxTranslationContant0);
                Vector256<sbyte> mask    = Avx2.CompareGreaterThan(avxStr, avxTranslationContant1);
                Vector256<sbyte> tmp     = Avx2.Subtract(indices.AsSByte(), mask);
                avxStr                   = Avx2.Add(avxStr, Avx2.Shuffle(avxLut, tmp));

                dest.AssertWrite<Vector256<sbyte>, T>(ref destStart, destLength);
                dest.WriteVector256(avxStr);

                src  = ref Unsafe.Add(ref src , 24);
                dest = ref Unsafe.Add(ref dest, 32);

                if (Unsafe.IsAddressGreaterThan(ref src, ref simdSrcEnd))
                    break;

                // Load at c-4, as required by enc_reshuffle
                src.AssertRead<Vector256<sbyte>, byte>(ref srcStart, sourceLength);
                avxStr = src.ReadVector256();
            }
#if DEBUG
            Avx2Encoded?.Invoke();
#endif
            src = ref Unsafe.Add(ref src, 4);

        Sse:
            simdSrcEnd = ref Unsafe.Add(ref srcStart, (IntPtr)((uint)sourceLength - 16 + 1));   //  +1 for <

            if (!Unsafe.IsAddressLessThan(ref src, ref simdSrcEnd))
                goto Exit;

            // Reuse AVX vectors
            Vector128<sbyte>  shuffleVec          = avxShuffleVec.GetUpper();
            Vector128<sbyte>  shuffleConstant0    = avxShuffleConstant0.GetLower();
            Vector128<sbyte>  shuffleConstant2    = avxShuffleConstant2.GetLower();
            Vector128<ushort> shuffleConstant1    = avxShuffleConstant1.GetLower();
            Vector128<short>  shuffleConstant3    = avxShuffleConstant3.GetLower();
            Vector128<byte>   translationContant0 = avxTranslationContant0.GetLower();
            Vector128<sbyte>  translationContant1 = avxTranslationContant1.GetLower();
            Vector128<sbyte>  lut                 = avxLut.GetLower();

            //while (remaining >= 16)
            do
            {
#if DEBUG
                Ssse3EncodingIteration?.Invoke();
#endif
                src.AssertRead<Vector128<sbyte>, byte>(ref srcStart, sourceLength);
                Vector128<sbyte> str = src.ReadVector128();

                // Reshuffle
                str                  = Ssse3.Shuffle(str, shuffleVec);
                Vector128<sbyte>  t0 = Sse2.And(str, shuffleConstant0);
                Vector128<sbyte>  t2 = Sse2.And(str, shuffleConstant2);
                Vector128<ushort> t1 = Sse2.MultiplyHigh(t0.AsUInt16(), shuffleConstant1);
                Vector128<short>  t3 = Sse2.MultiplyLow(t2.AsInt16(), shuffleConstant3);
                str                  = Sse2.Or(t1.AsSByte(), t3.AsSByte());

                // Translation
                Vector128<byte>  indices = Sse2.SubtractSaturate(str.AsByte(), translationContant0);
                Vector128<sbyte> mask    = Sse2.CompareGreaterThan(str, translationContant1);
                Vector128<sbyte> tmp     = Sse2.Subtract(indices.AsSByte(), mask);
                str                      = Sse2.Add(str, Ssse3.Shuffle(lut, tmp));

                dest.AssertWrite<Vector128<sbyte>, T>(ref destStart, destLength);
                dest.WriteVector128(str);

                src  = ref Unsafe.Add(ref src,  12);
                dest = ref Unsafe.Add(ref dest, 16);
            }
            while (Unsafe.IsAddressLessThan(ref src, ref simdSrcEnd));
#if DEBUG
            Ssse3Encoded?.Invoke();
#endif
        Exit:
            // Cast to ulong to avoid the overflow-check. Codegen for x86 is still good.
            sourceIndex = (uint)(ulong)Unsafe.ByteOffset(ref srcStart , ref src);
            destIndex   = (uint)(ulong)Unsafe.ByteOffset(ref destStart, ref dest) / (uint)Unsafe.SizeOf<T>();

            src  = ref srcStart;
            dest = ref destStart;
        }
        //---------------------------------------------------------------------
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Ssse3Encode<T>(ref byte src, ref T dest, int sourceLength, int destLength, ref uint sourceIndex, ref uint destIndex)
            where T : unmanaged
        {
            ref byte srcStart   = ref src;
            ref T destStart     = ref dest;
            ref byte simdSrcEnd = ref Unsafe.Add(ref srcStart, (IntPtr)((uint)sourceLength - 16 + 1));   //  +1 for <=

            // Shift to workspace
            src  = ref Unsafe.Add(ref src , (IntPtr)sourceIndex);
            dest = ref Unsafe.Add(ref dest, (IntPtr)destIndex);

            // The JIT won't hoist these "constants", so help it
            Vector128<sbyte>  shuffleVec          = SseEncodeShuffleVec.ReadVector128();
            Vector128<sbyte>  shuffleConstant0    = Vector128.Create(0x0fc0fc00).AsSByte();
            Vector128<sbyte>  shuffleConstant2    = Vector128.Create(0x003f03f0).AsSByte();
            Vector128<ushort> shuffleConstant1    = Vector128.Create(0x04000040).AsUInt16();
            Vector128<short>  shuffleConstant3    = Vector128.Create(0x01000010).AsInt16();
            Vector128<byte>   translationContant0 = Vector128.Create((byte) 51);
            Vector128<sbyte>  translationContant1 = Vector128.Create((sbyte)25);
            Vector128<sbyte>  lut                 = SseEncodeLut.ReadVector128();

            //while (remaining >= 16)
            do
            {
#if DEBUG
                Ssse3EncodingIteration?.Invoke();
#endif
                src.AssertRead<Vector128<sbyte>, byte>(ref srcStart, sourceLength);
                Vector128<sbyte> str = src.ReadVector128();

                // Reshuffle
                str                  = Ssse3.Shuffle(str, shuffleVec);
                Vector128<sbyte>  t0 = Sse2.And(str, shuffleConstant0);
                Vector128<sbyte>  t2 = Sse2.And(str, shuffleConstant2);
                Vector128<ushort> t1 = Sse2.MultiplyHigh(t0.AsUInt16(), shuffleConstant1);
                Vector128<short>  t3 = Sse2.MultiplyLow(t2.AsInt16(), shuffleConstant3);
                str                  = Sse2.Or(t1.AsSByte(), t3.AsSByte());

                // Translation
                Vector128<byte>  indices = Sse2.SubtractSaturate(str.AsByte(), translationContant0);
                Vector128<sbyte> mask    = Sse2.CompareGreaterThan(str, translationContant1);
                Vector128<sbyte> tmp     = Sse2.Subtract(indices.AsSByte(), mask);
                str                      = Sse2.Add(str, Ssse3.Shuffle(lut, tmp));

                dest.AssertWrite<Vector128<sbyte>, T>(ref destStart, destLength);
                dest.WriteVector128(str);

                src  = ref Unsafe.Add(ref src,  12);
                dest = ref Unsafe.Add(ref dest, 16);
            }
            while (Unsafe.IsAddressLessThan(ref src, ref simdSrcEnd));

            // Cast to ulong to avoid the overflow-check. Codegen for x86 is still good.
            sourceIndex = (uint)(ulong)Unsafe.ByteOffset(ref srcStart , ref src);
            destIndex   = (uint)(ulong)Unsafe.ByteOffset(ref destStart, ref dest) / (uint)Unsafe.SizeOf<T>();

            src  = ref srcStart;
            dest = ref destStart;
#if DEBUG
            Ssse3Encoded?.Invoke();
#endif
        }
        //---------------------------------------------------------------------
        private static ReadOnlySpan<sbyte> SseEncodeLut => new sbyte[16]
        {
            65,  71, -4, -4,
            -4,  -4, -4, -4,
            -4,  -4, -4, -4,
           -19, -16,  0,  0
        };

        private static ReadOnlySpan<sbyte> AvxEncodeLut => new sbyte[32]
        {
            65,  71, -4, -4,
            -4,  -4, -4, -4,
            -4,  -4, -4, -4,
           -19, -16,  0,  0,
            65,  71, -4, -4,
            -4,  -4, -4, -4,
            -4,  -4, -4, -4,
           -19, -16,  0,  0
        };
    }
}
