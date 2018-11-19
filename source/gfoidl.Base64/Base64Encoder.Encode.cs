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
#if !NETCOREAPP2_1
        private const int MaxStackallocBytes = 256;
#endif
        public const int MaximumEncodeLength = (int.MaxValue / 4) * 3; // 1610612733
        //---------------------------------------------------------------------
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetEncodedLength(int sourceLength)
        {
            if ((uint)sourceLength > MaximumEncodeLength)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.length);

            return (sourceLength + 2) / 3 * 4;
        }
        //---------------------------------------------------------------------
        public OperationStatus Encode(ReadOnlySpan<byte> data, Span<byte> encoded, out int consumed, out int written) => this.EncodeCore(data, encoded, out consumed, out written);
        public OperationStatus Encode(ReadOnlySpan<byte> data, Span<char> encoded, out int consumed, out int written) => this.EncodeCore(data, encoded, out consumed, out written);
        //---------------------------------------------------------------------
        public unsafe string Encode(ReadOnlySpan<byte> data)
        {
            if (data.IsEmpty) return string.Empty;

            int encodedLength = this.GetEncodedLength(data.Length);
#if NETCOREAPP2_1
            fixed (byte* ptr = data)
            {
                return string.Create(encodedLength, (Ptr: (IntPtr)ptr, data.Length), (encoded, state) =>
                {
                    ref byte srcBytes      = ref Unsafe.AsRef<byte>(state.Ptr.ToPointer());
                    OperationStatus status = this.EncodeCore(ref srcBytes, state.Length, encoded, out int consumed, out int written);

                    Debug.Assert(status         == OperationStatus.Done);
                    Debug.Assert(state.Length   == consumed);
                    Debug.Assert(encoded.Length == written);
                });
            }
#elif NETCOREAPP2_0
            char[] arrayToReturnToPool = null;
            try
            {
                Span<char> encoded = encodedLength <= MaxStackallocBytes / sizeof(char)
                    ? stackalloc char[encodedLength]
                    : arrayToReturnToPool = ArrayPool<char>.Shared.Rent(encodedLength);

                OperationStatus status = this.EncodeCore(data, encoded, out int consumed, out int written);
                Debug.Assert(status        == OperationStatus.Done);
                Debug.Assert(data.Length   == consumed);
                Debug.Assert(encodedLength == written);

                fixed (char* ptr = encoded)
                    return new string(ptr, 0, written);
            }
            finally
            {
                if (arrayToReturnToPool != null)
                    ArrayPool<char>.Shared.Return(arrayToReturnToPool);
            }
#else
            Span<char> encoded = encodedLength <= MaxStackallocBytes / sizeof(char)
                ? stackalloc char[encodedLength]
                : new char[encodedLength];

            OperationStatus status = this.EncodeCore(data, encoded, out int consumed, out int written);
            Debug.Assert(status         == OperationStatus.Done);
            Debug.Assert(data.Length    == consumed);
            Debug.Assert(encoded.Length == written);

            fixed (char* ptr = encoded)
                return new string(ptr, 0, written);
#endif
        }
        //---------------------------------------------------------------------
        internal OperationStatus EncodeCore<T>(ReadOnlySpan<byte> data, Span<T> encoded, out int consumed, out int written)
        {
            if (data.IsEmpty)
            {
                consumed = 0;
                written = 0;
                return OperationStatus.Done;
            }

            ref byte srcBytes = ref MemoryMarshal.GetReference(data);
            int srcLength     = data.Length;

            return this.EncodeCore(ref srcBytes, srcLength, encoded, out consumed, out written);
        }
        //---------------------------------------------------------------------
        private OperationStatus EncodeCore<T>(ref byte srcBytes, int srcLength, Span<T> encoded, out int consumed, out int written)
        {
            int destLength  = encoded.Length;
            int sourceIndex = 0;
            int destIndex   = 0;

            ref T dest = ref MemoryMarshal.GetReference(encoded);
#if NETCOREAPP2_1
            if (Sse2.IsSupported && Ssse3.IsSupported && srcLength >= 16)
            {
                Sse2Encode(ref srcBytes, ref dest, srcLength, ref sourceIndex, ref destIndex);

                if (sourceIndex == srcLength)
                    goto DoneExit;
            }
#endif
            int maxSrcLength = 0;

            if (srcLength <= MaximumEncodeLength && destLength >= this.GetEncodedLength(srcLength))
                maxSrcLength = srcLength - 2;
            else
                maxSrcLength = (destLength >> 2) * 3 - 2;

            ref byte encodingMap = ref s_encodingMap[0];

            while (sourceIndex < maxSrcLength)
            {
                EncodeThreeBytes(ref Unsafe.Add(ref srcBytes, sourceIndex), ref Unsafe.Add(ref dest, destIndex), ref encodingMap);
                destIndex   += 4;
                sourceIndex += 3;
            }

            if (maxSrcLength != srcLength - 2)
                goto DestinationSmallExit;

            if (sourceIndex == srcLength - 1)
            {
                EncodeOneByte(ref Unsafe.Add(ref srcBytes, sourceIndex), ref Unsafe.Add(ref dest, destIndex), ref encodingMap);
                destIndex   += 4;
                sourceIndex += 1;
            }
            else if (sourceIndex == srcLength - 2)
            {
                EncodeTwoBytes(ref Unsafe.Add(ref srcBytes, sourceIndex), ref Unsafe.Add(ref dest, destIndex), ref encodingMap);
                destIndex   += 4;
                sourceIndex += 2;
            }
#if NETCOREAPP2_1
        DoneExit:
#endif
            consumed = sourceIndex;
            written  = destIndex;
            return OperationStatus.Done;

        DestinationSmallExit:
            consumed = sourceIndex;
            written  = destIndex;
            return OperationStatus.DestinationTooSmall;
        }
        //---------------------------------------------------------------------
