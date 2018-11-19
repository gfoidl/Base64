using System.Runtime.Intrinsics.X86;

namespace gfoidl.Base64
{
    public partial class Base64Encoder : IBase64Encoder
    {
        static Base64Encoder()
        {
            if (Sse2.IsSupported && Ssse3.IsSupported)
            {
                s_encodeShuffleVec = Sse2.SetVector128(
                    10, 11, 9, 10,
                    7, 8, 6, 7,
                    4, 5, 3, 4,
                    1, 2, 0, 1
                );

                s_encodeLut = Sse2.SetVector128(
                    0, 0, -16, -19,
                    -4, -4, -4, -4,
                    -4, -4, -4, -4,
                    -4, -4, 71, 65
                );

                s_decodeShuffleVec = Sse2.SetVector128(
                    -1, -1, -1, -1,
                    12, 13, 14,
                    8, 9, 10,
                    4, 5, 6,
                    0, 1, 2
                );

                s_decodeLutLo = Sse2.SetVector128(
                    0x1A, 0x1B, 0x1B, 0x1B, 0x1A, 0x13, 0x11, 0x11,
                    0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x15
                );

                s_decodeLutHi = Sse2.SetVector128(
                    0x10, 0x10, 0x10, 0x10, 0x10, 0x10, 0x10, 0x10,
                    0x08, 0x04, 0x08, 0x04, 0x02, 0x01, 0x10, 0x10
                );

                s_decodeLutRoll = Sse2.SetVector128(
                    0, 0, 0, 0, 0, 0, 0, 0,
                    -71, -71, -65, -65, 4, 19, 16, 0
                );

                s_decodeMask2F = Sse2.SetAllVector128((sbyte)0x2F); // ASCII: /
            }
        }
        //---------------------------------------------------------------------
        private const byte EncodingPad = (byte)'=';     // '=', for padding
    }
}
