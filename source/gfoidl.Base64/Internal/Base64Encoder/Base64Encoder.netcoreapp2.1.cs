using System.Runtime.Intrinsics.X86;

namespace gfoidl.Base64.Internal
{
    partial class Base64Encoder
    {
        static Base64Encoder()
        {
            if (Sse2.IsSupported && Ssse3.IsSupported)
            {
                s_sse_encodeLut = Sse2.SetVector128(
                     0,  0, -16, -19,
                    -4, -4,  -4,  -4,
                    -4, -4,  -4,  -4,
                    -4, -4,  71,  65
                );

                s_sse_decodeLutLo = Sse2.SetVector128(
                    0x1A, 0x1B, 0x1B, 0x1B,
                    0x1A, 0x13, 0x11, 0x11,
                    0x11, 0x11, 0x11, 0x11,
                    0x11, 0x11, 0x11, 0x15
                );

                s_sse_decodeLutHi = Sse2.SetVector128(
                    0x10, 0x10, 0x10, 0x10,
                    0x10, 0x10, 0x10, 0x10,
                    0x08, 0x04, 0x08, 0x04,
                    0x02, 0x01, 0x10, 0x10
                );

                s_sse_decodeLutShift = Sse2.SetVector128(
                      0,   0,   0,   0,
                      0,   0,   0,   0,
                    -71, -71, -65, -65,
                      4,  19,  16,   0
                );

                s_sse_decodeMask2F = Sse2.SetAllVector128((sbyte)0x2F); // ASCII: /
            }
        }
    }
}
