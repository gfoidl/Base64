using System;
using System.Buffers;
using System.Runtime.InteropServices;
using gfoidl.Base64.Internal;

namespace gfoidl.Base64
{
    /// <summary>
    /// Base64 encoding / decoding.
    /// </summary>
    public abstract class Base64 : IBase64
    {
        private static readonly Base64Encoder    s_default = new Base64Encoder();
        private static readonly Base64UrlEncoder s_url     = new Base64UrlEncoder();
        //---------------------------------------------------------------------
        /// <summary>
        /// The base64 encoder / decoder.
        /// </summary>
        public static Base64Encoder Default => s_default;
        //---------------------------------------------------------------------
        /// <summary>
        /// The base64Url encoder / decoder.
        /// </summary>
        public static Base64UrlEncoder Url => s_url;
        //---------------------------------------------------------------------
        /// <summary>
        /// Gets the length of the encoded data.
        /// </summary>
        /// <param name="sourceLength">The length of the data.</param>
        /// <returns>The base64 encoded length of <paramref name="sourceLength" />.</returns>
        public abstract int GetEncodedLength(int sourceLength);
        //---------------------------------------------------------------------
        /// <summary>
        /// Gets the maximum length of the decoded data. 
        /// The result may not be the exact length due to padding. 
        /// Use <see cref="GetDecodedLength(ReadOnlySpan{byte})" /> or <see cref="GetDecodedLength(ReadOnlySpan{char})" /> 
        /// for an accurate length.
        /// </summary>     
        /// <param name="encodedLength">The length of the encoded data.</param>
        /// <returns>The maximum base64 decoded length of <paramref name="encodedLength" />.</returns>
        /// <remarks>
        /// This method can be used for buffer-chains, to get the size which is at least
        /// required for decoding.
        /// </remarks>
        public abstract int GetMaxDecodedLength(int encodedLength);
        //---------------------------------------------------------------------
        /// <summary>
        /// Gets the length of the decoded data.
        /// </summary>
        /// <param name="encoded">The encoded data.</param>
        /// <returns>The base64 decoded length of <paramref name="encoded" />. Any padding is handled.</returns>
        public abstract int GetDecodedLength(ReadOnlySpan<byte> encoded);
        //---------------------------------------------------------------------
        /// <summary>
        /// Gets the length of the decoded data.
        /// </summary>
        /// <param name="encoded">The encoded data.</param>
        /// <returns>The base64 decoded length of <paramref name="encoded" />. Any padding is handled.</returns>
        public abstract int GetDecodedLength(ReadOnlySpan<char> encoded);
        //---------------------------------------------------------------------
        /// <summary>
        /// Base64 encodes <paramref name="data" />.
        /// </summary>
        /// <param name="data">The data to be base64 encoded.</param>
        /// <param name="encoded">The base64 encoded data.</param>
        /// <param name="consumed">
        /// The number of input bytes consumed during the operation. This can be used to slice the input for 
        /// subsequent calls, if necessary.
        /// </param>
        /// <param name="written">
        /// The number of bytes written into the output span. This can be used to slice the output for 
        /// subsequent calls, if necessary.
        /// </param>
        /// <param name="isFinalBlock">
        /// <c>true</c> (default) when the input span contains the entire data to decode.
        /// Set to <c>false</c> only if it is known that the input span contains partial data with more data to follow.
        /// </param>
        /// <returns>
        /// It returns the OperationStatus enum values:
        /// <list type="bullet">
        /// <item><description>Done - on successful processing of the entire input span</description></item>
        /// <item><description>DestinationTooSmall - if there is not enough space in the output span to fit the decoded input</description></item>
        /// <item><description>
        /// NeedMoreData - only if isFinalBlock is false and the input is not a multiple of 4, otherwise the partial input 
        /// would be considered as InvalidData
        /// </description></item>
        /// <item><description>
        /// InvalidData - if the input contains bytes outside of the expected base64 range, or if it contains invalid/more 
        /// than two padding characters, or if the input is incomplete (i.e. not a multiple of 4) and isFinalBlock is true.
        /// </description></item>
        /// </list>
        /// </returns>
        public abstract OperationStatus Encode(
            ReadOnlySpan<byte> data,
            Span<byte>         encoded,
            out                int consumed,
            out                int written,
            bool               isFinalBlock = true);
        //---------------------------------------------------------------------
        /// <summary>
        /// Base64 encodes <paramref name="data" />.
        /// </summary>
        /// <param name="data">The data to be base64 encoded.</param>
        /// <param name="encoded">The base64 encoded data.</param>
        /// <param name="consumed">
        /// The number of input bytes consumed during the operation. This can be used to slice the input for 
        /// subsequent calls, if necessary.
        /// </param>
        /// <param name="written">
        /// The number of chars written into the output span. This can be used to slice the output for 
        /// subsequent calls, if necessary.
        /// </param>
        /// <param name="isFinalBlock">
        /// <c>true</c> (default) when the input span contains the entire data to decode.
        /// Set to <c>false</c> only if it is known that the input span contains partial data with more data to follow.
        /// </param>
        /// <returns>
        /// It returns the OperationStatus enum values:
        /// <list type="bullet">
        /// <item><description>Done - on successful processing of the entire input span</description></item>
        /// <item><description>DestinationTooSmall - if there is not enough space in the output span to fit the decoded input</description></item>
        /// <item><description>
        /// NeedMoreData - only if isFinalBlock is false and the input is not a multiple of 4, otherwise the partial input 
        /// would be considered as InvalidData
        /// </description></item>
        /// <item><description>
        /// InvalidData - if the input contains bytes outside of the expected base64 range, or if it contains invalid/more 
        /// than two padding characters, or if the input is incomplete (i.e. not a multiple of 4) and isFinalBlock is true.
        /// </description></item>
        /// </list>
        /// </returns>
        public abstract OperationStatus Encode(
            ReadOnlySpan<byte> data,
            Span<char>         encoded,
            out                int consumed,
            out                int written,
            bool               isFinalBlock = true);
        //---------------------------------------------------------------------
        /// <summary>
        /// Base64 decodes <paramref name="encoded" />.
        /// </summary>
        /// <param name="encoded">The base64 encoded data.</param>
        /// <param name="data">The base64 decoded data.</param>
        /// <param name="consumed">
        /// The number of input bytes consumed during the operation. This can be used to slice the input for 
        /// subsequent calls, if necessary.
        /// </param>
        /// <param name="written">
        /// The number of bytes written into the output span. This can be used to slice the output for 
        /// subsequent calls, if necessary.
        /// </param>
        /// <param name="isFinalBlock">
        /// <c>true</c> (default) when the input span contains the entire data to decode.
        /// Set to <c>false</c> only if it is known that the input span contains partial data with more data to follow.
        /// </param>
        /// <returns>
        /// It returns the OperationStatus enum values:
        /// <list type="bullet">
        /// <item><description>Done - on successful processing of the entire input span</description></item>
        /// <item><description>DestinationTooSmall - if there is not enough space in the output span to fit the decoded input</description></item>
        /// <item><description>
        /// NeedMoreData - only if isFinalBlock is false and the input is not a multiple of 4, otherwise the partial input 
        /// would be considered as InvalidData
        /// </description></item>
        /// <item><description>
        /// InvalidData - if the input contains bytes outside of the expected base64 range, or if it contains invalid/more 
        /// than two padding characters, or if the input is incomplete (i.e. not a multiple of 4) and isFinalBlock is true.
        /// </description></item>
        /// </list>
        /// </returns>
        public abstract OperationStatus Decode(
            ReadOnlySpan<byte> encoded,
            Span<byte>         data,
            out                int consumed,
            out                int written,
            bool               isFinalBlock = true);
        //---------------------------------------------------------------------
        /// <summary>
        /// Base64 decodes <paramref name="encoded" />.
        /// </summary>
        /// <param name="encoded">The base64 encoded data.</param>
        /// <param name="data">The base64 decoded data.</param>
        /// <param name="consumed">
        /// The number of input chars consumed during the operation. This can be used to slice the input for 
        /// subsequent calls, if necessary.
        /// </param>
        /// <param name="written">
        /// The number of bytes written into the output span. This can be used to slice the output for 
        /// subsequent calls, if necessary.
        /// </param>
        /// <param name="isFinalBlock">
        /// <c>true</c> (default) when the input span contains the entire data to decode.
        /// Set to <c>false</c> only if it is known that the input span contains partial data with more data to follow.
        /// </param>
        /// <returns>
        /// It returns the OperationStatus enum values:
        /// <list type="bullet">
        /// <item><description>Done - on successful processing of the entire input span</description></item>
        /// <item><description>DestinationTooSmall - if there is not enough space in the output span to fit the decoded input</description></item>
        /// <item><description>
        /// NeedMoreData - only if isFinalBlock is false and the input is not a multiple of 4, otherwise the partial input 
        /// would be considered as InvalidData
        /// </description></item>
        /// <item><description>
        /// InvalidData - if the input contains chars outside of the expected base64 range, or if it contains invalid/more 
        /// than two padding characters, or if the input is incomplete (i.e. not a multiple of 4) and isFinalBlock is true.
        /// </description></item>
        /// </list>
        /// </returns>
        public abstract OperationStatus Decode(
            ReadOnlySpan<char> encoded,
            Span<byte>         data,
            out                int consumed,
            out                int written,
            bool               isFinalBlock = true);
        //---------------------------------------------------------------------
        /// <summary>
        /// Base64 encoded <paramref name="data" /> to a <see cref="string" />.
        /// </summary>
        /// <param name="data">The data to be base64 encoded.</param>
        /// <returns>The base64 encoded <see cref="string" />.</returns>
        public abstract string Encode(ReadOnlySpan<byte> data);
        //---------------------------------------------------------------------
        /// <summary>
        /// Base64 decodes <paramref name="encoded" /> into a <see cref="byte" /> array.
        /// </summary>
        /// <param name="encoded">The base64 encoded data in string-form.</param>
        /// <returns>The base64 decoded data.</returns>
        public abstract byte[] Decode(ReadOnlySpan<char> encoded);
        //---------------------------------------------------------------------
        /// <summary>
        /// Detects whether <paramref name="encoded" /> is base64 or base64Url.
        /// </summary>
        /// <param name="encoded">The base64 encoded data.</param>
        /// <param name="fast">
        /// When <c>false</c> (default) <paramref name="encoded" /> is scanned 
        /// one time for base64 chars and a second time for base64Url chars.
        /// So if there is a mix of them, <see cref="EncodingType.Unknown" />
        /// will be returned.
        /// <para>
        /// When <c>true</c> <paramref name="encoded" /> is scanned only once
        /// and for base64Url chars. So if there is a mix of base64 and base64Url,
        /// the result will be <see cref="EncodingType.Base64Url" />, and may
        /// throw a <see cref="FormatException" /> on decoding.
        /// </para>
        /// </param>
        /// <returns>base64 or base64Url</returns>
        /// <remarks>
        /// It is an O(n) scan / detection of the encoding type, and input is 
        /// not validated for conforming the base64 standard. Thus there is no
        /// 'Invalid' encoding type.
        /// </remarks>
        public static EncodingType DetectEncoding(ReadOnlySpan<byte> encoded, bool fast = false)
            => DetectEncoding<byte>(encoded, fast);
        //---------------------------------------------------------------------
        /// <summary>
        /// Detects whether <paramref name="encoded" /> is base64 or base64Url.
        /// </summary>
        /// <param name="encoded">The base64 encoded data.</param>
        /// <param name="fast">
        /// When <c>false</c> (default) <paramref name="encoded" /> is scanned 
        /// one time for base64 chars and a second time for base64Url chars.
        /// So if there is a mix of them, <see cref="EncodingType.Unknown" />
        /// will be returned.
        /// <para>
        /// When <c>true</c> <paramref name="encoded" /> is scanned only once
        /// and for base64Url chars. So if there is a mix of base64 and base64Url,
        /// the result will be <see cref="EncodingType.Base64Url" />, and may
        /// throw a <see cref="FormatException" /> on decoding.
        /// </para>
        /// </param>
        /// <returns>base64 or base64Url</returns>
        /// <remarks>
        /// It is an O(n) fast scan / detection of the encoding type, and input is 
        /// not validated for conforming the base64 standard. Thus there is no
        /// 'Invalid' encoding type.
        /// </remarks>
        public static EncodingType DetectEncoding(ReadOnlySpan<char> encoded, bool fast = false)
            => DetectEncoding<char>(encoded, fast);
        //---------------------------------------------------------------------
        // Also used for tests
        internal static EncodingType DetectEncoding<T>(ReadOnlySpan<T> encoded, bool fast = false)
            where T : IEquatable<T>
        {
            if (encoded.Length < 4) return EncodingType.Unknown;

            T plus, slash, minus, underscore;

            if (typeof(T) == typeof(byte))
            {
                plus       = (T)(object)(byte)'+';
                slash      = (T)(object)(byte)'/';
                minus      = (T)(object)(byte)'-';
                underscore = (T)(object)(byte)'_';
            }
            else if (typeof(T) == typeof(char))
            {
                plus       = (T)(object)'+';
                slash      = (T)(object)'/';
                minus      = (T)(object)'-';
                underscore = (T)(object)'_';
            }
            else
            {
                throw new NotSupportedException(); // just in case new types are introduced in the future
            }

            int indexBase64Url = encoded.LastIndexOfAny(minus, underscore);

            if (fast)
            {
                return indexBase64Url >= 0 ? EncodingType.Base64Url : EncodingType.Base64;
            }
            else
            {
                int indexBase64 = encoded.LastIndexOfAny(plus, slash);

                return indexBase64Url >= 0
                    ? indexBase64 >= 0
                        ? EncodingType.Unknown
                        : EncodingType.Base64Url
                    : EncodingType.Base64;
            }
        }
    }
}
