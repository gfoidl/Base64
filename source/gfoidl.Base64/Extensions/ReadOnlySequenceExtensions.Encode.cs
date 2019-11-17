//#define HANDLE_EMPTY      // don't handle it, as it just blows up asm
//-----------------------------------------------------------------------------
using System;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace gfoidl.Base64
{
    /// <summary>
    /// Provides methods for encoding / decoding to <see cref="ReadOnlySequence{Byte}" />
    /// and <see cref="IBufferWriter{Byte}" />.
    /// </summary>
    public static partial class ReadOnlySequenceExtensions
    {
        /// <summary>
        /// Base64 encoded <paramref name="data" /> into <paramref name="writer" />.
        /// </summary>
        /// <param name="encoder">Either <see cref="Base64.Default" /> or <see cref="Base64.Url" /></param>
        /// <param name="data">The data to be base64 encoded.</param>
        /// <param name="writer">The buffer-writer, where the encoded data is written to.</param>
        /// <param name="consumed">The total number of bytes consumed during the operation.</param>
        /// <param name="written">The total number of bytes written to <paramref name="writer" />.</param>
        /// <seealso cref="IBase64.Encode(ReadOnlySpan{byte}, Span{byte}, out int, out int, bool)" />.
        public static void Encode(
            this Base64?              encoder,
            in ReadOnlySequence<byte> data,
            IBufferWriter<byte>?      writer,
            out long                  consumed,
            out long                  written)
        {
#if HANDLE_EMPTY
            if (data.IsEmpty)
            {
                consumed = 0;
                written  = 0;
                return;
            }
#endif
            if (encoder is null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.encoder);

            if (writer is null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.writer);

            if (data.IsSingleSegment)
            {
                EncodeSingleSegment(encoder, data.FirstSpan, writer, out consumed, out written);
            }
            else
            {
                EncodeMultiSegment(encoder, data, writer, out consumed, out written);
            }
        }
        //---------------------------------------------------------------------
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void EncodeSingleSegment(
            Base64              encoder,
            ReadOnlySpan<byte>  data,
            IBufferWriter<byte> writer,
            out long            consumed,
            out long            written,
            bool                isFinalBlock = true)
        {
            int encodedLength  = encoder.GetEncodedLength(data.Length);
            Span<byte> encoded = writer.GetSpan(encodedLength);

            OperationStatus status = encoder.Encode(data, encoded, out int consumedBytes, out int writtenBytes, isFinalBlock);
            writer.Advance(writtenBytes);

            Debug.Assert(status == OperationStatus.Done || status == OperationStatus.NeedMoreData);

            consumed = consumedBytes;
            written  = writtenBytes;
        }
        //---------------------------------------------------------------------
        private static void EncodeMultiSegment(
            Base64                 encoder,
            ReadOnlySequence<byte> data,
            IBufferWriter<byte>    writer,
            out long               consumed,
            out long               written)
        {
            long totalConsumed = 0;
            long totalWritten  = 0;
            bool isFinalSegment;

            do
            {
                isFinalSegment               = data.IsSingleSegment;
                ReadOnlySpan<byte> firstSpan = data.FirstSpan;
                EncodeSingleSegment(encoder, firstSpan, writer, out long consumedInThisIteration, out long writtenInThisIteration, isFinalSegment);

                totalConsumed += consumedInThisIteration;
                totalWritten  += writtenInThisIteration;

                data = data.Slice(consumedInThisIteration);

                // If there are less than 3 elements remaining in this span, process them separately
                int remainingFromIteration = (int)(firstSpan.Length - consumedInThisIteration);
                Debug.Assert(remainingFromIteration < 3);

                if (remainingFromIteration > 0)
                {
                    Debug.Assert(!isFinalSegment);

                    Span<byte> tmpBuffer = stackalloc byte[3];
                    firstSpan[^remainingFromIteration..].CopyTo(tmpBuffer);

                    int dataNeeded                = tmpBuffer.Length - remainingFromIteration;
                    Span<byte> tmpBufferRemaining = tmpBuffer[remainingFromIteration..];
                    data                          = data.Slice(remainingFromIteration);
                    firstSpan                     = data.FirstSpan;

                    if (firstSpan.Length > dataNeeded)
                    {
                        firstSpan[0..dataNeeded].CopyTo(tmpBufferRemaining);
                        data = data.Slice(dataNeeded);
                    }
                    else
                    {
                        firstSpan.CopyTo(tmpBufferRemaining);
                        isFinalSegment = true;
                        tmpBuffer      = tmpBuffer[0..(remainingFromIteration + firstSpan.Length)];
                    }

                    EncodeSingleSegment(encoder, tmpBuffer, writer, out consumedInThisIteration, out writtenInThisIteration, isFinalSegment);

                    Debug.Assert(consumedInThisIteration == tmpBuffer.Length);
                    Debug.Assert(0 < writtenInThisIteration && writtenInThisIteration <= 4);

                    totalConsumed += consumedInThisIteration;
                    totalWritten  += writtenInThisIteration;
                }
            } while (!isFinalSegment);

            consumed = totalConsumed;
            written  = totalWritten;
        }
    }
}
