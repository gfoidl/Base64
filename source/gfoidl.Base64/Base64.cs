namespace gfoidl.Base64
{
    /// <summary>
    ///  Base64 encoding / decoding.
    /// </summary>
    public static class Base64
    {
        private static readonly IBase64Encoder _default = new Base64Encoder();
        private static readonly IBase64Encoder _url     = new Base64UrlEncoder();
        //---------------------------------------------------------------------
        /// <summary>
        /// The base64 encoder / decoder.
        /// </summary>
        public static IBase64Encoder Default => _default;

        /// <summary>
        /// The base64Url encoder / decoder.
        /// </summary>
        public static IBase64Encoder Url => _url;
    }
}
