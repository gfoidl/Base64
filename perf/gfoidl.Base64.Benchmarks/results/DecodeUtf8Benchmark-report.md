``` ini

BenchmarkDotNet=v0.11.3, OS=Windows 10.0.17134.407 (1803/April2018Update/Redstone4)
Intel Core i7-7700HQ CPU 2.80GHz (Kaby Lake), 1 CPU, 8 logical and 4 physical cores
Frequency=2742187 Hz, Resolution=364.6724 ns, Timer=TSC
.NET Core SDK=3.0.100-preview-009765
  [Host]     : .NET Core 3.0.0-preview-27117-01 (CoreCLR 4.6.27116.05, CoreFX 4.7.18.56608), 64bit RyuJIT
  DefaultJob : .NET Core 3.0.0-preview-27117-01 (CoreCLR 4.6.27116.05, CoreFX 4.7.18.56608), 64bit RyuJIT


```
|        Method | DataLen |      Mean |      Error |     StdDev | Ratio | RatioSD |
|-------------- |-------- |----------:|-----------:|-----------:|------:|--------:|
| **BuffersBase64** |       **5** |  **22.42 ns** |  **0.4855 ns** |  **0.9468 ns** |  **1.00** |    **0.00** |
|  gfoidlBase64 |       5 |  43.23 ns |  0.8973 ns |  2.4864 ns |  1.90 |    0.14 |
|               |         |           |            |            |       |         |
| **BuffersBase64** |      **16** |  **33.00 ns** |  **0.5872 ns** |  **0.5493 ns** |  **1.00** |    **0.00** |
|  gfoidlBase64 |      16 |  41.09 ns |  0.7648 ns |  0.7153 ns |  1.25 |    0.03 |
|               |         |           |            |            |       |         |
| **BuffersBase64** |    **1000** | **910.44 ns** | **17.6669 ns** | **17.3513 ns** |  **1.00** |    **0.00** |
|  gfoidlBase64 |    1000 | 298.26 ns |  5.9701 ns | 10.1377 ns |  0.33 |    0.01 |
