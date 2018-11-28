using System;
using System.Text;
using gfoidl.Base64.Internal;
using NUnit.Framework;

namespace gfoidl.Base64.Tests.Internal.Base64EncoderTests
{
    [TestFixture]
    public class GetDecodedLength
    {
        [Test]
        public void EncodedLength_is_0___0()
        {
            var sut = new Base64Encoder();

            int actual = sut.GetDecodedLength(0);

            Assert.AreEqual(0, actual);
        }
        //---------------------------------------------------------------------
        [Test]
        public void EncodedSpan_1_to_50_byte_given___correct_decoded_len()
        {
            var sut = new Base64Encoder();

            Assert.Multiple(() =>
            {
                for (int i = 1; i < 50; ++i)
                {
                    var data                     = new byte[i];
                    string base64WoPaddingString = Convert.ToBase64String(data);
                    byte[] base64WoPadding       = Encoding.ASCII.GetBytes(base64WoPaddingString);

                    int actual = sut.GetDecodedLength(base64WoPadding);

                    Assert.AreEqual(i, actual);
                }
            });
        }
        //---------------------------------------------------------------------
        [Test]
        public void EncodedSpan_1_to_50_char_given___correct_decoded_len()
        {
            var sut = new Base64Encoder();

            Assert.Multiple(() =>
            {
                for (int i = 1; i < 50; ++i)
                {
                    var data                     = new byte[i];
                    string base64WoPaddingString = Convert.ToBase64String(data);

                    int actual = sut.GetDecodedLength(base64WoPaddingString.AsSpan());

                    Assert.AreEqual(i, actual);
                }
            });
        }
        //---------------------------------------------------------------------
        [Test]
        public void EncodedLength_is_int_Max___OK()
        {
            var sut = new Base64Encoder();

            int actual = sut.GetDecodedLength(int.MaxValue);

            Assert.AreEqual(int.MaxValue / 4 * 3, actual);
        }
        //---------------------------------------------------------------------
        [Test]
        public void EncodedLength_is_negative___throws_ArgumentOutOfRange()
        {
            var sut = new Base64Encoder();

            Assert.Throws<ArgumentOutOfRangeException>(() => sut.GetDecodedLength(-1));
        }
    }
}
