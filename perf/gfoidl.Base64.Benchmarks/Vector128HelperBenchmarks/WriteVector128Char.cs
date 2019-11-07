using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using BenchmarkDotNet.Attributes;

namespace gfoidl.Base64.Benchmarks.Vector128HelperBenchmarks
{
    public class WriteVector128Char
    {
        private const int Iterations = 1_000;
        private const int Size       = 16;
        private char[]? _dest;
        //---------------------------------------------------------------------
        [Params(0, 3, 15)]
        public int MisAlignment { get; set; } = 3;
        //---------------------------------------------------------------------
        [GlobalSetup]
        public void GlobalSetup()
        {
            _dest = new char[2 * Size * Iterations + this.MisAlignment];
        }
        //---------------------------------------------------------------------
        [Benchmark(Baseline = true, OperationsPerInvoke = Iterations)]
        public void WriteAs2Vector128()
        {
            ref char dest = ref this.GetDest();

            Vector128<sbyte> vec = Vector128<sbyte>.Zero;
            Vector128<sbyte> one = Vector128.Create((sbyte)1);

            for (int i = 0; i < Iterations; ++i)
            {
                Vector128<sbyte> zero = Vector128<sbyte>.Zero;
                Vector128<sbyte> c0   = Sse2.UnpackLow(vec, zero);
                Vector128<sbyte> c1   = Sse2.UnpackHigh(vec, zero);

                // As has better CQ than WriteUnaligned
                // https://github.com/dotnet/coreclr/issues/21132
                Unsafe.As<char, Vector128<sbyte>>(ref dest) = c0;
                Unsafe.As<char, Vector128<sbyte>>(ref Unsafe.Add(ref dest, 8)) = c1;

                vec  = Sse2.Add(vec, one);
                dest = ref Unsafe.Add(ref dest, 16);
            }
        }
        //---------------------------------------------------------------------
        [Benchmark(OperationsPerInvoke = Iterations)]
        public void WriteAsVector256()
        {
            ref char dest = ref this.GetDest();

            Vector128<sbyte> vec = Vector128<sbyte>.Zero;
            Vector128<sbyte> one = Vector128.Create((sbyte)1);

            for (int i = 0; i < Iterations; ++i)
            {
                Vector128<sbyte> zero = Vector128<sbyte>.Zero;
                Vector128<sbyte> c0   = Sse2.UnpackLow (vec, zero);
                Vector128<sbyte> c1   = Sse2.UnpackHigh(vec, zero);

                // As has better CQ than WriteUnaligned
                // https://github.com/dotnet/coreclr/issues/21132
                Vector256<sbyte> tmp = Vector256.Create(c0, c1);
                Unsafe.As<char, Vector256<sbyte>>(ref dest) = tmp;

                vec  = Sse2.Add(vec, one);
                dest = ref Unsafe.Add(ref dest, 16);
            }
        }
        //---------------------------------------------------------------------
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref char GetDest()
        {
            Debug.Assert(_dest != null);

            ref char ptr = ref _dest[0];
            ref char src = ref Unsafe.Add(ref ptr, this.MisAlignment);

            return ref src;
        }
    }
}
