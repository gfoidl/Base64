using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using gfoidl.Base64.Internal;
using NUnit.Framework;

namespace gfoidl.Base64.Tests.Internal.Base64EncoderTests.Decode
{
    [TestFixture(typeof(byte))]
    [TestFixture(typeof(char))]
    public class InvalidInput<T> where T : unmanaged
    {
        [Test, TestCaseSource(nameof(Malformed_input___status_InvalidData_TestCasesWithIsFinalBlock))]
        public void Malformed_input___status_InvalidData(string input, bool isFinalBlock, int expectedConsumed, int expectedWritten)
        {
            var sut = new Base64Encoder();
            ReadOnlySpan<T> encoded;

            if (typeof(T) == typeof(byte))
            {
                ReadOnlySpan<byte> tmp = Encoding.ASCII.GetBytes(input);
                encoded                = MemoryMarshal.Cast<byte, T>(tmp);
            }
            else if (typeof(T) == typeof(char))
            {
                encoded = MemoryMarshal.Cast<char, T>(input.AsSpan());  // AsSpan() for net48
            }
            else
            {
                throw new NotSupportedException(); // just in case new types are introduced in the future
            }

            Span<byte> data = new byte[sut.GetMaxDecodedLength(encoded.Length)];
            OperationStatus status = sut.DecodeCore(encoded, data, out int consumed, out int written, isFinalBlock);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(OperationStatus.InvalidData, status);
                Assert.AreEqual(expectedConsumed           , consumed);
                Assert.AreEqual(expectedWritten            , written);
            });
        }
        //---------------------------------------------------------------------
        [Test, TestCaseSource(nameof(Malformed_input_to_byte_array___throws_FormatException_TestCases))]
        public void Malformed_input_to_byte_array___throws_FormatException(string input)
        {
            var sut = new Base64Encoder();

            Assert.Throws<FormatException>(() => sut.Decode(input.AsSpan()));
        }
        //---------------------------------------------------------------------
        private static IEnumerable<(string Input, int ExpectedConsumed, int ExpectedWritten)> Malformed_input_to_byte_array___RawTestCases()
        {
            yield return ("1234abc?" , 4, 3);
            yield return ("1234ab?c" , 4, 3);
            yield return ("1234ab=c" , 4, 3);
            yield return ("1234abc-" , 4, 3);      // char from base64Url
            yield return ("1234abc_" , 4, 3);      // char from base64Url
            yield return ("1324567\0", 4, 3);
            yield return ("z\0dYsqEkYYYYYEkYYYYYQYYeQYYeker", 0, 0);                                    // SSSE3 path
            yield return ("zadYsqEkYYYYYEkYYYYYQYYeQYYekerz\0dYsqEkYYYYYEkYYYYYQYYeQYYeker", 32, 24);   // AVX2 path
        }
        //---------------------------------------------------------------------
        private static IEnumerable<string> Malformed_input_to_byte_array___throws_FormatException_TestCases()
            => Malformed_input_to_byte_array___RawTestCases().Select(i => i.Input);
        //---------------------------------------------------------------------
        private static IEnumerable<object[]> Malformed_input___status_InvalidData_TestCasesWithIsFinalBlock()
        {
            foreach (var item in Malformed_input_to_byte_array___RawTestCases())
                yield return new object[] { item.Input, true, item.ExpectedConsumed, item.ExpectedWritten };

            foreach (var item in Malformed_input_to_byte_array___RawTestCases())
                yield return new object[] { item.Input, false, item.ExpectedConsumed, item.ExpectedWritten };
        }
        //---------------------------------------------------------------------
        [Test]
        public void Invalid_data_various_length___status_InvalidData()
        {
            var sut = new Base64Encoder();
            var rnd = new Random(0);

            for (int i = 4; i < 200; ++i)
            {
                var data = new byte[i];
                rnd.NextBytes(data);

                int encodedLength      = sut.GetEncodedLength(data.Length);
                Span<T> encoded        = new T[encodedLength];
                OperationStatus status = sut.EncodeCore(data, encoded, out int consumed, out int written);
                Assume.That(status  , Is.EqualTo(OperationStatus.Done));
                Assume.That(consumed, Is.EqualTo(data.Length));
                Assume.That(written , Is.EqualTo(encodedLength));

                int decodedLength;

                if (typeof(T) == typeof(byte))
                {
                    Span<byte> tmp = MemoryMarshal.AsBytes(encoded);
                    decodedLength  = sut.GetDecodedLength(tmp);

                    // Insert invalid data, just before eventual padding
                    tmp[tmp.Length - 3] = (byte)'~';
                }
                else if (typeof(T) == typeof(char))
                {
                    Span<char> tmp = MemoryMarshal.Cast<T, char>(encoded);
                    decodedLength  = sut.GetDecodedLength(tmp);

                    // Insert invalid data, just before eventual padding
                    tmp[tmp.Length - 3] = '~';
                }
                else
                {
                    throw new NotSupportedException(); // just in case new types are introduced in the future
                }

                Span<byte> decoded = new byte[decodedLength];

                status = sut.DecodeCore<T>(encoded, decoded, out consumed, out written);

                // Invalid data is in the last 4 bytes, so everyting up to the last multiple of 4 is read.
                int expectedConsumed = (encoded.Length - 3) / 4 * 4;
                int expectedWritten  = expectedConsumed / 4 * 3;

                Assert.Multiple(() =>
                {
                    Assert.AreEqual(OperationStatus.InvalidData, status);
                    Assert.AreEqual(expectedConsumed, consumed, "Fail at i = {0}", i);
                    Assert.AreEqual(expectedWritten , written , "Fail at i = {0}", i);
                });
            }
        }
        //---------------------------------------------------------------------
        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void Invalid_bytes___InvalidData(bool isFinalBlock)
        {
            var sut = new Base64Encoder();

            // Don't test padding ('=') here -> -1
            byte[] invalidBytes = TestHelper.GetInvalidBytes(Base64Encoder.DecodingMap)
                .Where(b => b != Base64Encoder.EncodingPad)
                .ToArray();

            Assume.That(invalidBytes.Length + 1, Is.EqualTo(byte.MaxValue - 64 + 1));   // +1 for padding on the left side
            T[] invalid = new T[invalidBytes.Length - 1];

            if (typeof(T) == typeof(byte))
            {
                invalid = invalidBytes.Select(b => (T)(object)b).ToArray();
            }
            else if (typeof(T) == typeof(char))
            {
                invalid = invalidBytes.Select(b => (T)(object)(char)b).ToArray();
            }
            else
            {
                throw new NotSupportedException(); // just in case new types are introduced in the future
            }

            for (int j = 0; j < 8; ++j)
            {
                Span<byte> tmp  = stackalloc byte[8] { 50, 50, 50, 50, 80, 80, 80, 80 };  // valid input - "2222PPPP"
                Span<T> encoded = stackalloc T[tmp.Length];

                if (typeof(T) == typeof(byte))
                {
                    encoded = MemoryMarshal.Cast<byte, T>(tmp);
                }
                else if (typeof(T) == typeof(char))
                {
                    for (int i = 0; i < tmp.Length; ++i)
                        encoded[i] = (T)(object)(char)tmp[i];
                }
                else
                {
                    throw new NotSupportedException(); // just in case new types are introduced in the future
                }

                byte[] data = new byte[sut.GetMaxDecodedLength(encoded.Length)];

                for (int i = 0; i < invalid.Length; ++i)
                {
                    encoded[j] = invalid[i];

                    OperationStatus status = sut.DecodeCore<T>(encoded, data, out int consumed, out int written, isFinalBlock);

                    Assert.Multiple(() =>
                    {
                        Assert.AreEqual(OperationStatus.InvalidData, status, "j = {0}, i = {1}", j, i);

                        if (j < 4)
                        {
                            Assert.AreEqual(0, consumed, "j = {0}, i = {1}", j, i);
                            Assert.AreEqual(0, written , "j = {0}, i = {1}", j, i);
                        }
                        else
                        {
                            Assert.AreEqual(4, consumed, "j = {0}, i = {1}", j, i);
                            Assert.AreEqual(3, written , "j = {0}, i = {1}", j, i);
#if NETCOREAPP
                            string actual = Convert.ToBase64String(data.AsSpan(0, 3));
#else
                            string actual = Convert.ToBase64String(data, 0, 3);
#endif
                            Assert.AreEqual("2222", actual, "j = {0}, i = {1}", j, i);
                        }
                    });
                }
            }
        }
        //---------------------------------------------------------------------
        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void Padding_can_only_be_last_two___InvalidData(bool isFinalBlock)
        {
            var sut = new Base64Encoder();

            for (int j = 0; j < 7; ++j)
            {
                Span<byte> tmp  = stackalloc byte[8] { 50, 50, 50, 50, 80, 80, 80, 80 };  // valid input - "2222PPPP"
                Span<T> encoded = stackalloc T[tmp.Length];
                T padding;

                if (typeof(T) == typeof(byte))
                {
                    padding = (T)(object)Base64Encoder.EncodingPad;
                    encoded = MemoryMarshal.Cast<byte, T>(tmp);
                }
                else if (typeof(T) == typeof(char))
                {
                    padding = (T)(object)(char)Base64Encoder.EncodingPad;
                    for (int i = 0; i < tmp.Length; ++i)
                        encoded[i] = (T)(object)(char)tmp[i];
                }
                else
                {
                    throw new NotSupportedException(); // just in case new types are introduced in the future
                }

                byte[] data = new byte[sut.GetMaxDecodedLength(encoded.Length)];

                encoded[j] = padding;

                OperationStatus status = sut.DecodeCore<T>(encoded, data, out int consumed, out int written, isFinalBlock);

                Assert.Multiple(() =>
                {
                    Assert.AreEqual(OperationStatus.InvalidData, status, "j = {0}", j);

                    if (j < 4)
                    {
                        Assert.AreEqual(0, consumed, "j = {0}", j);
                        Assert.AreEqual(0, written , "j = {0}", j);
                    }
                    else
                    {
                        Assert.AreEqual(4, consumed, "j = {0}", j);
                        Assert.AreEqual(3, written , "j = {0}", j);
#if NETCOREAPP
                        string actual = Convert.ToBase64String(data.AsSpan(0, 3));
#else
                        string actual = Convert.ToBase64String(data, 0, 3);
#endif
                        Assert.AreEqual("2222", actual, "j = {0}", j);
                    }
                });
            }
        }
        //---------------------------------------------------------------------
        [Test]
        public void Invalid_input_with_padding___InvalidData(
            [Values("2222P*==", "2222PP**=")] string encodedString,
            [Values(true, false)]             bool isFinalBlock)
        {
            var sut = new Base64Encoder();
            ReadOnlySpan<T> encoded;

            if (typeof(T) == typeof(byte))
            {
                ReadOnlySpan<byte> tmp = Encoding.ASCII.GetBytes(encodedString);
                encoded                = MemoryMarshal.Cast<byte, T>(tmp);
            }
            else if (typeof(T) == typeof(char))
            {
                encoded = MemoryMarshal.Cast<char, T>(encodedString.AsSpan());  // .AsSpan() for net48
            }
            else
            {
                throw new NotSupportedException(); // just in case new types are introduced in the future
            }

            Span<byte> data        = new byte[sut.GetMaxDecodedLength(encoded.Length)];
            OperationStatus status = sut.DecodeCore(encoded, data, out int consumed, out int written, isFinalBlock);
            byte[] actualData      = data.Slice(0, written).ToArray();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(OperationStatus.InvalidData, status);
                Assert.AreEqual(4, consumed);
                Assert.AreEqual(3, written);

                byte[] expectedData = Convert.FromBase64String(encodedString.Substring(0, consumed));
                CollectionAssert.AreEqual(expectedData, actualData);
            });
        }
        //---------------------------------------------------------------------
        [Test]
        public void Padding_is_only_valid_for_isFinalBlock_true(
            [Values("2222PP==", "2222PPP=")] string encodedString,
            [Values(true, false)]            bool isFinalBlock)
        {
            var sut = new Base64Encoder();
            ReadOnlySpan<T> encoded;

            if (typeof(T) == typeof(byte))
            {
                ReadOnlySpan<byte> tmp = Encoding.ASCII.GetBytes(encodedString);
                encoded = MemoryMarshal.Cast<byte, T>(tmp);
            }
            else if (typeof(T) == typeof(char))
            {
                encoded = MemoryMarshal.Cast<char, T>(encodedString.AsSpan());  // .AsSpan() for net48
            }
            else
            {
                throw new NotSupportedException(); // just in case new types are introduced in the future
            }

            Span<byte> data        = new byte[sut.GetMaxDecodedLength(encoded.Length)];
            OperationStatus status = sut.DecodeCore(encoded, data, out int consumed, out int written, isFinalBlock);
            byte[] actualData      = data.Slice(0, written).ToArray();

            Assert.Multiple(() =>
            {
                OperationStatus expectedStatus = isFinalBlock ? OperationStatus.Done : OperationStatus.InvalidData;
                int expectedConsumed = isFinalBlock ? 8 : 4;
                int expectedWritten  = isFinalBlock
                    ? encodedString.Count(c => c == '=') == 2 ? 4 : 5
                    : 3;

                Assert.AreEqual(expectedStatus  , status);
                Assert.AreEqual(expectedConsumed, consumed);
                Assert.AreEqual(expectedWritten , written);

                byte[] expectedData = Convert.FromBase64String(encodedString.Substring(0, consumed));
                CollectionAssert.AreEqual(expectedData, actualData);
            });
        }
    }
}
