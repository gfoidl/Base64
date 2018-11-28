#include "avx.h"
#include <immintrin.h>
//-----------------------------------------------------------------------------
__m256i dec_reshuffle(__m256i in)
{
    const __m256i merge_ab_and_bc = _mm256_maddubs_epi16(in, _mm256_set1_epi32(0x01400140));
    __m256i out                   = _mm256_madd_epi16(merge_ab_and_bc, _mm256_set1_epi32(0x00011000));

    out = _mm256_shuffle_epi8(out, _mm256_setr_epi8(
        2, 1, 0, 6, 5, 4, 10, 9, 8, 14, 13, 12, -1, -1, -1, -1,
        2, 1, 0, 6, 5, 4, 10, 9, 8, 14, 13, 12, -1, -1, -1, -1));

    return _mm256_permutevar8x32_epi32(out, _mm256_setr_epi32(0, 1, 2, 4, 5, 6, -1, -1));
}
//-----------------------------------------------------------------------------
void avx_decode(char* base64, char* data, size_t srcLen, size_t& written)
{
    //_mm256_zeroupper();

    const __m256i lut_lo = _mm256_setr_epi8(
        0x15, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11,
        0x11, 0x11, 0x13, 0x1A, 0x1B, 0x1B, 0x1B, 0x1A,
        0x15, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11,
        0x11, 0x11, 0x13, 0x1A, 0x1B, 0x1B, 0x1B, 0x1A);

    const __m256i lut_hi = _mm256_setr_epi8(
        0x10, 0x10, 0x01, 0x02, 0x04, 0x08, 0x04, 0x08,
        0x10, 0x10, 0x10, 0x10, 0x10, 0x10, 0x10, 0x10,
        0x10, 0x10, 0x01, 0x02, 0x04, 0x08, 0x04, 0x08,
        0x10, 0x10, 0x10, 0x10, 0x10, 0x10, 0x10, 0x10);

    const __m256i lut_roll = _mm256_setr_epi8(
        0, 16, 19, 4, -65, -65, -71, -71,
        0, 0, 0, 0, 0, 0, 0, 0,
        0, 16, 19, 4, -65, -65, -71, -71,
        0, 0, 0, 0, 0, 0, 0, 0);

    const __m256i mask_2F = _mm256_set1_epi8(0x2f);

    size_t outl = 0;

    while (srcLen >= 45)
    {
        __m256i str = _mm256_loadu_si256(reinterpret_cast<__m256i*>(base64));

        // lookup
        const __m256i hi_nibbles = _mm256_and_si256(_mm256_srli_epi32(str, 4), mask_2F);
        const __m256i lo_nibbles = _mm256_and_si256(str, mask_2F);
        const __m256i hi         = _mm256_shuffle_epi8(lut_hi, hi_nibbles);
        const __m256i lo         = _mm256_shuffle_epi8(lut_lo, lo_nibbles);
        const __m256i eq_2F      = _mm256_cmpeq_epi8(str, mask_2F);
        const __m256i roll       = _mm256_shuffle_epi8(lut_roll, _mm256_add_epi8(eq_2F, hi_nibbles));

        if (!_mm256_testz_si256(lo, hi))
            break;

        // Now simply add the delta values to the input:
        str = _mm256_add_epi8(str, roll);

        // Reshuffle the input to packed 12-byte output format:
        str = dec_reshuffle(str);

        _mm256_storeu_si256(reinterpret_cast<__m256i*>(data), str);

        base64 += 32;
        data   += 24;
        srcLen -= 32;
        outl   += 24;
    }

    written = outl;
}
