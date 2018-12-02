using System;
using System.Text;
using gfoidl.Base64.Internal;
using NUnit.Framework;

namespace gfoidl.Base64.Tests.Internal.Base64UrlEncoderTests
{
    [TestFixture]
    public class GetDecodedLength
    {
        [Test]
        public void EncodedLength_is_0___0()
        {
            var sut = new Base64UrlEncoder();

            int actual = sut.GetDecodedLength(0);

            Assert.AreEqual(0, actual);
        }
        //---------------------------------------------------------------------
        [Test]
        public void EncodedSpan_1_to_50_byte_given___correct_decoded_len()
        {
            var sut = new Base64UrlEncoder();

            Assert.Multiple(() =>
            {
                for (int i = 1; i < 50; ++i)
                {
                    var data              = new byte[i];
                    string base64Url      = Convert.ToBase64String(data).ToBase64Url();
                    byte[] base64UrlBytes = Encoding.ASCII.GetBytes(base64Url);

                    int actual = sut.GetDecodedLength(base64UrlBytes);

                    Assert.AreEqual(i, actual);
                }
            });
        }
        //---------------------------------------------------------------------
        [Test]
        public void EncodedSpan_1_to_50_char_given___correct_decoded_len()
        {
            var sut = new Base64UrlEncoder();

            Assert.Multiple(() =>
            {
                for (int i = 1; i < 50; ++i)
                {
                    var data         = new byte[i];
                    string base64Url = Convert.ToBase64String(data).ToBase64Url();

                    int actual = sut.GetDecodedLength(base64Url.AsSpan());

                    Assert.AreEqual(i, actual);
                }
            });
        }
        //---------------------------------------------------------------------
        [Test]
        public void EncodedLength_is_int_Max___throws_ArgumentOutOfRange()
        {
            var sut = new Base64UrlEncoder();

            Assert.Throws<ArgumentOutOfRangeException>(() => sut.GetDecodedLength(int.MaxValue));
        }
        //---------------------------------------------------------------------
        [Test]
        public void EncodedLength_is_negative___throws_ArgumentOutOfRange()
        {
            var sut = new Base64UrlEncoder();

            Assert.Throws<ArgumentOutOfRangeException>(() => sut.GetDecodedLength(-1));
        }
        //---------------------------------------------------------------------
        [Test]
        [TestCase(1)]
        [TestCase(5)]
        public void Malformed_input_length___throws_FormatException(int len)
        {
            var sut = new Base64UrlEncoder();

            Assert.Throws<FormatException>(() => sut.GetDecodedLength(len));
        }
    }
}
