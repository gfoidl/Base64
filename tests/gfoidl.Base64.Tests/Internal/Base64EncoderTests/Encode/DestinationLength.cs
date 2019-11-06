using System;
using System.Buffers;
using gfoidl.Base64.Internal;
using NUnit.Framework;

namespace gfoidl.Base64.Tests.Internal.Base64EncoderTests.Encode
{
    [TestFixture(typeof(byte))]
    [TestFixture(typeof(char))]
    public class DestinationLength<T> where T : unmanaged
    {
        [Test]
        [TestCase(3)]
        [TestCase(24)]
        [TestCase(60)]
        public void DestinationLength_too_small___status_DestinationTooSmall(int dataLength)
        {
            var sut         = new Base64Encoder();
            var data        = new byte[dataLength];
            Span<T> encoded = new T[1];

            OperationStatus status = sut.EncodeCore(data, encoded, out int consumed, out int written);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(OperationStatus.DestinationTooSmall, status);
                Assert.AreEqual(0, written);
            });
        }
        //---------------------------------------------------------------------
        [Test]
        public void DestiantationLength_large_but_too_small___status_DestinationTooSmall()
        {
            const int dataLength = 300;
            const int destLength = 350;

            var sut         = new Base64Encoder();
            var data        = new byte[dataLength];
            Span<T> encoded = new T[destLength];

            OperationStatus status = sut.EncodeCore(data, encoded, out int consumed, out int written);

            Assert.Multiple(() =>
            {
                const int expectedConsumed = destLength / 4 * 3;
                const int expectedWritten  = expectedConsumed / 3 * 4;

                Assert.AreEqual(OperationStatus.DestinationTooSmall, status);
                Assert.AreEqual(expectedConsumed, consumed);
                Assert.AreEqual(expectedWritten ,  written);
            });
        }
        //---------------------------------------------------------------------
        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void DestinationLength_too_small_but_retry___OK(bool isFinalBlock)
        {
            const int dataLength = 300;
            const int destLength = 350;

            var sut          = new Base64Encoder();
            Span<byte> data  = new byte[dataLength];
            Span<T> encoded  = new T[destLength];
            int requitedSize = sut.GetEncodedLength(dataLength);

            OperationStatus status = sut.EncodeCore(data, encoded, out int consumed, out int written, isFinalBlock);

            Assert.Multiple(() =>
            {
                const int expectedConsumed = destLength / 4 * 3;
                const int expectedWritten  = expectedConsumed / 3 * 4;

                Assert.AreEqual(OperationStatus.DestinationTooSmall, status);
                Assert.AreEqual(expectedConsumed, consumed);
                Assert.AreEqual(expectedWritten ,  written);
            });

            data    = data.Slice(consumed);
            encoded = new T[sut.GetEncodedLength(data.Length)];
            status  = sut.EncodeCore(data, encoded, out consumed, out written, isFinalBlock);

            Assert.AreEqual(OperationStatus.Done, status);
            Assert.AreEqual(data.Length   , consumed);
            Assert.AreEqual(encoded.Length, written);
        }
    }
}
