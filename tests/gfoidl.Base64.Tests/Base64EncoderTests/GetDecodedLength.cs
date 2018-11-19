using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace gfoidl.Base64.Tests.Base64EncoderTests
{
    [TestFixture]
    public  class GetDecodedLength
    {
        [Test]
        public void EncodedLength_is_0___0()
        {
            var sut = new Base64Encoder();

            int actual = sut.GetDecodedLength(0);

            Assert.AreEqual(0, actual);
        }
        //---------------------------------------------------------------------
        [Test, TestCaseSource(nameof(EncodedLength_given___correct_decoded_len_TestCases))]
        public int EncodedLength_given___correct_decoded_len(int encodedLength)
        {
            var sut = new Base64Encoder();

            return sut.GetDecodedLength(encodedLength);
        }
        //---------------------------------------------------------------------
        private static IEnumerable<TestCaseData> EncodedLength_given___correct_decoded_len_TestCases()
        {
            yield return new TestCaseData(1).Returns(0);
            yield return new TestCaseData(2).Returns(0);
            yield return new TestCaseData(3).Returns(0);

            yield return new TestCaseData(4).Returns(3);    
            yield return new TestCaseData(5).Returns(3);    
        }
        //---------------------------------------------------------------------
        [Test, TestCaseSource(nameof(EncodedSpan_byte_given___correct_decoded_len_TestCases))]
        public int EncodedSpan_byte_given___correct_decoded_len(int noOfPadding)
        {
            Span<byte> encoded = stackalloc byte[4];
            encoded.Fill((byte)'a');

            for (int i = 0; i < noOfPadding; ++i)
                encoded[encoded.Length - 1 - i] = (byte)'=';

            var sut = new Base64Encoder();

            return sut.GetDecodedLength(encoded);
        }
        //---------------------------------------------------------------------
        private static IEnumerable<TestCaseData> EncodedSpan_byte_given___correct_decoded_len_TestCases()
        {
            yield return new TestCaseData(0).Returns(3);
            yield return new TestCaseData(1).Returns(2);
            yield return new TestCaseData(2).Returns(1);
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
