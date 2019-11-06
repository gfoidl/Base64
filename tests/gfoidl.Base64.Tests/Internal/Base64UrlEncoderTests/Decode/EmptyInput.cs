using System;
using System.Buffers;
using gfoidl.Base64.Internal;
using NUnit.Framework;

namespace gfoidl.Base64.Tests.Internal.Base64UrlEncoderTests.Decode
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
            var sut                 = new Base64UrlEncoder();
            ReadOnlySpan<T> encoded = ReadOnlySpan<T>.Empty;

            Span<byte> data        = new byte[sut.GetDecodedLength(encoded.Length)];
            OperationStatus status = sut.DecodeCore(encoded, data, out int consumed, out int written, isFinalBlock);

            Assert.AreEqual(OperationStatus.Done, status);
            Assert.AreEqual(0, consumed);
            Assert.AreEqual(0, written);
            Assert.IsTrue(data.IsEmpty);
        }
        //---------------------------------------------------------------------
        [Test]
        public void Empty_input_decode_from_string___empty_array()
        {
            var sut        = new Base64UrlEncoder();
            string encoded = string.Empty;

            byte[] actual = sut.Decode(encoded.AsSpan());               // AsSpan() for net48

            Assert.AreEqual(Array.Empty<byte>(), actual);
        }
    }
}
