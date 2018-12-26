using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace gfoidl.Base64.Internal
{
    partial class Base64EncoderImpl
    {
        protected static readonly Vector128<sbyte> s_sse_encodeShuffleVec;
        protected static readonly Vector128<sbyte> s_sse_decodeShuffleVec;

        protected static readonly Vector256<int>   s_avx_encodePermuteVec;
        protected static readonly Vector256<sbyte> s_avx_encodeShuffleVec;
        protected static readonly Vector256<sbyte> s_avx_decodeShuffleVec;
        protected static readonly Vector256<int>   s_avx_decodePermuteVec;
        //---------------------------------------------------------------------
        static Base64EncoderImpl()
        {
            if (Ssse3.IsSupported)
            {
                s_sse_encodeShuffleVec = Vector128.Create(
                     1,  0,  2,  1,
                     4,  3,  5,  4,
                     7,  6,  8,  7,
                    10,  9, 11, 10
                );

                s_sse_decodeShuffleVec = Vector128.Create(
                     2,  1,  0,  6,
                     5,  4, 10,  9,
                     8, 14, 13, 12,
                    -1, -1, -1, -1
                );
            }

            if (Avx2.IsSupported)
            {
                s_avx_encodePermuteVec = Vector256.Create(0, 0, 1, 2, 3, 4, 5, 6);

                s_avx_encodeShuffleVec = Vector256.Create(
                     5,  4,  6,  5,
                     8,  7,  9,  8,
                    11, 10, 12, 11,
                    14, 13, 15, 14,
                     1,  0,  2,  1,
                     4,  3,  5,  4,
                     7,  6,  8,  7,
                    10,  9, 11, 10
                );

                s_avx_decodeShuffleVec = Vector256.Create(
                     2,  1,  0,  6,
                     5,  4, 10,  9,
                     8, 14, 13, 12,
                    -1, -1, -1, -1,
                     2,  1,  0,  6,
                     5,  4, 10,  9,
                     8, 14, 13, 12,
                    -1, -1, -1, -1
                );

                s_avx_decodePermuteVec = Vector256.Create(0, 1, 2, 4, 5, 6, -1, -1);
            }
        }
    }
}
