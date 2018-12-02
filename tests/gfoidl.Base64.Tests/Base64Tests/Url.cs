using System;
using System.Buffers;
using System.Runtime.InteropServices;
using NUnit.Framework;

namespace gfoidl.Base64.Tests.Base64Tests
{
    [TestFixture(typeof(byte))]
    [TestFixture(typeof(char))]
    public class Url<T> where T : unmanaged
    {
        [Test]
        public void Url___base64Url_is_used()
        {
            byte[] data     = { 0xFF, 0xFE, 0x00 };
            string expected = Convert.ToBase64String(data).ToBase64Url();

            string actual = gfoidl.Base64.Base64.Url.Encode(data);

            Assert.AreEqual(expected, actual);
        }
        //---------------------------------------------------------------------
        [Test]
        public void Url_with_buffers___base64Url_is_used()
        {
            byte[] data             = { 0x00 };
            const int encodedLength = 2;
            Span<T> base64          = stackalloc T[encodedLength];
            OperationStatus status;
            int consumed, written;

            if (typeof(T) == typeof(byte))
            {
                status = Base64.Url.Encode(data, MemoryMarshal.AsBytes(base64), out consumed, out written);
            }
            else if (typeof(T) == typeof(char))
            {
                status = Base64.Url.Encode(data, MemoryMarshal.Cast<T, char>(base64), out consumed, out written);
            }
            else
            {
                throw new NotSupportedException(); // just in case new types are introduced in the future
            }

            Assert.Multiple(() =>
            {
                Assert.AreEqual(OperationStatus.Done, status);
                Assert.AreEqual(1, consumed);
                Assert.AreEqual(2, written);
            });

            Span<byte> decoded = stackalloc byte[10];

            if (typeof(T) == typeof(byte))
            {
                status = Base64.Url.Decode(MemoryMarshal.AsBytes(base64), decoded, out consumed, out written);
            }
            else if (typeof(T) == typeof(char))
            {
                status = Base64.Url.Decode(MemoryMarshal.Cast<T, char>(base64), decoded, out consumed, out written);
            }
            else
            {
                throw new NotSupportedException(); // just in case new types are introduced in the future
            }

            Assert.Multiple(() =>
            {
                Assert.AreEqual(OperationStatus.Done, status);
                Assert.AreEqual(2, consumed);
                Assert.AreEqual(1, written);
            });
        }
    }
}
