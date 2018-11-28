using System;
using gfoidl.Base64.Internal;
using NUnit.Framework;

namespace gfoidl.Base64.Tests.Internal.Base64EncoderTests
{
    [TestFixture]
    public class GetEncodedLength
    {
        [Test]
        public void SourceLength_is_0___0()
        {
            var sut = new Base64Encoder();

            int actual = sut.GetEncodedLength(0);

            Assert.AreEqual(0, actual);
        }
        //---------------------------------------------------------------------
        [Test]
        public void SourceLength_1_to_50___correct_encoded_len()
        {
            var sut = new Base64Encoder();

            Assert.Multiple(() =>
            {
                for (int i = 1; i < 50; ++i)
                {
                    var data               = new byte[i];
                    string base64WoPadding = Convert.ToBase64String(data);

                    int actual = sut.GetEncodedLength(i);

                    Assert.AreEqual(base64WoPadding.Length, actual);
                }
            });
        }
        //---------------------------------------------------------------------
        [Test]
        public void SourceLength_gt_MaximumEncodeLength___throws_ArgumentOutOfRange()
        {
            var sut = new Base64Encoder();

            Assert.Throws<ArgumentOutOfRangeException>(() => sut.GetEncodedLength(Base64Encoder.MaximumEncodeLength + 1));
        }
        //---------------------------------------------------------------------
        [Test]
        public void SourceLength_is_negative___throws_ArgumentOutOfRange()
        {
            var sut = new Base64Encoder();

            Assert.Throws<ArgumentOutOfRangeException>(() => sut.GetEncodedLength(-1));
        }
    }
}
