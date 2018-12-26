using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using gfoidl.Base64.Internal;
using NUnit.Framework;

namespace gfoidl.Base64.Tests.Internal.Vector128HelperTests
{
    [TestFixture]
    public class WriteVector128
    {
        [Test]
        public void Byte___correct_data_written()
        {
            sbyte[] data    = new sbyte[16];
            byte[] expected = new byte[16];

            for (int i = 0; i < data.Length; ++i)
            {
                data[i]     = (sbyte)(i + 1);
                expected[i] = (byte) (i + 1);
            }

            Vector128<sbyte> vec = Unsafe.As<sbyte, Vector128<sbyte>>(ref data[0]);

            byte[] actual = new byte[16];
            actual[0].WriteVector128(vec);

            CollectionAssert.AreEqual(expected, actual);
        }
        //---------------------------------------------------------------------
        [Test]
        public void Char___correct_data_written()
        {
            sbyte[] data    = new sbyte[16];
            char[] expected = new char[16];

            for (int i = 0; i < data.Length; ++i)
            {
                data[i]     = (sbyte)(i + 1);
                expected[i] = (char) (i + 1);
            }

            Vector128<sbyte> vec = Unsafe.As<sbyte, Vector128<sbyte>>(ref data[0]);

            char[] actual = new char[16];
            actual[0].WriteVector128(vec);

            CollectionAssert.AreEqual(expected, actual);
        }
    }
}
