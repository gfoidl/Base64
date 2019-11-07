using System;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;

namespace gfoidl.Base64.Benchmarks.StringBenchmarks
{
    // dotnet run -c Relase --filter *Base64EncodeToString*

    [ShortRunJob]
    [MemoryDiagnoser]
    public class Base64EncodeToString
    {
        public static void Execute(int dataLen = 20)
        {
            var bench = new Base64EncodeToString { DataLen = dataLen };
            bench.GlobalSetup();

            Console.WriteLine(bench.ConvertToBase64());
            Console.WriteLine(bench.NewString());
            Console.WriteLine(bench.StringCreate());
        }
        //---------------------------------------------------------------------
        private byte[]? _data;
        //---------------------------------------------------------------------
        [Params(5, 16, 32, 72, 64, 80, 96)]
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
        public unsafe string NewString()
        {
            ReadOnlySpan<byte> data = _data;
            int encodedLength       = Base64.Default.GetEncodedLength(data.Length);

            // stackallocing a power of 2 is preferred, as the JIT can produce better code,
            // especially if `locals init` is skipped, so it's just a pointer move `sub rsp, 64`
            char* ptr              = stackalloc char[128];
            ref char encoded       = ref Unsafe.AsRef<char>(ptr);
            ref byte srcBytes      = ref MemoryMarshal.GetReference(data);
            OperationStatus status = Base64.Default.EncodeImpl(ref srcBytes, data.Length, ref encoded, encodedLength, encoded, out int consumed, out int written);

            Debug.Assert(status        == OperationStatus.Done);
            Debug.Assert(data.Length   == consumed);
            Debug.Assert(encodedLength == written);

            return new string(ptr, 0, written);
        }
        //---------------------------------------------------------------------
        [Benchmark]
        public unsafe string StringCreate()
        {
            ReadOnlySpan<byte> data = _data;
            int encodedLength       = Base64.Default.GetEncodedLength(data.Length);

            fixed (byte* ptr = &MemoryMarshal.GetReference(data))
            {
                return string.Create(encodedLength, (Ptr: (IntPtr)ptr, data.Length), (encoded, state) =>
                {
                    ref byte srcBytes      = ref Unsafe.AsRef<byte>(state.Ptr.ToPointer());
                    ref char dest          = ref MemoryMarshal.GetReference(encoded);
                    OperationStatus status = Base64.Default.EncodeImpl(ref srcBytes, state.Length, ref dest, encoded.Length, encoded.Length, out int consumed, out int written);

                    Debug.Assert(status         == OperationStatus.Done);
                    Debug.Assert(state.Length   == consumed);
                    Debug.Assert(encoded.Length == written);
                });
            }
        }
    }
}
