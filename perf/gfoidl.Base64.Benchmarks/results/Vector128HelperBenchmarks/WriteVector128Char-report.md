``` ini

BenchmarkDotNet=v0.11.3, OS=Windows 10.0.17134.472 (1803/April2018Update/Redstone4)
Intel Core i7-7700HQ CPU 2.80GHz (Kaby Lake), 1 CPU, 8 logical and 4 physical cores
Frequency=2742191 Hz, Resolution=364.6719 ns, Timer=TSC
.NET Core SDK=3.0.100-preview-009844
  [Host]     : .NET Core 3.0.0-preview-27218-01 (CoreCLR 4.6.27217.02, CoreFX 4.7.18.61304), 64bit RyuJIT
  DefaultJob : .NET Core 3.0.0-preview-27218-01 (CoreCLR 4.6.27217.02, CoreFX 4.7.18.61304), 64bit RyuJIT


```
|            Method | MisAlignment |       Mean |     Error |    StdDev | Ratio | RatioSD |
|------------------ |------------- |-----------:|----------:|----------:|------:|--------:|
| **WriteAs2Vector128** |            **0** |   **769.6 ns** | **15.051 ns** | **14.078 ns** |  **1.00** |    **0.00** |
|  WriteAsVector256 |            0 |   974.1 ns |  9.546 ns |  8.930 ns |  1.27 |    0.03 |
|                   |              |            |           |           |       |         |
| **WriteAs2Vector128** |            **3** |   **978.2 ns** | **14.413 ns** | **12.777 ns** |  **1.00** |    **0.00** |
|  WriteAsVector256 |            3 |   965.1 ns |  7.626 ns |  6.760 ns |  0.99 |    0.02 |
|                   |              |            |           |           |       |         |
| **WriteAs2Vector128** |           **15** | **1,014.1 ns** | **19.640 ns** | **20.169 ns** |  **1.00** |    **0.00** |
|  WriteAsVector256 |           15 | 1,017.2 ns | 20.147 ns | 36.329 ns |  1.01 |    0.03 |
