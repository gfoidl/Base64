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
    public class BufferChains<T> where T : unmanaged
    {
        [Test]
        public void Buffer_chain_basic_encode()
        {
            var sut  = new Base64Encoder();
            var data = new byte[200];
            var rnd  = new Random(0);
            rnd.NextBytes(data);

            int encodedLength = sut.GetEncodedLength(data.Length);
            Span<T> encoded   = new T[encodedLength];

            OperationStatus status = sut.EncodeCore(data.AsSpan(0, data.Length / 2), encoded, out int consumed, out int written1, isFinalBlock: false);
            Assert.AreEqual(OperationStatus.NeedMoreData, status);

            status = sut.EncodeCore(data.AsSpan(consumed), encoded.Slice(written1), out consumed, out int written2, isFinalBlock: true);
            Assert.AreEqual(OperationStatus.Done, status);
            Assert.AreEqual(encodedLength, written1 + written2);

            Span<T> expected = new T[encodedLength];
            sut.EncodeCore(data, expected, out int _, out int _);

            string encodedText;
            string expectedText;

            if (typeof(T) == typeof(byte))
            {
                Span<byte> encodedBytes  = MemoryMarshal.AsBytes(encoded);
                Span<byte> expectedBytes = MemoryMarshal.AsBytes(expected);
#if NETCOREAPP
                encodedText  = Encoding.ASCII.GetString(encodedBytes);
                expectedText = Encoding.ASCII.GetString(expectedBytes);
#else
                encodedText  = Encoding.ASCII.GetString(encodedBytes .ToArray());
                expectedText = Encoding.ASCII.GetString(expectedBytes.ToArray());
#endif
            }
            else if (typeof(T) == typeof(char))
            {
#if NETCOREAPP
                encodedText  = new string(MemoryMarshal.Cast<T, char>(encoded));
                expectedText = new string(MemoryMarshal.Cast<T, char>(expected));
#else
                encodedText  = new string(MemoryMarshal.Cast<T, char>(encoded) .ToArray());
                expectedText = new string(MemoryMarshal.Cast<T, char>(expected).ToArray());
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
        public void Buffer_chain_various_length_encode()
        {
            var sut = new Base64Encoder();
            var rnd = new Random(0);

            for (int i = 2; i < 200; ++i)
            {
                var data = new byte[i];
                rnd.NextBytes(data);

                int encodedLength = sut.GetEncodedLength(data.Length);
                Span<T> encoded   = new T[encodedLength];

                int partialLength      = data.Length / 2;
                OperationStatus status = sut.EncodeCore(data.AsSpan(0, partialLength), encoded, out int consumed, out int written1, isFinalBlock: false);
                Assert.AreEqual(partialLength % 3 == 0 ? OperationStatus.Done : OperationStatus.NeedMoreData, status, "fail at i = {0}", i);

                status = sut.EncodeCore(data.AsSpan(consumed), encoded.Slice(written1), out consumed, out int written2, isFinalBlock: true);
                Assert.AreEqual(OperationStatus.Done, status, "fail at i = {0}", i);
                Assert.AreEqual(encodedLength, written1 + written2, "fail at i = {0}", i);

                Span<T> expected = new T[encodedLength];
                sut.EncodeCore(data, expected, out int _, out int _);

                string encodedText;
                string expectedText;

                if (typeof(T) == typeof(byte))
                {
                    Span<byte> encodedBytes  = MemoryMarshal.AsBytes(encoded);
                    Span<byte> expectedBytes = MemoryMarshal.AsBytes(expected);
#if NETCOREAPP
                    encodedText  = Encoding.ASCII.GetString(encodedBytes);
                    expectedText = Encoding.ASCII.GetString(expectedBytes);
#else
                    encodedText  = Encoding.ASCII.GetString(encodedBytes .ToArray());
                    expectedText = Encoding.ASCII.GetString(expectedBytes.ToArray());
#endif
                }
                else if (typeof(T) == typeof(char))
                {
#if NETCOREAPP
                    encodedText  = new string(MemoryMarshal.Cast<T, char>(encoded));
                    expectedText = new string(MemoryMarshal.Cast<T, char>(expected));
#else
                    encodedText  = new string(MemoryMarshal.Cast<T, char>(encoded) .ToArray());
                    expectedText = new string(MemoryMarshal.Cast<T, char>(expected).ToArray());
#endif
                }
                else
                {
                    throw new NotSupportedException(); // just in case new types are introduced in the future
                }

                Assert.AreEqual(expectedText, encodedText, "fail at i = {0}", i);
            }
        }
    }
}
