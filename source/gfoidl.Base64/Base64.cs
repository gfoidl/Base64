namespace gfoidl.Base64
{
    public static class Base64
    {
        private static readonly IBase64Encoder _default = new Base64Encoder();
        private static readonly IBase64Encoder _url     = new Base64UrlEncoder();
        //---------------------------------------------------------------------
        public static IBase64Encoder Default => _default;
        public static IBase64Encoder Url     => _url;
    }
}
