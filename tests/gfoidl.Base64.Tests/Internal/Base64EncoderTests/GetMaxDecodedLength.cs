using System;
using gfoidl.Base64.Internal;
using NUnit.Framework;

namespace gfoidl.Base64.Tests.Internal.Base64EncoderTests
{
    [TestFixture]
    public class GetMaxDecodedLength
    {
        [Test]
        public void EncodedLength_is_negative___throws_ArgumentOutOfRange([Values(-1, int.MinValue)]int encodedLength)
        {
            var sut = new Base64Encoder();

            Exception exception = Assert.Catch(() => sut.GetMaxDecodedLength(encodedLength));

            Assert.Multiple(() =>
            {
                Assert.IsInstanceOf<ArgumentOutOfRangeException>(exception);
                string msg = $"The 'encodedLength' is outside the allowed range by the base64 standard. It must be >= 4.";
                StringAssert.StartsWith(msg, exception.Message);
            });
        }
        //---------------------------------------------------------------------
        [Test]
        public void EncodedLength_1_to_50_given___correct_max_decoded_len()
        {
            var sut = new Base64Encoder();

            Assert.Multiple(() =>
            {
                for (int i = 1; i < 50; ++i)
                {
                    var data      = new byte[i];
                    string base64 = Convert.ToBase64String(data);

                    int actual = sut.GetMaxDecodedLength(base64.Length);

                    Assert.GreaterOrEqual(actual, i);
                }
            });
        }
    }
}
