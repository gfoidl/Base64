``` ini

BenchmarkDotNet=v0.11.3, OS=Windows 10.0.17134.407 (1803/April2018Update/Redstone4)
Intel Core i7-7700HQ CPU 2.80GHz (Kaby Lake), 1 CPU, 8 logical and 4 physical cores
Frequency=2742187 Hz, Resolution=364.6724 ns, Timer=TSC
.NET Core SDK=3.0.100-preview-009765
  [Host]     : .NET Core 3.0.0-preview-27117-01 (CoreCLR 4.6.27116.05, CoreFX 4.7.18.56608), 64bit RyuJIT
  DefaultJob : .NET Core 3.0.0-preview-27117-01 (CoreCLR 4.6.27116.05, CoreFX 4.7.18.56608), 64bit RyuJIT


```
|        Method | DataLen |        Mean |     Error |    StdDev | Ratio | RatioSD |
|-------------- |-------- |------------:|----------:|----------:|------:|--------:|
| **BuffersBase64** |       **5** |    **55.71 ns** |  **1.491 ns** |  **1.886 ns** |  **1.00** |    **0.00** |
|  gfoidlBase64 |       5 |    45.07 ns |  1.008 ns |  2.529 ns |  0.82 |    0.05 |
|               |         |             |           |           |       |         |
| **BuffersBase64** |      **16** |    **82.24 ns** |  **1.742 ns** |  **2.910 ns** |  **1.00** |    **0.00** |
|  gfoidlBase64 |      16 |    48.65 ns |  1.259 ns |  1.638 ns |  0.59 |    0.02 |
|               |         |             |           |           |       |         |
| **BuffersBase64** |    **1000** | **2,211.43 ns** | **27.922 ns** | **24.752 ns** |  **1.00** |    **0.00** |
|  gfoidlBase64 |    1000 |   425.14 ns |  2.484 ns |  2.202 ns |  0.19 |    0.00 |
