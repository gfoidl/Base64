#if NETCOREAPP
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
#endif

namespace gfoidl.Base64
{
    public partial class Base64Encoder : IBase64Encoder
    {
        private static readonly bool s_isMac = false;
        //---------------------------------------------------------------------
        static Base64Encoder()
        {
#if NETCOREAPP
            s_isMac = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

#if NETCOREAPP3_0
            if (Ssse3.IsSupported)
#else
            if (Sse2.IsSupported && Ssse3.IsSupported)
#endif
            {
                s_sse_encodeShuffleVec = Sse2.SetVector128(
                    10, 11,  9, 10,
                     7,  8,  6,  7,
                     4,  5,  3,  4,
                     1,  2,  0,  1
                );

                s_sse_encodeLut = Sse2.SetVector128(
                     0,  0, -16, -19,
                    -4, -4,  -4,  -4,
                    -4, -4,  -4,  -4,
                    -4, -4,  71,  65
                );

                s_sse_decodeShuffleVec = Sse2.SetVector128(
                    -1, -1, -1, -1,
                    12, 13, 14,  8,
                     9, 10,  4,  5,
                     6,  0,  1,  2
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

                s_sse_decodeLutRoll = Sse2.SetVector128(
                      0,   0,   0,   0,
                      0,   0,   0,   0,
                    -71, -71, -65, -65,
                      4,  19,  16,   0
                );

                s_sse_decodeMask2F = Sse2.SetAllVector128((sbyte)0x2F); // ASCII: /
            }

#if NETCOREAPP3_0
            if (Avx2.IsSupported)
#else
            if (Avx.IsSupported && Avx2.IsSupported)
#endif
            {
                s_avx_encodePermuteVec = Avx.SetVector256(6, 5, 4, 3, 2, 1, 0, 0);

                s_avx_encodeShuffleVec = Avx.SetVector256(
                    10, 11,  9, 10,
                     7,  8,  6,  7,
                     4,  5,  3,  4,
                     1,  2,  0,  1,
                    14, 15, 13, 14,
                    11, 12, 10, 11,
                     8,  9,  7,  8,
                     5,  6,  4,  5
                );

                s_avx_encodeLut = Avx.SetVector256(
                     0,  0, -16, -19,
                    -4, -4,  -4,  -4,
                    -4, -4,  -4,  -4,
                    -4, -4,  71,  65,
                     0,  0, -16, -19,
                    -4, -4,  -4,  -4,
                    -4, -4,  -4,  -4,
                    -4, -4,  71, 65
                );

                s_avx_decodeShuffleVec = Avx.SetVector256(
                    -1, -1, -1, -1,
                    12, 13, 14,  8,
                     9, 10,  4,  5,
                     6,  0,  1,  2,
                    -1, -1, -1, -1,
                    12, 13, 14,  8,
                     9, 10,  4,  5,
                     6,  0,  1,  2
                );

                s_avx_decodeLutLo = Avx.SetVector256(
                    0x1A, 0x1B, 0x1B, 0x1B,
                    0x1A, 0x13, 0x11, 0x11,
                    0x11, 0x11, 0x11, 0x11,
                    0x11, 0x11, 0x11, 0x15,
                    0x1A, 0x1B, 0x1B, 0x1B,
                    0x1A, 0x13, 0x11, 0x11,
                    0x11, 0x11, 0x11, 0x11,
                    0x11, 0x11, 0x11, 0x15
                );

                s_avx_decodeLutHi = Avx.SetVector256(
                    0x10, 0x10, 0x10, 0x10,
                    0x10, 0x10, 0x10, 0x10,
                    0x08, 0x04, 0x08, 0x04,
                    0x02, 0x01, 0x10, 0x10,
                    0x10, 0x10, 0x10, 0x10,
                    0x10, 0x10, 0x10, 0x10,
                    0x08, 0x04, 0x08, 0x04,
                    0x02, 0x01, 0x10, 0x10
                );

                s_avx_decodeLutRoll = Avx.SetVector256(
                      0,   0,   0,   0,
                      0,   0,   0,   0,
                    -71, -71, -65, -65,
                      4,  19,  16,   0,
                      0,   0,   0,   0,
                      0,   0,   0,   0,
                    -71, -71, -65, -65,
                      4,  19,  16,   0
                );

                s_avx_decodeMask2F = Avx.SetAllVector256((sbyte)0x2F);     // ASCII: /

                s_avx_decodePermuteVec = Avx.SetVector256(-1, -1, 6, 5, 4, 2, 1, 0);
            }
#endif
        }
        //---------------------------------------------------------------------
        private const byte EncodingPad = (byte)'=';     // '=', for padding
    }
}
