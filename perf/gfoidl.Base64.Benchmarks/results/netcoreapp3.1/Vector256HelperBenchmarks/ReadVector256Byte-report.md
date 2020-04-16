``` ini

BenchmarkDotNet=v0.11.3, OS=ubuntu 16.04
Intel Xeon CPU 2.00GHz, 1 CPU, 2 logical cores and 1 physical core
.NET Core SDK=3.0.100-preview-009844
  [Host]     : .NET Core 3.0.0-preview-27218-01 (CoreCLR 4.6.27217.02, CoreFX 4.7.18.61304), 64bit RyuJIT
  DefaultJob : .NET Core 3.0.0-preview-27218-01 (CoreCLR 4.6.27217.02, CoreFX 4.7.18.61304), 64bit RyuJIT


```
|           Method | MisAlignment |     Mean |    Error |   StdDev | Ratio | RatioSD |
|----------------- |------------- |---------:|---------:|---------:|------:|--------:|
|       **ReadDirect** |            **0** | **608.2 ns** | **9.732 ns** | **9.103 ns** |  **1.00** |    **0.00** |
| ReadAs2Vector128 |            0 | 794.4 ns | 8.807 ns | 8.238 ns |  1.31 |    0.03 |
|                  |              |          |          |          |       |         |
|       **ReadDirect** |            **3** | **597.4 ns** | **1.984 ns** | **1.759 ns** |  **1.00** |    **0.00** |
| ReadAs2Vector128 |            3 | 797.4 ns | 3.889 ns | 3.638 ns |  1.33 |    0.01 |
|                  |              |          |          |          |       |         |
|       **ReadDirect** |           **15** | **597.8 ns** | **1.166 ns** | **1.090 ns** |  **1.00** |    **0.00** |
| ReadAs2Vector128 |           15 | 791.4 ns | 7.013 ns | 6.216 ns |  1.32 |    0.01 |
