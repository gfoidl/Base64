using System;
using gfoidl.Base64.Internal;
using NUnit.Framework;

namespace gfoidl.Base64.Tests.Internal.Base64UrlEncoderTests
{
    [TestFixture]
    public class GetMaxDecodedLength
    {
        [Test]
        public void EncodedLength_is_negative___throws_ArgumentOutOfRange([Values(-1, int.MinValue)]int encodedLength)
        {
            var sut = new Base64UrlEncoder();

            Assert.Throws<ArgumentOutOfRangeException>(() => sut.GetMaxDecodedLength(encodedLength));
        }
        //---------------------------------------------------------------------
        [Test]
        public void EncodedLength_1_to_50_given___correct_max_decoded_len()
        {
            var sut = new Base64UrlEncoder();

            Assert.Multiple(() =>
            {
                for (int i = 1; i < 50; ++i)
                {
                    var data         = new byte[i];
                    string base64Url = Convert.ToBase64String(data).ToBase64Url();

                    int actual = sut.GetMaxDecodedLength(base64Url.Length);

                    Assert.GreaterOrEqual(actual, i);
                }
            });
        }
    }
}
