using System;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using BenchmarkDotNet.Attributes;

namespace gfoidl.Base64.Benchmarks.Vector128HelperBenchmarks
{
    public class ReadVector128Char
    {
        private const int Iterations = 1_000;
        private const int Size       = 16;
        private char[] _src;
        //---------------------------------------------------------------------
        [Params(0, 3, 15)]
        public int MisAlignment { get; set; } = 3;
        //---------------------------------------------------------------------
        [GlobalSetup]
        public void GlobalSetup()
        {
            var rnd = new Random(0);
            _src    = new char[2 * Size * Iterations + this.MisAlignment];

            for (int i = 0; i < _src.Length; ++i)
                _src[i] = (char)rnd.Next(0, byte.MaxValue + 1);
        }
        //---------------------------------------------------------------------
        [Benchmark(Baseline = true, OperationsPerInvoke = Iterations)]
        public Vector128<sbyte> ReadAs2Vector128()
        {
            ref char src = ref this.GetSrc();

            Vector128<sbyte> res = Vector128<sbyte>.Zero;

            for (int i = 0; i < Iterations; ++i)
            {
                Vector128<short> c0  = Unsafe.As<char, Vector128<short>>(ref src);
                Vector128<short> c1  = Unsafe.As<char, Vector128<short>>(ref Unsafe.Add(ref src, 8));
                Vector128<byte> tmp  = Sse2.PackUnsignedSaturate(c0, c1);
                Vector128<sbyte> vec = tmp.AsSByte();

                res = Sse2.Or(res, vec);
                src = ref Unsafe.Add(ref src, 16);
            }

            return res;
        }
        //---------------------------------------------------------------------
        [Benchmark(OperationsPerInvoke = Iterations)]
        public Vector128<sbyte> ReadAsVector256()
        {
            ref char src = ref this.GetSrc();

            Vector128<sbyte> res = Vector128<sbyte>.Zero;

            for (int i = 0; i < Iterations; ++i)
            {
                Vector256<short> t0  = Unsafe.As<char, Vector256<short>>(ref src);
                Vector128<short> c0  = t0.GetLower();
                Vector128<short> c1  = t0.GetUpper();
                Vector128<byte> tmp  = Sse2.PackUnsignedSaturate(c0, c1);
                Vector128<sbyte> vec = tmp.AsSByte();

                res = Sse2.Or(res, vec);
                src = ref Unsafe.Add(ref src, 16);
            }

            return res;
        }
        //---------------------------------------------------------------------
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref char GetSrc()
        {
            ref char ptr = ref _src[0];
            ref char src = ref Unsafe.Add(ref ptr, this.MisAlignment);

            return ref src;
        }
    }
}
