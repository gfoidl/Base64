using System;
using gfoidl.Base64.Internal;
using NUnit.Framework;

namespace gfoidl.Base64.Tests.Internal.Base64EncoderTests
{
    [TestFixture]
    public class GetMaxDecodedLength
    {
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
