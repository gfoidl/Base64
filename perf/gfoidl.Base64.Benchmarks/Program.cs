using System;
using BenchmarkDotNet.Running;

#if NETCOREAPP
using System.Runtime.Intrinsics.X86;
#endif

namespace gfoidl.Base64.Benchmarks
{
    static class Program
    {
        static void Main(string[] args)
        {
            PrintSimdInfo();

            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
        }
        //---------------------------------------------------------------------
        private static void PrintSimdInfo()
        {
#if NETCOREAPP
            Console.WriteLine($"Sse  : {Sse.IsSupported}");
            Console.WriteLine($"Sse2 : {Sse2.IsSupported}");
            Console.WriteLine($"Ssse3: {Ssse3.IsSupported}");
            Console.WriteLine($"Avx  : {Avx.IsSupported}");
            Console.WriteLine($"Avx2 : {Avx2.IsSupported}");
#endif
            Console.WriteLine();
        }
    }
}
