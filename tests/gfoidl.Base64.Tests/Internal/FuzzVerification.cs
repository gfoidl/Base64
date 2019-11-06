using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text;
using gfoidl.Base64.Internal;
using NUnit.Framework;

namespace gfoidl.Base64.Tests.Internal
{
    [TestFixture(typeof(Base64Encoder))]
    [TestFixture(typeof(Base64UrlEncoder))]
    public class FuzzVerification<T> where T : IBase64, new()
    {
        private const string BasePath     = "./data/fuzz-findings/decoding";
        private readonly IBase64 _encoder = new T();
        //---------------------------------------------------------------------
        [Test, TestCaseSource(nameof(Invalid_data_TestCases))]
        public void Invalid_data(string fileName)
        {
            var sut        = _encoder;
            byte[] encoded = File.ReadAllBytes(fileName);

            TestContext.WriteLine(Encoding.UTF8.GetString(encoded));

            Span<byte> data        = new byte[sut.GetMaxDecodedLength(encoded.Length)];
            OperationStatus status = sut.Decode(encoded, data, out int _, out int _);

            Assert.AreEqual(OperationStatus.InvalidData, status);
        }
        //---------------------------------------------------------------------
        private static IEnumerable<TestCaseData> Invalid_data_TestCases()
        {
            foreach (string file in Directory.EnumerateFiles(Path.Combine(TestContext.CurrentContext.TestDirectory, BasePath, "InvalidData")))
            {
                string fileName = Path.GetFileName(file);
                yield return new TestCaseData(file).SetArgDisplayNames(fileName);
            }
        }
    }
}
