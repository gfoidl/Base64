using System.Runtime.Intrinsics.X86;

namespace gfoidl.Base64.Internal
{
    partial class Base64UrlEncoder
    {
        static Base64UrlEncoder()
        {
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
        }
    }
}
