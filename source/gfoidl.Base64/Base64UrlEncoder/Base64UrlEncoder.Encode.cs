using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#if NETCOREAPP
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
#endif

// Sequential based on https://github.com/dotnet/corefx/tree/master/src/System.Memory/src/System/Buffers/Text
// SSE2 based on https://github.com/aklomp/base64/tree/master/lib/arch/ssse3

namespace gfoidl.Base64
{
    partial class Base64UrlEncoder
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetEncodedLength(int sourceLength)
        {
            // Shortcut for Guid and other 16 byte data
            if (sourceLength == 16)
                return 22;

            int numPaddingChars  = GetNumBase64PaddingCharsAddedByEncode(sourceLength);
            int base64EncodedLen = GetBase64EncodedLength(sourceLength);

            return base64EncodedLen - numPaddingChars;
        }
        //---------------------------------------------------------------------
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetBufferSizeRequiredToBase64Encode(int sourceLength, out int numPaddingChars)
        {
            // Shortcut for Guid and other 16 byte data
            if (sourceLength == 16)
            {
                numPaddingChars = 2;
                return 24;
            }

            numPaddingChars = GetNumBase64PaddingCharsAddedByEncode(sourceLength);
            return GetBase64EncodedLength(sourceLength);
        }
        //---------------------------------------------------------------------
#if NETCOREAPP3_0
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
#endif
        protected override OperationStatus EncodeCore<T>(ref byte srcBytes, int srcLength, Span<T> encoded, out int consumed, out int written, bool isFinalBlock = true)
        {
            int destLength   = encoded.Length;
            uint sourceIndex = 0;
            uint destIndex   = 0;

            ref T dest = ref MemoryMarshal.GetReference(encoded);

#if NETCOREAPP
#if NETCOREAPP3_0
            if (Ssse3.IsSupported && srcLength >= 16)
#else
            if (Sse2.IsSupported && Ssse3.IsSupported && srcLength >= 16)
#endif
            {
                Sse2Encode(ref srcBytes, ref dest, srcLength, ref sourceIndex, ref destIndex);

                if (sourceIndex == srcLength)
                    goto DoneExit;
            }
#endif
            int maxSrcLength = -2;

            if (srcLength <= MaximumEncodeLength && destLength >= this.GetEncodedLength(srcLength))
                maxSrcLength += srcLength;
            else
                maxSrcLength += (destLength >> 2) * 3;

            ref byte encodingMap = ref s_encodingMap[0];

            // In order to elide the movsxd in the loop
            if (sourceIndex < maxSrcLength)
            {
                do
                {
                    EncodeThreeBytes(ref Unsafe.Add(ref srcBytes, (IntPtr)sourceIndex), ref Unsafe.Add(ref dest, (IntPtr)destIndex), ref encodingMap);
                    destIndex   += 4;
                    sourceIndex += 3;
                }
                while (sourceIndex < (uint)maxSrcLength);
            }

            if (maxSrcLength != srcLength - 2)
                goto DestinationSmallExit;

            if (!isFinalBlock)
                goto NeedMoreDataExit;

            if (sourceIndex == srcLength - 1)
            {
                EncodeOneByte(ref Unsafe.Add(ref srcBytes, (IntPtr)sourceIndex), ref Unsafe.Add(ref dest, (IntPtr)destIndex), ref encodingMap);
                destIndex   += 2;
                sourceIndex += 1;
            }
            else if (sourceIndex == srcLength - 2)
            {
                EncodeTwoBytes(ref Unsafe.Add(ref srcBytes, (IntPtr)sourceIndex), ref Unsafe.Add(ref dest, (IntPtr)destIndex), ref encodingMap);
                destIndex   += 3;
                sourceIndex += 2;
            }
#if NETCOREAPP
        DoneExit:
#endif
            consumed = (int)sourceIndex;
            written  = (int)destIndex;
            return OperationStatus.Done;

        NeedMoreDataExit:
            consumed = (int)sourceIndex;
            written  = (int)destIndex;
            return OperationStatus.NeedMoreData;

        DestinationSmallExit:
            consumed = (int)sourceIndex;
            written  = (int)destIndex;
            return OperationStatus.DestinationTooSmall;
        }
        //---------------------------------------------------------------------
#if NETCOREAPP
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Sse2Encode<T>(ref byte src, ref T dest, int sourceLength, ref uint sourceIndex, ref uint destIndex)
        {
            ref byte srcStart   = ref src;
            ref T destStart     = ref dest;
            ref byte simdSrcEnd = ref Unsafe.Add(ref src, (IntPtr)(sourceLength - 16 + 1));

            // Shift to workspace
            src  = ref Unsafe.Add(ref src , (IntPtr)sourceIndex);
            dest = ref Unsafe.Add(ref dest, (IntPtr)destIndex);

            // The JIT won't hoist these "constants", so help him
            Vector128<sbyte>  shuffleVec          = s_sse_encodeShuffleVec;
            Vector128<sbyte>  shuffleConstant0    = Sse.StaticCast<int, sbyte>(Sse2.SetAllVector128(0x0fc0fc00));
            Vector128<sbyte>  shuffleConstant2    = Sse.StaticCast<int, sbyte>(Sse2.SetAllVector128(0x003f03f0));
            Vector128<ushort> shuffleConstant1    = Sse.StaticCast<int, ushort>(Sse2.SetAllVector128(0x04000040));
            Vector128<short>  shuffleConstant3    = Sse.StaticCast<int, short>(Sse2.SetAllVector128(0x01000010));
            Vector128<byte>   translationContant0 = Sse2.SetAllVector128((byte)51);
            Vector128<sbyte>  translationContant1 = Sse2.SetAllVector128((sbyte)25);
            Vector128<sbyte>  lut                 = s_sse_encodeLut;

            //while (remaining >= 16)
            while (Unsafe.IsAddressLessThan(ref src, ref simdSrcEnd))
            {
                Vector128<sbyte> str = Unsafe.ReadUnaligned<Vector128<sbyte>>(ref src);

                // Reshuffle
                str                  = Ssse3.Shuffle(str, shuffleVec);
                Vector128<sbyte>  t0 = Sse2.And(str, shuffleConstant0);
                Vector128<sbyte>  t2 = Sse2.And(str, shuffleConstant2);
                Vector128<ushort> t1 = Sse2.MultiplyHigh(Sse.StaticCast<sbyte, ushort>(t0), shuffleConstant1);
                Vector128<short>  t3 = Sse2.MultiplyLow(Sse.StaticCast<sbyte, short>(t2), shuffleConstant3);
                str                  = Sse2.Or(Sse.StaticCast<ushort, sbyte>(t1), Sse.StaticCast<short, sbyte>(t3));

                // Translation
                Vector128<byte>  indices = Sse2.SubtractSaturate(Sse.StaticCast<sbyte, byte>(str), translationContant0);
                Vector128<sbyte> mask    = Sse2.CompareGreaterThan(str, translationContant1);
                Vector128<sbyte> tmp     = Sse2.Subtract(Sse.StaticCast<byte, sbyte>(indices), mask);
                str                      = Sse2.Add(str, Ssse3.Shuffle(lut, tmp));

                if (typeof(T) == typeof(byte))
                {
                    // As has better CQ than WriteUnaligned
                    Unsafe.As<T, Vector128<sbyte>>(ref dest) = str;
                }
                else if (typeof(T) == typeof(char))
                {
#if NETCOREAPP3_0
                    // https://github.com/dotnet/coreclr/issues/21130
                    //Vector128<sbyte> zero = Vector128<sbyte>.Zero;
                    Vector128<sbyte> zero = Sse2.SetZeroVector128<sbyte>();
#else
                    Vector128<sbyte> zero = Sse2.SetZeroVector128<sbyte>();
#endif
                    Vector128<sbyte> c0   = Sse2.UnpackLow(str, zero);
                    Vector128<sbyte> c1   = Sse2.UnpackHigh(str, zero);

                    // As has better CQ than WriteUnaligned
                    // https://github.com/dotnet/coreclr/issues/21132
                    Unsafe.As<T, Vector128<sbyte>>(ref dest)                    = c0;
                    Unsafe.As<T, Vector128<sbyte>>(ref Unsafe.Add(ref dest, 8)) = c1;
                }
                else
                {
                    throw new NotSupportedException(); // just in case new types are introduced in the future
                }

                src  = ref Unsafe.Add(ref src,  12);
                dest = ref Unsafe.Add(ref dest, 16);
            }

            // Cast to ulong to avoid the overflow-check. Codegen for x86 is still good.
            sourceIndex = (uint)(ulong)Unsafe.ByteOffset(ref srcStart, ref src);
            destIndex   = (uint)(ulong)Unsafe.ByteOffset(ref destStart, ref dest) / (uint)Unsafe.SizeOf<T>();

            src  = ref srcStart;
            dest = ref destStart;
        }
#endif
        //---------------------------------------------------------------------
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void EncodeTwoBytes<T>(ref byte twoBytes, ref T encoded, ref byte encodingMap)
        {
            uint i = (uint)twoBytes << 16
                | (uint)Unsafe.Add(ref twoBytes, 1) << 8;

            uint i0 = Unsafe.Add(ref encodingMap, (IntPtr)(i >> 18));
            uint i1 = Unsafe.Add(ref encodingMap, (IntPtr)((i >> 12) & 0x3F));
            uint i2 = Unsafe.Add(ref encodingMap, (IntPtr)((i >> 6) & 0x3F));

            if (typeof(T) == typeof(byte))
            {
                ref byte enc = ref Unsafe.As<T, byte>(ref encoded);
                Unsafe.Add(ref enc, 0) = (byte)i0;
                Unsafe.Add(ref enc, 1) = (byte)i1;
                Unsafe.Add(ref enc, 2) = (byte)i2;
            }
            else if (typeof(T) == typeof(char))
            {
                ref char enc = ref Unsafe.As<T, char>(ref encoded);
                Unsafe.Add(ref enc, 0) = (char)i0;
                Unsafe.Add(ref enc, 1) = (char)i1;
                Unsafe.Add(ref enc, 2) = (char)i2;
            }
            else
            {
                throw new NotSupportedException();  // just in case new types are introduced in the future
            }
        }
        //---------------------------------------------------------------------
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void EncodeOneByte<T>(ref byte oneByte, ref T encoded, ref byte encodingMap)
        {
            uint i = (uint)oneByte << 8;

            uint i0 = Unsafe.Add(ref encodingMap, (IntPtr)(i >> 10));
            uint i1 = Unsafe.Add(ref encodingMap, (IntPtr)((i >> 4) & 0x3F));

            if (typeof(T) == typeof(byte))
            {
                ref byte enc = ref Unsafe.As<T, byte>(ref encoded);
                Unsafe.Add(ref enc, 0) = (byte)i0;
                Unsafe.Add(ref enc, 1) = (byte)i1;
            }
            else if (typeof(T) == typeof(char))
            {
                ref char enc = ref Unsafe.As<T, char>(ref encoded);
                Unsafe.Add(ref enc, 0) = (char)i0;
                Unsafe.Add(ref enc, 1) = (char)i1;
            }
            else
            {
                throw new NotSupportedException();  // just in case new types are introduced in the future
            }
        }
        //---------------------------------------------------------------------
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetNumBase64PaddingCharsAddedByEncode(int dataLength)
        {
            // Calculation is:
            // switch (dataLength % 3)
            // 0 -> 0
            // 1 -> 2
            // 2 -> 1

            return dataLength % 3 == 0 ? 0 : 3 - dataLength % 3;
        }
        //---------------------------------------------------------------------
#if NETCOREAPP
        private static readonly Vector128<sbyte> s_sse_encodeShuffleVec;
        private static readonly Vector128<sbyte> s_sse_encodeLut;
#endif
        // internal because tests use this map too
        internal static readonly byte[] s_encodingMap = {
            65, 66, 67, 68, 69, 70, 71, 72,         //A..H
            73, 74, 75, 76, 77, 78, 79, 80,         //I..P
            81, 82, 83, 84, 85, 86, 87, 88,         //Q..X
            89, 90, 97, 98, 99, 100, 101, 102,      //Y..Z, a..f
            103, 104, 105, 106, 107, 108, 109, 110, //g..n
            111, 112, 113, 114, 115, 116, 117, 118, //o..v
            119, 120, 121, 122, 48, 49, 50, 51,     //w..z, 0..3
            52, 53, 54, 55, 56, 57, 45, 95          //4..9, -, _
        };
    }
}
