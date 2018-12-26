using System;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using BenchmarkDotNet.Attributes;

namespace gfoidl.Base64.Benchmarks.Vector256HelperBenchmarks
{
    public class ReadVector256Byte
    {
        private const int Iterations = 1_000;
        private const int Size       = 32;
        private byte[] _src;
        //---------------------------------------------------------------------
        [Params(0, 3, 15)]
        public int MisAlignment { get; set; } = 3;
        //---------------------------------------------------------------------
        [GlobalSetup]
        public void GlobalSetup()
        {
            var rnd = new Random(0);
            _src    = new byte[Size * Iterations + this.MisAlignment];

            rnd.NextBytes(_src);
        }
        //---------------------------------------------------------------------
        [Benchmark(Baseline = true)]
        public Vector256<sbyte> ReadDirect()
        {
            ref byte src = ref this.GetSrc();

            Vector256<sbyte> res = Vector256<sbyte>.Zero;

            for (int i = 0; i < Iterations; ++i)
            {
                Vector256<sbyte> vec = Unsafe.As<byte, Vector256<sbyte>>(ref src);

                res = Avx2.Or(res, vec);
                src = ref Unsafe.Add(ref src, 32);
            }

            return res;
        }
        //---------------------------------------------------------------------
        [Benchmark]
        public Vector256<sbyte> ReadAs2Vector128()
        {
            ref byte src = ref this.GetSrc();

            Vector256<sbyte> res = Vector256<sbyte>.Zero;

            for (int i = 0; i < Iterations; ++i)
            {
                Vector128<sbyte> t0  = Unsafe.As<byte, Vector128<sbyte>>(ref Unsafe.Add(ref src, 0));
                Vector128<sbyte> t1  = Unsafe.As<byte, Vector128<sbyte>>(ref Unsafe.Add(ref src, 16));
                Vector256<sbyte> vec = Vector256.Create(t0, t1);

                res = Avx2.Or(res, vec);
                src = ref Unsafe.Add(ref src, 32);
            }

            return res;
        }
        //---------------------------------------------------------------------
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref byte GetSrc()
        {
            ref byte ptr = ref _src[0];
            ref byte src = ref Unsafe.Add(ref ptr, this.MisAlignment);

            return ref src;
        }
    }
}
