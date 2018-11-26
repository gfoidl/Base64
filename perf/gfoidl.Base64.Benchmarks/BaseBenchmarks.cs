using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;

namespace gfoidl.Base64.Benchmarks
{
    [Config(typeof(HardwarConfigCustomConfig))]
    public abstract class BaseBenchmarks
    {
        private const int ByteArraySize = 500;
        private readonly byte[]           _data;
        private readonly string           _dataEncoded;
        private readonly byte[]           _guid;
        private readonly string           _guidEncoded;
        protected readonly IBase64Encoder _encoder;
        //---------------------------------------------------------------------
        protected BaseBenchmarks(IBase64Encoder encoder)
        {
            _encoder = encoder ?? throw new ArgumentNullException(nameof(encoder));

            var random   = new Random();
            _data        = new byte[ByteArraySize];
            random.NextBytes(_data);
            _dataEncoded = _encoder.Encode(_data);

            _guid        = Guid.NewGuid().ToByteArray();
            _guidEncoded = _encoder.Encode(_guid);
        }
        //---------------------------------------------------------------------
        [Benchmark]
        public string Base64UrlEncode_Data() => _encoder.Encode(_data);
        //---------------------------------------------------------------------
        [Benchmark]
        public string Base64UrlEncode_Guid() => _encoder.Encode(_guid);
        //---------------------------------------------------------------------
        [Benchmark]
        public byte[] Base64UrlDecode_Data() => _encoder.Decode(_dataEncoded);
        //---------------------------------------------------------------------
        [Benchmark]
        public byte[] Base64UrlDecode_Guid() => _encoder.Decode(_guidEncoded);
        //---------------------------------------------------------------------
        [Benchmark]
        public int GetArraySizeRequiredToEncode() => _encoder.GetEncodedLength(ByteArraySize);
        //---------------------------------------------------------------------
        [Benchmark]
        public int GetArraySizeRequiredToDecode() => _encoder.GetDecodedLength(_dataEncoded);
        //---------------------------------------------------------------------
        private class HardwarConfigCustomConfig : ManualConfig
        {
            private const string EnableAVX2  = "COMPlus_EnableAVX2";
            private const string EnableSSSE3 = "COMPlus_EnableSSSE3";

            public HardwarConfigCustomConfig()
            {
                this.Add(Job.Core.WithId("AVX2"));

                this.Add(Job.Core
                    .With(new[] { new EnvironmentVariable(EnableAVX2, "0") })
                    .WithId("SSSE3"));

                this.Add(Job.Core
                    .With(new[] { new EnvironmentVariable(EnableAVX2, "0"), new EnvironmentVariable(EnableSSSE3, "0") })
                    .WithId("Scalar"));
            }
        }
    }
}
