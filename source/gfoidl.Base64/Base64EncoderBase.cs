using System;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace gfoidl.Base64
{
    public abstract class Base64EncoderBase : IBase64Encoder
    {
#if NETSTANDARD2_0
        private const int MaxStackallocBytes = 256;
#endif
        public const int MaximumEncodeLength = int.MaxValue / 4 * 3; // 1610612733
        //---------------------------------------------------------------------
        public OperationStatus Encode(ReadOnlySpan<byte> data, Span<byte> encoded, out int consumed, out int written, bool isFinalBlock = true) => this.EncodeCore(data, encoded, out consumed, out written, isFinalBlock);
        public OperationStatus Encode(ReadOnlySpan<byte> data, Span<char> encoded, out int consumed, out int written, bool isFinalBlock = true) => this.EncodeCore(data, encoded, out consumed, out written, isFinalBlock);

        public OperationStatus Decode(ReadOnlySpan<byte> encoded, Span<byte> data, out int consumed, out int written, bool isFinalBlock = true) => this.DecodeCore(encoded, data, out consumed, out written, isFinalBlock);
        public OperationStatus Decode(ReadOnlySpan<char> encoded, Span<byte> data, out int consumed, out int written, bool isFinalBlock = true) => this.DecodeCore(encoded, data, out consumed, out written, isFinalBlock);
        //---------------------------------------------------------------------
        public abstract int GetEncodedLength(int sourceLength);
        public abstract int GetDecodedLength(ReadOnlySpan<byte> encoded);
        public abstract int GetDecodedLength(ReadOnlySpan<char> encoded);
        //---------------------------------------------------------------------
        public unsafe string Encode(ReadOnlySpan<byte> data)
        {
            if (data.IsEmpty) return string.Empty;

            int encodedLength = this.GetEncodedLength(data.Length);
#if NETCOREAPP
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
        internal OperationStatus EncodeCore<T>(ReadOnlySpan<byte> data, Span<T> encoded, out int consumed, out int written, bool isFinalBlock = true)
        {
            if (data.IsEmpty)
            {
                consumed = 0;
                written  = 0;
                return OperationStatus.Done;
            }

            ref byte srcBytes = ref MemoryMarshal.GetReference(data);
            int srcLength     = data.Length;

            return this.EncodeCore(ref srcBytes, srcLength, encoded, out consumed, out written, isFinalBlock);
        }
        //---------------------------------------------------------------------
        internal OperationStatus DecodeCore<T>(ReadOnlySpan<T> encoded, Span<byte> data, out int consumed, out int written, bool isFinalBlock = true)
        {
            if (encoded.IsEmpty)
            {
                consumed = 0;
                written  = 0;
                return OperationStatus.Done;
            }

            ref T src     = ref MemoryMarshal.GetReference(encoded);
            int srcLength = encoded.Length;

            return this.DecodeCore(ref src, srcLength, data, out consumed, out written, isFinalBlock);
        }
        //---------------------------------------------------------------------
        protected abstract OperationStatus EncodeCore<T>(
            ref byte srcBytes,
            int srcLength,
            Span<T> encoded,
            out int consumed,
            out int written,
            bool isFinalBlock = true);
        //---------------------------------------------------------------------
        protected abstract OperationStatus DecodeCore<T>(
            ref T src,
            int inputLength,
            Span<byte> data,
            out int consumed,
            out int written,
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
