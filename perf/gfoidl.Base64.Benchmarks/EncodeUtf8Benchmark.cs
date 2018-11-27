using System;
using System.Buffers;
using BenchmarkDotNet.Attributes;
using gfoidl.Base64.Internal;

namespace gfoidl.Base64.Benchmarks
{
    public class EncodeUtf8Benchmark
    {
        private static readonly Base64Encoder s_encoder = new Base64Encoder();
        private byte[] _data;
        private byte[] _base64;
        //---------------------------------------------------------------------
        [Params(5, 16, 1_000)]
        public int DataLen { get; set; } = 16;
        //---------------------------------------------------------------------
        [GlobalSetup]
        public void GlobalSetup()
        {
            _data   = new byte[this.DataLen];
            _base64 = new byte[Base64.Default.GetEncodedLength(this.DataLen)];

            var rnd = new Random();
            rnd.NextBytes(_data);
        }
        //---------------------------------------------------------------------
        [Benchmark(Baseline = true)]
        public OperationStatus BuffersBase64()
        {
            return System.Buffers.Text.Base64.EncodeToUtf8(_data, _base64, out int _, out int _);
        }
        //---------------------------------------------------------------------
        [Benchmark]
        public OperationStatus gfoidlBase64()
        {
            return Base64.Default.Encode(_data, _base64, out int _, out int _);
        }
        //---------------------------------------------------------------------
        [Benchmark]
        public OperationStatus gfoidlBase64Static()
        {
            return s_encoder.Encode(_data, _base64, out int _, out int _);
        }
    }
}
