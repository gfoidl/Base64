using System;
using BenchmarkDotNet.Attributes;

namespace gfoidl.Base64.Benchmarks
{
    [Config(typeof(HardwareIntrinsicsCustomConfig))]
    public abstract class BaseBenchmarks
    {
        private const int ByteArraySize = 500;
        private readonly byte[]    _data;
        private readonly string    _dataEncoded;
        private readonly byte[]    _guid;
        private readonly string    _guidEncoded;
        protected readonly IBase64 _encoder;
        //---------------------------------------------------------------------
        protected BaseBenchmarks(IBase64 encoder)
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
        public string Encode_Data() => _encoder.Encode(_data);
        //---------------------------------------------------------------------
        [Benchmark]
        public string Encode_Guid() => _encoder.Encode(_guid);
        //---------------------------------------------------------------------
        [Benchmark]
        public byte[] Decode_Data() => _encoder.Decode(_dataEncoded);
        //---------------------------------------------------------------------
        [Benchmark]
        public byte[] Decode_Guid() => _encoder.Decode(_guidEncoded);
        //---------------------------------------------------------------------
        [Benchmark]
        public int GetArraySizeRequiredToEncode() => _encoder.GetEncodedLength(ByteArraySize);
        //---------------------------------------------------------------------
        [Benchmark]
        public int GetArraySizeRequiredToDecode() => _encoder.GetDecodedLength(_dataEncoded);
    }
}
