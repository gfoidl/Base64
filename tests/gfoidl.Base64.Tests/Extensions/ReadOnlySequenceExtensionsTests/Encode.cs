using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading.Tasks;
using gfoidl.Base64.Internal;
using NUnit.Framework;

namespace gfoidl.Base64.Tests.Extensions.ReadOnlySequenceExtensionsTests
{
    [TestFixture(typeof(Base64Encoder))]
    [TestFixture(typeof(Base64UrlEncoder))]
    public class Encode<TEncoder> where TEncoder : Base64, new()
    {
        private readonly TEncoder _encoder = new TEncoder();
        //---------------------------------------------------------------------
        [Test]
        public void BufferWriter_is_null___throws_ArgumentNull()
        {
            var sequence               = new ReadOnlySequence<byte>(new byte[100]);
            IBufferWriter<byte> writer = null;

            Assert.Throws<ArgumentNullException>(() => _encoder.Encode(sequence, writer, out long _, out long _));
        }
        //---------------------------------------------------------------------
        [Test]
        public void Empty_sequence___OK_and_nothing_read_and_written()
        {
            var sequence               = new ReadOnlySequence<byte>();
            IBufferWriter<byte> writer = new ArrayBufferWriter<byte>();

            _encoder.Encode(sequence, writer, out long consumed, out long written);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(0, consumed, nameof(consumed));
                Assert.AreEqual(0, written , nameof(written));
            });
        }
        //---------------------------------------------------------------------
        [Test]
        public async Task SingleSegment_encode___OK()
        {
            byte[] data = new byte[300];
            var rnd     = new Random(42);
            rnd.NextBytes(data);

            var sequence = new ReadOnlySequence<byte>(data);
            var pipe     = new Pipe();

            _encoder.Encode(sequence, pipe.Writer, out long consumed, out long written);
            FlushResult flushResult = await pipe.Writer.FlushAsync();
            await pipe.Writer.CompleteAsync();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(300, consumed, nameof(consumed));
                Assert.AreEqual(400, written , nameof(written));
            });

            ReadResult readResult = await pipe.Reader.ReadAsync();

            Assert.Multiple(() =>
            {
                Assert.IsTrue(readResult.IsCompleted);
                Assert.IsTrue(readResult.Buffer.IsSingleSegment);
                Assert.AreEqual(written, readResult.Buffer.First.Length);
            });

            byte[] base64Expected = new byte[400];
            _encoder.Encode(data, base64Expected, out int _, out int _);
            CollectionAssert.AreEqual(base64Expected, readResult.Buffer.ToArray());
        }
        //---------------------------------------------------------------------
        [Test]
        [TestCase(2 * 4096 + 4096 / 2)]     // 2 * pipeOptions.MinimumSegmentSize + pipeOptions.MinimumSegmentSize / 2
        [TestCase(4097)]
        public async Task MultiSegment_encode___OK(int dataSize)
        {
            var pipeOptions = PipeOptions.Default;
            var pipe        = new Pipe(pipeOptions);

            var rnd  = new Random(42);
            var data = new byte[dataSize];
            rnd.NextBytes(data);

            pipe.Writer.Write(data);
            await pipe.Writer.CompleteAsync();

            ReadResult readResult = await pipe.Reader.ReadAsync();
            Assume.That(readResult.Buffer.IsSingleSegment, Is.False, "buffer is not multiple segements");

            var resultPipe          = new Pipe();
            _encoder.Encode(readResult.Buffer, resultPipe.Writer, out long consumed, out long written);
            FlushResult flushResult = await resultPipe.Writer.FlushAsync();
            await resultPipe.Writer.CompleteAsync();

            int consumedExpected = data.Length;
            int writtenExpected  = _encoder.GetEncodedLength(consumedExpected);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(consumedExpected, consumed, nameof(consumed));
                Assert.AreEqual(writtenExpected , written , nameof(written));
            });

            readResult = await resultPipe.Reader.ReadAsync();

            Assert.Multiple(() =>
            {
                Assert.IsTrue(readResult.IsCompleted);
                Assert.AreEqual(written, readResult.Buffer.Length);
            });

            byte[] base64Expected = new byte[writtenExpected];
            _encoder.Encode(data, base64Expected, out int _, out int _);
            CollectionAssert.AreEqual(base64Expected, readResult.Buffer.ToArray());
        }
    }
}
