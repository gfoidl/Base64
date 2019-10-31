using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using BenchmarkDotNet.Attributes;

namespace gfoidl.Base64.Benchmarks.Vector256HelperBenchmarks
{
    public class WriteVector256Byte
    {
        private const int Iterations = 1_000;
        private const int Size       = 32;
        private byte[] _dest;
        //---------------------------------------------------------------------
        [Params(0, 3, 15)]
        public int MisAlignment { get; set; } = 3;
        //---------------------------------------------------------------------
        [GlobalSetup]
        public void GlobalSetup()
        {
            _dest = new byte[Size * Iterations + this.MisAlignment];
        }
        //---------------------------------------------------------------------
        [Benchmark(Baseline = true, OperationsPerInvoke = Iterations)]
        public void WriteDirect()
        {
            ref byte dest = ref this.GetDest();

            Vector256<sbyte> vec = Vector256<sbyte>.Zero;
            Vector256<sbyte> one = Vector256.Create((sbyte)1);

            for (int i = 0; i < Iterations; ++i)
            {
                Unsafe.As<byte, Vector256<sbyte>>(ref dest) = vec;

                vec  = Avx2.Add(vec, one);
                dest = ref Unsafe.Add(ref dest, 32);
            }
        }
        //---------------------------------------------------------------------
        [Benchmark(OperationsPerInvoke = Iterations)]
        public void WriteAs2Vector128()
        {
            ref byte dest = ref this.GetDest();

            Vector256<sbyte> vec = Vector256<sbyte>.Zero;
            Vector256<sbyte> one = Vector256.Create((sbyte)1);

            for (int i = 0; i < Iterations; ++i)
            {
                Vector128<sbyte> t0 = vec.GetLower();
                Vector128<sbyte> t1 = vec.GetUpper();

                // As has better CQ than WriteUnaligned
                // https://github.com/dotnet/coreclr/issues/21132
                Unsafe.As<byte, Vector128<sbyte>>(ref Unsafe.Add(ref dest,  0)) = t0;
                Unsafe.As<byte, Vector128<sbyte>>(ref Unsafe.Add(ref dest, 16)) = t1;

                vec  = Avx2.Add(vec, one);
                dest = ref Unsafe.Add(ref dest, 32);
            }
        }
        //---------------------------------------------------------------------
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref byte GetDest()
        {
            ref byte ptr = ref _dest[0];
            ref byte src = ref Unsafe.Add(ref ptr, this.MisAlignment);

            return ref src;
        }
    }
}
