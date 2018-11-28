#include "sse.h"
#include <immintrin.h>
//-----------------------------------------------------------------------------
#define CMPGT(s, n) _mm_cmpgt_epi8((s), _mm_set1_epi8(n))
//-----------------------------------------------------------------------------
__m128i dec_reshuffle(__m128i in)
{
    const __m128i merge_ab_and_bc = _mm_maddubs_epi16(in, _mm_set1_epi32(0x01400140));
    const __m128i out             = _mm_madd_epi16(merge_ab_and_bc, _mm_set1_epi32(0x00011000));
    
    return  _mm_shuffle_epi8(out, _mm_setr_epi8(
         2,  1,  0,
         6,  5,  4,
        10,  9,  8,
        14, 13, 12,
        -1, -1, -1, -1));
}
//-----------------------------------------------------------------------------
void sse_decode(char* base64, char* data, size_t srcLen, size_t& written)
{
    const __m128i lut_lo = _mm_setr_epi8(
        0x15, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11,
        0x11, 0x11, 0x13, 0x1A, 0x1B, 0x1B, 0x1B, 0x1A);

    const __m128i lut_hi = _mm_setr_epi8(
        0x10, 0x10, 0x01, 0x02, 0x04, 0x08, 0x04, 0x08,
        0x10, 0x10, 0x10, 0x10, 0x10, 0x10, 0x10, 0x10);

    const __m128i lut_roll = _mm_setr_epi8(
        0, 16, 19, 4, -65, -65, -71, -71,
        0, 0, 0, 0, 0, 0, 0, 0);

    const __m128i mask_2F = _mm_set1_epi8(0x2f);

    size_t outl = 0;

    // normally 24, but here 45 to make it comparable to the avx version
    while (srcLen >= 45)
    {
        __m128i str = _mm_loadu_si128(reinterpret_cast<__m128i*>(base64));      

        const __m128i hi_nibbles = _mm_and_si128(_mm_srli_epi32(str, 4), mask_2F);
        const __m128i lo_nibbles = _mm_and_si128(str, mask_2F);
        const __m128i hi         = _mm_shuffle_epi8(lut_hi, hi_nibbles);
        const __m128i lo         = _mm_shuffle_epi8(lut_lo, lo_nibbles);
        const __m128i eq_2F      = _mm_cmpeq_epi8(str, mask_2F);
        const __m128i roll       = _mm_shuffle_epi8(lut_roll, _mm_add_epi8(eq_2F, hi_nibbles));

        if (_mm_movemask_epi8(CMPGT(_mm_and_si128(lo, hi), 0)) != 0)
            break;

        str = _mm_add_epi8(str, roll);
        str = dec_reshuffle(str);

        _mm_storeu_si128(reinterpret_cast<__m128i*>(data), str);

        base64 += 16;
        data   += 12;
        srcLen -= 16;
        outl   += 12;
    }

    written = outl;
}
