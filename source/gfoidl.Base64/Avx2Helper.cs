#define AVX_PERMUTE
//-----------------------------------------------------------------------------
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace gfoidl.Base64
{
    internal static class Avx2Helper
    {
        // https://github.com/dotnet/coreclr/issues/21105
        private static readonly bool s_macHack;
        //---------------------------------------------------------------------
        static Avx2Helper()
        {
            s_macHack = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
        }
        //---------------------------------------------------------------------
        public static int DummyToInitType(int a, int b) => a + b;
        //---------------------------------------------------------------------
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(Vector256<sbyte> vec, ref char dest)
        {
            if (s_macHack)
            {
                Vector<sbyte> v = Unsafe.As<Vector256<sbyte>, Vector<sbyte>>(ref vec);
                Vector.Widen(v, out Vector<short> v1, out Vector<short> v2);

                Unsafe.As<char, Vector<short>>(ref Unsafe.Add(ref dest,  0)) = v1;
                Unsafe.As<char, Vector<short>>(ref Unsafe.Add(ref dest, 16)) = v2;

                return;
            }

            // https://github.com/dotnet/coreclr/issues/21130
            Vector256<sbyte> zero = Avx.SetZeroVector256<sbyte>();

            Vector256<sbyte> c0 = Avx2.UnpackLow(vec, zero);
            Vector256<sbyte> c1 = Avx2.UnpackHigh(vec, zero);

#if AVX_PERMUTE
            // Variant with permute is ~10% faster than the other variant
            Vector256<sbyte> t0 = Avx2.Permute2x128(c0, c1, 0x20);
            Vector256<sbyte> t1 = Avx2.Permute2x128(c0, c1, 0x31);

            Unsafe.As<char, Vector256<sbyte>>(ref Unsafe.Add(ref dest,  0)) = t0;
            Unsafe.As<char, Vector256<sbyte>>(ref Unsafe.Add(ref dest, 16)) = t1;
#else
            // https://github.com/dotnet/coreclr/issues/21130
            // Same issue for c0.GetLower(); c0.GetUpper();
            Vector128<sbyte> t0 = Avx.GetLowerHalf(c0);
            Vector128<sbyte> t1 = Avx.GetLowerHalf(c1);
            Vector128<sbyte> t2 = Avx2.ExtractVector128(c0, 1);
            Vector128<sbyte> t3 = Avx2.ExtractVector128(c1, 1);

            Unsafe.As<char, Vector128<sbyte>>(ref Unsafe.Add(ref dest, 0))  = t0;
            Unsafe.As<char, Vector128<sbyte>>(ref Unsafe.Add(ref dest, 8))  = t1;
            Unsafe.As<char, Vector128<sbyte>>(ref Unsafe.Add(ref dest, 16)) = t2;
            Unsafe.As<char, Vector128<sbyte>>(ref Unsafe.Add(ref dest, 24)) = t3;
#endif
        }
        //---------------------------------------------------------------------
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector256<sbyte> Read(ref char src)
        {
            Vector256<short> c0 = Unsafe.As<char, Vector256<short>>(ref Unsafe.Add(ref src,  0));
            Vector256<short> c1 = Unsafe.As<char, Vector256<short>>(ref Unsafe.Add(ref src, 16));

            if (s_macHack)
            {
                Vector<short> v0 = Unsafe.As<Vector256<short>, Vector<short>>(ref c0);
                Vector<short> v1 = Unsafe.As<Vector256<short>, Vector<short>>(ref c1);

                return Unsafe.As<Vector<sbyte>, Vector256<sbyte>>(ref Unsafe.AsRef(Vector.Narrow(v0, v1)));
            }

            Vector256<byte> t0 = Avx2.PackUnsignedSaturate(c0, c1);
            Vector256<long> t1 = Avx2.Permute4x64(Avx.StaticCast<byte, long>(t0), 0b_11_01_10_00);

            return Avx.StaticCast<long, sbyte>(t1);
        }
    }
}
