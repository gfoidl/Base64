using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace gfoidl.Base64.Internal
{
    internal static class Vector128Helper
    {
        public static Vector128<sbyte> ReadVector128(this ReadOnlySpan<sbyte> data)
        {
            ref sbyte tmp = ref MemoryMarshal.GetReference(data);
            return Unsafe.As<sbyte, Vector128<sbyte>>(ref tmp);
        }
        //---------------------------------------------------------------------
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<sbyte> ReadVector128<T>(this ref T src) where T : unmanaged
        {
            if (typeof(T) == typeof(byte))
            {
                return Unsafe.As<T, byte>(ref src).ReadVector128();
            }
            else if (typeof(T) == typeof(char))
            {
                return Unsafe.As<T, char>(ref src).ReadVector128();
            }
            else
            {
                throw new NotSupportedException(); // just in case new types are introduced in the future
            }
        }
        //---------------------------------------------------------------------
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<sbyte> ReadVector128(this ref byte src)
        {
            return Unsafe.As<byte, Vector128<sbyte>>(ref src);
        }
        //---------------------------------------------------------------------
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<sbyte> ReadVector128(this ref char src)
        {
            Vector128<short> c0 = Unsafe.As<char, Vector128<short>>(ref src);
            Vector128<short> c1 = Unsafe.As<char, Vector128<short>>(ref Unsafe.Add(ref src, 8));
            Vector128<byte> tmp = Sse2.PackUnsignedSaturate(c0, c1);

#if NETCOREAPP2_1
            return Sse.StaticCast<byte, sbyte>(tmp);
#else
            return tmp.AsSByte();
#endif
        }
        //---------------------------------------------------------------------
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteVector128<T>(this ref T dest, Vector128<sbyte> vec) where T : unmanaged
        {
            if (typeof(T) == typeof(byte))
            {
                Unsafe.As<T, byte>(ref dest).WriteVector128(vec);
            }
            else if (typeof(T) == typeof(char))
            {
                Unsafe.As<T, char>(ref dest).WriteVector128(vec);
            }
            else
            {
                throw new NotSupportedException(); // just in case new types are introduced in the future
            }
        }
        //---------------------------------------------------------------------
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteVector128(this ref byte dest, Vector128<sbyte> vec)
        {
            Unsafe.As<byte, Vector128<sbyte>>(ref dest) = vec;
        }
        //---------------------------------------------------------------------
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteVector128(this ref char dest, Vector128<sbyte> vec)
        {
#if NETCOREAPP2_1
            Vector128<sbyte> zero = Sse2.SetZeroVector128<sbyte>();
#else
            Vector128<sbyte> zero = Vector128<sbyte>.Zero;
#endif
            Vector128<sbyte> c0   = Sse2.UnpackLow(vec, zero);
            Vector128<sbyte> c1   = Sse2.UnpackHigh(vec, zero);

            // As has better CQ than WriteUnaligned
            // https://github.com/dotnet/coreclr/issues/21132
            Unsafe.As<char, Vector128<sbyte>>(ref dest) = c0;
            Unsafe.As<char, Vector128<sbyte>>(ref Unsafe.Add(ref dest, 8)) = c1;
        }
    }
}
