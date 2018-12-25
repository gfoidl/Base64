using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace gfoidl.Base64.Internal
{
    partial class Base64UrlEncoder
    {
        static Base64UrlEncoder()
        {
            if (Ssse3.IsSupported)
            {
                s_sse_encodeLut = Vector128.Create(
                     65, 71, -4, -4,
                     -4, -4, -4, -4,
                     -4, -4, -4, -4,
                    -17, 32,  0,  0
                );

                unchecked
                {
                    const sbyte lInv  = (sbyte)0xFF;
                    s_sse_decodeLutLo = Vector128.Create(
                        lInv, lInv, 0x2D, 0x30,
                        0x41, 0x50, 0x61, 0x70,
                        lInv, lInv, lInv, lInv,
                        lInv, lInv, lInv, lInv
                    );
                }

                const sbyte hInv  = 0x00;
                s_sse_decodeLutHi = Vector128.Create(
                    hInv, hInv, 0x2D, 0x39,
                    0x4F, 0x5A, 0x6F, 0x7A,
                    hInv, hInv, hInv, hInv,
                    hInv, hInv, hInv, hInv
                );

                s_sse_decodeLutShift = Vector128.Create(
                      0,   0,  17,   4,
                    -65, -65, -71, -71,
                      0,   0,   0,   0,
                      0,   0,   0,   0
                );

                s_sse_decodeMask5F = Vector128.Create((sbyte)0x5F); // ASCII: _
            }

            if (Avx2.IsSupported)
            {
                s_avx_encodeLut = Vector256.Create(
                     65, 71, -4, -4,
                     -4, -4, -4, -4,
                     -4, -4, -4, -4,
                    -17, 32,  0,  0,
                     65, 71, -4, -4,
                     -4, -4, -4, -4,
                     -4, -4, -4, -4,
                    -17, 32,  0,  0
                );

                unchecked
                {
                    const sbyte lInv = (sbyte)0xFF;
                    s_avx_decodeLutLo = Vector256.Create(
                        lInv, lInv, 0x2D, 0x30,
                        0x41, 0x50, 0x61, 0x70,
                        lInv, lInv, lInv, lInv,
                        lInv, lInv, lInv, lInv,
                        lInv, lInv, 0x2D, 0x30,
                        0x41, 0x50, 0x61, 0x70,
                        lInv, lInv, lInv, lInv,
                        lInv, lInv, lInv, lInv
                    );
                }

                const sbyte hInv = 0x00;
                s_avx_decodeLutHi = Vector256.Create(
                    hInv, hInv, 0x2D, 0x39,
                    0x4F, 0x5A, 0x6F, 0x7A,
                    hInv, hInv, hInv, hInv,
                    hInv, hInv, hInv, hInv,
                    hInv, hInv, 0x2D, 0x39,
                    0x4F, 0x5A, 0x6F, 0x7A,
                    hInv, hInv, hInv, hInv,
                    hInv, hInv, hInv, hInv
                );

                s_avx_decodeLutShift = Vector256.Create(
                      0,   0,  17,   4,
                    -65, -65, -71, -71,
                      0,   0,   0,   0,
                      0,   0,   0,   0,
                      0,   0,  17,   4,
                    -65, -65, -71, -71,
                      0,   0,   0,   0,
                      0,   0,   0,   0
                );

                s_avx_decodeMask5F = Vector256.Create((sbyte)0x5F);    // ASCII: _
            }
        }
    }
}
