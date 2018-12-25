using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace gfoidl.Base64.Internal
{
    partial class Base64Encoder
    {
        static Base64Encoder()
        {
            if (Ssse3.IsSupported)
            {
                s_sse_encodeLut = Vector128.Create(
                     65,  71, -4, -4,
                     -4,  -4, -4, -4,
                     -4,  -4, -4, -4,
                    -19, -16,  0,  0
                );

                s_sse_decodeLutLo = Vector128.Create(
                    0x15, 0x11, 0x11, 0x11,
                    0x11, 0x11, 0x11, 0x11,
                    0x11, 0x11, 0x13, 0x1A,
                    0x1B, 0x1B, 0x1B, 0x1A
                );

                s_sse_decodeLutHi = Vector128.Create(
                    0x10, 0x10, 0x01, 0x02,
                    0x04, 0x08, 0x04, 0x08,
                    0x10, 0x10, 0x10, 0x10,
                    0x10, 0x10, 0x10, 0x10
                );

                s_sse_decodeLutShift = Vector128.Create(
                      0,  16,  19,   4,
                    -65, -65, -71, -71,
                      0,   0,   0,   0,
                      0,   0,   0,   0
                );

                s_sse_decodeMask2F = Vector128.Create((sbyte)0x2F);     // ASCII: /
            }

            if (Avx2.IsSupported)
            {
                s_avx_encodeLut = Vector256.Create(
                     65,  71, -4, -4,
                     -4,  -4, -4, -4,
                     -4,  -4, -4, -4,
                    -19, -16,  0,  0,
                     65,  71, -4, -4,
                     -4,  -4, -4, -4,
                     -4,  -4, -4, -4,
                    -19, -16,  0,  0
                );

                s_avx_decodeLutLo = Vector256.Create(
                    0x15, 0x11, 0x11, 0x11,
                    0x11, 0x11, 0x11, 0x11,
                    0x11, 0x11, 0x13, 0x1A,
                    0x1B, 0x1B, 0x1B, 0x1A,
                    0x15, 0x11, 0x11, 0x11,
                    0x11, 0x11, 0x11, 0x11,
                    0x11, 0x11, 0x13, 0x1A,
                    0x1B, 0x1B, 0x1B, 0x1A
                );

                s_avx_decodeLutHi = Vector256.Create(
                    0x10, 0x10, 0x01, 0x02,
                    0x04, 0x08, 0x04, 0x08,
                    0x10, 0x10, 0x10, 0x10,
                    0x10, 0x10, 0x10, 0x10,
                    0x10, 0x10, 0x01, 0x02,
                    0x04, 0x08, 0x04, 0x08,
                    0x10, 0x10, 0x10, 0x10,
                    0x10, 0x10, 0x10, 0x10
                );

                s_avx_decodeLutShift = Vector256.Create(
                     0,  16,  19,   4,
                    -65, -65, -71, -71,
                      0,   0,   0,   0,
                      0,   0,   0,   0,
                      0,  16,  19,   4,
                    -65, -65, -71, -71,
                      0,   0,   0,   0,
                      0,   0,   0,   0
                );

                s_avx_decodeMask2F = Vector256.Create((sbyte)0x2F);    // ASCII: /
            }
        }
    }
}
