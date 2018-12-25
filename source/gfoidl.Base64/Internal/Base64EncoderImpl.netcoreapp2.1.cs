using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace gfoidl.Base64.Internal
{
    partial class Base64EncoderImpl
    {
        protected static readonly Vector128<sbyte> s_sse_encodeShuffleVec;
        protected static readonly Vector128<sbyte> s_sse_decodeShuffleVec;
        //---------------------------------------------------------------------
        static Base64EncoderImpl()
        {
            if (Sse2.IsSupported && Ssse3.IsSupported)
            {
                s_sse_encodeShuffleVec = Sse2.SetVector128(
                    10, 11, 9, 10,
                     7,  8, 6,  7,
                     4,  5, 3,  4,
                     1,  2, 0,  1
                );

                s_sse_decodeShuffleVec = Sse2.SetVector128(
                    -1, -1, -1, -1,
                    12, 13, 14,  8,
                     9, 10,  4,  5,
                     6,  0,  1,  2
                );
            }
        }
    }
}
