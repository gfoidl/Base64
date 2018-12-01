using gfoidl.Base64.Internal;
using NUnit.Framework;

namespace gfoidl.Base64.Tests.Internal.Base64UrlEncoderTests
{
    [TestFixture]
    public class GetBufferSizeRequiredToBase64Encode
    {
        [Test]
        [TestCase(1, 4, 2)]
        [TestCase(2, 4, 1)]
        [TestCase(3, 4, 0)]
        [TestCase(16, 24, 2)]
        public void SourceLength_given___correct_encoded_length_and_padding_count_returned(int sourceLength, int expectedEncodedLength, int expectedPaddingCount)
        {
            var sut = new Base64UrlEncoder();

            int encodedLength = sut.GetBufferSizeRequiredToBase64Encode(sourceLength, out int numPaddingChars);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(expectedEncodedLength, encodedLength);
                Assert.AreEqual(expectedPaddingCount , numPaddingChars);
            });
        }
    }
}
