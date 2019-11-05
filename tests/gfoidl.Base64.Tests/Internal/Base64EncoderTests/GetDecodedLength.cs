using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using gfoidl.Base64.Internal;
using NUnit.Framework;

namespace gfoidl.Base64.Tests.Internal.Base64EncoderTests
{
    [TestFixture(typeof(byte))]
    [TestFixture(typeof(char))]
    public class GetDecodedLength<T> where T : struct
    {
        [Test]
        public void EncodedLength_is_0___0()
        {
            var sut = new Base64Encoder();

            int actual = sut.GetDecodedLength(0);

            Assert.AreEqual(0, actual);
        }
        //---------------------------------------------------------------------
        [Test]
        public void EncodedSpan_is_empty___0()
        {
            var sut       = new Base64Encoder();
            var emptySpan = Array.Empty<T>().AsSpan();

            int actual = sut.GetDecodedLengthImpl<T>(emptySpan);

            Assert.AreEqual(0, actual);
        }
        //---------------------------------------------------------------------
        [Test]
        public void EncodedSpan_1_to_50_given___correct_decoded_len()
        {
            var sut = new Base64Encoder();

            Assert.Multiple(() =>
            {
                for (int i = 1; i < 50; ++i)
                {
                    var data      = new byte[i];
                    string base64 = Convert.ToBase64String(data);

                    int actual;

                    if (typeof(T) == typeof(byte))
                    {
                        byte[] base64Bytes = Encoding.ASCII.GetBytes(base64);
                        actual             = sut.GetDecodedLength(base64Bytes);
                    }
                    else if (typeof(T) == typeof(char))
                    {
                        actual = sut.GetDecodedLength(base64.AsSpan());  // AsSpan for net48
                    }
                    else
                    {
                        throw new NotSupportedException(); // just in case new types are introduced in the future
                    }

                    Assert.AreEqual(i, actual);
                }
            });
        }
        //---------------------------------------------------------------------
        [Test]
        public void EncodedLength_is_int_Max___OK()
        {
            var sut = new Base64Encoder();

            int actual = sut.GetDecodedLength(int.MaxValue);

            Assert.AreEqual(int.MaxValue / 4 * 3, actual);
        }
        //---------------------------------------------------------------------
        [Test]
        [Description("https://github.com/gfoidl/Base64/issues/32")]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void EncodedSpanLength_is_lt_4___throws_ArgumentOutOfRange(int encodedLength)
        {
            T a         = Unsafe.As<char, T>(ref Unsafe.AsRef('='));
            T[] encoded = Enumerable.Repeat(a, encodedLength).ToArray();

            var sut = new Base64Encoder();

            Exception exception = Assert.Catch(() => sut.GetDecodedLengthImpl<T>(encoded));

            Assert.Multiple(() =>
            {
                Assert.IsInstanceOf<ArgumentOutOfRangeException>(exception);
                string msg = $"The 'encodedLength' is outside the allowed range by the base64 standard. It must be >= 4.";
                StringAssert.StartsWith(msg, exception.Message);
            });
        }
        //---------------------------------------------------------------------
        [Test, TestCaseSource(nameof(EncodedLength_given___correct_decoded_length_TestCases))]
        public int EncodedLength_given___correct_decoded_length(int encodedLength)
        {
            var sut = new Base64Encoder();

            return sut.GetDecodedLength(encodedLength);
        }
        //---------------------------------------------------------------------
        private static IEnumerable<TestCaseData> EncodedLength_given___correct_decoded_length_TestCases()
        {
            // int.MaxValue - (int.MaxValue % 4) => 2147483644, largest multiple of 4 less than int.MaxValue
            int[] input    = { 0, 4, 8, 12, 16, 20, 2000000000, 2147483640, 2147483644 };
            int[] expected = { 0, 3, 6,  9, 12, 15, 1500000000, 1610612730, 1610612733 };

            Assume.That(input.Length, Is.EqualTo(expected.Length));

            for (int i = 0; i < input.Length; ++i)
            {
                yield return new TestCaseData(input[i]).Returns(expected[i]);
            }

            // Lengths that are not a multiple of 4.
            input    = new int[] { 1, 2, 3, 5, 6, 7, 9, 10, 11, 13, 14, 15, 1001, 1002, 1003, 2147483645, 2147483646, 2147483647 };
            expected = new int[] { 0, 0, 0, 3, 3, 3, 6,  6,  6,  9,  9,  9,  750,  750,  750, 1610612733, 1610612733, 1610612733 };

            Assume.That(input.Length, Is.EqualTo(expected.Length));

            for (int i = 0; i < input.Length; ++i)
            {
                yield return new TestCaseData(input[i]).Returns(expected[i]);
            }
        }
    }
}
