using System;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace gfoidl.Base64.Internal
{
    internal static class Avx2Helper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector256<sbyte> ReadVector256<T>(this ref T src) where T : unmanaged
        {
            if (typeof(T) == typeof(byte))
            {
                return Unsafe.As<T, byte>(ref src).ReadVector256();
            }
            else if (typeof(T) == typeof(char))
            {
                return Unsafe.As<T, char>(ref src).ReadVector256();
            }
            else
            {
                throw new NotSupportedException(); // just in case new types are introduced in the future
            }
        }
        //---------------------------------------------------------------------
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector256<sbyte> ReadVector256(this ref byte src)
        {
            return Unsafe.As<byte, Vector256<sbyte>>(ref src);
        }
        //---------------------------------------------------------------------
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector256<sbyte> ReadVector256(this ref char src)
        {
            Vector256<short> c0 = Unsafe.As<char, Vector256<short>>(ref Unsafe.Add(ref src, 0));
            Vector256<short> c1 = Unsafe.As<char, Vector256<short>>(ref Unsafe.Add(ref src, 16));

            Vector256<byte> t0 = Avx2.PackUnsignedSaturate(c0, c1);
            Vector256<long> t1 = Avx2.Permute4x64(t0.AsInt64(), 0b_11_01_10_00);

            return t1.AsSByte();
        }
        //---------------------------------------------------------------------
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteVector256<T>(this ref T dest, Vector256<sbyte> vec) where T : unmanaged
        {
            if (typeof(T) == typeof(byte))
            {
                Unsafe.As<T, byte>(ref dest).WriteVector256(vec);
            }
            else if (typeof(T) == typeof(char))
            {
                Unsafe.As<T, char>(ref dest).WriteVector256(vec);
            }
            else
            {
                throw new NotSupportedException(); // just in case new types are introduced in the future
            }
        }
        //---------------------------------------------------------------------
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteVector256(this ref byte dest, Vector256<sbyte> vec)
        {
            // As has better CQ than WriteUnaligned
            // https://github.com/dotnet/coreclr/issues/21132
            Unsafe.As<byte, Vector256<sbyte>>(ref dest) = vec;
        }
        //---------------------------------------------------------------------
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteVector256(this ref char dest, Vector256<sbyte> vec)
        {
            Vector256<sbyte> zero = Vector256<sbyte>.Zero;
            Vector256<sbyte> c0   = Avx2.UnpackLow (vec, zero);
            Vector256<sbyte> c1   = Avx2.UnpackHigh(vec, zero);

            Vector256<sbyte> t0 = Avx2.Permute2x128(c0, c1, 0x20);
            Vector256<sbyte> t1 = Avx2.Permute2x128(c0, c1, 0x31);

            Unsafe.As<char, Vector256<sbyte>>(ref Unsafe.Add(ref dest,  0)) = t0;
            Unsafe.As<char, Vector256<sbyte>>(ref Unsafe.Add(ref dest, 16)) = t1;
        }
        //---------------------------------------------------------------------
        public static Vector256<sbyte> LessThan(Vector256<sbyte> left, Vector256<sbyte> right)
        {
            Vector256<sbyte> allOnes = Vector256.Create((sbyte)-1);
            return LessThan(left, right, allOnes);
        }
        //---------------------------------------------------------------------
        // There is no intrinsics for that
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Vector256<sbyte> LessThan(Vector256<sbyte> left, Vector256<sbyte> right, Vector256<sbyte> allOnes)
        {
            // (a < b) = ~(a > b) & ~(a = b) = ~((a > b) | (a = b))

            Vector256<sbyte> eq  = Avx2.CompareEqual(left, right);
            Vector256<sbyte> gt  = Avx2.CompareGreaterThan(left, right);
            Vector256<sbyte> or  = Avx2.Or(eq, gt);

            // -1 = 0xFF = true in simd
            return Avx2.AndNot(or, allOnes);
        }
    }
}
