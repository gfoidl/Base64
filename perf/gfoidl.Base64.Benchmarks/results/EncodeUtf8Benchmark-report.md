``` ini

BenchmarkDotNet=v0.11.3, OS=Windows 10.0.17134.407 (1803/April2018Update/Redstone4)
Intel Core i7-7700HQ CPU 2.80GHz (Kaby Lake), 1 CPU, 8 logical and 4 physical cores
Frequency=2742187 Hz, Resolution=364.6724 ns, Timer=TSC
.NET Core SDK=3.0.100-preview-009765
  [Host]     : .NET Core 3.0.0-preview-27117-01 (CoreCLR 4.6.27116.05, CoreFX 4.7.18.56608), 64bit RyuJIT
  DefaultJob : .NET Core 3.0.0-preview-27117-01 (CoreCLR 4.6.27116.05, CoreFX 4.7.18.56608), 64bit RyuJIT


```
|        Method | DataLen |        Mean |     Error |    StdDev |      Median | Ratio | RatioSD |
|-------------- |-------- |------------:|----------:|----------:|------------:|------:|--------:|
| **BuffersBase64** |       **5** |    **22.14 ns** | **0.4619 ns** | **0.4744 ns** |    **22.08 ns** |  **1.00** |    **0.00** |
|  gfoidlBase64 |       5 |    40.43 ns | 0.8724 ns | 1.8016 ns |    40.66 ns |  1.75 |    0.07 |
|               |         |             |           |           |             |       |         |
| **BuffersBase64** |      **16** |    **32.97 ns** | **0.4116 ns** | **0.3648 ns** |    **32.93 ns** |  **1.00** |    **0.00** |
|  gfoidlBase64 |      16 |    44.40 ns | 0.9934 ns | 2.3608 ns |    43.52 ns |  1.34 |    0.08 |
|               |         |             |           |           |             |       |         |
| **BuffersBase64** |    **1000** | **1,061.10 ns** | **7.6773 ns** | **7.1813 ns** | **1,062.36 ns** |  **1.00** |    **0.00** |
|  gfoidlBase64 |    1000 |   115.01 ns | 1.3991 ns | 1.3087 ns |   114.87 ns |  0.11 |    0.00 |
