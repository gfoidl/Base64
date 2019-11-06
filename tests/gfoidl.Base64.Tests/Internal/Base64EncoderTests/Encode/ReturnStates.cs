using System;
using System.Buffers;
using System.Collections.Generic;
using gfoidl.Base64.Internal;
using NUnit.Framework;

namespace gfoidl.Base64.Tests.Internal.Base64EncoderTests.Encode
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
            var sut          = new Base64Encoder();
            Span<byte> input = new byte[inputSize];
            Span<T> output   = new T[outputSize];

            OperationStatus actualStatus = sut.EncodeCore(input, output, out int actuaConsumed, out int actuaWritten, isFinalBlock);

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

            // E1
            bool isFinalBlock              = true;
            int sourceLength               = 3;
            int destLength                 = 4;
            OperationStatus expectedStatus = OperationStatus.Done;
            int expectedConsumed           = 3;
            int expectedWritten            = 4;
            yield return CreateTestCase();

            // E2
            sourceLength     = 3;
            destLength       = 3;
            expectedStatus   = OperationStatus.DestinationTooSmall;
            expectedConsumed = 0;
            expectedWritten  = 0;
            yield return CreateTestCase();

            // E2
            sourceLength     = 6;
            destLength       = 6;
            expectedStatus   = OperationStatus.DestinationTooSmall;
            expectedConsumed = 3;
            expectedWritten  = 4;
            yield return CreateTestCase();

            // E3
            sourceLength     = Base64.MaximumEncodeLength;
            destLength       = 4;
            expectedStatus   = OperationStatus.DestinationTooSmall;
            expectedConsumed = 3;
            expectedWritten  = 4;
            yield return CreateTestCase();

            // E4
            isFinalBlock     = false;
            sourceLength     = 3;
            destLength       = 4;
            expectedStatus   = OperationStatus.Done;
            expectedConsumed = 3;
            expectedWritten  = 4;
            yield return CreateTestCase();

            // E5
            sourceLength     = 4;
            destLength       = 10;
            expectedStatus   = OperationStatus.NeedMoreData;
            expectedConsumed = 3;
            expectedWritten  = 4;
            yield return CreateTestCase();

            // E6
            sourceLength     = 6;
            destLength       = 4;
            expectedStatus   = OperationStatus.DestinationTooSmall;
            expectedConsumed = 3;
            expectedWritten  = 4;
            yield return CreateTestCase();

            // E7
            sourceLength     = 7;
            destLength       = 4;
            expectedStatus   = OperationStatus.DestinationTooSmall;
            expectedConsumed = 3;
            expectedWritten  = 4;
            yield return CreateTestCase();

            // E8
            sourceLength     = Base64.MaximumEncodeLength;
            destLength       = 4;
            expectedStatus   = OperationStatus.DestinationTooSmall;
            expectedConsumed = 3;
            expectedWritten  = 4;
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
