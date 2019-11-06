using System;
using System.Buffers;
using System.Runtime.InteropServices;
using System.Text;
using gfoidl.Base64.Internal;
using NUnit.Framework;

namespace gfoidl.Base64.Tests.Internal.Base64UrlEncoderTests.Decode
{
    [TestFixture(typeof(byte))]
    [TestFixture(typeof(char))]
    public class BasicDecoding<T> where T : unmanaged
    {
        [Test]
        [TestCase("AQ", 1)]
        [TestCase("AQI", 2)]
        [TestCase("AQID", 3)]
        [TestCase("AQIDBA", 4)]
        [TestCase("AQIDBAU", 5)]
        [TestCase("AQIDBAUG", 6)]
        public void Basic_decoding_with_known_input___Done(string input, int expectedWritten)
        {
            var sut = new Base64UrlEncoder();
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

                byte[] expectedData = Convert.FromBase64String(input.FromBase64Url());
                CollectionAssert.AreEqual(expectedData, actualData);
            });
        }
        //---------------------------------------------------------------------
        [Test]
        [TestCase("AQID", 3)]
        [TestCase("AQIDBAUG", 6)]
        public void Basic_decoding_with_known_input_isFinalBlock_false___Done(string input, int expectedWritten)
        {
            var sut = new Base64UrlEncoder();
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

                byte[] expectedData = Convert.FromBase64String(input.FromBase64Url());
                CollectionAssert.AreEqual(expectedData, actualData);
            });
        }
        //---------------------------------------------------------------------
        [Test]
        [TestCase("A", 0, 0)]
        public void Basic_decoding_with_invalid_input___InvalidData(string input, int expectedConsumed, int expectedWritten)
        {
            var sut = new Base64UrlEncoder();
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

                byte[] expectedData = Convert.FromBase64String(input.Substring(0, consumed).FromBase64Url());
                CollectionAssert.AreEqual(expectedData, actualData);
            });
        }
        //---------------------------------------------------------------------
        [Test]
        [TestCase("A", 0, 0)]
        [TestCase("AQ", 0, 0)]
        [TestCase("AQI", 0, 0)]
        [TestCase("AQIDB", 4, 3)]
        [TestCase("AQIDBA", 4, 3)]
        [TestCase("AQIDBAU", 4, 3)]
        public void Basic_decoding_with_invalid_input_isFinalBlock_false___NeedMoreData(string input, int expectedConsumed, int expectedWritten)
        {
            var sut = new Base64UrlEncoder();
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

                byte[] expectedData = Convert.FromBase64String(input.Substring(0, consumed).FromBase64Url());
                CollectionAssert.AreEqual(expectedData, actualData);
            });
        }
    }
}
