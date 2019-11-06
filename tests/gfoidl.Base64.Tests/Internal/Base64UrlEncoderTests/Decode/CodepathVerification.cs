using System;
using System.Buffers;
using System.Runtime.InteropServices;
using gfoidl.Base64.Internal;
using NUnit.Framework;

#if NETCOREAPP
using System.Runtime.Intrinsics.X86;
#endif

namespace gfoidl.Base64.Tests.Internal.Base64UrlEncoderTests.Decode
{
    [TestFixture(typeof(byte))]
    [TestFixture(typeof(char))]
    public class CodepathVerification<T> where T : unmanaged
    {
#if NETCOREAPP && DEBUG
        [Test]
        public void Large_data___avx2_event_fired()
        {
            Assume.That(Avx2.IsSupported);

            var sut  = new Base64UrlEncoder();
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
            Base64UrlEncoder.Avx2Decoded += (s, e) => avxExecuted = true;

            status = sut.DecodeCore<T>(encoded, decoded, out int _, out int _);
            Assume.That(status, Is.EqualTo(OperationStatus.Done));

            Assert.IsTrue(avxExecuted);
        }
        //---------------------------------------------------------------------
        [Test]
        public void Large_data_but_to_small_for_avx2___ssse3_event_fired()
        {
            var sut  = new Base64UrlEncoder();
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

            bool ssse3Executed = false;
            Base64UrlEncoder.Ssse3Decoded += (s, e) => ssse3Executed = true;

            status = sut.DecodeCore<T>(encoded, decoded, out int _, out int _);
            Assume.That(status, Is.EqualTo(OperationStatus.Done));

            Assert.IsTrue(ssse3Executed);
        }
        //---------------------------------------------------------------------
        [Test]
        public void Guid___ssse3_event_fired()
        {
            var sut  = new Base64UrlEncoder();
            var data = Guid.NewGuid().ToByteArray();

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

            bool ssse3Executed = false;
            Base64UrlEncoder.Ssse3Decoded += (s, e) => ssse3Executed = true;

            status = sut.DecodeCore<T>(encoded, decoded, out int _, out int _);
            Assume.That(status, Is.EqualTo(OperationStatus.Done));

            Assert.IsTrue(ssse3Executed);
        }
#endif
    }
}
