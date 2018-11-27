using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using gfoidl.Base64.Internal;
using NUnit.Framework;

namespace gfoidl.Base64.Tests.Internal.Avx2HelperTests
{
    [TestFixture]
    public class Read
    {
        [Test]
        public void Correct_combined_sbyte_vector()
        {
            char[] chars     = new char[32];
            sbyte[] expected = new sbyte[32];

            for (int i = 0; i < chars.Length; ++i)
            {
                chars[i]    = (char) (i + 1);
                expected[i] = (sbyte)(i + 1);
            }

            Vector256<sbyte> vec = Avx2Helper.Read(ref chars[0]);

            sbyte[] actual = new sbyte[32];
            Unsafe.As<sbyte, Vector256<sbyte>>(ref actual[0]) = vec;

            CollectionAssert.AreEqual(expected, actual);
        }
    }
}
