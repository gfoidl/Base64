using System;
using NUnit.Framework;

namespace gfoidl.Base64.Tests.Base64Tests
{
    [TestFixture]
    public class DetectEncoding
    {
        [Test]
        public void Base64_given_byte___OK()
        {
            byte[] base64 = { (byte)'a', (byte)'+', (byte)'b', (byte)'/' };

            EncodingType actual = Base64.DetectEncoding(base64);

            Assert.AreEqual(EncodingType.Base64, actual);
        }
        //---------------------------------------------------------------------
        [Test]
        public void Base64_given_char___OK()
        {
            string base64 = "a+b/";

            EncodingType actual = Base64.DetectEncoding(base64.AsSpan());

            Assert.AreEqual(EncodingType.Base64, actual);
        }
    }
}
