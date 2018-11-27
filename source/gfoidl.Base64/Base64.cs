using System.Threading;

namespace gfoidl.Base64
{
    /// <summary>
    /// Base64 encoding / decoding.
    /// </summary>
    public static class Base64
    {
        private static IBase64 _default;
        private static IBase64 _url;
        //---------------------------------------------------------------------
        /// <summary>
        /// The base64 encoder / decoder.
        /// </summary>
        public static IBase64 Default => _default
            ?? Interlocked.CompareExchange(ref _default, new Base64Encoder(), null)
            ?? _default;

        /// <summary>
        /// The base64Url encoder / decoder.
        /// </summary>
        public static IBase64 Url => _url
            ?? Interlocked.CompareExchange(ref _url, new Base64UrlEncoder(), null)
            ?? _url;
    }
}
