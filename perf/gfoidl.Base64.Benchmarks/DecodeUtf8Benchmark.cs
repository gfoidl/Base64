using System;
using System.Buffers;
using BenchmarkDotNet.Attributes;
using gfoidl.Base64.Internal;

namespace gfoidl.Base64.Benchmarks
{
    [ShortRunJob]
    public class DecodeUtf8Benchmark
    {
        private static readonly Base64Encoder  s_encoder  = new Base64Encoder();
        private static readonly Base64Encoder1 s_encoder1 = new Base64Encoder1();
        private static readonly Base64Encoder2 s_encoder2 = new Base64Encoder2();
        //---------------------------------------------------------------------
        private byte[] _base64;
        private byte[] _decoded;
        //---------------------------------------------------------------------
        //[Params(5, 16, 1_000)]
        public int DataLen { get; set; } = 1_000;
        //---------------------------------------------------------------------
        [GlobalSetup]
        public void GlobalSetup()
        {
            var data = new byte[this.DataLen];
            _base64  = new byte[Base64.Default.GetEncodedLength(this.DataLen)];

            var rnd = new Random();
            rnd.NextBytes(data);

            Base64.Default.Encode(data, _base64, out int _, out int _);

            _decoded = new byte[Base64.Default.GetDecodedLength(_base64)];
        }
        //---------------------------------------------------------------------
        [Benchmark(Baseline = true)]
        public OperationStatus Base()
        {
            return s_encoder2.Decode(_base64, _decoded, out int _, out int _);
        }
        //---------------------------------------------------------------------
        [Benchmark]
        public OperationStatus Dev()
        {
            return s_encoder.Decode(_base64, _decoded, out int _, out int _);
        }
        //---------------------------------------------------------------------
        [Benchmark]
        public OperationStatus PureSSSE3()
        {
            return s_encoder1.Decode(_base64, _decoded, out int _, out int _);
        }
    }
}
