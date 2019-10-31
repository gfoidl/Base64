using System;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using gfoidl.Base64.Internal;
using NUnit.Framework;


namespace gfoidl.Base64.Tests.Internal.AssertHelperTests
{
    [TestFixture]
    public partial class AssertWrite
    {
        [Test]
        public void Avx_byte_write_and_destLength_is_32()
        {
            Span<byte> destination = GetDestination(32);
            ref byte dest          = ref destination[0];
            ref byte destStart     = ref dest;

            dest.AssertWrite<Vector256<sbyte>, byte>(ref destStart, destination.Length);
        }
        //---------------------------------------------------------------------
        [Test]
        public void Avx_byte_write_and_srcLength_is_33()
        {
            Span<byte> destination = GetDestination(33);
            ref byte dest          = ref destination[0];
            ref byte destStart     = ref dest;

            dest.AssertWrite<Vector256<sbyte>, byte>(ref destStart, destination.Length);
        }
        //---------------------------------------------------------------------
        [Test]
        public void Avx_byte_write_and_srcLength_is_31___throws_InvalidOperation()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                Span<byte> destination = GetDestination(31);
                ref byte dest          = ref destination[0];
                ref byte destStart     = ref dest;

                dest.AssertWrite<Vector256<sbyte>, byte>(ref destStart, destination.Length);
            });
        }
        //---------------------------------------------------------------------
        [Test]
        public void Avx_byte_write_with_offset_and_srcLength_is_32_plus_offset()
        {
            Span<byte> destination = GetDestination(32 + 3);
            ref byte dest          = ref destination[0];
            ref byte destStart     = ref dest;

            dest = ref Unsafe.Add(ref dest, 3);

            dest.AssertWrite<Vector256<sbyte>, byte>(ref destStart, destination.Length);
        }
    }
}
