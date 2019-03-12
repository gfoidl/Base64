using System;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// Scalar based on https://github.com/dotnet/corefx/tree/master/src/System.Memory/src/System/Buffers/Text
// SSE2 based on https://github.com/aklomp/base64/tree/master/lib/arch/ssse3
// AVX2 based on https://github.com/aklomp/base64/tree/master/lib/arch/avx2

namespace gfoidl.Base64.Internal
{
    partial class Base64Encoder
    {
        public override int GetMaxDecodedLength(int encodedLength)
        {
            if (encodedLength < 0)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.encodedLength, ExceptionRessource.EncodedLengthOutOfRange);

            return this.GetDecodedLength(encodedLength);
        }
        //---------------------------------------------------------------------
        public override int GetDecodedLength(ReadOnlySpan<byte> encoded) => this.GetDecodedLengthImpl(encoded);
        public override int GetDecodedLength(ReadOnlySpan<char> encoded) => this.GetDecodedLengthImpl(encoded);
        //---------------------------------------------------------------------
        internal int GetDecodedLengthImpl<T>(ReadOnlySpan<T> encoded)
        {
            if (encoded.IsEmpty) return 0;

            if (encoded.Length < 4)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.encodedLength, ExceptionRessource.EncodedLengthOutOfRange);

            int maxLen = this.GetDecodedLength(encoded.Length);

            Debug.Assert(maxLen >= 3, "maxLen >= 3");
            Debug.Assert(encoded.Length >= 4, "encoded.Length >= 4");

            ref T enc = ref MemoryMarshal.GetReference(encoded);
            ref T end = ref Unsafe.Add(ref enc, (IntPtr)((uint)encoded.Length - 1));

            int padding = 0;

            if (typeof(T) == typeof(byte))
            {
                ref byte e = ref Unsafe.As<T, byte>(ref end);

                if (e == EncodingPad) padding++;
                if (Unsafe.Subtract(ref e, 1) == EncodingPad) padding++;
            }
            else if (typeof(T) == typeof(char))
            {
                ref char e = ref Unsafe.As<T, char>(ref end);
                const char encodingPad = (char)EncodingPad;

                if (e == encodingPad) padding++;
                if (Unsafe.Subtract(ref e, 1) == encodingPad) padding++;
            }
            else
            {
                throw new NotSupportedException(); // just in case new types are introduced in the future
            }

            return maxLen - padding;
        }
        //---------------------------------------------------------------------
        internal int GetDecodedLength(int encodedLength) => (encodedLength >> 2) * 3;
        //---------------------------------------------------------------------
        // PERF: can't be in base class due to inlining (generic virtual)
        public override byte[] Decode(ReadOnlySpan<char> encoded)
        {
            if (encoded.IsEmpty) return Array.Empty<byte>();

            int dataLength         = this.GetDecodedLength(encoded);
            byte[] data            = new byte[dataLength];
            OperationStatus status = this.DecodeImpl(encoded, data, out int consumed, out int written, decodedLength: dataLength);

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

            if (decodedLength == -1)
                decodedLength = this.GetDecodedLength(srcLength);

            return this.DecodeImpl(ref src, srcLength, data, decodedLength, out consumed, out written, isFinalBlock);
        }
        //---------------------------------------------------------------------
        // internal because tests use this map too
        internal static ReadOnlySpan<sbyte> s_decodingMap => new sbyte[] {
            0,      // https://github.com/dotnet/coreclr/issues/23194
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
