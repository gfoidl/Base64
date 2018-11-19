using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace gfoidl.Base64.Tests.Base64EncoderTests
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
        [Test, TestCaseSource(nameof(SourceLength_given___correct_encoded_len_TestCases))]
        public int SourceLength_given___correct_encoded_len(int sourceLength)
        {
            var sut = new Base64Encoder();

            return sut.GetEncodedLength(sourceLength);
        }
        //---------------------------------------------------------------------
        private static IEnumerable<TestCaseData> SourceLength_given___correct_encoded_len_TestCases()
        {
            yield return new TestCaseData(1).Returns(4);
            yield return new TestCaseData(2).Returns(4);
            yield return new TestCaseData(3).Returns(4);

            yield return new TestCaseData(4).Returns(8);
            yield return new TestCaseData(5).Returns(8);
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
