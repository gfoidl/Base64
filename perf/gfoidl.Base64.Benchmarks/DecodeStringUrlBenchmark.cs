using System;
using System.Diagnostics;
using System.Text;
using BenchmarkDotNet.Attributes;

namespace gfoidl.Base64.Benchmarks
{
#if NETCOREAPP
    [Config(typeof(HardwareIntrinsicsCustomConfig))]
#endif
    [MemoryDiagnoser]
    public class DecodeStringUrlBenchmark
    {
        private char[]? _base64Url;
        //---------------------------------------------------------------------
        [Params(5, 16, 1_000)]
        public int DataLen { get; set; } = 16;
        //---------------------------------------------------------------------
        [GlobalSetup]
        public void GlobalSetup()
        {
            var data   = new byte[this.DataLen];
            _base64Url = new char[Base64.Url.GetEncodedLength(this.DataLen)];

            var rnd = new Random(42);
            rnd.NextBytes(data);

            Base64.Url.Encode(data, _base64Url, out int _, out int _);
        }
        //---------------------------------------------------------------------
        [Benchmark(Baseline = true)]
        public byte[] ConvertFromBase64StringWithStringReplace()
        {
            Debug.Assert(_base64Url != null);

            int urlEncodedLen  = _base64Url.Length;
            int noPaddingChars = (4 - urlEncodedLen) & 3;
            var sb             = new StringBuilder(urlEncodedLen + noPaddingChars);

            sb.Append(_base64Url);
            sb.Append('=', noPaddingChars);
            sb.Replace('-', '+');
            sb.Replace('_', '/');
            string base64 = sb.ToString();

            return Convert.FromBase64String(base64);
        }
        //---------------------------------------------------------------------
        [Benchmark]
        public byte[] gfoidlBase64Url()
        {
            return Base64.Url.Decode(_base64Url);
        }
    }
}
