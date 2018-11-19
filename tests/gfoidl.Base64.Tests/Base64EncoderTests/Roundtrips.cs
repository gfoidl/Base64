using System;
using System.Buffers;
using System.Runtime.InteropServices;
using System.Text;
using NUnit.Framework;

namespace gfoidl.Base64.Tests.Base64EncoderTests
{
    [TestFixture(typeof(byte))]
    [TestFixture(typeof(char))]
    public  class Roundtrips<T> where T : unmanaged
    {
        [Test]
        public void Data_of_various_length___roundtrips_correclty()
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
                int decodedLength;

                if (typeof(T) == typeof(byte))
                {
                    Span<byte> encodedBytes = MemoryMarshal.AsBytes(encoded);
                    encodedText = Encoding.ASCII.GetString(encodedBytes).AsSpan();
                    decodedLength = sut.GetDecodedLength(encodedBytes);
                }
                else if (typeof(T) == typeof(char))
                {
                    encodedText = MemoryMarshal.Cast<T, char>(encoded);
                    decodedLength = sut.GetDecodedLength(encodedText);
                }
                else
                {
                    throw new NotSupportedException(); // just in case new types are introduced in the future
                }

                string expectedText = Convert.ToBase64String(source);
                Assert.True(encodedText.SequenceEqual(expectedText));

                Span<byte> decoded = new byte[decodedLength];
                status = sut.DecodeCore<T>(encoded, decoded, out consumed, out written);

                Assert.AreEqual(OperationStatus.Done, status);
                Assert.AreEqual(encoded.Length, consumed);
                Assert.AreEqual(decodedLength, written);

                Assert.True(source.SequenceEqual(decoded));
            }
        }
        //---------------------------------------------------------------------
        [Test]
        public void Data_of_various_length___roundtrips_correclty_as_string()
        {
            var sut   = new Base64Encoder();
            var bytes = new byte[byte.MaxValue + 1];

            for (int i = 0; i < bytes.Length; ++i)
                bytes[i] = (byte)i;

            for (int i = 0; i < 256; ++i)
            {
                Span<byte> source      = bytes.AsSpan(0, i + 1);
                string encoded         = sut.Encode(source);
                string encodedExpected = Convert.ToBase64String(source);

                Assert.AreEqual(encodedExpected, encoded);

                Span<byte> decoded = sut.Decode(encoded);

                Assert.IsTrue(source.SequenceEqual(decoded));
            }
        }
    }
}
