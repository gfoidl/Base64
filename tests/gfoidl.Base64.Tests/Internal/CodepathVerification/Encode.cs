using System;
using System.Buffers;
using gfoidl.Base64.Internal;
using NUnit.Framework;

#if NETCOREAPP
using System.Runtime.Intrinsics.X86;
#endif

namespace gfoidl.Base64.Tests.Internal.CodepathVerification
{
    [TestFixture(typeof(Base64Encoder)   , typeof(byte))]
    [TestFixture(typeof(Base64UrlEncoder), typeof(byte))]
    [TestFixture(typeof(Base64Encoder)   , typeof(char))]
    [TestFixture(typeof(Base64UrlEncoder), typeof(char))]
    public class Encode<TEncoder, T>
        where TEncoder : Base64EncoderImpl, new()
        where T        : unmanaged
    {
#if NETCOREAPP && DEBUG
        [Test]
        public void Data_size_32___avx2_executed_ssse3_not()
        {
            Assume.That(Avx2 .IsSupported, "AVX2 is not available");
            Assume.That(Ssse3.IsSupported, "SSSE3 is not available");

            var sut  = new TEncoder();
            var data = new byte[32];
            var rnd  = new Random(42);
            rnd.NextBytes(data);

            int encodedLength = sut.GetEncodedLength(data.Length);
            Span<T> encoded   = new T[encodedLength];

            int avx2Iterations  = 0;
            int ssse3Iterations = 0;
            bool avx2Executed   = false;
            bool ssse3Executed  = false;

            sut.Avx2EncodingIteration  += () => avx2Iterations++;
            sut.Ssse3EncodingIteration += () => ssse3Iterations++;
            sut.Avx2Encoded            += () => avx2Executed  = true;
            sut.Ssse3Encoded           += () => ssse3Executed = true;

            OperationStatus status = sut.EncodeCore(data, encoded, out int consumed, out int written);

            Assert.Multiple(() =>
            {
                Assert.IsTrue(avx2Executed        , nameof(avx2Executed));
                Assert.IsFalse(ssse3Executed      , nameof(ssse3Executed));
                Assert.AreEqual(1, avx2Iterations , nameof(avx2Iterations));
                Assert.AreEqual(0, ssse3Iterations, nameof(ssse3Iterations));
            });
        }
        //---------------------------------------------------------------------
        [Test]
        public void Data_size_1000___avx2_executed_ssse3_executed()
        {
            Assume.That(Avx2.IsSupported , "AVX2 is not available");
            Assume.That(Ssse3.IsSupported, "SSSE3 is not available");

            var sut  = new TEncoder();
            var data = new byte[1_000];
            var rnd  = new Random(42);
            rnd.NextBytes(data);

            int encodedLength = sut.GetEncodedLength(data.Length);
            Span<T> encoded   = new T[encodedLength];

            int avx2Iterations  = 0;
            int ssse3Iterations = 0;
            bool avx2Executed   = false;
            bool ssse3Executed  = false;

            sut.Avx2EncodingIteration  += () => avx2Iterations++;
            sut.Ssse3EncodingIteration += () => ssse3Iterations++;
            sut.Avx2Encoded            += () => avx2Executed  = true;
            sut.Ssse3Encoded           += () => ssse3Executed = true;

            OperationStatus status = sut.EncodeCore(data, encoded, out int consumed, out int written);

            Assert.Multiple(() =>
            {
                Assert.IsTrue(avx2Executed                 , nameof(avx2Executed));
                Assert.IsTrue(ssse3Executed                , nameof(ssse3Executed));
                Assert.Greater(avx2Iterations, 0           , nameof(avx2Iterations));
                Assert.Less(ssse3Iterations, avx2Iterations, nameof(ssse3Iterations));
            });
        }
        //---------------------------------------------------------------------
        [Test]
        public void Guid___ssse3_event_fired()
        {
            Assume.That(Ssse3.IsSupported, "SSSE3 is not available");

            var sut  = new TEncoder();
            var data = Guid.NewGuid().ToByteArray();

            int encodedLength = sut.GetEncodedLength(data.Length);
            Span<T> encoded   = new T[encodedLength];

            int avx2Iterations  = 0;
            int ssse3Iterations = 0;
            bool avx2Executed   = false;
            bool ssse3Executed  = false;

            sut.Avx2EncodingIteration  += () => avx2Iterations++;
            sut.Ssse3EncodingIteration += () => ssse3Iterations++;
            sut.Avx2Encoded            += () => avx2Executed  = true;
            sut.Ssse3Encoded           += () => ssse3Executed = true;

            OperationStatus status = sut.EncodeCore(data, encoded, out int consumed, out int written);

            Assert.Multiple(() =>
            {
                Assert.IsFalse(avx2Executed       , nameof(avx2Executed));
                Assert.IsTrue(ssse3Executed       , nameof(ssse3Executed));
                Assert.AreEqual(0, avx2Iterations , nameof(avx2Iterations));
                Assert.AreEqual(1, ssse3Iterations, nameof(ssse3Iterations));
            });
        }
#endif
    }
}
