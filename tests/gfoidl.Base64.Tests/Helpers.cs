using System;

namespace gfoidl.Base64.Tests
{
    internal static class Helpers
    {
        public static string ToBase64Url(this string base64) => base64.Replace('+', '-').Replace('/', '_').TrimEnd('=');
        //---------------------------------------------------------------------
        public static string FromBase64Url(this string base64Url)
        {
            int length    = base64Url.Length;
            int base64Len = length + GetPaddingCount(length);

            return base64Url.Replace('-', '+').Replace('_', '/').PadRight(base64Len, '=');
            //-----------------------------------------------------------------
            static int GetPaddingCount(int length) => (length % 4) switch
            {
                0 => 0,
                2 => 2,
                3 => 1,
                _ => throw new FormatException("should not be here")
            };
        }
    }
}
