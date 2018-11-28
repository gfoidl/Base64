``` ini

BenchmarkDotNet=v0.11.3, OS=ubuntu 16.04
Intel Xeon CPU 2.00GHz, 1 CPU, 2 logical cores and 1 physical core
.NET Core SDK=3.0.100-preview-009765
  [Host] : .NET Core 3.0.0-preview-27117-01 (CoreCLR 4.6.27116.05, CoreFX 4.7.18.56608), 64bit RyuJIT
  AVX2   : .NET Core 3.0.0-preview-27117-01 (CoreCLR 4.6.27116.05, CoreFX 4.7.18.56608), 64bit RyuJIT
  SSSE3  : .NET Core 3.0.0-preview-27117-01 (CoreCLR 4.6.27116.05, CoreFX 4.7.18.56608), 64bit RyuJIT
  Scalar : .NET Core 3.0.0-preview-27117-01 (CoreCLR 4.6.27116.05, CoreFX 4.7.18.56608), 64bit RyuJIT

Runtime=Core  

```
|                     Method |    Job |                       EnvironmentVariables | DataLen |        Mean |       Error |      StdDev | Ratio | RatioSD |
|--------------------------- |------- |------------------------------------------- |-------- |------------:|------------:|------------:|------:|--------:|
| **ConvertFromBase64CharArray** |   **AVX2** |                                      **Empty** |       **5** |   **100.45 ns** |   **1.5381 ns** |   **1.4387 ns** |  **1.00** |    **0.00** |
|               gfoidlBase64 |   AVX2 |                                      Empty |       5 |    60.69 ns |   1.1294 ns |   1.0564 ns |  0.60 |    0.01 |
|                            |        |                                            |         |             |             |             |       |         |
| ConvertFromBase64CharArray |  SSSE3 |                       COMPlus_EnableAVX2=0 |       5 |   101.78 ns |   1.3445 ns |   1.2576 ns |  1.00 |    0.00 |
|               gfoidlBase64 |  SSSE3 |                       COMPlus_EnableAVX2=0 |       5 |    60.03 ns |   1.2012 ns |   1.1236 ns |  0.59 |    0.01 |
|                            |        |                                            |         |             |             |             |       |         |
| ConvertFromBase64CharArray | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |       5 |    97.79 ns |   2.0844 ns |   2.9894 ns |  1.00 |    0.00 |
|               gfoidlBase64 | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |       5 |    59.39 ns |   0.4564 ns |   0.4269 ns |  0.60 |    0.02 |
|                            |        |                                            |         |             |             |             |       |         |
| **ConvertFromBase64CharArray** |   **AVX2** |                                      **Empty** |      **16** |   **156.60 ns** |   **3.1303 ns** |   **3.4793 ns** |  **1.00** |    **0.00** |
|               gfoidlBase64 |   AVX2 |                                      Empty |      16 |    71.64 ns |   0.5708 ns |   0.5340 ns |  0.46 |    0.01 |
|                            |        |                                            |         |             |             |             |       |         |
| ConvertFromBase64CharArray |  SSSE3 |                       COMPlus_EnableAVX2=0 |      16 |   158.57 ns |   2.8389 ns |   2.6555 ns |  1.00 |    0.00 |
|               gfoidlBase64 |  SSSE3 |                       COMPlus_EnableAVX2=0 |      16 |    69.00 ns |   1.3491 ns |   1.2620 ns |  0.44 |    0.01 |
|                            |        |                                            |         |             |             |             |       |         |
| ConvertFromBase64CharArray | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |      16 |   158.52 ns |   2.7691 ns |   2.5902 ns |  1.00 |    0.00 |
|               gfoidlBase64 | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |      16 |    78.68 ns |   1.4812 ns |   1.3855 ns |  0.50 |    0.01 |
|                            |        |                                            |         |             |             |             |       |         |
| **ConvertFromBase64CharArray** |   **AVX2** |                                      **Empty** |    **1000** | **5,367.08 ns** | **106.4836 ns** | **104.5811 ns** |  **1.00** |    **0.00** |
|               gfoidlBase64 |   AVX2 |                                      Empty |    1000 |   691.29 ns |  11.7365 ns |   9.1631 ns |  0.13 |    0.00 |
|                            |        |                                            |         |             |             |             |       |         |
| ConvertFromBase64CharArray |  SSSE3 |                       COMPlus_EnableAVX2=0 |    1000 | 5,379.58 ns |  92.1243 ns |  86.1731 ns |  1.00 |    0.00 |
|               gfoidlBase64 |  SSSE3 |                       COMPlus_EnableAVX2=0 |    1000 |   469.94 ns |  10.8277 ns |  31.4131 ns |  0.08 |    0.00 |
|                            |        |                                            |         |             |             |             |       |         |
| ConvertFromBase64CharArray | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |    1000 | 5,375.86 ns |  45.4937 ns |  42.5549 ns |  1.00 |    0.00 |
|               gfoidlBase64 | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |    1000 | 1,653.16 ns |  32.1652 ns |  34.4164 ns |  0.31 |    0.01 |
