using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using gfoidl.Base64.Internal;
using NUnit.Framework;

namespace gfoidl.Base64.Tests.Internal.Base64EncoderTests.Decode
{
    [TestFixture(typeof(byte))]
    [TestFixture(typeof(char))]
    public class BasicDecoding<T> where T : unmanaged
    {
        [Test]
        [TestCase("AQ==", 1)]
        [TestCase("AQI=", 2)]
        [TestCase("AQID", 3)]
        [TestCase("AQIDBA==", 4)]
        [TestCase("AQIDBAU=", 5)]
        [TestCase("AQIDBAUG", 6)]
        public void Basic_decoding_with_known_input___Done(string input, int expectedWritten)
        {
            var sut = new Base64Encoder();
            ReadOnlySpan<T> encoded;

            if (typeof(T) == typeof(byte))
            {
                ReadOnlySpan<byte> tmp = Encoding.ASCII.GetBytes(input);
                encoded                = MemoryMarshal.Cast<byte, T>(tmp);
            }
            else if (typeof(T) == typeof(char))
            {
                encoded = MemoryMarshal.Cast<char, T>(input.AsSpan());  // AsSpan() for net48
            }
            else
            {
                throw new NotSupportedException(); // just in case new types are introduced in the future
            }

            Span<byte> data        = new byte[sut.GetMaxDecodedLength(encoded.Length)];
            OperationStatus status = sut.DecodeCore(encoded, data, out int consumed, out int written);
            byte[] actualData      = data.Slice(0, written).ToArray();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(OperationStatus.Done, status);
                Assert.AreEqual(input.Length        , consumed);
                Assert.AreEqual(expectedWritten     , written);

                byte[] expectedData = Convert.FromBase64String(input);
                CollectionAssert.AreEqual(expectedData, actualData);
            });
        }
        //---------------------------------------------------------------------
        [Test]
        [TestCase("AQID", 3)]
        [TestCase("AQIDBAUG", 6)]
        public void Basic_decoding_with_known_input_isFinalBlock_false___Done(string input, int expectedWritten)
        {
            var sut = new Base64Encoder();
            ReadOnlySpan<T> encoded;

            if (typeof(T) == typeof(byte))
            {
                ReadOnlySpan<byte> tmp = Encoding.ASCII.GetBytes(input);
                encoded                = MemoryMarshal.Cast<byte, T>(tmp);
            }
            else if (typeof(T) == typeof(char))
            {
                encoded = MemoryMarshal.Cast<char, T>(input.AsSpan());  // .AsSpan() for net48
            }
            else
            {
                throw new NotSupportedException(); // just in case new types are introduced in the future
            }

            Span<byte> data        = new byte[sut.GetMaxDecodedLength(encoded.Length)];
            OperationStatus status = sut.DecodeCore(encoded, data, out int consumed, out int written, isFinalBlock: false);
            byte[] actualData      = data.Slice(0, written).ToArray();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(OperationStatus.Done, status);
                Assert.AreEqual(input.Length, consumed);
                Assert.AreEqual(expectedWritten, written);

                byte[] expectedData = Convert.FromBase64String(input);
                CollectionAssert.AreEqual(expectedData, actualData);
            });
        }
        //---------------------------------------------------------------------
        [Test]
        [TestCase(" ", 0, 0)]
        [TestCase("A", 0, 0)]
        [TestCase("AQ", 0, 0)]
        [TestCase("AQI", 0, 0)]
        [TestCase("AQIDBA", 4, 3)]
        [TestCase("AQIDBAU", 4, 3)]
        public void Basic_decoding_with_invalid_input___InvalidData(string input, int expectedConsumed, int expectedWritten)
        {
            var sut = new Base64Encoder();
            ReadOnlySpan<T> encoded;

            if (typeof(T) == typeof(byte))
            {
                ReadOnlySpan<byte> tmp = Encoding.ASCII.GetBytes(input);
                encoded                = MemoryMarshal.Cast<byte, T>(tmp);
            }
            else if (typeof(T) == typeof(char))
            {
                encoded = MemoryMarshal.Cast<char, T>(input.AsSpan());  // .AsSpan() for net48
            }
            else
            {
                throw new NotSupportedException(); // just in case new types are introduced in the future
            }

            Span<byte> data        = new byte[sut.GetMaxDecodedLength(encoded.Length)];
            OperationStatus status = sut.DecodeCore(encoded, data, out int consumed, out int written);
            byte[] actualData      = data.Slice(0, written).ToArray();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(OperationStatus.InvalidData, status);
                Assert.AreEqual(expectedConsumed           , consumed);
                Assert.AreEqual(expectedWritten            , written);

                byte[] expectedData = Convert.FromBase64String(input.Substring(0, consumed));
                CollectionAssert.AreEqual(expectedData, actualData);
            });
        }
        //---------------------------------------------------------------------
        [Test]
        [TestCase(" ", 0, 0)]
        [TestCase("A", 0, 0)]
        [TestCase("AQ", 0, 0)]
        [TestCase("AQI", 0, 0)]
        [TestCase("AQIDB", 4, 3)]
        [TestCase("AQIDBA", 4, 3)]
        [TestCase("AQIDBAU", 4, 3)]
        public void Basic_decoding_with_invalid_input_isFinalBlock_false___NeedMoreData(string input, int expectedConsumed, int expectedWritten)
        {
            var sut = new Base64Encoder();
            ReadOnlySpan<T> encoded;

            if (typeof(T) == typeof(byte))
            {
                ReadOnlySpan<byte> tmp = Encoding.ASCII.GetBytes(input);
                encoded                = MemoryMarshal.Cast<byte, T>(tmp);
            }
            else if (typeof(T) == typeof(char))
            {
                encoded = MemoryMarshal.Cast<char, T>(input.AsSpan());  // .AsSpan() for net48
            }
            else
            {
                throw new NotSupportedException(); // just in case new types are introduced in the future
            }

            Span<byte> data        = new byte[sut.GetMaxDecodedLength(encoded.Length)];
            OperationStatus status = sut.DecodeCore(encoded, data, out int consumed, out int written, isFinalBlock: false);
            byte[] actualData      = data.Slice(0, written).ToArray();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(OperationStatus.NeedMoreData, status);
                Assert.AreEqual(expectedConsumed            , consumed);
                Assert.AreEqual(expectedWritten             , written);

                byte[] expectedData = Convert.FromBase64String(input.Substring(0, consumed));
                CollectionAssert.AreEqual(expectedData, actualData);
            });
        }
        //---------------------------------------------------------------------
        [Test]
        public void Basic_decoding_with_invalid_input_length_isFinalBlock_false___NeedMoreData()
        {
            var sut = new Base64Encoder();
            var rnd = new Random(42);

            for (int i = 0; i < 10; ++i)
            {
                int encodedLength;
                do
                {
                    encodedLength = rnd.Next(100, 1_000 * 1_000);
                }
                while (encodedLength % 4 == 0);      // ensure we have invalid length

                ReadOnlySpan<T> encoded;

                if (typeof(T) == typeof(byte))
                {
                    ReadOnlySpan<byte> tmp = Enumerable.Repeat((byte)'a', encodedLength).ToArray();
                    encoded = MemoryMarshal.Cast<byte, T>(tmp);
                }
                else if (typeof(T) == typeof(char))
                {
                    ReadOnlySpan<char> tmp = Enumerable.Repeat('a', encodedLength).ToArray();
                    encoded = MemoryMarshal.Cast<char, T>(tmp);
                }
                else
                {
                    throw new NotSupportedException(); // just in case new types are introduced in the future
                }

                Span<byte> data        = new byte[sut.GetMaxDecodedLength(encoded.Length)];
                OperationStatus status = sut.DecodeCore(encoded, data, out int consumed, out int written, isFinalBlock: false);

                Assert.AreEqual(OperationStatus.NeedMoreData, status, "fail at i = {0}", i);
            }
        }
        //---------------------------------------------------------------------
        [Test]
        [TestCase("AQ==", 0, 0)]
        [TestCase("AQI=", 0, 0)]
        [TestCase("AQIDBA==", 4, 3)]
        [TestCase("AQIDBAU=", 4, 3)]
        public void Basic_decoding_with_padding_at_end_isFinalBlock_false___InvalidData(string input, int expectedConsumed, int expectedWritten)
        {
            var sut = new Base64Encoder();
            ReadOnlySpan<T> encoded;

            if (typeof(T) == typeof(byte))
            {
                ReadOnlySpan<byte> tmp = Encoding.ASCII.GetBytes(input);
                encoded                = MemoryMarshal.Cast<byte, T>(tmp);
            }
            else if (typeof(T) == typeof(char))
            {
                encoded = MemoryMarshal.Cast<char, T>(input.AsSpan());  // .AsSpan() for net48
            }
            else
            {
                throw new NotSupportedException(); // just in case new types are introduced in the future
            }

            Span<byte> data        = new byte[sut.GetMaxDecodedLength(encoded.Length)];
            OperationStatus status = sut.DecodeCore(encoded, data, out int consumed, out int written, isFinalBlock: false);
            byte[] actualData      = data.Slice(0, written).ToArray();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(OperationStatus.InvalidData, status);
                Assert.AreEqual(expectedConsumed           , consumed);
                Assert.AreEqual(expectedWritten            , written);

                byte[] expectedData = Convert.FromBase64String(input.Substring(0, consumed));
                CollectionAssert.AreEqual(expectedData, actualData);
            });
        }
    }
}
