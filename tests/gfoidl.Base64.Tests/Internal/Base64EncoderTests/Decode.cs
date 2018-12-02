using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using gfoidl.Base64.Internal;
using NUnit.Framework;

namespace gfoidl.Base64.Tests.Internal.Base64EncoderTests
{
    [TestFixture(typeof(byte))]
    [TestFixture(typeof(char))]
    public class Decode<T> where T : unmanaged
    {
        [Test]
        public void Empty_input()
        {
            var sut                 = new Base64Encoder();
            ReadOnlySpan<T> encoded = ReadOnlySpan<T>.Empty;

            Span<byte> data        = new byte[sut.GetDecodedLength(encoded.Length)];
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
            yield return new TestCaseData("abc?");
            yield return new TestCaseData("ab?c");
            yield return new TestCaseData("ab=c");
        }
        //---------------------------------------------------------------------
        [Test]
        public void Invalid_data_various_length___status_InvalidData()
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
                Assume.That(status, Is.EqualTo(OperationStatus.Done));
                Assume.That(consumed, Is.EqualTo(data.Length));
                Assume.That(written, Is.EqualTo(encodedLength));

                int decodedLength;

                if (typeof(T) == typeof(byte))
                {
                    Span<byte> tmp = MemoryMarshal.AsBytes(encoded);
                    decodedLength  = sut.GetDecodedLength(tmp);

                    // Insert invalid data
                    tmp[0] = (byte)'~';
                }
                else if (typeof(T) == typeof(char))
                {
                    Span<char> tmp = MemoryMarshal.Cast<T, char>(encoded);
                    decodedLength  = sut.GetDecodedLength(tmp);

                    // Insert invalid data
                    tmp[0] = '~';
                }
                else
                {
                    throw new NotSupportedException(); // just in case new types are introduced in the future
                }

                Span<byte> decoded = new byte[decodedLength];

                status = sut.DecodeCore<T>(encoded, decoded, out int _, out int _);

                Assert.AreEqual(OperationStatus.InvalidData, status);
            }
        }
        //---------------------------------------------------------------------
#if NETCOREAPP3_0 && DEBUG && !OSX
        [Test]
        public void Large_data___avx2_event_fired()
        {
            var sut  = new Base64Encoder();
            var data = new byte[50];
            var rnd  = new Random(0);
            rnd.NextBytes(data);

            int encodedLength      = sut.GetEncodedLength(data.Length);
            Span<T> encoded        = new T[encodedLength];
            OperationStatus status = sut.EncodeCore(data, encoded, out int consumed, out int written);
            Assume.That(status  , Is.EqualTo(OperationStatus.Done));
            Assume.That(consumed, Is.EqualTo(data.Length));
            Assume.That(written , Is.EqualTo(encodedLength));

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

            bool avxExecuted = false;
            Base64Encoder.Avx2Decoded += (s, e) => avxExecuted = true;

            status = sut.DecodeCore<T>(encoded, decoded, out int _, out int _);
            Assume.That(status, Is.EqualTo(OperationStatus.Done));

            Assert.IsTrue(avxExecuted);
        }
#endif
        //---------------------------------------------------------------------
#if NETCOREAPP && DEBUG
        [Test]
        public void Large_data_but_to_small_for_avx2___sse2_event_fired()
        {
            var sut  = new Base64Encoder();
            var data = new byte[20];
            var rnd  = new Random(0);
            rnd.NextBytes(data);

            int encodedLength      = sut.GetEncodedLength(data.Length);
            Span<T> encoded        = new T[encodedLength];
            OperationStatus status = sut.EncodeCore(data, encoded, out int consumed, out int written);
            Assume.That(status  , Is.EqualTo(OperationStatus.Done));
            Assume.That(consumed, Is.EqualTo(data.Length));
            Assume.That(written , Is.EqualTo(encodedLength));

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

            bool sse2Executed = false;
            Base64Encoder.Sse2Decoded += (s, e) => sse2Executed = true;

            status = sut.DecodeCore<T>(encoded, decoded, out int _, out int _);
            Assume.That(status, Is.EqualTo(OperationStatus.Done));

            Assert.IsTrue(sse2Executed);
        }
        //---------------------------------------------------------------------
        [Test]
        public void Guid___sse2_event_fired()
        {
            var sut  = new Base64Encoder();
            var data = Guid.NewGuid().ToByteArray();

            int encodedLength      = sut.GetEncodedLength(data.Length);
            Span<T> encoded        = new T[encodedLength];
            OperationStatus status = sut.EncodeCore(data, encoded, out int consumed, out int written);
            Assume.That(status, Is.EqualTo(OperationStatus.Done));
            Assume.That(consumed, Is.EqualTo(data.Length));
            Assume.That(written, Is.EqualTo(encodedLength));

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

            bool sse2Executed = false;
            Base64Encoder.Sse2Decoded += (s, e) => sse2Executed = true;

            status = sut.DecodeCore<T>(encoded, decoded, out int _, out int _);
            Assume.That(status, Is.EqualTo(OperationStatus.Done));

            Assert.IsTrue(sse2Executed);
        }
#endif
        //---------------------------------------------------------------------
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
                Assume.That(status  , Is.EqualTo(OperationStatus.Done));
                Assume.That(consumed, Is.EqualTo(data.Length));
                Assume.That(written , Is.EqualTo(encodedLength));

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
