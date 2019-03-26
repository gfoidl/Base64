using System;
using BenchmarkDotNet.Running;

namespace gfoidl.Base64.Benchmarks
{
    static class Program
    {
        static void Main(string[] args)
        {
            SimdInfo.PrintSimdInfo(Console.Out);

            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
        }
    }
}
