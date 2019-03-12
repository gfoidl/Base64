using System;

namespace gfoidl.Base64.Internal
{
    partial class Base64EncoderImpl
    {
        // Originally this is of type int, but ROSpan for static data can handly (s)byte only
        // due to endianess concerns.
        protected static ReadOnlySpan<sbyte> s_avxEncodePermuteVec => new sbyte[]
        {
            0, 0, 0, 0,
            0, 0, 0, 0,
            1, 0, 0, 0,
            2, 0, 0, 0,
            3, 0, 0, 0,
            4, 0, 0, 0,
            5, 0, 0, 0,
            6, 0, 0, 0
        };

        protected static ReadOnlySpan<sbyte> s_avxEncodeShuffleVec => new sbyte[]
        {
            5,  4,  6,  5,
            8,  7,  9,  8,
           11, 10, 12, 11,
           14, 13, 15, 14,
            1,  0,  2,  1,
            4,  3,  5,  4,
            7,  6,  8,  7,
           10,  9, 11, 10
        };

        protected static ReadOnlySpan<sbyte> s_avxDecodeShuffleVec => new sbyte[]
        {
             2,  1,  0,  6,
             5,  4, 10,  9,
             8, 14, 13, 12,
            -1, -1, -1, -1,
             2,  1,  0,  6,
             5,  4, 10,  9,
             8, 14, 13, 12,
            -1, -1, -1, -1
        };

        // Originally this is of type int, but ROSpan for static data can handly (s)byte only
        // due to endianess concerns.
        protected static ReadOnlySpan<sbyte> s_avxDecodePermuteVec => new sbyte[]
        {
             0,  0,  0,  0,
             1,  0,  0,  0,
             2,  0,  0,  0,
             4,  0,  0,  0,
             5,  0,  0,  0,
             6,  0,  0,  0,
            -1, -1, -1, -1,
            -1, -1, -1, -1
        };
    }
}
