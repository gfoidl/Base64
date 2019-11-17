using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Linq;
using System.Threading.Tasks;
using gfoidl.Base64.Internal;
using NUnit.Framework;

namespace gfoidl.Base64.Tests.Extensions.ReadOnlySequenceExtensionsTests
{
    [TestFixture(typeof(Base64Encoder))]
    [TestFixture(typeof(Base64UrlEncoder))]
    public class Decode<TEncoder> where TEncoder : Base64, new()
    {
        private readonly TEncoder _encoder = new TEncoder();
        //---------------------------------------------------------------------
        [Test]
        public void BufferWriter_is_null___throws_ArgumentNull()
        {
            var sequence               = new ReadOnlySequence<byte>(new byte[100]);
            IBufferWriter<byte> writer = null;

            Assert.Throws<ArgumentNullException>(() => _encoder.TryDecode(sequence, writer, out long _, out long _));
        }
        //---------------------------------------------------------------------
        [Test]
        public void Empty_sequence___true_and_nothing_read_and_written()
        {
            var sequence               = new ReadOnlySequence<byte>();
            IBufferWriter<byte> writer = new ArrayBufferWriter<byte>();

            bool result = _encoder.TryDecode(sequence, writer, out long consumed, out long written);

            Assert.Multiple(() =>
            {
                Assert.IsTrue(result);
                Assert.AreEqual(0, consumed);
                Assert.AreEqual(0, written);
            });
        }
        //---------------------------------------------------------------------
        [Test]
        public async Task SingleSegment_decode___true()
        {
            byte[] data            = new byte[300];
            byte[] base64          = new byte[400];
            var rnd                = new Random(42);
            rnd.NextBytes(data);
            OperationStatus status = _encoder.Encode(data, base64, out int _, out int _);
            Assume.That(status, Is.EqualTo(OperationStatus.Done));

            var sequence = new ReadOnlySequence<byte>(base64);
            var pipe     = new Pipe();

            bool result             = _encoder.TryDecode(sequence, pipe.Writer, out long consumed, out long written);
            FlushResult flushResult = await pipe.Writer.FlushAsync();
            await pipe.Writer.CompleteAsync();

            Assert.Multiple(() =>
            {
                Assert.IsTrue(result);
                Assert.AreEqual(400, consumed, nameof(consumed));
                Assert.AreEqual(300, written , nameof(written));
            });

            ReadResult readResult = await pipe.Reader.ReadAsync();

            Assert.Multiple(() =>
            {
                Assert.IsTrue(readResult.IsCompleted);
                Assert.IsTrue(readResult.Buffer.IsSingleSegment);
                Assert.AreEqual(written, readResult.Buffer.First.Length);
            });

            CollectionAssert.AreEqual(data, readResult.Buffer.ToArray());
        }
        //---------------------------------------------------------------------
        [Test]
        public async Task SingleSegment_decode_invalid_data___false()
        {
            byte[] data            = new byte[300];
            byte[] base64          = new byte[400];
            var rnd                = new Random(42);
            rnd.NextBytes(data);
            OperationStatus status = _encoder.Encode(data, base64, out int _, out int _);
            Assume.That(status, Is.EqualTo(OperationStatus.Done));

            // Insert invalid data
            base64[^1] = (byte)'~';

            var sequence = new ReadOnlySequence<byte>(base64);
            var pipe     = new Pipe();

            bool result             = _encoder.TryDecode(sequence, pipe.Writer, out long consumed, out long written);
            FlushResult flushResult = await pipe.Writer.FlushAsync();
            await pipe.Writer.CompleteAsync();

            Assert.Multiple(() =>
            {
                Assert.IsFalse(result);
                Assert.AreEqual(400 - 4, consumed, nameof(consumed));
                Assert.AreEqual(300 - 3, written , nameof(written));
            });

            Assert.Multiple(async () =>
            {
                ReadResult readResult = await pipe.Reader.ReadAsync();
                Assert.IsTrue(readResult.IsCompleted);
                Assert.IsTrue(readResult.Buffer.IsSingleSegment);
                Assert.AreEqual(written, readResult.Buffer.First.Length);
            });
        }
        //---------------------------------------------------------------------
        [Test]
        [TestCase(2 * 4096 + 4096 / 2)]     // 2 * pipeOptions.MinimumSegmentSize + pipeOptions.MinimumSegmentSize / 2
        [TestCase(4097)]
        public async Task MultiSegment_decode___true(int dataSize)
        {
            var pipeOptions = PipeOptions.Default;
            var pipe        = new Pipe(pipeOptions);

            var data               = new byte[dataSize];
            var base64             = new byte[_encoder.GetEncodedLength(data.Length)];
            var rnd                = new Random(42);
            rnd.NextBytes(data);
            OperationStatus status = _encoder.Encode(data, base64, out int _, out int _);
            Assume.That(status, Is.EqualTo(OperationStatus.Done));

            pipe.Writer.Write(base64);
            await pipe.Writer.CompleteAsync();

            ReadResult readResult = await pipe.Reader.ReadAsync();
            Assume.That(readResult.Buffer.IsSingleSegment, Is.False, "buffer is not multiple segements");

            var resultPipe          = new Pipe();
            bool result             = _encoder.TryDecode(readResult.Buffer, resultPipe.Writer, out long consumed, out long written);
            FlushResult flushResult = await resultPipe.Writer.FlushAsync();
            await resultPipe.Writer.CompleteAsync();

            int encodedLengthExpected = base64.Length;
            int dataLengthExpected    = _encoder.GetDecodedLength(base64);

            Assert.Multiple(() =>
            {
                Assert.IsTrue(result);
                Assert.AreEqual(encodedLengthExpected, consumed, nameof(consumed));
                Assert.AreEqual(dataLengthExpected   , written , nameof(written));
            });

            readResult = await resultPipe.Reader.ReadAsync();

            Assert.Multiple(() =>
            {
                Assert.IsTrue(readResult.IsCompleted);
                Assert.AreEqual(dataLengthExpected, readResult.Buffer.Length);
            });

            CollectionAssert.AreEqual(data, readResult.Buffer.ToArray());
        }
        //---------------------------------------------------------------------
        [Test]
        [TestCase(2 * 4096 + 4096 / 2)]     // 2 * pipeOptions.MinimumSegmentSize + pipeOptions.MinimumSegmentSize / 2
        [TestCase(4097)]
        public async Task MultiSegment_decode_invalid_data___false(int dataSize)
        {
            var pipeOptions = PipeOptions.Default;
            var pipe        = new Pipe(pipeOptions);

            var data               = new byte[dataSize];
            var base64             = new byte[_encoder.GetEncodedLength(data.Length)];
            var rnd                = new Random(42);
            rnd.NextBytes(data);
            OperationStatus status = _encoder.Encode(data, base64, out int _, out int _);
            Assume.That(status, Is.EqualTo(OperationStatus.Done));

            // Insert invalid data
            base64[^1] = (byte)'~';

            pipe.Writer.Write(base64);
            await pipe.Writer.CompleteAsync();

            ReadResult readResult = await pipe.Reader.ReadAsync();
            Assume.That(readResult.Buffer.IsSingleSegment, Is.False, "buffer is not multiple segements");

            var resultPipe          = new Pipe();
            bool result             = _encoder.TryDecode(readResult.Buffer, resultPipe.Writer, out long consumed, out long written);
            FlushResult flushResult = await resultPipe.Writer.FlushAsync();
            await resultPipe.Writer.CompleteAsync();

            int expectedConsumed = (base64.Length - 1) / 4 * 4;   // up to the next multiple of 4
            int expectedWritten  = expectedConsumed / 4 * 3;

            Assert.Multiple(() =>
            {
                Assert.IsFalse(result);
                Assert.AreEqual(expectedConsumed, consumed, nameof(consumed));
                Assert.AreEqual(expectedWritten , written , nameof(written));
            });

            Assert.Multiple(async () =>
            {
                readResult = await resultPipe.Reader.ReadAsync();
                Assert.IsTrue(readResult.IsCompleted);
            });
        }
        //---------------------------------------------------------------------
        [Test]
        [TestCase(4097)]
        public async Task MultiSegment_decode_invalid_length___false(int base64Size)
        {
            var pipeOptions = PipeOptions.Default;
            var pipe        = new Pipe(pipeOptions);
            byte[] base64   = Enumerable.Repeat((byte)'A', base64Size).ToArray();

            pipe.Writer.Write(base64);
            await pipe.Writer.CompleteAsync();

            ReadResult readResult = await pipe.Reader.ReadAsync();
            Assume.That(readResult.Buffer.IsSingleSegment, Is.False, "buffer is not multiple segements");

            var resultPipe          = new Pipe();
            bool result             = _encoder.TryDecode(readResult.Buffer, resultPipe.Writer, out long consumed, out long written);
            FlushResult flushResult = await resultPipe.Writer.FlushAsync();
            await resultPipe.Writer.CompleteAsync();

            int encodedLengthExpected = base64.Length / 4 * 4;
            //int dataLengthExpected    = _encoder.GetDecodedLength(base64);
            int dataLengthExpected = encodedLengthExpected / 4 * 3;

            Assert.Multiple(() =>
            {
                Assert.IsFalse(result);
                Assert.AreEqual(encodedLengthExpected, consumed, nameof(consumed));
                Assert.AreEqual(dataLengthExpected   , written , nameof(written));
            });

            readResult = await resultPipe.Reader.ReadAsync();

            Assert.Multiple(() =>
            {
                Assert.IsTrue(readResult.IsCompleted);
                Assert.AreEqual(dataLengthExpected, readResult.Buffer.Length);
            });
        }
    }
}
