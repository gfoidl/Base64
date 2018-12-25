using System;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#if NETCOREAPP
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
#endif

namespace gfoidl.Base64.Internal
{
    public abstract class Base64EncoderImpl : Base64
    {
#if NETCOREAPP
        protected static readonly Vector128<sbyte> s_sse_encodeShuffleVec;
        protected static readonly Vector128<sbyte> s_sse_decodeShuffleVec;
#if NETCOREAPP3_0
        protected static readonly Vector256<int>   s_avx_encodePermuteVec;
        protected static readonly Vector256<sbyte> s_avx_encodeShuffleVec;
        protected static readonly Vector256<sbyte> s_avx_decodeShuffleVec;
        protected static readonly Vector256<int>   s_avx_decodePermuteVec;
#endif
#endif
        //---------------------------------------------------------------------
#if NETCOREAPP3_0
        protected static readonly bool s_isMac = false;
#endif
        //---------------------------------------------------------------------
        static Base64EncoderImpl()
        {
#if NETCOREAPP3_0
            s_isMac = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
#endif

#if NETCOREAPP && !NETCOREAPP3_0
            if (Sse2.IsSupported && Ssse3.IsSupported)
            {
                s_sse_encodeShuffleVec = Sse2.SetVector128(
                    10, 11, 9, 10,
                     7,  8, 6,  7,
                     4,  5, 3,  4,
                     1,  2, 0,  1
                );

                s_sse_decodeShuffleVec = Sse2.SetVector128(
                    -1, -1, -1, -1,
                    12, 13, 14,  8,
                     9, 10,  4,  5,
                     6,  0,  1,  2
                );
            }
#endif
#if NETCOREAPP3_0
            if (Ssse3.IsSupported)
            {
                s_sse_encodeShuffleVec = Vector128.Create(
                     1,  0,  2,  1,
                     4,  3,  5,  4,
                     7,  6,  8,  7,
                    10,  9, 11, 10
                );

                s_sse_decodeShuffleVec = Vector128.Create(
                     2,  1,  0,  6,
                     5,  4, 10,  9,
                     8, 14, 13, 12,
                    -1, -1, -1, -1
                );
            }

            if (Avx2.IsSupported)
            {
                s_avx_encodePermuteVec = Vector256.Create(0, 0, 1, 2, 3, 4, 5, 6);

                s_avx_encodeShuffleVec = Vector256.Create(
                     5,  4,  6,  5,
                     8,  7,  9,  8,
                    11, 10, 12, 11,
                    14, 13, 15, 14,
                     1,  0,  2,  1,
                     4,  3,  5,  4,
                     7,  6,  8,  7,
                    10,  9, 11, 10
                );

                s_avx_decodeShuffleVec = Vector256.Create(
                     2,  1,  0,  6,
                     5,  4, 10,  9,
                     8, 14, 13, 12,
                    -1, -1, -1, -1,
                     2,  1,  0,  6,
                     5,  4, 10,  9,
                     8, 14, 13, 12,
                    -1, -1, -1, -1
                );

                s_avx_decodePermuteVec = Vector256.Create(0, 1, 2, 4, 5, 6, -1, -1);
            }
#endif
        }
        //---------------------------------------------------------------------
        protected const int MaxStackallocBytes = 256;
        //---------------------------------------------------------------------
        public override OperationStatus Encode(ReadOnlySpan<byte> data, Span<byte> encoded, out int consumed, out int written, bool isFinalBlock = true) => this.EncodeCore(data, encoded, out consumed, out written, encodedLength: -1, isFinalBlock);
        public override OperationStatus Encode(ReadOnlySpan<byte> data, Span<char> encoded, out int consumed, out int written, bool isFinalBlock = true) => this.EncodeCore(data, encoded, out consumed, out written, encodedLength: -1, isFinalBlock);

