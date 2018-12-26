``` ini

BenchmarkDotNet=v0.11.3, OS=ubuntu 16.04
Intel Xeon CPU 2.00GHz, 1 CPU, 2 logical cores and 1 physical core
.NET Core SDK=3.0.100-preview-009844
  [Host]     : .NET Core 3.0.0-preview-27218-01 (CoreCLR 4.6.27217.02, CoreFX 4.7.18.61304), 64bit RyuJIT
  DefaultJob : .NET Core 3.0.0-preview-27218-01 (CoreCLR 4.6.27217.02, CoreFX 4.7.18.61304), 64bit RyuJIT


```
|                             Method | MisAlignment |     Mean |     Error |    StdDev | Ratio | RatioSD |
|----------------------------------- |------------- |---------:|----------:|----------:|------:|--------:|
|                  **WriteAs2Vector256** |            **0** | **1.621 us** | **0.0026 us** | **0.0023 us** |  **1.00** |    **0.00** |
| WriteAs4Vector128UnpackAsVector256 |            0 | 1.987 us | 0.0149 us | 0.0116 us |  1.23 |    0.01 |
|                  WriteAs4Vector128 |            0 | 1.955 us | 0.0204 us | 0.0191 us |  1.20 |    0.01 |
|                                    |              |          |           |           |       |         |
|                  **WriteAs2Vector256** |            **3** | **1.966 us** | **0.0058 us** | **0.0055 us** |  **1.00** |    **0.00** |
| WriteAs4Vector128UnpackAsVector256 |            3 | 2.733 us | 0.0223 us | 0.0197 us |  1.39 |    0.01 |
|                  WriteAs4Vector128 |            3 | 2.739 us | 0.0108 us | 0.0095 us |  1.39 |    0.01 |
|                                    |              |          |           |           |       |         |
|                  **WriteAs2Vector256** |           **15** | **1.984 us** | **0.0218 us** | **0.0204 us** |  **1.00** |    **0.00** |
| WriteAs4Vector128UnpackAsVector256 |           15 | 2.738 us | 0.0460 us | 0.0430 us |  1.38 |    0.02 |
|                  WriteAs4Vector128 |           15 | 2.701 us | 0.0137 us | 0.0128 us |  1.36 |    0.01 |