#if NETCOREAPP2_1
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Sse2Encode<T>(ref byte src, ref T dest, int sourceLength, ref int sourceIndex, ref int destIndex)
        {
            ref byte srcStart   = ref src;
            ref T destStart     = ref dest;
            ref byte simdSrcEnd = ref Unsafe.Add(ref src, sourceLength - 16 + 1);

            // The JIT won't hoist these "constants", so help him
            Vector128<sbyte>  shuffleVec          = s_encodeShuffleVec;
            Vector128<sbyte>  shuffleConstant0    = Sse.StaticCast<int, sbyte>(Sse2.SetAllVector128(0x0fc0fc00));
            Vector128<sbyte>  shuffleConstant2    = Sse.StaticCast<int, sbyte>(Sse2.SetAllVector128(0x003f03f0));
            Vector128<ushort> shuffleConstant1    = Sse.StaticCast<int, ushort>(Sse2.SetAllVector128(0x04000040));
            Vector128<short>  shuffleConstant3    = Sse.StaticCast<int, short>(Sse2.SetAllVector128(0x01000010));
            Vector128<byte>   translationContant0 = Sse2.SetAllVector128((byte)51);
            Vector128<sbyte>  translationContant1 = Sse2.SetAllVector128((sbyte)25);
            Vector128<sbyte>  lut                 = s_encodeLut;

            //while (remaining >= 16)
            while (Unsafe.IsAddressLessThan(ref src, ref simdSrcEnd))
            {
                Vector128<sbyte> str = Unsafe.ReadUnaligned<Vector128<sbyte>>(ref src);

                // Reshuffle
                str                  = Ssse3.Shuffle(str, shuffleVec);
                Vector128<sbyte>  t0 = Sse2.And(str, shuffleConstant0);
                Vector128<sbyte>  t2 = Sse2.And(str, shuffleConstant2);
                Vector128<ushort> t1 = Sse2.MultiplyHigh(Sse.StaticCast<sbyte, ushort>(t0), shuffleConstant1);
                Vector128<short>  t3 = Sse2.MultiplyLow(Sse.StaticCast<sbyte, short>(t2), shuffleConstant3);
                str                  = Sse2.Or(Sse.StaticCast<ushort, sbyte>(t1), Sse.StaticCast<short, sbyte>(t3));

                // Translation
                Vector128<byte>  indices = Sse2.SubtractSaturate(Sse.StaticCast<sbyte, byte>(str), translationContant0);
                Vector128<sbyte> mask    = Sse2.CompareGreaterThan(str, translationContant1);
                Vector128<sbyte> tmp     = Sse2.Subtract(Sse.StaticCast<byte, sbyte>(indices), mask);
                str                      = Sse2.Add(str, Ssse3.Shuffle(lut, tmp));

                if (typeof(T) == typeof(byte))
                {
                    ref byte destination = ref Unsafe.As<T, byte>(ref dest);
                    Unsafe.WriteUnaligned(ref destination, str);
                }
                else if (typeof(T) == typeof(char))
                {
                    Vector128<sbyte> zero = Sse2.SetZeroVector128<sbyte>();
                    Vector128<sbyte> c0   = Sse2.UnpackLow(str, zero);
                    Vector128<sbyte> c1   = Sse2.UnpackHigh(str, zero);

                    ref byte destination = ref Unsafe.As<T, byte>(ref dest);
                    Unsafe.WriteUnaligned(ref destination, c0);
                    Unsafe.WriteUnaligned(ref Unsafe.Add(ref destination, 16), c1);
                }
                else
                {
                    throw new NotSupportedException(); // just in case new types are introduced in the future
                }

                src  = ref Unsafe.Add(ref src,  12);
                dest = ref Unsafe.Add(ref dest, 16);
            }

            // Cast to long to avoid the overflow-check
            sourceIndex = (int)(long)Unsafe.ByteOffset(ref srcStart, ref src);
            destIndex   = ((int)(long)Unsafe.ByteOffset(ref destStart, ref dest)) / Unsafe.SizeOf<T>();

            src  = ref srcStart;
            dest = ref destStart;
        }
