using System;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace gfoidl.Base64.Internal
{
    partial class Base64Encoder
    {
        // PERF: can't be in base class due to inlining (generic virtual)
        public override unsafe string Encode(ReadOnlySpan<byte> data)
        {
            // Threshoulds found by testing -- may not be ideal on all targets

            if (data.Length < 26)
                return Convert.ToBase64String(data);

            // Get the encoded length here, to avoid this compution in the above path
            int encodedLength = this.GetEncodedLength(data.Length);

            if (data.Length < 82)
            {
                char* ptr              = stackalloc char[encodedLength];
                ref char encoded       = ref Unsafe.AsRef<char>(ptr);
                ref byte srcBytes      = ref MemoryMarshal.GetReference(data);
                OperationStatus status = this.EncodeImpl(ref srcBytes, data.Length, ref encoded, encodedLength, encodedLength, out int consumed, out int written);

                Debug.Assert(status        == OperationStatus.Done);
                Debug.Assert(data.Length   == consumed);
                Debug.Assert(encodedLength == written);

                return new string(ptr, 0, written);
            }

            fixed (byte* ptr = data)
            {
                return string.Create(encodedLength, (Ptr: (IntPtr)ptr, data.Length), (encoded, state) =>
                {
                    ref byte srcBytes      = ref Unsafe.AsRef<byte>(state.Ptr.ToPointer());
                    ref char dest          = ref MemoryMarshal.GetReference(encoded);
                    OperationStatus status = this.EncodeImpl(ref srcBytes, state.Length, ref dest, encoded.Length, encoded.Length, out int consumed, out int written);

                    Debug.Assert(status         == OperationStatus.Done);
                    Debug.Assert(state.Length   == consumed);
                    Debug.Assert(encoded.Length == written);
                });
            }
        }
    }
}
