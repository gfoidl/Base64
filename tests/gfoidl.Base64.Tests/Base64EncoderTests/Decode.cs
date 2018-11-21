using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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
        //---------------------------------------------------------------------
        [Test]
        public void Buffer_chain_basic_decode()
        {
            var sut  = new Base64Encoder();
            var data = new byte[200];
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
        }
    }
}
