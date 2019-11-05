using System;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// Scalar based on https://github.com/dotnet/corefx/tree/ec34e99b876ea1119f37986ead894f4eded1a19a/src/System.Memory/src/System/Buffers/Text

namespace gfoidl.Base64.Internal
{
    public partial class Base64Encoder
    {
        // PERF: can't be in base class due to inlining (generic virtual)
        public override unsafe string Encode(ReadOnlySpan<byte> data)
        {
            if (data.IsEmpty)
                return string.Empty;

            int encodedLength           = this.GetEncodedLength(data.Length);
            char[]? arrayToReturnToPool = null;

            Span<char> encoded = encodedLength <= MaxStackallocBytes / sizeof(char)
                ? stackalloc char[encodedLength]
                : arrayToReturnToPool = ArrayPool<char>.Shared.Rent(encodedLength);

            try
            {
                OperationStatus status = this.EncodeImpl(data, encoded, out int consumed, out int written, encodedLength);
                Debug.Assert(status         == OperationStatus.Done);
                Debug.Assert(data.Length    == consumed);
                Debug.Assert(encoded.Length >= written);

                fixed (char* ptr = encoded)
                    return new string(ptr, 0, written);
            }
            finally
            {
                if (arrayToReturnToPool != null)
                    ArrayPool<char>.Shared.Return(arrayToReturnToPool);
            }
        }
        //---------------------------------------------------------------------
        private OperationStatus EncodeImpl<T>(
            ref byte srcBytes,
            int srcLength,
            ref T dest,
            int destLength,
            int encodedLength,
            out int consumed,
            out int written,
            bool isFinalBlock = true)
        {
            uint sourceIndex = 0;
            uint destIndex   = 0;

            int maxSrcLength = -2;

            if (srcLength <= MaximumEncodeLength && destLength >= encodedLength)
                maxSrcLength += srcLength;
            else
                maxSrcLength += (destLength >> 2) * 3;

            ref byte encodingMap = ref MemoryMarshal.GetReference(EncodingMap);

            // In order to elide the movsxd in the loop
            if (sourceIndex < maxSrcLength)
            {
                do
                {
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
    }
}
