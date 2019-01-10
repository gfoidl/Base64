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
    public partial class AssertRead
    {
        private static readonly byte[] _source = Enumerable.Range(0, 100).Select(i => (byte)i).ToArray();
        //---------------------------------------------------------------------
        [Test]
        public void Sse_byte_read_and_srcLength_is_16()
        {
            Span<byte> source = GetSource(16);
            ref byte src      = ref source[0];
            ref byte srcStart = ref src;

            src.AssertRead<Vector128<sbyte>, byte>(ref srcStart, source.Length);
        }
        //---------------------------------------------------------------------
        [Test]
        public void Sse_char_read_and_srcLength_is_8()
        {
            Span<char> source = MemoryMarshal.Cast<byte, char>(GetSource(16));
            ref char src      = ref source[0];
            ref char srcStart = ref src;

            src.AssertRead<Vector128<sbyte>, char>(ref srcStart, source.Length);
        }
        //---------------------------------------------------------------------
        [Test]
        public void Sse_byte_read_and_srcLength_is_17()
        {
            Span<byte> source = GetSource(17);
            ref byte src      = ref source[0];
            ref byte srcStart = ref src;

            src.AssertRead<Vector128<sbyte>, byte>(ref srcStart, source.Length);
        }
        //---------------------------------------------------------------------
        [Test]
        public void Sse_char_read_and_srcLength_is_9()
        {
            Span<char> source = MemoryMarshal.Cast<byte, char>(GetSource(17));
            ref char src      = ref source[0];
            ref char srcStart = ref src;

            src.AssertRead<Vector128<sbyte>, char>(ref srcStart, source.Length);
        }
        //---------------------------------------------------------------------
        [Test]
        public void Sse_byte_read_and_srcLength_is_15___throws_InvalidOperation()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                Span<byte> source = GetSource(15);
                ref byte src      = ref source[0];
                ref byte srcStart = ref src;

                src.AssertRead<Vector128<sbyte>, byte>(ref srcStart, source.Length);
            });
        }
        //---------------------------------------------------------------------
        [Test]
        public void Sse_char_read_and_srcLength_is_7___throws_InvalidOperation()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                Span<char> source = MemoryMarshal.Cast<byte, char>(GetSource(15));
                ref char src      = ref source[0];
                ref char srcStart = ref src;

                src.AssertRead<Vector128<sbyte>, char>(ref srcStart, source.Length);
            });
        }
        //---------------------------------------------------------------------
        [Test]
        public void Sse_byte_read_with_offset_and_srcLength_is_16_plus_offset()
        {
            Span<byte> source = GetSource(16 + 3);
            ref byte src      = ref source[0];
            ref byte srcStart = ref src;

            src = ref Unsafe.Add(ref src, 3);

            src.AssertRead<Vector128<sbyte>, byte>(ref srcStart, source.Length);
        }
        //---------------------------------------------------------------------
        [Test]
        public void Sse_char_read_with_offset_and_srcLength_is_8_plus_offset()
        {
            Span<char> source = MemoryMarshal.Cast<byte, char>(GetSource(16 + 6));
            ref char src      = ref source[0];
            ref char srcStart = ref src;

            src = ref Unsafe.Add(ref src, 3);

            src.AssertRead<Vector128<sbyte>, char>(ref srcStart, source.Length);
        }
        //---------------------------------------------------------------------
        private static Span<byte> GetSource(int size) => _source.AsSpan(0, size);
    }
}
