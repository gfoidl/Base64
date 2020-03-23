``` ini

BenchmarkDotNet=v0.11.3, OS=Windows 10.0.17134.472 (1803/April2018Update/Redstone4)
Intel Core i7-7700HQ CPU 2.80GHz (Kaby Lake), 1 CPU, 8 logical and 4 physical cores
Frequency=2742191 Hz, Resolution=364.6719 ns, Timer=TSC
.NET Core SDK=3.0.100-preview-009844
  [Host]     : .NET Core 3.0.0-preview-27218-01 (CoreCLR 4.6.27217.02, CoreFX 4.7.18.61304), 64bit RyuJIT
  DefaultJob : .NET Core 3.0.0-preview-27218-01 (CoreCLR 4.6.27217.02, CoreFX 4.7.18.61304), 64bit RyuJIT


```
|           Method | MisAlignment |     Mean |     Error |    StdDev | Ratio | RatioSD |
|----------------- |------------- |---------:|----------:|----------:|------:|--------:|
| **ReadAs2Vector128** |            **0** | **690.4 ns** | **15.493 ns** | **18.443 ns** |  **1.00** |    **0.00** |
|  ReadAsVector256 |            0 | 684.1 ns |  8.949 ns |  8.371 ns |  0.99 |    0.03 |
|                  |              |          |           |           |       |         |
| **ReadAs2Vector128** |            **3** | **683.5 ns** |  **6.485 ns** |  **6.066 ns** |  **1.00** |    **0.00** |
|  ReadAsVector256 |            3 | 683.1 ns |  7.031 ns |  6.233 ns |  1.00 |    0.01 |
|                  |              |          |           |           |       |         |
| **ReadAs2Vector128** |           **15** | **682.7 ns** |  **9.644 ns** |  **9.021 ns** |  **1.00** |    **0.00** |
|  ReadAsVector256 |           15 | 684.8 ns |  9.705 ns |  8.603 ns |  1.00 |    0.02 |
