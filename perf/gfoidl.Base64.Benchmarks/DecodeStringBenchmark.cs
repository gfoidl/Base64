using System;
using BenchmarkDotNet.Attributes;

namespace gfoidl.Base64.Benchmarks
{
    [Config(typeof(HardwareIntrinsicsCustomConfig))]
    public class DecodeStringBenchmark
    {
        private char[] _base64;
        //---------------------------------------------------------------------
        [Params(5, 16, 1_000)]
        public int DataLen { get; set; } = 16;
        //---------------------------------------------------------------------
        [GlobalSetup]
        public void GlobalSetup()
        {
            var data = new byte[this.DataLen];
            _base64  = new char[Base64.Default.GetEncodedLength(this.DataLen)];

            var rnd = new Random();
            rnd.NextBytes(data);

            Base64.Default.Encode(data, _base64, out int _, out int _);
        }
        //---------------------------------------------------------------------
        [Benchmark(Baseline = true)]
        public byte[] ConvertFromBase64CharArray()
        {
            return Convert.FromBase64CharArray(_base64, 0, _base64.Length);
        }
        //---------------------------------------------------------------------
        [Benchmark]
        public byte[] gfoidlBase64()
        {
            return Base64.Default.Decode(_base64);
        }
    }
}
