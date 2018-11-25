using System.Threading;

namespace gfoidl.Base64
{
    /// <summary>
    /// Base64 encoding / decoding.
    /// </summary>
    public static class Base64
    {
        private static IBase64Encoder _default;
        private static IBase64Encoder _url;
        //---------------------------------------------------------------------
        /// <summary>
        /// The base64 encoder / decoder.
        /// </summary>
        public static IBase64Encoder Default => _default
            ?? Interlocked.CompareExchange(ref _default, new Base64Encoder(), null)
            ?? _default;

        /// <summary>
        /// The base64Url encoder / decoder.
        /// </summary>
        public static IBase64Encoder Url => _url
            ?? Interlocked.CompareExchange(ref _url, new Base64UrlEncoder(), null)
            ?? _url;
    }
}
