using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using gfoidl.Base64.Internal;
using NUnit.Framework;

namespace gfoidl.Base64.Tests.Internal.Vector256HelperTests
{
    [TestFixture]
    public class WriteVector256
    {
        [Test]
        public void Byte___correct_data_written()
        {
            Assume.That(Avx2.IsSupported);

            sbyte[] data    = new sbyte[32];
            byte[] expected = new byte[32];

            for (int i = 0; i < data.Length; ++i)
            {
                data[i]     = (sbyte)(i + 1);
                expected[i] = (byte) (i + 1);
            }

            Vector256<sbyte> vec = Unsafe.As<sbyte, Vector256<sbyte>>(ref data[0]);

            byte[] actual = new byte[32];
            actual[0].WriteVector256(vec);

            CollectionAssert.AreEqual(expected, actual);
        }
        //---------------------------------------------------------------------
        [Test]
        public void Char___correct_data_written()
        {
            Assume.That(Avx2.IsSupported);

            sbyte[] data    = new sbyte[32];
            char[] expected = new char[32];

            for (int i = 0; i < data.Length; ++i)
            {
                data[i]     = (sbyte)(i + 1);
                expected[i] = (char) (i + 1);
            }

            Vector256<sbyte> vec = Unsafe.As<sbyte, Vector256<sbyte>>(ref data[0]);

            char[] actual = new char[32];
            actual[0].WriteVector256(vec);

            CollectionAssert.AreEqual(expected, actual);
        }
    }
}
