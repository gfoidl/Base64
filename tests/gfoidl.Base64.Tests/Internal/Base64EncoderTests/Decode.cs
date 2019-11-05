using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using gfoidl.Base64.Internal;
using NUnit.Framework;
using System.Linq;
using System.Text;

#if NETCOREAPP
using System.Runtime.Intrinsics.X86;
#endif

namespace gfoidl.Base64.Tests.Internal.Base64EncoderTests
{
    [TestFixture(typeof(byte))]
    [TestFixture(typeof(char))]
    public class Decode<T> where T : unmanaged
    {
        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void Empty_input(bool isFinalBlock)
        {
            var sut                 = new Base64Encoder();
            ReadOnlySpan<T> encoded = ReadOnlySpan<T>.Empty;

            Span<byte> data        = new byte[sut.GetDecodedLength(encoded.Length)];
            OperationStatus status = sut.DecodeCore(encoded, data, out int consumed, out int written, isFinalBlock);

            Assert.AreEqual(OperationStatus.Done, status);
            Assert.AreEqual(0, consumed);
            Assert.AreEqual(0, written);
            Assert.IsTrue(data.IsEmpty);
        }
        //---------------------------------------------------------------------
        [Test]
        public void Empty_input_decode_from_string___empty_array()
        {
            var sut        = new Base64Encoder();
            string encoded = string.Empty;

            byte[] actual = sut.Decode(encoded.AsSpan());               // AsSpan() for net48

            Assert.AreEqual(Array.Empty<byte>(), actual);
        }
        //---------------------------------------------------------------------
        [Test, TestCaseSource(nameof(Malformed_input___status_InvalidData_TestCasesWithIsFinalBlock))]
        public void Malformed_input___status_InvalidData(string input, bool isFinalBlock)
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
                Assert.AreEqual(4, consumed);
                Assert.AreEqual(3, written);
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
        private static IEnumerable<TestCaseData> Malformed_input_to_byte_array___throws_FormatException_TestCases()
        {
            yield return new TestCaseData("1234abc?");
            yield return new TestCaseData("1234ab?c");
            yield return new TestCaseData("1234ab=c");
            yield return new TestCaseData("1234abc-");      // char from base64Url
            yield return new TestCaseData("1234abc_");      // char from base64Url
        }
        //---------------------------------------------------------------------
        private static IEnumerable<TestCaseData> Malformed_input___status_InvalidData_TestCasesWithIsFinalBlock()
        {
            foreach (TestCaseData testCaseData in Malformed_input_to_byte_array___throws_FormatException_TestCases())
                yield return new TestCaseData(testCaseData.Arguments[0], true);

            foreach (TestCaseData testCaseData in Malformed_input_to_byte_array___throws_FormatException_TestCases())
                yield return new TestCaseData(testCaseData.Arguments[0], false);
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
        [TestCase("AQ==", 1)]
        [TestCase("AQI=", 2)]
        [TestCase("AQID", 3)]
        [TestCase("AQIDBA==", 4)]
        [TestCase("AQIDBAU=", 5)]
        [TestCase("AQIDBAUG", 6)]
        public void Basic_decoding_with_known_input___Done(string input, int expectedWritten)
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

            Span<byte> data        = new byte[sut.GetMaxDecodedLength(encoded.Length)];
            OperationStatus status = sut.DecodeCore(encoded, data, out int consumed, out int written);
            byte[] actualData      = data.Slice(0, written).ToArray();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(OperationStatus.Done, status);
                Assert.AreEqual(input.Length        , consumed);
                Assert.AreEqual(expectedWritten     , written);

                byte[] expectedData = Convert.FromBase64String(input);
                CollectionAssert.AreEqual(expectedData, actualData);
            });
        }
        //---------------------------------------------------------------------
        [Test]
        [TestCase("AQID", 3)]
        [TestCase("AQIDBAUG", 6)]
        public void Basic_decoding_with_known_input_isFinalBlock_false___Done(string input, int expectedWritten)
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
                encoded = MemoryMarshal.Cast<char, T>(input.AsSpan());  // .AsSpan() for net48
            }
            else
            {
                throw new NotSupportedException(); // just in case new types are introduced in the future
            }

            Span<byte> data        = new byte[sut.GetMaxDecodedLength(encoded.Length)];
            OperationStatus status = sut.DecodeCore(encoded, data, out int consumed, out int written, isFinalBlock: false);
            byte[] actualData      = data.Slice(0, written).ToArray();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(OperationStatus.Done, status);
                Assert.AreEqual(input.Length, consumed);
                Assert.AreEqual(expectedWritten, written);

                byte[] expectedData = Convert.FromBase64String(input);
                CollectionAssert.AreEqual(expectedData, actualData);
            });
        }
        //---------------------------------------------------------------------
        [Test]
        [TestCase(" ", 0, 0)]
        [TestCase("A", 0, 0)]
        [TestCase("AQ", 0, 0)]
        [TestCase("AQI", 0, 0)]
        [TestCase("AQIDBA", 4, 3)]
        [TestCase("AQIDBAU", 4, 3)]
        public void Basic_decoding_with_invalid_input___InvalidData(string input, int expectedConsumed, int expectedWritten)
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
                encoded = MemoryMarshal.Cast<char, T>(input.AsSpan());  // .AsSpan() for net48
            }
            else
            {
                throw new NotSupportedException(); // just in case new types are introduced in the future
            }

            Span<byte> data        = new byte[sut.GetMaxDecodedLength(encoded.Length)];
            OperationStatus status = sut.DecodeCore(encoded, data, out int consumed, out int written);
            byte[] actualData      = data.Slice(0, written).ToArray();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(OperationStatus.InvalidData, status);
                Assert.AreEqual(expectedConsumed           , consumed);
                Assert.AreEqual(expectedWritten            , written);

                byte[] expectedData = Convert.FromBase64String(input.Substring(0, consumed));
                CollectionAssert.AreEqual(expectedData, actualData);
            });
        }
        //---------------------------------------------------------------------
        [Test]
        [TestCase(" ", 0, 0)]
        [TestCase("A", 0, 0)]
        [TestCase("AQ", 0, 0)]
        [TestCase("AQI", 0, 0)]
        [TestCase("AQIDB", 4, 3)]
        [TestCase("AQIDBA", 4, 3)]
        [TestCase("AQIDBAU", 4, 3)]
        public void Basic_decoding_with_invalid_input_isFinalBlock_false___NeedMoreData(string input, int expectedConsumed, int expectedWritten)
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
                encoded = MemoryMarshal.Cast<char, T>(input.AsSpan());  // .AsSpan() for net48
            }
            else
            {
                throw new NotSupportedException(); // just in case new types are introduced in the future
            }

            Span<byte> data        = new byte[sut.GetMaxDecodedLength(encoded.Length)];
            OperationStatus status = sut.DecodeCore(encoded, data, out int consumed, out int written, isFinalBlock: false);
            byte[] actualData      = data.Slice(0, written).ToArray();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(OperationStatus.NeedMoreData, status);
                Assert.AreEqual(expectedConsumed            , consumed);
                Assert.AreEqual(expectedWritten             , written);

                byte[] expectedData = Convert.FromBase64String(input.Substring(0, consumed));
                CollectionAssert.AreEqual(expectedData, actualData);
            });
        }
        //---------------------------------------------------------------------
        [Test]
        [TestCase("AQ==", 0, 0)]
        [TestCase("AQI=", 0, 0)]
        [TestCase("AQIDBA==", 4, 3)]
        [TestCase("AQIDBAU=", 4, 3)]
        public void Basic_decoding_with_padding_at_end_isFinalBlock_false___InvalidData(string input, int expectedConsumed, int expectedWritten)
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
                encoded = MemoryMarshal.Cast<char, T>(input.AsSpan());  // .AsSpan() for net48
            }
            else
            {
                throw new NotSupportedException(); // just in case new types are introduced in the future
            }

            Span<byte> data        = new byte[sut.GetMaxDecodedLength(encoded.Length)];
            OperationStatus status = sut.DecodeCore(encoded, data, out int consumed, out int written, isFinalBlock: false);
            byte[] actualData      = data.Slice(0, written).ToArray();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(OperationStatus.InvalidData, status);
                Assert.AreEqual(expectedConsumed           , consumed);
                Assert.AreEqual(expectedWritten            , written);

                byte[] expectedData = Convert.FromBase64String(input.Substring(0, consumed));
                CollectionAssert.AreEqual(expectedData, actualData);
            });
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
            [Values(true, false)] bool isFinalBlock)
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
            [Values(true, false)] bool isFinalBlock)
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

            Span<byte> data = new byte[sut.GetMaxDecodedLength(encoded.Length)];
            OperationStatus status = sut.DecodeCore(encoded, data, out int consumed, out int written, isFinalBlock);
            byte[] actualData = data.Slice(0, written).ToArray();

            Assert.Multiple(() =>
            {
                OperationStatus expectedStatus = isFinalBlock ? OperationStatus.Done : OperationStatus.InvalidData;
                int expectedConsumed = isFinalBlock ? 8 : 4;
                int expectedWritten = isFinalBlock
                    ? encodedString.Count(c => c == '=') == 2 ? 4 : 5
                    : 3;

                Assert.AreEqual(expectedStatus, status);
                Assert.AreEqual(expectedConsumed, consumed);
                Assert.AreEqual(expectedWritten, written);

                byte[] expectedData = Convert.FromBase64String(encodedString.Substring(0, consumed));
                CollectionAssert.AreEqual(expectedData, actualData);
            });
        }
        //---------------------------------------------------------------------
