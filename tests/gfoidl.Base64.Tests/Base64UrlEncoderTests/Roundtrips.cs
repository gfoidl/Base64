using System;
using System.Buffers;
using System.Runtime.InteropServices;
using System.Text;
using NUnit.Framework;

namespace gfoidl.Base64.Tests.Base64UrlEncoderTests
{
    [TestFixture(typeof(byte))]
    [TestFixture(typeof(char))]
    public  class Roundtrips<T> where T : unmanaged
    {
        [Test]
        public void Data_of_various_length___roundtrips_correclty()
        {
            var sut   = new Base64UrlEncoder();
            var bytes = new byte[byte.MaxValue + 1];

            for (int i = 0; i < bytes.Length; ++i)
                bytes[i] = (byte)(255 - i);

            for (int i = 0; i < 256; ++i)
            {
                Span<byte> source      = bytes.AsSpan(0, i + 1);
                Span<T> encoded        = new T[sut.GetEncodedLength(source.Length)];
                OperationStatus status = sut.EncodeCore(source, encoded, out int consumed, out int written);

                Assert.AreEqual(OperationStatus.Done, status);
                Assert.AreEqual(source.Length, consumed);
                Assert.AreEqual(encoded.Length, written);

                string encodedText;
                int decodedLength;

                if (typeof(T) == typeof(byte))
                {
                    Span<byte> encodedBytes = MemoryMarshal.AsBytes(encoded);
#if NETCOREAPP
                    encodedText = Encoding.ASCII.GetString(encodedBytes);
#else
                    encodedText = Encoding.ASCII.GetString(encodedBytes.ToArray());
#endif
                    decodedLength = sut.GetDecodedLength(encodedBytes);
                }
                else if (typeof(T) == typeof(char))
                {
#if NETCOREAPP
                    encodedText = new string(MemoryMarshal.Cast<T, char>(encoded));
#else
                    encodedText = new string(MemoryMarshal.Cast<T, char>(encoded).ToArray());
#endif
                    decodedLength = sut.GetDecodedLength(encodedText.AsSpan());
                }
                else
                {
                    throw new NotSupportedException(); // just in case new types are introduced in the future
                }
#if NETCOREAPP
                string expectedText = Convert.ToBase64String(source);
#else
                string expectedText = Convert.ToBase64String(source.ToArray());
#endif
                Assert.AreEqual(expectedText.ToBase64Url(), encodedText);

                Span<byte> decoded = new byte[decodedLength];
                status             = sut.DecodeCore<T>(encoded, decoded, out consumed, out written);

                Assert.AreEqual(OperationStatus.Done, status);
                Assert.AreEqual(encoded.Length, consumed);
                Assert.AreEqual(decodedLength , written);

                CollectionAssert.AreEqual(source.ToArray(), decoded.ToArray());
            }
        }
        //---------------------------------------------------------------------
        [Test]
        public void Data_of_various_length___roundtrips_correclty_as_string()
        {
            var sut   = new Base64UrlEncoder();
            var bytes = new byte[byte.MaxValue + 1];

            for (int i = 0; i < bytes.Length; ++i)
                bytes[i] = (byte)(255 - i);

            for (int i = 0; i < 256; ++i)
            {
                Span<byte> source      = bytes.AsSpan(0, i + 1);
                string encoded         = sut.Encode(source);
#if NETCOREAPP
                string encodedExpected = Convert.ToBase64String(source);
#else
                string encodedExpected = Convert.ToBase64String(source.ToArray());
#endif
                Assert.AreEqual(encodedExpected.ToBase64Url(), encoded);

                Span<byte> decoded = sut.Decode(encoded.AsSpan());

                CollectionAssert.AreEqual(source.ToArray(), decoded.ToArray());
            }
        }
    }
}