#endif
        //---------------------------------------------------------------------
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void EncodeThreeBytes<T>(ref byte threeBytes, ref T encoded, ref byte encodingMap)
        {
            int i = (threeBytes << 16) | (Unsafe.Add(ref threeBytes, 1) << 8) | Unsafe.Add(ref threeBytes, 2);

            byte i0 = Unsafe.Add(ref encodingMap, i >> 18);
            byte i1 = Unsafe.Add(ref encodingMap, (i >> 12) & 0x3F);
            byte i2 = Unsafe.Add(ref encodingMap, (i >> 6) & 0x3F);
            byte i3 = Unsafe.Add(ref encodingMap, i & 0x3F);

            if (typeof(T) == typeof(byte))
            {
                i = i0 | (i1 << 8) | (i2 << 16) | (i3 << 24);
                Unsafe.WriteUnaligned(ref Unsafe.As<T, byte>(ref encoded), i);
            }
            else if (typeof(T) == typeof(char))
            {
                ref char enc = ref Unsafe.As<T, char>(ref encoded);
                Unsafe.Add(ref enc, 0) = (char)i0;
                Unsafe.Add(ref enc, 1) = (char)i1;
                Unsafe.Add(ref enc, 2) = (char)i2;
                Unsafe.Add(ref enc, 3) = (char)i3;
            }
            else
            {
                throw new NotSupportedException();  // just in case new types are introduced in the future
            }
        }
        //---------------------------------------------------------------------
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void EncodeTwoBytes<T>(ref byte twoBytes, ref T encoded, ref byte encodingMap)
        {
            int i = (twoBytes << 16) | (Unsafe.Add(ref twoBytes, 1) << 8);

            byte i0 = Unsafe.Add(ref encodingMap, i >> 18);
            byte i1 = Unsafe.Add(ref encodingMap, (i >> 12) & 0x3F);
            byte i2 = Unsafe.Add(ref encodingMap, (i >> 6) & 0x3F);

            if (typeof(T) == typeof(byte))
            {
                i = i0 | (i1 << 8) | (i2 << 16) | (EncodingPad << 24);
                Unsafe.WriteUnaligned(ref Unsafe.As<T, byte>(ref encoded), i);
            }
            else if (typeof(T) == typeof(char))
            {
                ref char enc = ref Unsafe.As<T, char>(ref encoded);
                Unsafe.Add(ref enc, 0) = (char)i0;
                Unsafe.Add(ref enc, 1) = (char)i1;
                Unsafe.Add(ref enc, 2) = (char)i2;
                Unsafe.Add(ref enc, 3) = (char)EncodingPad;
            }
            else
            {
                throw new NotSupportedException();  // just in case new types are introduced in the future
            }
        }
        //---------------------------------------------------------------------
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void EncodeOneByte<T>(ref byte oneByte, ref T encoded, ref byte encodingMap)
        {
            int i = (oneByte << 8);

            byte i0 = Unsafe.Add(ref encodingMap, i >> 10);
            byte i1 = Unsafe.Add(ref encodingMap, (i >> 4) & 0x3F);

            if (typeof(T) == typeof(byte))
            {
                i = i0 | (i1 << 8) | (EncodingPad << 16) | (EncodingPad << 24);
                Unsafe.WriteUnaligned(ref Unsafe.As<T, byte>(ref encoded), i);
            }
            else if (typeof(T) == typeof(char))
            {
                ref char enc = ref Unsafe.As<T, char>(ref encoded);
                Unsafe.Add(ref enc, 0) = (char)i0;
                Unsafe.Add(ref enc, 1) = (char)i1;
                Unsafe.Add(ref enc, 2) = (char)EncodingPad;
                Unsafe.Add(ref enc, 3) = (char)EncodingPad;
            }
            else
            {
                throw new NotSupportedException();  // just in case new types are introduced in the future
            }
        }
        //---------------------------------------------------------------------
#if NETCOREAPP2_1
        private static readonly Vector128<sbyte> s_encodeShuffleVec;
        private static readonly Vector128<sbyte> s_encodeLut;
#endif
        // internal because tests use this map too
        internal static readonly byte[] s_encodingMap = {
            65, 66, 67, 68, 69, 70, 71, 72,         //A..H
            73, 74, 75, 76, 77, 78, 79, 80,         //I..P
            81, 82, 83, 84, 85, 86, 87, 88,         //Q..X
            89, 90, 97, 98, 99, 100, 101, 102,      //Y..Z, a..f
            103, 104, 105, 106, 107, 108, 109, 110, //g..n
            111, 112, 113, 114, 115, 116, 117, 118, //o..v
            119, 120, 121, 122, 48, 49, 50, 51,     //w..z, 0..3
            52, 53, 54, 55, 56, 57, 43, 47          //4..9, +, /
        };
    }
}
