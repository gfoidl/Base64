using System.Runtime.Intrinsics.X86;
using NUnit.Framework;

namespace gfoidl.Base64.Tests
{
    [SetUpFixture]
    public class HWInfo
    {
        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            TestContext.Progress.WriteLine(new string('-', 20));
            TestContext.Progress.WriteLine("SIMD-Info");
            TestContext.Progress.WriteLine($"Sse  : {Sse.IsSupported}");
            TestContext.Progress.WriteLine($"Sse2 : {Sse2.IsSupported}");
            TestContext.Progress.WriteLine($"Ssse3: {Ssse3.IsSupported}");
            TestContext.Progress.WriteLine($"Avx  : {Avx.IsSupported}");
            TestContext.Progress.WriteLine($"Avx2 : {Avx2.IsSupported}");
            TestContext.Progress.WriteLine(new string('-', 20));
        }
    }
}
