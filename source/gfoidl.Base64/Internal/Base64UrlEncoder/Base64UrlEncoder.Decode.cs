using System;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// Scalar based on https://github.com/dotnet/corefx/tree/ec34e99b876ea1119f37986ead894f4eded1a19a/src/System.Memory/src/System/Buffers/Text
// SSE2 based on https://github.com/aklomp/base64/tree/a27c565d1b6c676beaf297fe503c4518185666f7/lib/arch/ssse3
// AVX2 based on https://github.com/aklomp/base64/tree/a27c565d1b6c676beaf297fe503c4518185666f7/lib/arch/avx2
// Lookup and validation for SSE2 and AVX2 based on http://0x80.pl/notesen/2016-01-17-sse-base64-decoding.html#vector-lookup-pshufb

namespace gfoidl.Base64.Internal
{
    public partial class Base64UrlEncoder
    {
        public override int GetMaxDecodedLength(int encodedLength)
        {
            try
            {
                return this.GetDecodedLength(encodedLength);
            }
            catch (FormatException)
            {
                // In the case of malformed input, i.e. wrong number of padding,
                // just assume 2 padding chars and compute the data length.
                // It's "max" anyway.
                return (encodedLength + 2) / 4 * 3;
            }
        }
        //---------------------------------------------------------------------
        public override int GetDecodedLength(ReadOnlySpan<byte> encoded) => this.GetDecodedLength(encoded.Length);
        public override int GetDecodedLength(ReadOnlySpan<char> encoded) => this.GetDecodedLength(encoded.Length);
        //---------------------------------------------------------------------
        internal int GetDecodedLength(int encodedLength)
        {
            if ((uint)encodedLength >= int.MaxValue)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.encodedLength);

            return GetDataLen(encodedLength, out int _);
        }
        //---------------------------------------------------------------------
        // PERF: can't be in base class due to inlining (generic virtual)
        public override byte[] Decode(ReadOnlySpan<char> encoded)
        {
            if (encoded.IsEmpty) return Array.Empty<byte>();

            int dataLength         = this.GetDecodedLength(encoded);
            byte[] data            = new byte[dataLength];
            OperationStatus status = this.DecodeImpl(encoded, data, out int consumed, out int written);

            if (status == OperationStatus.InvalidData)
                ThrowHelper.ThrowForOperationNotDone(status);

            Debug.Assert(status         == OperationStatus.Done);
            Debug.Assert(encoded.Length == consumed);
            Debug.Assert(data.Length    == written);

            return data;
        }
        //---------------------------------------------------------------------
        // PERF: can't be in base class due to inlining (generic virtual)
        protected override OperationStatus DecodeCore(
            ReadOnlySpan<byte> encoded,
            Span<byte> data,
            out int consumed,
            out int written,
            int decodedLength = -1,
            bool isFinalBlock = true)
            => this.DecodeImpl(encoded, data, out consumed, out written, decodedLength, isFinalBlock);
        //---------------------------------------------------------------------
        // PERF: can't be in base class due to inlining (generic virtual)
        protected override OperationStatus DecodeCore(
            ReadOnlySpan<char> encoded,
            Span<byte> data,
            out int consumed,
            out int written,
            int decodedLength = -1,
            bool isFinalBlock = true)
            => this.DecodeImpl(encoded, data, out consumed, out written, decodedLength, isFinalBlock);
        //---------------------------------------------------------------------
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private OperationStatus DecodeImpl<T>(
            ReadOnlySpan<T> encoded,
            Span<byte> data,
            out int consumed,
            out int written,
            int decodedLength = -1,
            bool isFinalBlock = true)
            where T : unmanaged
        {
            if (encoded.IsEmpty)
            {
                consumed = 0;
                written  = 0;
                return OperationStatus.Done;
            }

            ref T src     = ref MemoryMarshal.GetReference(encoded);
            int srcLength = encoded.Length;

            // Not needed in base64Url
            //if (decodedLength == -1)
            //  decodedLength = this.GetDecodedLength(srcLength);

            try
            {
                return this.DecodeImpl(ref src, srcLength, data, decodedLength, out consumed, out written, isFinalBlock);
            }
            catch (FormatException)
            {
                consumed = 0;
                written  = 0;
                return OperationStatus.InvalidData;
            }
        }
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

                if (base64Len < 0)    // overflow
                {
                    ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.encodedLength);
                }

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
        // internal because tests use this map too
        internal static ReadOnlySpan<sbyte> DecodingMap
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                ReadOnlySpan<sbyte> map = new sbyte[256 + 1] {
                    0,      // https://github.com/dotnet/coreclr/issues/23194
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

                // Slicing is necessary to "unlink" the ref and let the JIT keep it in a register
                return map.Slice(1);
            }
        }
    }
}
