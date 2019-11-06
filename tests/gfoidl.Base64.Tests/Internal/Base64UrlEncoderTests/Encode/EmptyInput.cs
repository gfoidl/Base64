using System;
using System.Buffers;
using gfoidl.Base64.Internal;
using NUnit.Framework;

namespace gfoidl.Base64.Tests.Internal.Base64UrlEncoderTests.Encode
{
    [TestFixture(typeof(byte))]
    [TestFixture(typeof(char))]
    public class EmptyInput<T> where T : unmanaged
    {
        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void Empty_input(bool isFinalBlock)
        {
            var sut                   = new Base64UrlEncoder();
            ReadOnlySpan<byte> source = ReadOnlySpan<byte>.Empty;

            Span<T> encoded        = new T[sut.GetEncodedLength(source.Length)];
            OperationStatus status = sut.EncodeCore(source, encoded, out int consumed, out int written, isFinalBlock);

            Assert.AreEqual(OperationStatus.Done, status);
            Assert.AreEqual(0, consumed);
            Assert.AreEqual(0, written);
            Assert.IsTrue(encoded.IsEmpty);
        }
        //---------------------------------------------------------------------
        [Test]
        public void Empty_input_encode_to_string___empty_string()
        {
            var sut                   = new Base64UrlEncoder();
            ReadOnlySpan<byte> source = ReadOnlySpan<byte>.Empty;

            string actual = sut.Encode(source);

            Assert.AreEqual(string.Empty, actual);
        }
    }
}
