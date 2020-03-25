using System;
using System.Buffers;
using gfoidl.Base64.Internal;

namespace gfoidl.Base64
{
    /// <summary>
    /// Base64 encoding / decoding.
    /// </summary>
    public abstract class Base64 : IBase64
    {
        /// <summary>
        /// The maximum length that can be encoded.
        /// </summary>
        /// <remarks>
        /// If you need to encode data that is larger than <see cref="MaximumEncodeLength" />,
        /// you can <see cref="Span{T}.Slice(int, int)" /> the data and encode with
        /// buffer chains.
        /// </remarks>
        public const int MaximumEncodeLength = int.MaxValue / 4 * 3; // 1610612733
        //---------------------------------------------------------------------
        private static readonly Base64Encoder    s_default = new Base64Encoder();
        private static readonly Base64UrlEncoder s_url     = new Base64UrlEncoder();
        //---------------------------------------------------------------------
#pragma warning disable RCS1085 // Use auto-implemented property.
#pragma warning disable PUB0001 // Pubternal type in public API
        /// <summary>
        /// The base64 encoder / decoder.
        /// </summary>
        public static Base64Encoder Default => s_default;
        //---------------------------------------------------------------------
        /// <summary>
        /// The base64Url encoder / decoder.
        /// </summary>
        public static Base64UrlEncoder Url => s_url;
#pragma warning restore PUB0001 // Pubternal type in public API
#pragma warning restore RCS1085 // Use auto-implemented property.
        //---------------------------------------------------------------------
        /// <inheritdoc />
        public abstract int GetEncodedLength(int sourceLength);
        //---------------------------------------------------------------------
        /// <inheritdoc />
        public abstract int GetMaxDecodedLength(int encodedLength);
        //---------------------------------------------------------------------
        /// <inheritdoc />
        public abstract int GetDecodedLength(ReadOnlySpan<byte> encoded);
        //---------------------------------------------------------------------
        /// <inheritdoc />
        public abstract int GetDecodedLength(ReadOnlySpan<char> encoded);
        //---------------------------------------------------------------------
        /// <inheritdoc />
        public abstract OperationStatus Encode(
            ReadOnlySpan<byte> data,
            Span<byte>         encoded,
            out                int consumed,
            out                int written,
            bool               isFinalBlock = true);
        //---------------------------------------------------------------------
        /// <inheritdoc />
        public abstract OperationStatus Encode(
            ReadOnlySpan<byte> data,
            Span<char>         encoded,
            out                int consumed,
            out                int written,
            bool               isFinalBlock = true);
        //---------------------------------------------------------------------
        /// <inheritdoc />
        public abstract OperationStatus Decode(
            ReadOnlySpan<byte> encoded,
            Span<byte>         data,
            out                int consumed,
            out                int written,
            bool               isFinalBlock = true);
        //---------------------------------------------------------------------
        /// <inheritdoc />
        public abstract OperationStatus Decode(
            ReadOnlySpan<char> encoded,
            Span<byte>         data,
            out                int consumed,
            out                int written,
            bool               isFinalBlock = true);
        //---------------------------------------------------------------------
        /// <inheritdoc />
        public abstract string Encode(ReadOnlySpan<byte> data);
        //---------------------------------------------------------------------
        /// <inheritdoc />
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
