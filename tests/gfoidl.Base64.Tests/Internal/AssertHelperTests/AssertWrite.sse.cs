using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using gfoidl.Base64.Internal;
using NUnit.Framework;


namespace gfoidl.Base64.Tests.Internal.AssertHelperTests
{
    [TestFixture]
    public partial class AssertWrite
    {
        private static readonly byte[] _dest = Enumerable.Range(0, 100).Select(i => (byte)i).ToArray();
        //---------------------------------------------------------------------
        [Test]
        public void Sse_byte_write_and_destLength_is_16()
        {
            Span<byte> destination = GetDestination(16);
            ref byte dest          = ref destination[0];
            ref byte destStart     = ref dest;

            dest.AssertWrite<Vector128<sbyte>, byte>(ref destStart, destination.Length);
        }
        //---------------------------------------------------------------------
        [Test]
        public void Sse_char_write_and_destLength_is_8()
        {
            Span<char> destination = MemoryMarshal.Cast<byte, char>(GetDestination(16));
            ref char dest          = ref destination[0];
            ref char destStart     = ref dest;

            dest.AssertWrite<Vector128<sbyte>, char>(ref destStart, destination.Length);
        }
        //---------------------------------------------------------------------
        [Test]
        public void Sse_byte_write_and_srcLength_is_17()
        {
            Span<byte> destination = GetDestination(17);
            ref byte dest          = ref destination[0];
            ref byte destStart     = ref dest;

            dest.AssertWrite<Vector128<sbyte>, byte>(ref destStart, destination.Length);
        }
        //---------------------------------------------------------------------
        [Test]
        public void Sse_char_write_and_destLength_is_9()
        {
            Span<char> destination = MemoryMarshal.Cast<byte, char>(GetDestination(17));
            ref char dest          = ref destination[0];
            ref char destStart     = ref dest;

            dest.AssertWrite<Vector128<sbyte>, char>(ref destStart, destination.Length);
        }
        //---------------------------------------------------------------------
        [Test]
        public void Sse_byte_write_and_srcLength_is_15___throws_InvalidOperation()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                Span<byte> destination = GetDestination(15);
                ref byte dest          = ref destination[0];
                ref byte destStart     = ref dest;

                dest.AssertWrite<Vector128<sbyte>, byte>(ref destStart, destination.Length);
            });
        }
        //---------------------------------------------------------------------
        [Test]
        public void Sse_char_write_and_srcLength_is_7___throws_InvalidOperation()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                Span<char> destination = MemoryMarshal.Cast<byte, char>(GetDestination(15));
                ref char dest          = ref destination[0];
                ref char destStart     = ref dest;

                dest.AssertWrite<Vector128<sbyte>, char>(ref destStart, destination.Length);
            });
        }
        //---------------------------------------------------------------------
        [Test]
        public void Sse_byte_write_with_offset_and_srcLength_is_16_plus_offset()
        {
            Span<byte> destination = GetDestination(16 + 3);
            ref byte dest          = ref destination[0];
            ref byte destStart     = ref dest;

            dest = ref Unsafe.Add(ref dest, 3);

            dest.AssertWrite<Vector128<sbyte>, byte>(ref destStart, destination.Length);
        }
        //---------------------------------------------------------------------
        [Test]
        public void Sse_char_write_with_offset_and_srcLength_is_8_plus_offset()
        {
            Span<char> destination = MemoryMarshal.Cast<byte, char>(GetDestination(16 + 6));
            ref char dest          = ref destination[0];
            ref char destStart     = ref dest;

            dest.AssertWrite<Vector128<sbyte>, char>(ref destStart, destination.Length);
        }
        //---------------------------------------------------------------------
        private static Span<byte> GetDestination(int size) => _dest.AsSpan(0, size);
    }
}
