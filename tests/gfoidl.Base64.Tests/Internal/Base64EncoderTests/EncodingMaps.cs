using System;
using gfoidl.Base64.Internal;
using NUnit.Framework;

namespace gfoidl.Base64.Tests.Internal.Base64EncoderTests
{
    [TestFixture]
    public class EncodingMaps
    {
        private const string Characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
        //---------------------------------------------------------------------
        [Test]
        public void Verify_encoding_map()
        {
            byte[] expected = new byte[64];
            Base64Encoder.EncodingMap.CopyTo(expected.AsSpan());

            byte[] data = new byte[64];

            for (int i = 0; i < Characters.Length; ++i)
                data[i] = (byte)Characters[i];

            CollectionAssert.AreEqual(expected, data);
        }
        //---------------------------------------------------------------------
        [Test]
        public void Verify_decoding_map()
        {
            sbyte[] expected = new sbyte[256];
            Base64Encoder.DecodingMap.CopyTo(expected.AsSpan());

            sbyte[] data = new sbyte[256];
            data.AsSpan().Fill(-1);

            for (int i = 0; i < Characters.Length; ++i)
                data[Characters[i]] = (sbyte)i;

            CollectionAssert.AreEqual(expected, data);
        }
    }
}
