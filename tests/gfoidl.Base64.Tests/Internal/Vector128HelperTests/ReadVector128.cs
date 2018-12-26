using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using gfoidl.Base64.Internal;
using NUnit.Framework;

namespace gfoidl.Base64.Tests.Internal.Vector128HelperTests
{
    [TestFixture]
    public class ReadVector128
    {
        [Test]
        public void Byte___correct_combined_sbyte_vector()
        {
            byte[] chars     = new byte[16];
            sbyte[] expected = new sbyte[16];

            for (int i = 0; i < chars.Length; ++i)
            {
                chars[i]    = (byte) (i + 1);
                expected[i] = (sbyte)(i + 1);
            }

            Vector128<sbyte> vec = chars[0].ReadVector128();

            sbyte[] actual = new sbyte[16];
            Unsafe.As<sbyte, Vector128<sbyte>>(ref actual[0]) = vec;

            CollectionAssert.AreEqual(expected, actual);
        }
        //---------------------------------------------------------------------
        [Test]
        public void Char___correct_combined_sbyte_vector()
        {
            char[] chars     = new char[16];
            sbyte[] expected = new sbyte[16];

            for (int i = 0; i < chars.Length; ++i)
            {
                chars[i]    = (char) (i + 1);
                expected[i] = (sbyte)(i + 1);
            }

            Vector128<sbyte> vec = chars[0].ReadVector128();

            sbyte[] actual = new sbyte[16];
            Unsafe.As<sbyte, Vector128<sbyte>>(ref actual[0]) = vec;

            CollectionAssert.AreEqual(expected, actual);
        }
    }
}
