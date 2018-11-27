using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using gfoidl.Base64.Internal;
using NUnit.Framework;

namespace gfoidl.Base64.Tests.Internal.Avx2HelperTests
{
    [TestFixture]
    public class Write
    {
        [Test]
        public void Correct_data_written()
        {
            sbyte[] data    = new sbyte[32];
            char[] expected = new char[32];

            for (int i = 0; i < data.Length; ++i)
            {
                data[i]     = (sbyte)(i + 1);
                expected[i] = (char) (i + 1);
            }

            Vector256<sbyte> vec = Unsafe.As<sbyte, Vector256<sbyte>>(ref data[0]);

            char[] actual = new char[32];
            Avx2Helper.Write(vec, ref actual[0]);

            CollectionAssert.AreEqual(expected, actual);
        }
    }
}
