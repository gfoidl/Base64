using System;
using System.Buffers;
using System.Diagnostics;
using BenchmarkDotNet.Attributes;

namespace gfoidl.Base64.Benchmarks
{
    [Config(typeof(HardwareIntrinsicsCustomConfig))]
    [MemoryDiagnoser]
    public class EncodeUtf8Benchmark
    {
        private byte[]? _data;
        private byte[]? _base64;
        //---------------------------------------------------------------------
        [Params(5, 16, 1_000)]
        public int DataLen { get; set; } = 16;
        //---------------------------------------------------------------------
        [GlobalSetup]
        public void GlobalSetup()
        {
            _data   = new byte[this.DataLen];
            _base64 = new byte[Base64.Default.GetEncodedLength(this.DataLen)];

            var rnd = new Random(42);
            rnd.NextBytes(_data);
        }
        //---------------------------------------------------------------------
        [Benchmark(Baseline = true)]
        public OperationStatus BuffersBase64()
        {
            Debug.Assert(_data   != null);
            Debug.Assert(_base64 != null);

            return System.Buffers.Text.Base64.EncodeToUtf8(_data, _base64, out int _, out int _);
        }
        //---------------------------------------------------------------------
        [Benchmark]
        public OperationStatus gfoidlBase64()
        {
            Debug.Assert(_data   != null);
            Debug.Assert(_base64 != null);

            return Base64.Default.Encode(_data, _base64, out int _, out int _);
        }
    }
}
