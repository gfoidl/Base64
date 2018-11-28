namespace gfoidl.Base64.Tests.Internal.Base64UrlEncoderTests
{
    internal static class Extensions
    {
        public static string ToBase64Url(this string base64) => base64.Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }
}
