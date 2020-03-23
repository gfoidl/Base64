using System;
using System.Buffers;
using System.Diagnostics;
using BenchmarkDotNet.Attributes;

namespace gfoidl.Base64.Benchmarks
{
#if NETCOREAPP
    [Config(typeof(HardwareIntrinsicsCustomConfig))]
#endif
    [MemoryDiagnoser]
    public class DecodeUtf8Benchmark
    {
        private byte[]? _base64;
        private byte[]? _decoded;
        //---------------------------------------------------------------------
        [Params(5, 16, 1_000)]
        public int DataLen { get; set; } = 16;
        //---------------------------------------------------------------------
        [GlobalSetup]
        public void GlobalSetup()
        {
            var data = new byte[this.DataLen];
            _base64  = new byte[Base64.Default.GetEncodedLength(this.DataLen)];

            var rnd = new Random(42);
            rnd.NextBytes(data);

            Base64.Default.Encode(data, _base64, out int _, out int _);

            _decoded = new byte[Base64.Default.GetDecodedLength(_base64)];
        }
        //---------------------------------------------------------------------
        [Benchmark(Baseline = true)]
        public OperationStatus BuffersBase64()
        {
            Debug.Assert(_base64  != null);
            Debug.Assert(_decoded != null);

            return System.Buffers.Text.Base64.DecodeFromUtf8(_base64, _decoded, out int _, out int _);
        }
        //---------------------------------------------------------------------
        [Benchmark]
        public OperationStatus gfoidlBase64()
        {
            return Base64.Default.Decode(_base64, _decoded, out int _, out int _);
        }
    }
}
