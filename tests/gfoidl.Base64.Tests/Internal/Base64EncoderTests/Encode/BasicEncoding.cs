using System;
using System.Buffers;
using System.Runtime.InteropServices;
using System.Text;
using gfoidl.Base64.Internal;
using NUnit.Framework;

namespace gfoidl.Base64.Tests.Internal.Base64EncoderTests.Encode
{
    [TestFixture(typeof(byte))]
    [TestFixture(typeof(char))]
    public class BasicEncoding<T> where T : unmanaged
    {
        [Test]
        public void Data_of_various_length()
        {
            var sut   = new Base64Encoder();
            var bytes = new byte[byte.MaxValue + 1];

            for (int i = 0; i < bytes.Length; ++i)
                bytes[i] = (byte)(255 - i);

            for (int i = 0; i < 256; ++i)
            {
                Span<byte> source      = bytes.AsSpan(0, i + 1);
                Span<T> encoded        = new T[sut.GetEncodedLength(source.Length)];
                OperationStatus status = sut.EncodeCore(source, encoded, out int consumed, out int written);

                Assert.AreEqual(OperationStatus.Done, status);
                Assert.AreEqual(source.Length       , consumed);
                Assert.AreEqual(encoded.Length      , written);

                string encodedText;

                if (typeof(T) == typeof(byte))
                {
                    Span<byte> encodedBytes = MemoryMarshal.AsBytes(encoded);
#if NETCOREAPP
                    encodedText = Encoding.ASCII.GetString(encodedBytes);
#else
                    encodedText = Encoding.ASCII.GetString(encodedBytes.ToArray());
#endif
                }
                else if (typeof(T) == typeof(char))
                {
#if NETCOREAPP
                    encodedText = new string(MemoryMarshal.Cast<T, char>(encoded));
#else
                    encodedText = new string(MemoryMarshal.Cast<T, char>(encoded).ToArray());
#endif
                }
                else
                {
                    throw new NotSupportedException(); // just in case new types are introduced in the future
                }
#if NETCOREAPP
                string expected = Convert.ToBase64String(source);
#else
                string expected = Convert.ToBase64String(source.ToArray());
#endif
                Assert.AreEqual(expected, encodedText);
            }
        }
        //---------------------------------------------------------------------
        [Test]
        public void Data_of_various_length_isFinalBlock_false()
        {
            var sut   = new Base64Encoder();
            var bytes = new byte[byte.MaxValue + 1];

            for (int i = 0; i < bytes.Length; ++i)
                bytes[i] = (byte)(255 - i);

            for (int i = 0; i < 256; ++i)
            {
                Span<byte> source      = bytes.AsSpan(0, i + 1);
                Span<T> encoded        = new T[sut.GetEncodedLength(source.Length)];
                OperationStatus status = sut.EncodeCore(source, encoded, out int consumed, out int written, isFinalBlock: false);

                OperationStatus expectedStatus = source.Length % 3 == 0 ? OperationStatus.Done : OperationStatus.NeedMoreData;
                int expectedConsumed           = source.Length / 3 * 3;
                int expectedWritten            = expectedConsumed / 3 * 4;

                Assert.AreEqual(expectedStatus  , status, "fail at length = {0}", i + 1);
                Assert.AreEqual(expectedConsumed, consumed);
                Assert.AreEqual(expectedWritten , written);

                string encodedText;
                source  = source .Slice(0, consumed);
                encoded = encoded.Slice(0, written);

                if (typeof(T) == typeof(byte))
                {
                    Span<byte> encodedBytes = MemoryMarshal.AsBytes(encoded);
#if NETCOREAPP
                    encodedText = Encoding.ASCII.GetString(encodedBytes);
#else
                    encodedText = Encoding.ASCII.GetString(encodedBytes.ToArray());
#endif
                }
                else if (typeof(T) == typeof(char))
                {
#if NETCOREAPP
                    encodedText = new string(MemoryMarshal.Cast<T, char>(encoded));
#else
                    encodedText = new string(MemoryMarshal.Cast<T, char>(encoded).ToArray());
#endif
                }
                else
                {
                    throw new NotSupportedException(); // just in case new types are introduced in the future
                }
#if NETCOREAPP
                string expected = Convert.ToBase64String(source);
#else
                string expected = Convert.ToBase64String(source.ToArray());
#endif
                Assert.AreEqual(expected, encodedText);
            }
        }
        //---------------------------------------------------------------------
        [Test]
        public void Data_of_various_length_encoded_to_string()
        {
            var sut   = new Base64Encoder();
            var bytes = new byte[byte.MaxValue + 1];

            for (int i = 0; i < bytes.Length; ++i)
                bytes[i] = (byte)(255 - i);

            for (int value = 0; value < 256; ++value)
            {
                Span<byte> source = bytes.AsSpan(0, value + 1);
                string actual     = sut.Encode(source);
#if NETCOREAPP
                string expected = Convert.ToBase64String(source);
#else
                string expected = Convert.ToBase64String(source.ToArray());
#endif
                Assert.AreEqual(expected, actual);
            }
        }
        //---------------------------------------------------------------------
        [Test]
        [TestCase(1, "AQ==")]
        [TestCase(2, "AQI=")]
        [TestCase(3, "AQID")]
        [TestCase(4, "AQIDBA==")]
        [TestCase(5, "AQIDBAU=")]
        [TestCase(6, "AQIDBAUG")]
        [TestCase(7, "AQIDBAUGBw==")]
        public void Basic_encoding_with_known_input(int numBytes, string expectedText)
        {
            var sut              = new Base64Encoder();
            int expectedConsumed = numBytes;
            int expectedWritten  = expectedText.Length;

            Span<byte> source = new byte[numBytes];
            for (int i = 0; i < source.Length; ++i)
            {
                source[i] = (byte)(i + 1);
            }

            Span<T> encoded = new T[sut.GetEncodedLength(source.Length)];

            OperationStatus status = sut.EncodeCore(source, encoded, out int consumed, out int written);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(OperationStatus.Done, status);
                Assert.AreEqual(expectedConsumed    , consumed);
                Assert.AreEqual(expectedWritten     , written);
            });

