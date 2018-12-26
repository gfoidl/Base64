using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using gfoidl.Base64.Internal;
using NUnit.Framework;

namespace gfoidl.Base64.Tests.Internal.Vector256HelperTests
{
    [TestFixture]
    public class ReadVector256
    {
        [Test]
        public void Byte___correct_combined_sbyte_vector()
        {
            byte[] chars     = new byte[32];
            sbyte[] expected = new sbyte[32];

            for (int i = 0; i < chars.Length; ++i)
            {
                chars[i]    = (byte) (i + 1);
                expected[i] = (sbyte)(i + 1);
            }

            Vector256<sbyte> vec = chars[0].ReadVector256();

            sbyte[] actual = new sbyte[32];
            Unsafe.As<sbyte, Vector256<sbyte>>(ref actual[0]) = vec;

            CollectionAssert.AreEqual(expected, actual);
        }
        //---------------------------------------------------------------------
        [Test]
        public void Char___correct_combined_sbyte_vector()
        {
            char[] chars     = new char[32];
            sbyte[] expected = new sbyte[32];

            for (int i = 0; i < chars.Length; ++i)
            {
                chars[i]    = (char) (i + 1);
                expected[i] = (sbyte)(i + 1);
            }

            Vector256<sbyte> vec = chars[0].ReadVector256();

            sbyte[] actual = new sbyte[32];
            Unsafe.As<sbyte, Vector256<sbyte>>(ref actual[0]) = vec;

            CollectionAssert.AreEqual(expected, actual);
        }
    }
}
