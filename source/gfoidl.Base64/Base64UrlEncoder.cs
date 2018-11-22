#if NETCOREAPP
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
#endif

namespace gfoidl.Base64
{
    public sealed partial class Base64UrlEncoder : Base64EncoderBase
    {
        static Base64UrlEncoder()
        {
#if NETCOREAPP
#if NETCOREAPP3_0
            if (Ssse3.IsSupported)
#else
            if (Sse2.IsSupported && Ssse3.IsSupported)
#endif
            {
                s_sse_encodeShuffleVec = Sse2.SetVector128(
                    10, 11, 9, 10,
                     7,  8, 6,  7,
                     4,  5, 3,  4,
                     1,  2, 0,  1
                );

                s_sse_encodeLut = Sse2.SetVector128(
                     0,  0, 32, -17,
                    -4, -4, -4,  -4,
                    -4, -4, -4,  -4,
                    -4, -4, 71,  65
                );

                s_sse_decodeShuffleVec = Sse2.SetVector128(
                    -1, -1, -1, -1,
                    12, 13, 14,  8,
                     9, 10,  4,  5,
                     6,      0,  1,  2
                );

                unchecked
                {
                    const sbyte lInv  = (sbyte)0xFF;
                    s_sse_decodeLutLo = Sse2.SetVector128(
                        lInv, lInv, 0x2D, 0x30,
                        0x41, 0x50, 0x61, 0x70,
                        lInv, lInv, lInv, lInv,
                        lInv, lInv, lInv, lInv
                    );
                }

                const sbyte hInv  = 0x00;
                s_sse_decodeLutHi = Sse2.SetVector128(
                    hInv, hInv, 0x2D, 0x39,
                    0x4F, 0x5A, 0x6F, 0x7A,
                    hInv, hInv, hInv, hInv,
                    hInv, hInv, hInv, hInv
                );

                s_sse_decodeLutRoll = Sse2.SetVector128(
                           0x00,        0x00, 0x3E - 0x2D, 0x34 - 0x30,
                    0x00 - 0x41, 0x0F - 0x50, 0x1A - 0x61, 0x29 - 0x70,
                           0x00,        0x00,        0x00,        0x00,
                           0x00,        0x00,        0x00,        0x00
                );

                s_sse_decodeLutLo   = Reverse(s_sse_decodeLutLo);
                s_sse_decodeLutHi   = Reverse(s_sse_decodeLutHi);
                s_sse_decodeLutRoll = Reverse(s_sse_decodeLutRoll);

                s_sse_decodeMask2F = Sse2.SetAllVector128((sbyte)0x2F); // ASCII: /
            }
#endif
        }
        //---------------------------------------------------------------------
#if NETCOREAPP
        private static Vector128<sbyte> Reverse(Vector128<sbyte> vec)
        {
            Vector128<sbyte> mask = Sse2.SetVector128(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15);
            return Ssse3.Shuffle(vec, mask);
        }
#endif
        //---------------------------------------------------------------------
        private const byte EncodingPad = (byte)'=';     // '=', for padding
    }
}