#if NETCOREAPP3_0 && DEBUG
        [Test]
        public void Large_data___avx2_event_fired()
        {
            Assume.That(Avx2.IsSupported);

            var sut  = new Base64Encoder();
            var data = new byte[50];
            var rnd  = new Random(0);
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
                decodedLength = sut.GetDecodedLength(MemoryMarshal.AsBytes(encoded));
            }
            else if (typeof(T) == typeof(char))
            {
                decodedLength = sut.GetDecodedLength(MemoryMarshal.Cast<T, char>(encoded));
            }
            else
            {
                throw new NotSupportedException(); // just in case new types are introduced in the future
            }

            Span<byte> decoded = new byte[decodedLength];

            bool avxExecuted = false;
            Base64Encoder.Avx2Decoded += (s, e) => avxExecuted = true;

            status = sut.DecodeCore<T>(encoded, decoded, out int _, out int _);
            Assume.That(status, Is.EqualTo(OperationStatus.Done));

            Assert.IsTrue(avxExecuted);
        }
#endif
        //---------------------------------------------------------------------
#if NETCOREAPP && DEBUG
        [Test]
        public void Large_data_but_to_small_for_avx2___ssse3_event_fired()
        {
            var sut  = new Base64Encoder();
            var data = new byte[20];
            var rnd  = new Random(0);
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
                decodedLength = sut.GetDecodedLength(MemoryMarshal.AsBytes(encoded));
            }
            else if (typeof(T) == typeof(char))
            {
                decodedLength = sut.GetDecodedLength(MemoryMarshal.Cast<T, char>(encoded));
            }
            else
            {
                throw new NotSupportedException(); // just in case new types are introduced in the future
            }

            Span<byte> decoded = new byte[decodedLength];

            bool ssse3Executed = false;
            Base64Encoder.Ssse3Decoded += (s, e) => ssse3Executed = true;

            status = sut.DecodeCore<T>(encoded, decoded, out int _, out int _);
            Assume.That(status, Is.EqualTo(OperationStatus.Done));

            Assert.IsTrue(ssse3Executed);
        }
        //---------------------------------------------------------------------
        [Test]
        public void Guid___ssse3_event_fired()
        {
            var sut  = new Base64Encoder();
            var data = Guid.NewGuid().ToByteArray();

            int encodedLength      = sut.GetEncodedLength(data.Length);
            Span<T> encoded        = new T[encodedLength];
            OperationStatus status = sut.EncodeCore(data, encoded, out int consumed, out int written);
            Assume.That(status  , Is.EqualTo(OperationStatus.Done));
            Assume.That(consumed, Is.EqualTo(data.Length));
            Assume.That(written , Is.EqualTo(encodedLength));

            int decodedLength;

            if (typeof(T) == typeof(byte))
            {
                decodedLength = sut.GetDecodedLength(MemoryMarshal.AsBytes(encoded));
            }
            else if (typeof(T) == typeof(char))
            {
                decodedLength = sut.GetDecodedLength(MemoryMarshal.Cast<T, char>(encoded));
            }
            else
            {
                throw new NotSupportedException(); // just in case new types are introduced in the future
            }

            Span<byte> decoded = new byte[decodedLength];

            bool ssse3Executed = false;
            Base64Encoder.Ssse3Decoded += (s, e) => ssse3Executed = true;

            status = sut.DecodeCore<T>(encoded, decoded, out int _, out int _);
            Assume.That(status, Is.EqualTo(OperationStatus.Done));

            Assert.IsTrue(ssse3Executed);
        }
