using System;
using BenchmarkDotNet.Attributes;

namespace gfoidl.Base64.Benchmarks
{
    public class Base64EncoderBenchmark : BaseBenchmark
    {
        public Base64EncoderBenchmark() : base(Base64.Default) { }
    }
    //-------------------------------------------------------------------------
    public class Base64UrlEncoderBenchmark : BaseBenchmark
    {
        public Base64UrlEncoderBenchmark() : base(Base64.Url) { }
    }
    //-------------------------------------------------------------------------
#if NETCOREAPP
    [Config(typeof(HardwareIntrinsicsCustomConfig))]
#endif
    [MemoryDiagnoser]
    public abstract class BaseBenchmark
    {
        private const int ByteArraySize = 500;
        private readonly byte[]    _data;
        private readonly string    _dataEncoded;
        private readonly byte[]    _guid;
        private readonly string    _guidEncoded;
        protected readonly IBase64 _encoder;
        //---------------------------------------------------------------------
        protected BaseBenchmark(IBase64? encoder)
        {
            _encoder = encoder ?? throw new ArgumentNullException(nameof(encoder));

            var random   = new Random(42);
            _data        = new byte[ByteArraySize];
            random.NextBytes(_data);
            _dataEncoded = _encoder.Encode(_data);

            _guid        = Guid.NewGuid().ToByteArray();
            _guidEncoded = _encoder.Encode(_guid);
        }
        //---------------------------------------------------------------------
        [Benchmark]
        public string Encode_Data() => _encoder.Encode(_data.AsSpan());                                 // AsSpan for .NET Full
        //---------------------------------------------------------------------
        [Benchmark]
        public string Encode_Guid() => _encoder.Encode(_guid.AsSpan());                                 // AsSpan for .NET Full
        //---------------------------------------------------------------------
        [Benchmark]
        public byte[] Decode_Data() => _encoder.Decode(_dataEncoded.AsSpan());                          // AsSpan for .NET Full
        //---------------------------------------------------------------------
        [Benchmark]
        public byte[] Decode_Guid() => _encoder.Decode(_guidEncoded.AsSpan());                          // AsSpan for .NET Full
        //---------------------------------------------------------------------
        [Benchmark]
        public int GetArraySizeRequiredToEncode() => _encoder.GetEncodedLength(ByteArraySize);
        //---------------------------------------------------------------------
        [Benchmark]
        public int GetArraySizeRequiredToDecode() => _encoder.GetDecodedLength(_dataEncoded.AsSpan());  // AsSpan for .NET Full
        //---------------------------------------------------------------------
        [Benchmark]
        public int GetMaxDecodedLength() => _encoder.GetMaxDecodedLength(_dataEncoded.Length);
    }
}
