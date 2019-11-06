using System;
using System.Buffers;
using gfoidl.Base64.Internal;
using NUnit.Framework;

#if NETCOREAPP
using System.Runtime.Intrinsics.X86;
#endif

namespace gfoidl.Base64.Tests.Internal.Base64EncoderTests.Encode
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

            var sut  = new Base64Encoder();
            var data = new byte[50];
            var rnd  = new Random();
            rnd.NextBytes(data);

            int encodedLength = sut.GetEncodedLength(data.Length);
            Span<T> encoded   = new T[encodedLength];

            bool avx2Executed = false;
            Base64Encoder.Avx2Encoded += (s, e) => avx2Executed = true;

            OperationStatus status = sut.EncodeCore(data, encoded, out int consumed, out int written);

            Assert.IsTrue(avx2Executed);
        }
        //---------------------------------------------------------------------
        [Test]
        public void Guid___ssse3_event_fired()
        {
            var sut  = new Base64Encoder();
            var data = Guid.NewGuid().ToByteArray();

            int encodedLength = sut.GetEncodedLength(data.Length);
            Span<T> encoded   = new T[encodedLength];

            bool ssse3Executed = false;
            Base64Encoder.Ssse3Encoded += (s, e) => ssse3Executed = true;

            OperationStatus status = sut.EncodeCore(data, encoded, out int consumed, out int written);

            Assert.IsTrue(ssse3Executed);
        }
#endif
    }
}
