using System;
using System.Buffers;
using System.Runtime.InteropServices;
using gfoidl.Base64.Internal;
using NUnit.Framework;

namespace gfoidl.Base64.Tests.Internal.Base64EncoderTests.Decode
{
    [TestFixture(typeof(byte))]
    [TestFixture(typeof(char))]
    public class BufferChains<T> where T : unmanaged
    {
        [Test]
        public void Buffer_chain_basic_decode()
        {
            var sut  = new Base64Encoder();
            var data = new byte[500];
            var rnd  = new Random(0);
            rnd.NextBytes(data);

            int encodedLength      = sut.GetEncodedLength(data.Length);
            Span<T> encoded        = new T[encodedLength];
            OperationStatus status = sut.EncodeCore(data, encoded, out int consumed, out int written);
            Assume.That(status  , Is.EqualTo(OperationStatus.Done));
            Assume.That(consumed, Is.EqualTo(data.Length));
            Assume.That(written , Is.EqualTo(encodedLength));

            //int decodedLength  = sut.GetDecodedLength(encodedLength);

            int decodedLength;

            if (typeof(T) == typeof(byte))
            {
                decodedLength = sut.GetDecodedLength(MemoryMarshal.AsBytes(encoded));
            }
            else if (typeof(T) == typeof(char))
            {
                decodedLength = sut.GetDecodedLength(MemoryMarshal.Cast<T, char>(encoded));
            }
            else
            {
                throw new NotSupportedException(); // just in case new types are introduced in the future
            }

            Span<byte> decoded = new byte[decodedLength];

            status = sut.DecodeCore<T>(encoded.Slice(0, encoded.Length / 2), decoded, out consumed, out int written1, isFinalBlock: false);
            Assert.AreEqual(OperationStatus.NeedMoreData, status);

            status = sut.DecodeCore<T>(encoded.Slice(consumed), decoded.Slice(written1), out consumed, out int written2, isFinalBlock: true);
            Assert.AreEqual(OperationStatus.Done, status);
            Assert.AreEqual(decodedLength, written1 + written2);

            CollectionAssert.AreEqual(data, decoded.ToArray());
        }
        //---------------------------------------------------------------------
        [Test]
        public void Buffer_chain_various_length_decode()
        {
            var sut = new Base64Encoder();
            var rnd = new Random(0);

            for (int i = 2; i < 200; ++i)
            {
                var data = new byte[i];
                rnd.NextBytes(data);

                int encodedLength      = sut.GetEncodedLength(data.Length);
                Span<T> encoded        = new T[encodedLength];
                OperationStatus status = sut.EncodeCore(data, encoded, out int consumed, out int written);
                Assume.That(status  , Is.EqualTo(OperationStatus.Done), "fail at i = {0}", i);
                Assume.That(consumed, Is.EqualTo(data.Length), "fail at i = {0}", i);
                Assume.That(written , Is.EqualTo(encodedLength), "fail at i = {0}", i);

                int decodedLength;

                if (typeof(T) == typeof(byte))
                {
                    decodedLength = sut.GetDecodedLength(MemoryMarshal.AsBytes(encoded));
                }
                else if (typeof(T) == typeof(char))
                {
                    decodedLength = sut.GetDecodedLength(MemoryMarshal.Cast<T, char>(encoded));
                }
                else
                {
                    throw new NotSupportedException(); // just in case new types are introduced in the future
                }

                Span<byte> decoded = new byte[decodedLength];

                int partialLength = encoded.Length / 2;
                status            = sut.DecodeCore<T>(encoded.Slice(0, partialLength), decoded, out consumed, out int written1, isFinalBlock: false);
                Assert.AreEqual(partialLength % 4 == 0 ? OperationStatus.Done : OperationStatus.NeedMoreData, status, "fail at i = {0}", i);

                status = sut.DecodeCore<T>(encoded.Slice(consumed), decoded.Slice(written1), out consumed, out int written2, isFinalBlock: true);
                Assert.AreEqual(OperationStatus.Done, status, "fail at i = {0}", i);
                Assert.AreEqual(decodedLength, written1 + written2, "fail at i = {0}", i);

                CollectionAssert.AreEqual(data, decoded.ToArray(), "fail at i = {0}", i);
            }
        }
    }
}
