using System;
using gfoidl.Base64.Internal;
using NUnit.Framework;

namespace gfoidl.Base64.Tests.Internal.Base64UrlEncoderTests
{
    [TestFixture]
    public class EncodingMaps
    {
        private static readonly string s_characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_";
        //---------------------------------------------------------------------
        [Test]
        public void Verify_encoding_map()
        {
            byte[] expected = new byte[64];
            Base64UrlEncoder.EncodingMap.CopyTo(expected.AsSpan());

            byte[] data = new byte[64];

            for (int i = 0; i < s_characters.Length; ++i)
                data[i] = (byte)s_characters[i];

            CollectionAssert.AreEqual(expected, data);
        }
        //---------------------------------------------------------------------
        [Test]
        public void Verify_decoding_map()
        {
            sbyte[] expected = new sbyte[256];
            Base64UrlEncoder.DecodingMap.CopyTo(expected.AsSpan());

            sbyte[] data = new sbyte[256];
            data.AsSpan().Fill(-1);

            for (int i = 0; i < s_characters.Length; ++i)
                data[s_characters[i]] = (sbyte)i;

            CollectionAssert.AreEqual(expected, data);
        }
    }
}
