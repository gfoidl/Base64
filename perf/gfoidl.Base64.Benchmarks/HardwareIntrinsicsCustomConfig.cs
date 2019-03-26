using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;

namespace gfoidl.Base64.Benchmarks
{
    public class HardwareIntrinsicsCustomConfig : ManualConfig
    {
        private const string EnableAVX = "COMPlus_EnableAVX";
        private const string EnableSSE = "COMPlus_EnableSSE";
        //---------------------------------------------------------------------
        public HardwareIntrinsicsCustomConfig()
        {
            this.Add(Job.Core.WithId("AVX2"));

            this.Add(Job.Core
                .With(new[] { new EnvironmentVariable(EnableAVX, "0") })
                .WithId("SSSE3"));

            this.Add(Job.Core
                .With(new[] { new EnvironmentVariable(EnableSSE, "0") })
                .WithId("Scalar"));
        }
    }
}
