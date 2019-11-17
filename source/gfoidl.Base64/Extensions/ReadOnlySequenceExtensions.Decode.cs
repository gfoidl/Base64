//#define HANDLE_EMPTY      // don't handle it, as it just blows up asm  
//-----------------------------------------------------------------------------
using System;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace gfoidl.Base64
{
    public static partial class ReadOnlySequenceExtensions
    {
        /// <summary>
        /// Base64 decodes <paramref name="base64" /> into <paramref name="writer" />.
        /// </summary>
        /// <param name="encoder">Either <see cref="Base64.Default" /> or <see cref="Base64.Url" /></param>
        /// <param name="base64">The base64 encoded data to decode.</param>
        /// <param name="writer">The buffer-writer, where the decoded data is written to.</param>
        /// <param name="consumed">The total number of bytes consumed during the operation.</param>
        /// <param name="written">The total number of bytes written to <paramref name="writer" />.</param>
        /// <returns>
        /// <c>true</c> if the operation is successfull. <c>false</c> if there is any invalid. In that
        /// case as much as possible -- i.e. up to the point of invalid data -- of <paramref name="base64" />
        /// is consumed.
        /// </returns>
        /// <seealso cref="IBase64.Decode(ReadOnlySpan{byte}, Span{byte}, out int, out int, bool)" />.
        public static bool TryDecode(
            this Base64?              encoder,
            in ReadOnlySequence<byte> base64,
            IBufferWriter<byte>?      writer,
            out long                  consumed,
            out long                  written)
        {
#if HANDLE_EMPTY
            if (base64.IsEmpty)
            {
                consumed = 0;
                written  = 0;
                return true;
            }
#endif
            if (encoder is null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.encoder);

            if (writer is null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.writer);

            if (base64.IsSingleSegment)
            {
                return TryDecodeSingleSegment(encoder, base64.FirstSpan, writer, out consumed, out written);
            }

            return TryDecodeMultiSegment(encoder, base64, writer, out consumed, out written);
        }
        //---------------------------------------------------------------------
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryDecodeSingleSegment(
            Base64              encoder,
            ReadOnlySpan<byte>  base64,
            IBufferWriter<byte> writer,
            out long            consumed,
            out long            written,
            bool                isFinalBlock = true)
        {
            int decodedLength = encoder.GetMaxDecodedLength(base64.Length);
            Span<byte> data   = writer.GetSpan(decodedLength);

            OperationStatus status = encoder.Decode(base64, data, out int consumedBytes, out int writtenBytes, isFinalBlock);

            writer.Advance(writtenBytes);

            consumed = consumedBytes;
            written  = writtenBytes;

            return status == OperationStatus.Done;
        }
        //---------------------------------------------------------------------
        private static bool TryDecodeMultiSegment(
            Base64                 encoder,
            ReadOnlySequence<byte> base64,
            IBufferWriter<byte>    writer,
            out long               consumed,
            out long               written)
        {
            long totalConsumed = 0;
            long totalWritten  = 0;
            bool isFinalSegment;
            bool success;

            do
            {
                isFinalSegment               = base64.IsSingleSegment;
                ReadOnlySpan<byte> firstSpan = base64.FirstSpan;
                success                      = TryDecodeSingleSegment(encoder, firstSpan, writer, out long consumedInThisIteration, out long writtenInThisIteration, isFinalSegment);

                totalConsumed += consumedInThisIteration;
                totalWritten  += writtenInThisIteration;

                if (!success) break;

                base64 = base64.Slice(consumedInThisIteration);

                // If there are less than 4 elements remaining in this span, process them separately
                // For System.IO.Pipelines this code-path won't be hit, as the default sizes for
                // MinimumSegmentSize are a (higher) power of 2, so are multiples of 4, hence
                // for base64 it is valid or invalid data.
                // Here it is kept to be on the safe side, if non-stanard ROS should be processed.
                int remainingFromIteration = (int)(firstSpan.Length - consumedInThisIteration);
                Debug.Assert(remainingFromIteration < 4);

                if (remainingFromIteration > 0)
                {
                    Debug.Assert(!isFinalSegment);

                    Span<byte> tmpBuffer = stackalloc byte[4];
                    firstSpan[^remainingFromIteration..].CopyTo(tmpBuffer);

                    int base64Needed              = tmpBuffer.Length - remainingFromIteration;
                    Span<byte> tmpBufferRemaining = tmpBuffer[remainingFromIteration..];
                    base64                        = base64.Slice(remainingFromIteration);
                    firstSpan                     = base64.FirstSpan;

                    if (firstSpan.Length > base64Needed)
                    {
                        firstSpan[0..base64Needed].CopyTo(tmpBufferRemaining);
                        base64 = base64.Slice(base64Needed);
                    }
                    else
                    {
                        firstSpan.CopyTo(tmpBufferRemaining);
                        isFinalSegment = true;
                        tmpBuffer      = tmpBuffer[0..(remainingFromIteration + firstSpan.Length)];
                    }

                    success = TryDecodeSingleSegment(encoder, tmpBuffer, writer, out consumedInThisIteration, out writtenInThisIteration, isFinalSegment);

                    Debug.Assert(consumedInThisIteration == tmpBuffer.Length);
                    Debug.Assert(0 < writtenInThisIteration && writtenInThisIteration <= 3);

                    totalConsumed += consumedInThisIteration;
                    totalWritten  += writtenInThisIteration;
                }
            } while (!isFinalSegment);

            consumed = totalConsumed;
            written  = totalWritten;
            return success;
        }
    }
}
