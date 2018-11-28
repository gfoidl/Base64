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
| **ConvertFromBase64CharArray** |   **AVX2** |                                      **Empty** |       **5** |    **97.43 ns** |   **2.0407 ns** |   **2.8608 ns** |  **1.00** |    **0.00** |
|               gfoidlBase64 |   AVX2 |                                      Empty |       5 |    60.92 ns |   0.3572 ns |   0.3341 ns |  0.61 |    0.02 |
|                            |        |                                            |         |             |             |             |       |         |
| ConvertFromBase64CharArray |  SSSE3 |                       COMPlus_EnableAVX2=0 |       5 |    97.56 ns |   2.5223 ns |   5.4833 ns |  1.00 |    0.00 |
|               gfoidlBase64 |  SSSE3 |                       COMPlus_EnableAVX2=0 |       5 |    62.13 ns |   1.2070 ns |   1.1290 ns |  0.60 |    0.04 |
|                            |        |                                            |         |             |             |             |       |         |
| ConvertFromBase64CharArray | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |       5 |   101.54 ns |   1.9821 ns |   1.8541 ns |  1.00 |    0.00 |
|               gfoidlBase64 | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |       5 |    56.26 ns |   1.2815 ns |   2.7311 ns |  0.57 |    0.02 |
|                            |        |                                            |         |             |             |             |       |         |
| **ConvertFromBase64CharArray** |   **AVX2** |                                      **Empty** |      **16** |   **161.94 ns** |   **1.9280 ns** |   **1.8035 ns** |  **1.00** |    **0.00** |
|               gfoidlBase64 |   AVX2 |                                      Empty |      16 |    68.79 ns |   0.6582 ns |   0.6157 ns |  0.42 |    0.01 |
|                            |        |                                            |         |             |             |             |       |         |
| ConvertFromBase64CharArray |  SSSE3 |                       COMPlus_EnableAVX2=0 |      16 |   160.38 ns |   3.0354 ns |   2.8393 ns |  1.00 |    0.00 |
|               gfoidlBase64 |  SSSE3 |                       COMPlus_EnableAVX2=0 |      16 |    68.57 ns |   1.4879 ns |   1.3918 ns |  0.43 |    0.01 |
|                            |        |                                            |         |             |             |             |       |         |
| ConvertFromBase64CharArray | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |      16 |   162.69 ns |   1.0429 ns |   0.9755 ns |  1.00 |    0.00 |
|               gfoidlBase64 | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |      16 |    75.89 ns |   1.6617 ns |   3.2016 ns |  0.47 |    0.02 |
|                            |        |                                            |         |             |             |             |       |         |
| **ConvertFromBase64CharArray** |   **AVX2** |                                      **Empty** |    **1000** | **5,341.17 ns** | **104.0383 ns** | **138.8881 ns** |  **1.00** |    **0.00** |
|               gfoidlBase64 |   AVX2 |                                      Empty |    1000 |   527.81 ns |   9.7691 ns |   9.1380 ns |  0.10 |    0.00 |
|                            |        |                                            |         |             |             |             |       |         |
| ConvertFromBase64CharArray |  SSSE3 |                       COMPlus_EnableAVX2=0 |    1000 | 5,338.39 ns | 106.3279 ns | 194.4265 ns |  1.00 |    0.00 |
|               gfoidlBase64 |  SSSE3 |                       COMPlus_EnableAVX2=0 |    1000 |   555.00 ns |  10.8938 ns |  13.3786 ns |  0.11 |    0.01 |
|                            |        |                                            |         |             |             |             |       |         |
| ConvertFromBase64CharArray | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |    1000 | 5,404.49 ns |  84.7593 ns |  79.2839 ns |  1.00 |    0.00 |
|               gfoidlBase64 | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |    1000 | 1,726.46 ns |  31.1280 ns |  29.1171 ns |  0.32 |    0.01 |
