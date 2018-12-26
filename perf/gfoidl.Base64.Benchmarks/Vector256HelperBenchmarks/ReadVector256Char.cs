using System;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using BenchmarkDotNet.Attributes;

namespace gfoidl.Base64.Benchmarks.Vector256HelperBenchmarks
{
    public class ReadVector256Char
    {
        private const int Iterations = 1_000;
        private const int Size       = 32;
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
        [Benchmark(Baseline = true)]
        public Vector256<sbyte> ReadAs2Vector256()
        {
            ref char src = ref this.GetSrc();

            Vector256<sbyte> res = Vector256<sbyte>.Zero;

            for (int i = 0; i < Iterations; ++i)
            {
                Vector256<short> c0 = Unsafe.As<char, Vector256<short>>(ref Unsafe.Add(ref src, 0));
                Vector256<short> c1 = Unsafe.As<char, Vector256<short>>(ref Unsafe.Add(ref src, 16));

                Vector256<byte> t0   = Avx2.PackUnsignedSaturate(c0, c1);
                Vector256<sbyte> vec = Avx2.Permute4x64(t0.AsInt64(), 0b_11_01_10_00).AsSByte();

                res = Avx2.Or(res, vec);
                src = ref Unsafe.Add(ref src, 32);
            }

            return res;
        }
        //---------------------------------------------------------------------
        [Benchmark]
        public Vector256<sbyte> ReadAs4Vector128PackAsVector256()
        {
            ref char src = ref this.GetSrc();

            Vector256<sbyte> res = Vector256<sbyte>.Zero;

            for (int i = 0; i < Iterations; ++i)
            {
                Vector128<short> cc0 = Unsafe.As<char, Vector128<short>>(ref Unsafe.Add(ref src, 0));
                Vector128<short> cc1 = Unsafe.As<char, Vector128<short>>(ref Unsafe.Add(ref src, 8));
                Vector128<short> cc2 = Unsafe.As<char, Vector128<short>>(ref Unsafe.Add(ref src, 16));
                Vector128<short> cc3 = Unsafe.As<char, Vector128<short>>(ref Unsafe.Add(ref src, 24));

                Vector256<short> c0 = Vector256.Create(cc0, cc1);
                Vector256<short> c1 = Vector256.Create(cc2, cc3);

                Vector256<byte> t0   = Avx2.PackUnsignedSaturate(c0, c1);
                Vector256<sbyte> vec = Avx2.Permute4x64(t0.AsInt64(), 0b_11_01_10_00).AsSByte();

                res = Avx2.Or(res, vec);
                src = ref Unsafe.Add(ref src, 32);
            }

            return res;
        }
        //---------------------------------------------------------------------
        [Benchmark]
        public Vector256<sbyte> ReadAs4Vector128()
        {
            ref char src = ref this.GetSrc();

            Vector256<sbyte> res = Vector256<sbyte>.Zero;

            for (int i = 0; i < Iterations; ++i)
            {
                Vector128<short> c0 = Unsafe.As<char, Vector128<short>>(ref Unsafe.Add(ref src, 0));
                Vector128<short> c1 = Unsafe.As<char, Vector128<short>>(ref Unsafe.Add(ref src, 8));
                Vector128<short> c2 = Unsafe.As<char, Vector128<short>>(ref Unsafe.Add(ref src, 16));
                Vector128<short> c3 = Unsafe.As<char, Vector128<short>>(ref Unsafe.Add(ref src, 24));

                Vector128<byte> t0 = Sse2.PackUnsignedSaturate(c0, c1);
                Vector128<byte> t1 = Sse2.PackUnsignedSaturate(c2, c3);

                Vector256<sbyte> vec = Vector256.Create(t0, t1).AsSByte();

                res = Avx2.Or(res, vec);
                src = ref Unsafe.Add(ref src, 32);
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
