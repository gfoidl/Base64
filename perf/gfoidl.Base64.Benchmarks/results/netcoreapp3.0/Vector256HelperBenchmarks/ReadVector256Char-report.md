``` ini

BenchmarkDotNet=v0.11.3, OS=ubuntu 16.04
Intel Xeon CPU 2.00GHz, 1 CPU, 2 logical cores and 1 physical core
.NET Core SDK=3.0.100-preview-009844
  [Host]     : .NET Core 3.0.0-preview-27218-01 (CoreCLR 4.6.27217.02, CoreFX 4.7.18.61304), 64bit RyuJIT
  DefaultJob : .NET Core 3.0.0-preview-27218-01 (CoreCLR 4.6.27217.02, CoreFX 4.7.18.61304), 64bit RyuJIT


```
|                          Method | MisAlignment |       Mean |    Error |   StdDev | Ratio | RatioSD |
|-------------------------------- |------------- |-----------:|---------:|---------:|------:|--------:|
|                **ReadAs2Vector256** |            **0** |   **837.9 ns** | **6.895 ns** | **6.450 ns** |  **1.00** |    **0.00** |
| ReadAs4Vector128PackAsVector256 |            0 | 1,571.1 ns | 7.569 ns | 7.080 ns |  1.88 |    0.01 |
|                ReadAs4Vector128 |            0 | 1,588.9 ns | 7.370 ns | 6.534 ns |  1.89 |    0.02 |
|                                 |              |            |          |          |       |         |
|                **ReadAs2Vector256** |            **3** |   **901.2 ns** | **3.475 ns** | **2.902 ns** |  **1.00** |    **0.00** |
| ReadAs4Vector128PackAsVector256 |            3 | 1,619.6 ns | 6.369 ns | 5.957 ns |  1.80 |    0.01 |
|                ReadAs4Vector128 |            3 | 1,563.8 ns | 5.711 ns | 5.342 ns |  1.73 |    0.01 |
|                                 |              |            |          |          |       |         |
|                **ReadAs2Vector256** |           **15** |   **902.7 ns** | **4.684 ns** | **3.657 ns** |  **1.00** |    **0.00** |
| ReadAs4Vector128PackAsVector256 |           15 | 1,588.1 ns | 9.608 ns | 8.987 ns |  1.76 |    0.01 |
|                ReadAs4Vector128 |           15 | 1,562.8 ns | 4.765 ns | 4.224 ns |  1.73 |    0.01 |
