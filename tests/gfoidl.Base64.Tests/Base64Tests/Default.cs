using System;
using System.Buffers;
using System.Runtime.InteropServices;
using NUnit.Framework;

namespace gfoidl.Base64.Tests.Base64Tests
{
    [TestFixture(typeof(byte))]
    [TestFixture(typeof(char))]
    public class Default<T> where T : unmanaged
    {
        [Test]
        public void Default___base64_is_used()
        {
            byte[] data     = { 0xFF, 0xFE, 0x00 };
            string expected = Convert.ToBase64String(data);

            string actual = Base64.Default.Encode(data);

            Assert.AreEqual(expected, actual);
        }
        //---------------------------------------------------------------------
        [Test]
        public void Default_with_buffers___base64_is_used()
        {
            byte[] data             = { 0x00 };
            const int encodedLength = 4;
            Span<T> base64          = stackalloc T[encodedLength];
            OperationStatus status;
            int consumed, written;

            if (typeof(T) == typeof(byte))
            {
                status = Base64.Default.Encode(data, MemoryMarshal.AsBytes(base64), out consumed, out written);
            }
            else if (typeof(T) == typeof(char))
            {
                status = Base64.Default.Encode(data, MemoryMarshal.Cast<T, char>(base64), out consumed, out written);
            }
            else
            {
                throw new NotSupportedException(); // just in case new types are introduced in the future
            }

            Assert.Multiple(() =>
            {
                Assert.AreEqual(OperationStatus.Done, status);
                Assert.AreEqual(1, consumed);
                Assert.AreEqual(4, written);
            });

            Span<byte> decoded = stackalloc byte[10];

            if (typeof(T) == typeof(byte))
            {
                status = Base64.Default.Decode(MemoryMarshal.AsBytes(base64), decoded, out consumed, out written);
            }
            else if (typeof(T) == typeof(char))
            {
                status = Base64.Default.Decode(MemoryMarshal.Cast<T, char>(base64), decoded, out consumed, out written);
            }
            else
            {
                throw new NotSupportedException(); // just in case new types are introduced in the future
            }

            Assert.Multiple(() =>
            {
                Assert.AreEqual(OperationStatus.Done, status);
                Assert.AreEqual(4, consumed);
                Assert.AreEqual(1, written);
            });
        }
    }
}
