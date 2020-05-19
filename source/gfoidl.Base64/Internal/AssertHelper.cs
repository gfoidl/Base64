using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace gfoidl.Base64.Internal
{
    [ExcludeFromCodeCoverage]
    internal static class AssertHelper
    {
        [Conditional("DEBUG")]
        public static void AssertRead<TVector, T>(this ref T src, ref T srcStart, int srcLength) where T : unmanaged
        {
            int vectorElements = GetCount<TVector, T>();
            ref T readEnd      = ref Unsafe.Add(ref src, vectorElements);
            ref T srcEnd       = ref Unsafe.Add(ref srcStart, srcLength);

            bool isUnsafe = Unsafe.IsAddressGreaterThan(ref readEnd, ref srcEnd);

            if (isUnsafe)
            {
                int srcIndex = Unsafe.ByteOffset(ref srcStart, ref src).ToInt32();
                throw new InvalidOperationException($"Read for {typeof(TVector)} is not within safe bounds. srcIndex: {srcIndex}, srcLength: {srcLength}");
            }
        }
        //---------------------------------------------------------------------
        [Conditional("DEBUG")]
        public static void AssertWrite<TVector, T>(this ref T dest, ref T destStart, int destLength) where T : unmanaged
        {
            int vectorElements = GetCount<TVector, T>();
            ref T writeEnd     = ref Unsafe.Add(ref dest, vectorElements);
            ref T destEnd      = ref Unsafe.Add(ref destStart, destLength);

            bool isUnsafe = Unsafe.IsAddressGreaterThan(ref writeEnd, ref destEnd);

            if (isUnsafe)
            {
                int destIndex = Unsafe.ByteOffset(ref destStart, ref dest).ToInt32();
                throw new InvalidOperationException($"Write for {typeof(TVector)} is not within safe bounds. destIndex: {destIndex}, destLength: {destLength}");
            }
        }
        //---------------------------------------------------------------------
        private static int GetCount<TVector, T>() where T : unmanaged
        {
            int vectorSize  = Unsafe.SizeOf<TVector>();
            int elementSize = Unsafe.SizeOf<T>();

            return vectorSize / elementSize;
        }
    }
}
