using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using BenchmarkDotNet.Attributes;

namespace gfoidl.Base64.Benchmarks.Vector256HelperBenchmarks
{
    public class WriteVector256Char
    {
        private const int Iterations = 1_000;
        private const int Size       = 32;
        private char[] _dest;
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
        [Benchmark(Baseline = true)]
        public void WriteAs2Vector256()
        {
            ref char dest = ref this.GetDest();

            Vector256<sbyte> vec = Vector256<sbyte>.Zero;
            Vector256<sbyte> one = Vector256.Create((sbyte)1);

            for (int i = 0; i < Iterations; ++i)
            {
                Vector256<sbyte> zero = Vector256<sbyte>.Zero;
                Vector256<sbyte> c0   = Avx2.UnpackLow(vec, zero);
                Vector256<sbyte> c1   = Avx2.UnpackHigh(vec, zero);

                Vector256<sbyte> t0 = Avx2.Permute2x128(c0, c1, 0x20);
                Vector256<sbyte> t1 = Avx2.Permute2x128(c0, c1, 0x31);

                Unsafe.As<char, Vector256<sbyte>>(ref Unsafe.Add(ref dest,  0)) = t0;
                Unsafe.As<char, Vector256<sbyte>>(ref Unsafe.Add(ref dest, 16)) = t1;

                vec  = Avx2.Add(vec, one);
                dest = ref Unsafe.Add(ref dest, 32);
            }
        }
        //---------------------------------------------------------------------
        [Benchmark]
        public void WriteAs4Vector128UnpackAsVector256()
        {
            ref char dest = ref this.GetDest();

            Vector256<sbyte> vec = Vector256<sbyte>.Zero;
            Vector256<sbyte> one = Vector256.Create((sbyte)1);

            for (int i = 0; i < Iterations; ++i)
            {
                Vector256<sbyte> zero = Vector256<sbyte>.Zero;
                Vector256<sbyte> c0   = Avx2.UnpackLow(vec, zero);
                Vector256<sbyte> c1   = Avx2.UnpackHigh(vec, zero);

                Vector128<sbyte> t0 = c0.GetLower();
                Vector128<sbyte> t1 = c1.GetLower();
                Vector128<sbyte> t2 = c0.GetUpper();
                Vector128<sbyte> t3 = c1.GetUpper();

                Unsafe.As<char, Vector128<sbyte>>(ref Unsafe.Add(ref dest,  0)) = t0;
                Unsafe.As<char, Vector128<sbyte>>(ref Unsafe.Add(ref dest,  8)) = t1;
                Unsafe.As<char, Vector128<sbyte>>(ref Unsafe.Add(ref dest, 16)) = t2;
                Unsafe.As<char, Vector128<sbyte>>(ref Unsafe.Add(ref dest, 24)) = t3;

                vec  = Avx2.Add(vec, one);
                dest = ref Unsafe.Add(ref dest, 32);
            }
        }
        //---------------------------------------------------------------------
        [Benchmark]
        public void WriteAs4Vector128()
        {
            ref char dest = ref this.GetDest();

            Vector256<sbyte> vec = Vector256<sbyte>.Zero;
            Vector256<sbyte> one = Vector256.Create((sbyte)1);

            for (int i = 0; i < Iterations; ++i)
            {
                Vector128<sbyte> t0 = vec.GetLower();
                Vector128<sbyte> t1 = vec.GetUpper();

                Vector128<sbyte> zero = Vector128<sbyte>.Zero;
                Vector128<sbyte> c0   = Sse2.UnpackLow (t0, zero);
                Vector128<sbyte> c1   = Sse2.UnpackHigh(t0, zero);
                Vector128<sbyte> c2   = Sse2.UnpackLow (t1, zero);
                Vector128<sbyte> c3   = Sse2.UnpackHigh(t1, zero);

                Unsafe.As<char, Vector128<sbyte>>(ref Unsafe.Add(ref dest,  0)) = c0;
                Unsafe.As<char, Vector128<sbyte>>(ref Unsafe.Add(ref dest,  8)) = c1;
                Unsafe.As<char, Vector128<sbyte>>(ref Unsafe.Add(ref dest, 16)) = c2;
                Unsafe.As<char, Vector128<sbyte>>(ref Unsafe.Add(ref dest, 24)) = c3;

                vec  = Avx2.Add(vec, one);
                dest = ref Unsafe.Add(ref dest, 32);
            }
        }
        //---------------------------------------------------------------------
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref char GetDest()
        {
            ref char ptr = ref _dest[0];
            ref char src = ref Unsafe.Add(ref ptr, this.MisAlignment);

            return ref src;
        }
    }
}
