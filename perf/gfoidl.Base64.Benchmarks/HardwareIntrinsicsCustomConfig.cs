using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;

namespace gfoidl.Base64.Benchmarks
{
    public class HardwareIntrinsicsCustomConfig : ManualConfig
    {
        private const string EnableAVX2  = "COMPlus_EnableAVX2";
        private const string EnableSSSE3 = "COMPlus_EnableSSSE3";

        public HardwareIntrinsicsCustomConfig()
        {
            this.Add(Job.Core.WithId("AVX2"));

            this.Add(Job.Core
                .With(new[] { new EnvironmentVariable(EnableAVX2, "0") })
                .WithId("SSSE3"));

            this.Add(Job.Core
                .With(new[] { new EnvironmentVariable(EnableAVX2, "0"), new EnvironmentVariable(EnableSSSE3, "0") })
                .WithId("Scalar"));
        }
    }
}
