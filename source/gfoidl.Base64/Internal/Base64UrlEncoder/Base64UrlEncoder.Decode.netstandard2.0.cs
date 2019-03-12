using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// Scalar based on https://github.com/dotnet/corefx/tree/master/src/System.Memory/src/System/Buffers/Text

namespace gfoidl.Base64.Internal
{
    partial class Base64UrlEncoder
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

            decodedLength = GetDataLen(inputLength, out int base64Len, isFinalBlock);
            int srcLength = base64Len & ~0x3;       // only decode input up to the closest multiple of 4.

            ref byte destBytes = ref MemoryMarshal.GetReference(data);

            // https://github.com/dotnet/coreclr/issues/23194
            // Slicing is necessary to "unlink" the ref and let the JIT keep it in a register
            ref sbyte decodingMap = ref MemoryMarshal.GetReference(s_decodingMap.Slice(1));

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
            int numPaddingChars = base64Len - inputLength;
            ref T lastFourStart = ref Unsafe.Add(ref src, srcLength - 4);

            if (numPaddingChars == 0)
            {
                int result = DecodeFour(ref lastFourStart, ref decodingMap);

                if (result < 0) goto InvalidDataExit;
                if (destIndex > destLength - 3) goto DestinationTooSmallExit;

                WriteThreeLowOrderBytes(ref destBytes, destIndex, result);
                sourceIndex += 4;
                destIndex   += 3;
            }
            else if (numPaddingChars == 1)
            {
                int result = DecodeThree(ref lastFourStart, ref decodingMap);

                if (result < 0)
                    goto InvalidDataExit;

                if (destIndex > destLength - 2)
                    goto DestinationTooSmallExit;

                WriteTwoLowOrderBytes(ref destBytes, destIndex, result);
                sourceIndex += 3;
                destIndex   += 2;
            }
            else
            {
                int result = DecodeTwo(ref lastFourStart, ref decodingMap);

                if (result < 0)
                    goto InvalidDataExit;

                if (destIndex > destLength - 1)
                    goto DestinationTooSmallExit;

                WriteOneLowOrderByte(ref destBytes, destIndex, result);
                sourceIndex += 2;
                destIndex   += 1;
            }

            if (srcLength != base64Len)
                goto InvalidDataExit;

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
    }
}