            string encodedText;

            if (typeof(T) == typeof(byte))
            {
                Span<byte> encodedBytes = MemoryMarshal.AsBytes(encoded);
#if NETCOREAPP
                encodedText = Encoding.ASCII.GetString(encodedBytes);
#else
                encodedText = Encoding.ASCII.GetString(encodedBytes.ToArray());
#endif
            }
            else if (typeof(T) == typeof(char))
            {
#if NETCOREAPP
                encodedText = new string(MemoryMarshal.Cast<T, char>(encoded));
#else
                encodedText = new string(MemoryMarshal.Cast<T, char>(encoded).ToArray());
#endif
            }
            else
            {
                throw new NotSupportedException(); // just in case new types are introduced in the future
            }

            Assert.AreEqual(expectedText, encodedText);
        }
        //---------------------------------------------------------------------
        [Test]
        [TestCase(1, "", 0, 0)]
        [TestCase(2, "", 0, 0)]
        [TestCase(3, "AQID", 3, 4)]
        [TestCase(4, "AQID", 3, 4)]
        [TestCase(5, "AQID", 3, 4)]
        [TestCase(6, "AQIDBAUG", 6, 8)]
        [TestCase(7, "AQIDBAUG", 6, 8)]
        public void Basic_encoding_with_known_input_isFinalBlock_false(int numBytes, string expectedText, int expectedConsumed, int expectedWritten)
        {
            var sut = new Base64Encoder();

            Span<byte> source = new byte[numBytes];
            for (int i = 0; i < source.Length; ++i)
            {
                source[i] = (byte)(i + 1);
            }

            Span<T> encoded = new T[sut.GetEncodedLength(source.Length)];

            OperationStatus status         = sut.EncodeCore(source, encoded, out int consumed, out int written, isFinalBlock: false);
            OperationStatus expectedStatus = source.Length % 3 == 0 ? OperationStatus.Done : OperationStatus.NeedMoreData;
            
            Assert.Multiple(() =>
            {
                Assert.AreEqual(expectedStatus  , status);
                Assert.AreEqual(expectedConsumed, consumed);
                Assert.AreEqual(expectedWritten , written);
            });

            string encodedText;
            source  = source .Slice(0, consumed);
            encoded = encoded.Slice(0, written);

            if (typeof(T) == typeof(byte))
            {
                Span<byte> encodedBytes = MemoryMarshal.AsBytes(encoded);
#if NETCOREAPP
                encodedText = Encoding.ASCII.GetString(encodedBytes);
#else
                encodedText = Encoding.ASCII.GetString(encodedBytes.ToArray());
#endif
            }
            else if (typeof(T) == typeof(char))
            {
#if NETCOREAPP
                encodedText = new string(MemoryMarshal.Cast<T, char>(encoded));
#else
                encodedText = new string(MemoryMarshal.Cast<T, char>(encoded).ToArray());
#endif
            }
            else
            {
                throw new NotSupportedException(); // just in case new types are introduced in the future
            }

            Assert.AreEqual(expectedText, encodedText);
        }
        //---------------------------------------------------------------------
        [Test]
        [TestCase(1, "AQ==")]
        [TestCase(2, "AQI=")]
        [TestCase(3, "AQID")]
        [TestCase(4, "AQIDBA==")]
        [TestCase(5, "AQIDBAU=")]
        [TestCase(6, "AQIDBAUG")]
        [TestCase(7, "AQIDBAUGBw==")]
        public void Basic_encoding_with_known_input_to_string(int numBytes, string expectedText)
        {
            var sut = new Base64Encoder();

            Span<byte> source = new byte[numBytes];
            for (int i = 0; i < source.Length; ++i)
            {
                source[i] = (byte)(i + 1);
            }

            string encodedText = sut.Encode(source);

            Assert.AreEqual(expectedText, encodedText);
        }
    }
}
