using System;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using gfoidl.Base64.Internal;
using NUnit.Framework;

namespace gfoidl.Base64.Tests.Internal.AssertHelperTests
{
    [TestFixture]
    public partial class AssertRead
    {
        [Test]
        public void Avx_byte_read_and_srcLength_is_32()
        {
            Span<byte> source = GetSource(32);
            ref byte src      = ref source[0];
            ref byte srcStart = ref src;

            src.AssertRead<Vector256<sbyte>, byte>(ref srcStart, source.Length);
        }
        //---------------------------------------------------------------------
        [Test]
        public void Avx_byte_read_and_srcLength_is_33()
        {
            Span<byte> source = GetSource(33);
            ref byte src      = ref source[0];
            ref byte srcStart = ref src;

            src.AssertRead<Vector256<sbyte>, byte>(ref srcStart, source.Length);
        }
        //---------------------------------------------------------------------
        [Test]
        public void Avx_byte_read_and_srcLength_is_31___throws_InvalidOperation()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                Span<byte> source = GetSource(31);
                ref byte src      = ref source[0];
                ref byte srcStart = ref src;

                src.AssertRead<Vector256<sbyte>, byte>(ref srcStart, source.Length);
            });
        }
        //---------------------------------------------------------------------
        [Test]
        public void Avx_byte_read_with_offset_and_srcLength_is_32_plus_offset()
        {
            Span<byte> source = GetSource(32 + 3);
            ref byte src      = ref source[0];
            ref byte srcStart = ref src;

            src = ref Unsafe.Add(ref src, 3);

            src.AssertRead<Vector256<sbyte>, byte>(ref srcStart, source.Length);
        }
    }
}
