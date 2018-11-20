using System;
using System.Buffers;
using System.Runtime.InteropServices;
using System.Text;
using NUnit.Framework;

namespace gfoidl.Base64.Tests.Base64EncoderTests
{
    [TestFixture(typeof(byte))]
    [TestFixture(typeof(char))]
    public class Encode<T> where T : unmanaged
    {
        [Test]
        public void Empty_input()
        {
            var sut                   = new Base64Encoder();
            ReadOnlySpan<byte> source = ReadOnlySpan<byte>.Empty;

            Span<T> encoded        = new T[sut.GetEncodedLength(source.Length)];
            OperationStatus status = sut.EncodeCore(source, encoded, out int consumed, out int written);

            Assert.AreEqual(OperationStatus.Done, status);
            Assert.AreEqual(0, consumed);
            Assert.AreEqual(0, written);
            Assert.IsTrue(encoded.IsEmpty);
        }
        //---------------------------------------------------------------------
        [Test]
        public void Empty_input_encode_to_string___empty_string()
        {
            var sut                   = new Base64Encoder();
            ReadOnlySpan<byte> source = ReadOnlySpan<byte>.Empty;

            string actual = sut.Encode(source);

            Assert.AreEqual(string.Empty, actual);
        }
        //---------------------------------------------------------------------
        [Test]
        public void Data_of_various_length()
        {
            var sut   = new Base64Encoder();
            var bytes = new byte[byte.MaxValue + 1];

            for (int i = 0; i < bytes.Length; ++i)
                bytes[i] = (byte)i;

            for (int i = 0; i < 256; ++i)
            {
                Span<byte> source      = bytes.AsSpan(0, i + 1);
                Span<T> encoded        = new T[sut.GetEncodedLength(source.Length)];
                OperationStatus status = sut.EncodeCore(source, encoded, out int consumed, out int written);

                Assert.AreEqual(OperationStatus.Done, status);
                Assert.AreEqual(source.Length, consumed);
                Assert.AreEqual(encoded.Length, written);

                ReadOnlySpan<char> encodedText;

                if (typeof(T) == typeof(byte))
                {
                    Span<byte> encodedBytes = MemoryMarshal.AsBytes(encoded);
#if NETCOREAPP2_1 || NETCOREAPP3_0
                    encodedText = Encoding.ASCII.GetString(encodedBytes).AsSpan();
#else
                    encodedText = Encoding.ASCII.GetString(encodedBytes.ToArray()).AsSpan();
#endif
                }
                else if (typeof(T) == typeof(char))
                {
                    encodedText = MemoryMarshal.Cast<T, char>(encoded);
                }
                else
                {
                    throw new NotSupportedException(); // just in case new types are introduced in the future
                }
#if NETCOREAPP2_1 || NETCOREAPP3_0
                string expectedText = Convert.ToBase64String(source);
#else
                string expectedText = Convert.ToBase64String(source.ToArray());
#endif
                Assert.True(encodedText.SequenceEqual(expectedText.AsSpan()));
            }
        }
        //---------------------------------------------------------------------
        [Test]
        public void Data_of_various_length_encoded_to_string()
        {
            var sut   = new Base64Encoder();
            var bytes = new byte[byte.MaxValue + 1];

            for (int i = 0; i < bytes.Length; ++i)
                bytes[i] = (byte)i;

            for (int value = 0; value < 256; ++value)
            {
                Span<byte> source = bytes.AsSpan(0, value + 1);
                string actual     = sut.Encode(source);
#if NETCOREAPP2_1 || NETCOREAPP3_0
                string expected = Convert.ToBase64String(source);
#else
                string expected = Convert.ToBase64String(source.ToArray());
#endif
                Assert.AreEqual(expected, actual);
            }
        }
    }
}
