using System;
using System.Buffers;
using System.Collections.Generic;
using gfoidl.Base64.Internal;
using NUnit.Framework;

namespace gfoidl.Base64.Tests.Internal.Base64EncoderTests.Decode
{
    [TestFixture(typeof(byte))]
    [TestFixture(typeof(char))]
    public class ReturnStates<T> where T : unmanaged
    {
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