#endif
        //---------------------------------------------------------------------
        [Test]
        public void Buffer_chain_basic_decode()
        {
            var sut  = new Base64Encoder();
            var data = new byte[500];
            var rnd  = new Random(0);
            rnd.NextBytes(data);

            int encodedLength      = sut.GetEncodedLength(data.Length);
            Span<T> encoded        = new T[encodedLength];
            OperationStatus status = sut.EncodeCore(data, encoded, out int consumed, out int written);
            Assume.That(status  , Is.EqualTo(OperationStatus.Done));
            Assume.That(consumed, Is.EqualTo(data.Length));
            Assume.That(written , Is.EqualTo(encodedLength));

            //int decodedLength  = sut.GetDecodedLength(encodedLength);

            int decodedLength;

            if (typeof(T) == typeof(byte))
            {
                decodedLength = sut.GetDecodedLength(MemoryMarshal.AsBytes(encoded));
            }
            else if (typeof(T) == typeof(char))
            {
                decodedLength = sut.GetDecodedLength(MemoryMarshal.Cast<T, char>(encoded));
            }
            else
            {
                throw new NotSupportedException(); // just in case new types are introduced in the future
            }

            Span<byte> decoded = new byte[decodedLength];

            status = sut.DecodeCore<T>(encoded.Slice(0, encoded.Length / 2), decoded, out consumed, out int written1, isFinalBlock: false);
            Assert.AreEqual(OperationStatus.NeedMoreData, status);

            status = sut.DecodeCore<T>(encoded.Slice(consumed), decoded.Slice(written1), out consumed, out int written2, isFinalBlock: true);
            Assert.AreEqual(OperationStatus.Done, status);
            Assert.AreEqual(decodedLength, written1 + written2);

            CollectionAssert.AreEqual(data, decoded.ToArray());
        }
        //---------------------------------------------------------------------
        [Test]
        public void Buffer_chain_various_length_decode()
        {
            var sut = new Base64Encoder();
            var rnd = new Random(0);

            for (int i = 2; i < 200; ++i)
            {
                var data = new byte[i];
                rnd.NextBytes(data);

                int encodedLength      = sut.GetEncodedLength(data.Length);
                Span<T> encoded        = new T[encodedLength];
                OperationStatus status = sut.EncodeCore(data, encoded, out int consumed, out int written);
                Assume.That(status  , Is.EqualTo(OperationStatus.Done), "fail at i = {0}", i);
                Assume.That(consumed, Is.EqualTo(data.Length), "fail at i = {0}", i);
                Assume.That(written , Is.EqualTo(encodedLength), "fail at i = {0}", i);

                int decodedLength;

                if (typeof(T) == typeof(byte))
                {
                    decodedLength = sut.GetDecodedLength(MemoryMarshal.AsBytes(encoded));
                }
                else if (typeof(T) == typeof(char))
                {
                    decodedLength = sut.GetDecodedLength(MemoryMarshal.Cast<T, char>(encoded));
                }
                else
                {
                    throw new NotSupportedException(); // just in case new types are introduced in the future
                }

                Span<byte> decoded = new byte[decodedLength];

                int partialLength = encoded.Length / 2;
                status            = sut.DecodeCore<T>(encoded.Slice(0, partialLength), decoded, out consumed, out int written1, isFinalBlock: false);
                Assert.AreEqual(partialLength % 4 == 0 ? OperationStatus.Done : OperationStatus.NeedMoreData, status, "fail at i = {0}", i);

                status = sut.DecodeCore<T>(encoded.Slice(consumed), decoded.Slice(written1), out consumed, out int written2, isFinalBlock: true);
                Assert.AreEqual(OperationStatus.Done, status, "fail at i = {0}", i);
                Assert.AreEqual(decodedLength, written1 + written2, "fail at i = {0}", i);

                CollectionAssert.AreEqual(data, decoded.ToArray(), "fail at i = {0}", i);
            }
        }
        //---------------------------------------------------------------------
        [Test]
        [TestCase(8, 5)]
        [TestCase(32, 22)]
        [TestCase(60, 44)]
        public void DestinationLength_too_small___status_DestinationTooSmall(int base64Length, int dataLength)
        {
            var sut    = new Base64Encoder();
            var data   = new byte[dataLength];
            T[] base64 = null;

            if (typeof(T) == typeof(byte))
            {
                base64 = Enumerable.Repeat((T)(object)(byte)'A', base64Length).ToArray();
            }
            else if (typeof(T) == typeof(char))
            {
                base64 = Enumerable.Repeat((T)(object)'A', base64Length).ToArray();
            }
            else
            {
                throw new NotSupportedException(); // just in case new types are introduced in the future
            }

            OperationStatus status = sut.DecodeCore<T>(base64, data, out int consumed, out int written);

            Assert.Multiple(() =>
            {
                int expectedConsumed = base64Length - 4;
                int expectedWritten  = expectedConsumed / 4 * 3;

                Assert.AreEqual(OperationStatus.DestinationTooSmall, status);
                Assert.AreEqual(expectedConsumed, consumed);
                Assert.AreEqual(expectedWritten , written);
            });
        }
        //---------------------------------------------------------------------
        [Test]
        public void DestinationLength_large_but_too_small___status_DestinationTooSmall()
        {
            const int base64Length = 400;
            const int dataLength   = 250;

            var sut    = new Base64Encoder();
            var data   = new byte[dataLength];
            T[] base64 = null;

            if (typeof(T) == typeof(byte))
            {
                base64 = Enumerable.Repeat((T)(object)(byte)'A', base64Length).ToArray();
            }
            else if (typeof(T) == typeof(char))
            {
                base64 = Enumerable.Repeat((T)(object)'A', base64Length).ToArray();
            }
            else
            {
                throw new NotSupportedException(); // just in case new types are introduced in the future
            }

            OperationStatus status = sut.DecodeCore<T>(base64, data, out int consumed, out int written);

            Assert.Multiple(() =>
            {
                int expectedWritten  = 250 - 1;
                int expectedConsumed = expectedWritten / 3 * 4;

                Assert.AreEqual(OperationStatus.DestinationTooSmall, status);
                Assert.AreEqual(expectedConsumed, consumed);
                Assert.AreEqual(expectedWritten, written);
            });
        }
        //---------------------------------------------------------------------
        [Test, TestCaseSource(nameof(Correct_status_TestCases))]
        public void Correct_status(
            bool            isFinalBlock,
            int             inputSize,
            int             outputSize,
            OperationStatus expectedStatus,
            int             expectedConsumed,
            int             expectedWritten)
        {
            T value;

            if (typeof(T) == typeof(byte))
            {
                value = (T)(object)(byte)'a';
            }
            else if (typeof(T) == typeof(char))
            {
                value = (T)(object)'a';
            }
            else
            {
                throw new NotSupportedException(); // just in case new types are introduced in the future
            }

            var sut           = new Base64Encoder();
            Span<T> input     = new T[inputSize];
            Span<byte> output = new byte[outputSize];

            input.Fill(value);
            OperationStatus actualStatus = sut.DecodeCore<T>(input, output, out int actuaConsumed, out int actuaWritten, isFinalBlock);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(expectedStatus  , actualStatus);
                Assert.AreEqual(expectedConsumed, actuaConsumed);
                Assert.AreEqual(expectedWritten , actuaWritten);
            });
        }
        //---------------------------------------------------------------------
        private static IEnumerable<TestCaseData> Correct_status_TestCases()
        {
            // Cases based on https://github.com/dotnet/corefx/issues/42245#issuecomment-548359376

            // D1
            bool isFinalBlock              = true;
            int sourceLength               = 4;
            int destLength                 = 3;
            OperationStatus expectedStatus = OperationStatus.Done;
            int expectedConsumed           = 4;
            int expectedWritten            = 3;
            yield return CreateTestCase();

            // D2
            sourceLength     = 5;
            destLength       = 100;
            expectedStatus   = OperationStatus.InvalidData;
            expectedConsumed = 4;
            expectedWritten  = 3;
            yield return CreateTestCase();

            // D3
            sourceLength     = 4;
            destLength       = 2;
            expectedStatus   = OperationStatus.DestinationTooSmall;
            expectedConsumed = 0;
            expectedWritten  = 0;
            yield return CreateTestCase();

            // D3
            sourceLength     = 8;
            destLength       = 5;
            expectedStatus   = OperationStatus.DestinationTooSmall;
            expectedConsumed = 4;
            expectedWritten  = 3;
            yield return CreateTestCase();

            // D4
            sourceLength     = 9;
            destLength       = 6;
            expectedStatus   = OperationStatus.InvalidData;
            expectedConsumed = 8;
            expectedWritten  = 6;
            yield return CreateTestCase();

            // D6
            isFinalBlock     = false;
            sourceLength     = 4;
            destLength       = 3;
            expectedStatus   = OperationStatus.Done;
            expectedConsumed = 4;
            expectedWritten  = 3;
            yield return CreateTestCase();

            // D7
            sourceLength     = 5;
            destLength       = 3;
            expectedStatus   = OperationStatus.NeedMoreData;
            expectedConsumed = 4;
            expectedWritten  = 3;
            yield return CreateTestCase();

            // D8
            sourceLength     = 8;
            destLength       = 5;
            expectedStatus   = OperationStatus.DestinationTooSmall;
            expectedConsumed = 4;
            expectedWritten  = 3;
            yield return CreateTestCase();

            // D9
            sourceLength     = 9;
            destLength       = 5;
            expectedStatus   = OperationStatus.DestinationTooSmall;
            expectedConsumed = 4;
            expectedWritten  = 3;
            yield return CreateTestCase();
            //-----------------------------------------------------------------
            TestCaseData CreateTestCase()
            {
                var testCaseData = new TestCaseData(isFinalBlock, sourceLength, destLength, expectedStatus, expectedConsumed, expectedWritten);
                testCaseData.SetArgDisplayNames(
                    $"isFinalBlock={isFinalBlock.ToString().ToLower()}",
                    $"source={sourceLength}",
                    $"dest={destLength}",
                    $"status={expectedStatus}",
                    $"consumed={expectedConsumed}",
                    $"written={expectedWritten}"
                );

                return testCaseData;
            }
        }
    }
}
