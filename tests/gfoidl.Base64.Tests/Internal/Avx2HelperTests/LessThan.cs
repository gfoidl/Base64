using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using gfoidl.Base64.Internal;
using NUnit.Framework;

namespace gfoidl.Base64.Tests.Internal.Avx2HelperTests
{
    public class LessThan
    {
        private static readonly Vector256<sbyte> s_zero    = Avx.SetZeroVector256<sbyte>();
        private static readonly Vector256<sbyte> s_one     = Avx.SetAllVector256<sbyte>(1);
        private static readonly Vector256<sbyte> s_allOnes = Avx.SetAllVector256<sbyte>(-1);
        //---------------------------------------------------------------------
        [Test]
        public void Zero_and_zero___false()
        {
            Vector256<sbyte> left  = s_zero;
            Vector256<sbyte> right = s_zero;

            Vector256<sbyte> res = Avx2Helper.LessThan(left, right);

            bool actual = Avx.TestC(res, s_allOnes);

            Assert.IsFalse(actual);
        }
        //---------------------------------------------------------------------
        [Test]
        public void One_and_zero___false()
        {
            Vector256<sbyte> left  = s_one;
            Vector256<sbyte> right = s_zero;

            Vector256<sbyte> res = Avx2Helper.LessThan(left, right);

            bool actual = Avx.TestC(res, s_allOnes);

            Assert.IsFalse(actual);
        }
        //---------------------------------------------------------------------
        [Test]
        public void Zero_and_one___true()
        {
            Vector256<sbyte> left  = s_zero;
            Vector256<sbyte> right = s_one;

            Vector256<sbyte> res = Avx2Helper.LessThan(left, right);

            bool actual = Avx.TestC(res, s_allOnes);

            Assert.IsTrue(actual);
        }
        //---------------------------------------------------------------------
        [Test]
        public void Only_one_zero_and_one___true()
        {
            Vector256<sbyte> left  = s_one;
            Vector256<sbyte> right = s_one;

            left = Avx.Insert(left, 0, index: 3);

            Vector256<sbyte> res = Avx2Helper.LessThan(left, right);

            var expected = new sbyte[32];
            expected[3]  = -1;

            var actual = new sbyte[32];
            Unsafe.As<sbyte, Vector256<sbyte>>(ref actual[0]) = res;

            CollectionAssert.AreEqual(expected, actual);
        }
        //---------------------------------------------------------------------
        [Test]
        public void One_4_in_5s_and_5s___true()
        {
            Vector256<sbyte> left  = Avx.SetAllVector256<sbyte>(5);
            Vector256<sbyte> right = Avx.SetAllVector256<sbyte>(5);

            left = Avx.Insert(left, 4, index: 7);

            Vector256<sbyte> res = Avx2Helper.LessThan(left, right);

            var expected = new sbyte[32];
            expected[7]  = -1;

            var actual = new sbyte[32];
            Unsafe.As<sbyte, Vector256<sbyte>>(ref actual[0]) = res;

            CollectionAssert.AreEqual(expected, actual);
        }
    }
}
