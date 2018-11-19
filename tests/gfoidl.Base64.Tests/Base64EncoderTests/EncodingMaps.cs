using System;
using NUnit.Framework;

namespace gfoidl.Base64.Tests.Base64EncoderTests
{
    [TestFixture]
    public  class EncodingMaps
    {
        private static readonly string s_characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
        //---------------------------------------------------------------------
        [Test]
        public void Verify_encoding_map()
        {
            byte[] data = new byte[64];

            for (int i = 0; i < s_characters.Length; ++i)
                data[i] = (byte)s_characters[i];

            Assert.IsTrue(Base64Encoder.s_encodingMap.AsSpan().SequenceEqual(data));
        }
        //---------------------------------------------------------------------
        [Test]
        public void Verify_decoding_map()
        {
            sbyte[] data = new sbyte[256];
            data.AsSpan().Fill(-1);

            for (int i = 0; i < s_characters.Length; ++i)
                data[s_characters[i]] = (sbyte)i;

            Assert.IsTrue(Base64Encoder.s_decodingMap.AsSpan().SequenceEqual(data));
        }
    }
}
