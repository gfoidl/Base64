using System;
using System.Buffers;
using System.Collections.Generic;
using NUnit.Framework;

namespace gfoidl.Base64.Tests.Base64EncoderTests
{
    [TestFixture(typeof(byte))]
    [TestFixture(typeof(char))]
    public class Decode<T> where T : unmanaged
    {
        [Test]
        public void Empty_input()
        {
            var sut = new Base64Encoder();
            ReadOnlySpan<T> encoded = ReadOnlySpan<T>.Empty;

            Span<byte> data = new byte[sut.GetDecodedLength(encoded.Length)];
            OperationStatus status = sut.DecodeCore(encoded, data, out int consumed, out int written);

            Assert.AreEqual(OperationStatus.Done, status);
            Assert.AreEqual(0, consumed);
            Assert.AreEqual(0, written);
            Assert.IsTrue(data.IsEmpty);
        }
        //---------------------------------------------------------------------
        [Test]
        public void Empty_input_decode_from_string___empty_array()
        {
            var sut        = new Base64Encoder();
            string encoded = string.Empty;

            byte[] actual = sut.Decode(encoded.AsSpan());

            Assert.AreEqual(Array.Empty<byte>(), actual);
        }
        //---------------------------------------------------------------------
        [Test, TestCaseSource(nameof(Malformed_input___throws_FormatException_TestCases))]
        public void Malformed_input___throws_FormatException(string input)
        {
            var sut = new Base64Encoder();

            Assert.Throws<FormatException>(() => sut.Decode(input.AsSpan()));
        }
        //---------------------------------------------------------------------
        private static IEnumerable<TestCaseData> Malformed_input___throws_FormatException_TestCases()
        {
            yield return new TestCaseData(" ");
            yield return new TestCaseData("a");
            yield return new TestCaseData("ab");
            yield return new TestCaseData("abc");
            yield return new TestCaseData("abc?");
            yield return new TestCaseData("ab?c");
            yield return new TestCaseData("ab=c");
        }
    }
}
