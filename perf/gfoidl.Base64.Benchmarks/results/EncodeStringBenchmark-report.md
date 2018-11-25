``` ini

BenchmarkDotNet=v0.11.3, OS=Windows 10.0.17134.407 (1803/April2018Update/Redstone4)
Intel Core i7-7700HQ CPU 2.80GHz (Kaby Lake), 1 CPU, 8 logical and 4 physical cores
Frequency=2742187 Hz, Resolution=364.6724 ns, Timer=TSC
.NET Core SDK=3.0.100-preview-009765
  [Host]     : .NET Core 3.0.0-preview-27117-01 (CoreCLR 4.6.27116.05, CoreFX 4.7.18.56608), 64bit RyuJIT
  DefaultJob : .NET Core 3.0.0-preview-27117-01 (CoreCLR 4.6.27116.05, CoreFX 4.7.18.56608), 64bit RyuJIT


```
|          Method | DataLen |        Mean |      Error |     StdDev |      Median | Ratio | RatioSD |
|---------------- |-------- |------------:|-----------:|-----------:|------------:|------:|--------:|
| **ConvertToBase64** |       **5** |    **24.91 ns** |  **0.6173 ns** |  **1.6371 ns** |    **24.51 ns** |  **1.00** |    **0.00** |
|    gfoidlBase64 |       5 |    53.83 ns |  0.9522 ns |  0.8441 ns |    53.82 ns |  1.99 |    0.08 |
|                 |         |             |            |            |             |       |         |
| **ConvertToBase64** |      **16** |    **43.45 ns** |  **0.7860 ns** |  **0.7352 ns** |    **43.64 ns** |  **1.00** |    **0.00** |
|    gfoidlBase64 |      16 |    57.88 ns |  1.3070 ns |  1.8323 ns |    57.45 ns |  1.34 |    0.07 |
|                 |         |             |            |            |             |       |         |
| **ConvertToBase64** |    **1000** | **1,564.84 ns** | **31.1400 ns** | **79.8236 ns** | **1,528.28 ns** |  **1.00** |    **0.00** |
|    gfoidlBase64 |    1000 |   360.27 ns |  4.4083 ns |  4.1235 ns |   360.64 ns |  0.22 |    0.01 |
