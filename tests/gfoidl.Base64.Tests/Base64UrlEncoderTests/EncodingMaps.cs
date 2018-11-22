using System;
using NUnit.Framework;

namespace gfoidl.Base64.Tests.Base64UrlEncoderTests
{
    [TestFixture]
    public class EncodingMaps
    {
        private static readonly string s_characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_";
        //---------------------------------------------------------------------
        [Test]
        public void Verify_encoding_map()
        {
            byte[] data = new byte[64];

            for (int i = 0; i < s_characters.Length; ++i)
                data[i] = (byte)s_characters[i];

            CollectionAssert.AreEqual(Base64UrlEncoder.s_encodingMap, data);
        }
        //---------------------------------------------------------------------
        [Test]
        public void Verify_decoding_map()
        {
            sbyte[] data = new sbyte[256];
            data.AsSpan().Fill(-1);

            for (int i = 0; i < s_characters.Length; ++i)
                data[s_characters[i]] = (sbyte)i;

            CollectionAssert.AreEqual(Base64UrlEncoder.s_decodingMap, data);
        }
    }
}
