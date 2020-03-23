``` ini

BenchmarkDotNet=v0.11.3, OS=ubuntu 16.04
Intel Xeon CPU 2.00GHz, 1 CPU, 2 logical cores and 1 physical core
.NET Core SDK=3.0.100-preview-009844
  [Host]     : .NET Core 3.0.0-preview-27218-01 (CoreCLR 4.6.27217.02, CoreFX 4.7.18.61304), 64bit RyuJIT
  DefaultJob : .NET Core 3.0.0-preview-27218-01 (CoreCLR 4.6.27217.02, CoreFX 4.7.18.61304), 64bit RyuJIT


```
|            Method | MisAlignment |       Mean |     Error |    StdDev |     Median | Ratio | RatioSD |
|------------------ |------------- |-----------:|----------:|----------:|-----------:|------:|--------:|
|       **WriteDirect** |            **0** |   **776.2 ns** | **12.492 ns** | **11.685 ns** |   **770.7 ns** |  **1.00** |    **0.00** |
| WriteAs2Vector128 |            0 |   850.9 ns |  7.480 ns |  6.997 ns |   849.0 ns |  1.10 |    0.02 |
|                   |              |            |           |           |            |       |         |
|       **WriteDirect** |            **3** |   **736.6 ns** |  **8.619 ns** |  **8.062 ns** |   **734.6 ns** |  **1.00** |    **0.00** |
| WriteAs2Vector128 |            3 | 1,118.8 ns |  9.862 ns |  8.743 ns | 1,121.6 ns |  1.52 |    0.02 |
|                   |              |            |           |           |            |       |         |
|       **WriteDirect** |           **15** |   **756.3 ns** |  **5.248 ns** |  **4.909 ns** |   **756.3 ns** |  **1.00** |    **0.00** |
| WriteAs2Vector128 |           15 | 1,124.0 ns | 22.381 ns | 38.004 ns | 1,104.5 ns |  1.53 |    0.04 |
