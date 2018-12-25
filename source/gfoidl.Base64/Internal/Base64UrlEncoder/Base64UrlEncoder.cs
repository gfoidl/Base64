#if NETCOREAPP
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
#endif

namespace gfoidl.Base64.Internal
{
    public sealed partial class Base64UrlEncoder : Base64EncoderImpl
    {
        static Base64UrlEncoder()
        {
#if NETCOREAPP && !NETCOREAPP3_0
            if (Sse2.IsSupported && Ssse3.IsSupported)
            {
                s_sse_encodeLut = Sse2.SetVector128(
                     0,  0, 32, -17,
                    -4, -4, -4,  -4,
                    -4, -4, -4,  -4,
                    -4, -4, 71,  65
                );

                unchecked
                {
                    const sbyte lInv  = (sbyte)0xFF;
                    s_sse_decodeLutLo = Sse2.SetVector128(
                        lInv, lInv, lInv, lInv,
                        lInv, lInv, lInv, lInv,
                        0x70, 0x61, 0x50, 0x41,
                        0x30, 0x2D, lInv, lInv
                    );
                }

                const sbyte hInv  = 0x00;
                s_sse_decodeLutHi = Sse2.SetVector128(
                    hInv, hInv, hInv, hInv,
                    hInv, hInv, hInv, hInv,
                    0x7A, 0x6F, 0x5A, 0x4F,
                    0x39, 0x2D, hInv, hInv
                );

                s_sse_decodeLutShift = Sse2.SetVector128(
                    0,   0,   0,   0,
                    0,   0,   0,   0,
                  -71, -71, -65, -65,
                    4,  17,   0,   0
                );

                s_sse_decodeMask5F = Sse2.SetAllVector128((sbyte)0x5F); // ASCII: _
            }
#endif
#if NETCOREAPP3_0
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
#endif
        }
    }
}
