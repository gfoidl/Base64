using System;
using System.Collections.Generic;
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
                    var data      = new byte[i];
                    string base64 = Convert.ToBase64String(data);

                    int actual = sut.GetEncodedLength(i);

                    Assert.AreEqual(base64.Length, actual);
                }
            });
        }
        //---------------------------------------------------------------------
        [Test, TestCaseSource(nameof(SourceLength_given___correct_encoded_length_TestCases))]
        public int SourceLength_given___correct_encoded_length(int sourceLength)
        {
            var sut = new Base64Encoder();

            return sut.GetEncodedLength(sourceLength);
        }
        //---------------------------------------------------------------------
        private static IEnumerable<TestCaseData> SourceLength_given___correct_encoded_length_TestCases()
        {
            // (int.MaxValue - 4)/(4/3) => 1610612733, otherwise integer overflow
            int[] input    = { 0, 1, 2, 3, 4, 5, 6, 1610612728, 1610612729, 1610612730, 1610612731, 1610612732, 1610612733 };
            int[] expected = { 0, 4, 4, 4, 8, 8, 8, 2147483640, 2147483640, 2147483640, 2147483644, 2147483644, 2147483644 };

            Assume.That(input.Length, Is.EqualTo(expected.Length));

            for (int i = 0; i < input.Length; ++i)
            {
                yield return new TestCaseData(input[i]).Returns(expected[i]);
            }
        }
        //---------------------------------------------------------------------
        [Test]
        [TestCase(Base64.MaximumEncodeLength + 1)]
        [TestCase(int.MaxValue)]
        public void SourceLength_gt_MaximumEncodeLength___throws_ArgumentOutOfRange(int sourceLength)
        {
            var sut = new Base64Encoder();

            Assert.Throws<ArgumentOutOfRangeException>(() => sut.GetEncodedLength(sourceLength));
        }
        //---------------------------------------------------------------------
        [Test]
        [TestCase(-1)]
        [TestCase(int.MinValue)]
        public void SourceLength_is_negative___throws_ArgumentOutOfRange(int sourceLength)
        {
            var sut = new Base64Encoder();

            Assert.Throws<ArgumentOutOfRangeException>(() => sut.GetEncodedLength(sourceLength));
        }
    }
}