        public override OperationStatus Decode(ReadOnlySpan<byte> encoded, Span<byte> data, out int consumed, out int written, bool isFinalBlock = true) => this.DecodeCore(encoded, data, out consumed, out written, decodedLength: -1, isFinalBlock);
        public override OperationStatus Decode(ReadOnlySpan<char> encoded, Span<byte> data, out int consumed, out int written, bool isFinalBlock = true) => this.DecodeCore(encoded, data, out consumed, out written, decodedLength: -1, isFinalBlock);
        //---------------------------------------------------------------------
        // For testing
        internal OperationStatus EncodeCore<T>(
            ReadOnlySpan<byte> data,
            Span<T> encoded,
            out int consumed,
            out int written,
            bool isFinalBlock = true)
            where T : struct
        {
            if (typeof(T) == typeof(byte))
            {
                return this.EncodeCore(data, MemoryMarshal.AsBytes(encoded), out consumed, out written, encodedLength: -1, isFinalBlock);
            }
            else if (typeof(T) == typeof(char))
            {
                return this.EncodeCore(data, MemoryMarshal.Cast<T, char>(encoded), out consumed, out written, encodedLength: -1, isFinalBlock);
            }
            else
            {
                throw new NotSupportedException(); // just in case new types are introduced in the future
            }
        }
        //---------------------------------------------------------------------
        // For testing
        internal OperationStatus DecodeCore<T>(
            ReadOnlySpan<T> encoded,
            Span<byte> data,
            out int consumed,
            out int written,
            bool isFinalBlock = true)
            where T : struct
        {
            if (typeof(T) == typeof(byte))
            {
                return this.DecodeCore(MemoryMarshal.AsBytes(encoded), data, out consumed, out written, decodedLength: -1, isFinalBlock);
            }
            else if (typeof(T) == typeof(char))
            {
                return this.DecodeCore(MemoryMarshal.Cast<T, char>(encoded), data, out consumed, out written, decodedLength: -1, isFinalBlock);
            }
            else
            {
                throw new NotSupportedException(); // just in case new types are introduced in the future
            }
        }
        //---------------------------------------------------------------------
        // PERF: can't be generic for inlining (generic virtual)
        protected abstract OperationStatus EncodeCore(
            ReadOnlySpan<byte> data,
            Span<byte> encoded,
            out int consumed,
            out int written,
            int encodedLength = -1,
            bool isFinalBlock = true);
        //---------------------------------------------------------------------
        // PERF: can't be generic for inlining (generic virtual)
        protected abstract OperationStatus EncodeCore(
            ReadOnlySpan<byte> data,
            Span<char> encoded,
            out int consumed,
            out int written,
            int encodedLength = -1,
            bool isFinalBlock = true);
        //---------------------------------------------------------------------
        // PERF: can't be generic for inlining (generic virtual)
        protected abstract OperationStatus DecodeCore(
            ReadOnlySpan<byte> encoded,
            Span<byte> data,
            out int consumed,
            out int written,
            int decodedLength = -1,
            bool isFinalBlock = true);
        //---------------------------------------------------------------------
        // PERF: can't be generic for inlining (generic virtual)
        protected abstract OperationStatus DecodeCore(
            ReadOnlySpan<char> encoded,
            Span<byte> data,
            out int consumed,
            out int written,
            int decodedLength = -1,
            bool isFinalBlock = true);
        //---------------------------------------------------------------------
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static int GetBase64EncodedLength(int sourceLength)
        {
            if ((uint)sourceLength > MaximumEncodeLength)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.length);

            int numWholeOrPartialInputBlocks = (sourceLength + 2) / 3;
            return numWholeOrPartialInputBlocks * 4;
        }
        //---------------------------------------------------------------------
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static void EncodeThreeBytes<T>(ref byte threeBytes, ref T encoded, ref byte encodingMap)
        {
            uint i = (uint)threeBytes << 16
                | (uint)Unsafe.Add(ref threeBytes, 1) << 8
                | Unsafe.Add(ref threeBytes, 2);

            uint i0 = Unsafe.Add(ref encodingMap, (IntPtr)(i >> 18));
            uint i1 = Unsafe.Add(ref encodingMap, (IntPtr)((i >> 12) & 0x3F));
            uint i2 = Unsafe.Add(ref encodingMap, (IntPtr)((i >> 6) & 0x3F));
            uint i3 = Unsafe.Add(ref encodingMap, (IntPtr)(i & 0x3F));

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
        protected static int DecodeFour<T>(ref T encoded, ref sbyte decodingMap)
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
        protected static void WriteThreeLowOrderBytes(ref byte destination, uint destIndex, int value)
        {
            Unsafe.Add(ref destination, (IntPtr)(destIndex + 0)) = (byte)(value >> 16);
            Unsafe.Add(ref destination, (IntPtr)(destIndex + 1)) = (byte)(value >> 8);
            Unsafe.Add(ref destination, (IntPtr)(destIndex + 2)) = (byte)value;
        }
    }
}
