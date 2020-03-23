using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using gfoidl.Base64.Internal;
using NUnit.Framework;

namespace gfoidl.Base64.Tests.Internal.Base64UrlEncoderTests
{
    [TestFixture(typeof(byte))]
    [TestFixture(typeof(char))]
    public class GetDecodedLength<T> where T : struct
    {
        [Test]
        public void EncodedLength_is_0___0()
        {
            var sut = new Base64UrlEncoder();

            int actual = sut.GetDecodedLength(0);

            Assert.AreEqual(0, actual);
        }
        //---------------------------------------------------------------------
        [Test]
        public void EncodedSpan_is_empty___0()
        {
            var sut       = new Base64UrlEncoder();
            var emptySpan = ReadOnlySpan<T>.Empty;

            int actual;

            if (typeof(T) == typeof(byte))
            {
                actual = sut.GetDecodedLength(MemoryMarshal.AsBytes(emptySpan));
            }
            else if (typeof(T) == typeof(char))
            {
                actual = sut.GetDecodedLength(MemoryMarshal.Cast<T, char>(emptySpan));
            }
            else
            {
                throw new NotSupportedException(); // just in case new types are introduced in the future
            }

            Assert.AreEqual(0, actual);
        }
        //---------------------------------------------------------------------
        [Test]
        public void EncodedSpan_1_to_50_given___correct_decoded_len()
        {
            var sut = new Base64UrlEncoder();

            Assert.Multiple(() =>
            {
                for (int i = 1; i < 50; ++i)
                {
                    var data         = new byte[i];
                    string base64Url = Convert.ToBase64String(data).ToBase64Url();

                    int actual;

                    if (typeof(T) == typeof(byte))
                    {
                        byte[] base64UrlBytes = Encoding.ASCII.GetBytes(base64Url);
                        actual = sut.GetDecodedLength(base64UrlBytes);
                    }
                    else if (typeof(T) == typeof(char))
                    {
                        actual = sut.GetDecodedLength(base64Url.AsSpan());  // AsSpan for net48
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
        [TestCase(int.MaxValue)]
        [TestCase(2147483646)]
        public void EncodedLength_is_int_Max___throws_ArgumentOutOfRange(int encodedLength)
        {
            var sut = new Base64UrlEncoder();

            Assert.Throws<ArgumentOutOfRangeException>(() => sut.GetDecodedLength(encodedLength));
        }
        //---------------------------------------------------------------------
        [Test]
        [TestCase(-1)]
        [TestCase(int.MinValue)]
        public void EncodedLength_is_negative___throws_ArgumentOutOfRange(int encodedLength)
        {
            var sut = new Base64UrlEncoder();

            Assert.Throws<ArgumentOutOfRangeException>(() => sut.GetDecodedLength(encodedLength));
        }
        //---------------------------------------------------------------------
        [Test, TestCaseSource(nameof(EncodedLength_given___correct_decoded_length_TestCases))]
        public int EncodedLength_given___correct_decoded_length(int encodedLength)
        {
            var sut = new Base64UrlEncoder();

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
            input    = new int[] { 2, 3, 6, 7, 10, 11, 14, 15, 1002, 1003, 2147483643 };
            expected = new int[] { 1, 2, 4, 5,  7,  8, 10, 11,  751,  752, 1610612732 };

            Assume.That(input.Length, Is.EqualTo(expected.Length));

            for (int i = 0; i < input.Length; ++i)
            {
                yield return new TestCaseData(input[i]).Returns(expected[i]);
            }
        }
        //---------------------------------------------------------------------
        [Test, TestCaseSource(nameof(EncodedLength_given_malformed___throws_MalformedInput_TestCases))]
        public void EncodedLength_given_malformed___throws_MalformedInput(int encodedLength)
        {
            var sut = new Base64UrlEncoder();

            Assert.Throws<FormatException>(() => sut.GetDecodedLength(encodedLength));
        }
        //---------------------------------------------------------------------
        private static IEnumerable<int> EncodedLength_given_malformed___throws_MalformedInput_TestCases()
        {
            yield return 1;
            yield return 5;
            yield return 9;
            yield return 13;
            yield return 1001;
            yield return 2147483645;
        }
    }
}
