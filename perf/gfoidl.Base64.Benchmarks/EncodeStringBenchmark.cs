﻿using System;
using System.Diagnostics;
using BenchmarkDotNet.Attributes;

namespace gfoidl.Base64.Benchmarks
{
#if NETCOREAPP
    [Config(typeof(HardwareIntrinsicsCustomConfig))]
#endif
    [MemoryDiagnoser]
    public class EncodeStringBenchmark
    {
        private byte[]? _data;
        //---------------------------------------------------------------------
        [Params(5, 16, 1_000)]
        public int DataLen { get; set; } = 16;
        //---------------------------------------------------------------------
        [GlobalSetup]
        public void GlobalSetup()
        {
            _data = new byte[this.DataLen];

            var rnd = new Random(42);
            rnd.NextBytes(_data);
        }
        //---------------------------------------------------------------------
        [Benchmark(Baseline = true)]
        public string ConvertToBase64()
        {
            Debug.Assert(_data != null);

            return Convert.ToBase64String(_data);
        }
        //---------------------------------------------------------------------
        [Benchmark]
        public string gfoidlBase64()
        {
            Debug.Assert(_data != null);

            return Base64.Default.Encode(_data);
        }
    }
}
