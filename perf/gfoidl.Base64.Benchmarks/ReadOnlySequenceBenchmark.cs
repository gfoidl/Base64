using System;
using System.Buffers;
using System.IO.Pipelines;
using BenchmarkDotNet.Attributes;

namespace gfoidl.Base64.Benchmarks
{
    public class ReadOnlySequenceBase64Benchmark : ReadOnlySequenceBaseBenchmark
    {
        public ReadOnlySequenceBase64Benchmark() : base(Base64.Default) { }
    }
    //-------------------------------------------------------------------------
    public class ReadOnlySequenceBase64UrlBenchmark : ReadOnlySequenceBaseBenchmark
    {
        public ReadOnlySequenceBase64UrlBenchmark() : base(Base64.Url) { }
    }
    //-------------------------------------------------------------------------
    //[Config(typeof(HardwareIntrinsicsCustomConfig))]
    public abstract class ReadOnlySequenceBaseBenchmark
    {
        private const int ByteArraySize = 10_000_000;
        private readonly ReadOnlySequence<byte>  _dataSingleSegment;
        private readonly ReadOnlySequence<byte>  _dataMultiSegment;
        private readonly ReadOnlySequence<byte>  _guid;
        private readonly ArrayBufferWriter<byte> _arrayBufferWriter = new ArrayBufferWriter<byte>((ByteArraySize + 2) / 3 * 4);
        protected readonly Base64                _encoder;
        //---------------------------------------------------------------------
        protected ReadOnlySequenceBaseBenchmark(Base64 encoder)
        {
            _encoder = encoder ?? throw new ArgumentNullException(nameof(encoder));

            var rnd            = new Random(42);
            var data           = new byte[ByteArraySize];
            rnd.NextBytes(data);
            _dataSingleSegment = new ReadOnlySequence<byte>(data);

            var pipe          = new Pipe();
            pipe.Writer.Write(data);
            pipe.Writer.Complete();
            _dataMultiSegment = pipe.Reader.ReadAsync().GetAwaiter().GetResult().Buffer;

            byte[] guid = Guid.NewGuid().ToByteArray();
            _guid       = new ReadOnlySequence<byte>(guid);
        }
        //---------------------------------------------------------------------
        [IterationCleanup]
        public void IterationCleanup() => _arrayBufferWriter.Clear();
        //---------------------------------------------------------------------
        [Benchmark]
        public void EncodeSingleSegment()
        {
            _encoder.Encode(_dataSingleSegment, _arrayBufferWriter, out long _, out long _);
        }
        //---------------------------------------------------------------------
        [Benchmark]
        public void EncodeMultiSegment()
        {
            _encoder.Encode(_dataMultiSegment, _arrayBufferWriter, out long _, out long _);
        }
        //---------------------------------------------------------------------
        [Benchmark(OperationsPerInvoke = 10_000)]
        public void EncodeGuid()
        {
            for (int i = 0; i < 10_000; ++i)
            {
                _encoder.Encode(_guid, _arrayBufferWriter, out long _, out long _);
            }
        }
    }
}
