using System;

namespace gfoidl.Base64.Internal
{
    partial class Base64EncoderImpl
    {
        protected static ReadOnlySpan<sbyte> s_sseEncodeShuffleVec => new sbyte[16]
        {
             1,  0,  2,  1,
             4,  3,  5,  4,
             7,  6,  8,  7,
            10,  9, 11, 10
        };

        protected static ReadOnlySpan<sbyte> s_sseDecodeShuffleVec => new sbyte[16]
        {
              2,  1,  0,  6,
              5,  4, 10,  9,
              8, 14, 13, 12,
             -1, -1, -1, -1
        };
    }
}
