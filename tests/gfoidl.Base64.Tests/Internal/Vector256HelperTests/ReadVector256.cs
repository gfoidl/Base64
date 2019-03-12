using System;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using gfoidl.Base64.Internal;
using NUnit.Framework;

namespace gfoidl.Base64.Tests.Internal.Vector256HelperTests
{
    [TestFixture]
    public class ReadVector256
    {
        [Test]
        public void ROSpan___correct_sbyte_vector()
        {
            Assume.That(Avx2.IsSupported);

            ReadOnlySpan<sbyte> data = new sbyte[] { 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27, 0x28, 0x29, 0x30, 0x31, 0x32 };

            Vector256<sbyte> vec = data.ReadVector256();

            sbyte[] expected = data.ToArray();
            sbyte[] actual = new sbyte[32];
            Unsafe.As<sbyte, Vector256<sbyte>>(ref actual[0]) = vec;

            CollectionAssert.AreEqual(expected, actual);
        }
        //---------------------------------------------------------------------
        [Test]
        public void Byte___correct_combined_sbyte_vector()
        {
            Assume.That(Avx2.IsSupported);

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
            Assume.That(Avx2.IsSupported);

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
