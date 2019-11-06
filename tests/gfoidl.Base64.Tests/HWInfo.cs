using NUnit.Framework;

[assembly: Parallelizable(ParallelScope.Children)]

namespace gfoidl.Base64.Tests
{
    [SetUpFixture]
    public class HWInfo
    {
        [OneTimeSetUp]
        public void OneTimeSetup() => SimdInfo.PrintSimdInfo(TestContext.Progress);
    }
}
