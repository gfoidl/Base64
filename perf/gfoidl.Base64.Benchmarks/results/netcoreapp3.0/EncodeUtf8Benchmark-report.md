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
|        Method |    Job |                       EnvironmentVariables | DataLen |        Mean |      Error |     StdDev |      Median | Ratio | RatioSD |
|-------------- |------- |------------------------------------------- |-------- |------------:|-----------:|-----------:|------------:|------:|--------:|
| **BuffersBase64** |   **AVX2** |                                      **Empty** |       **5** |    **32.49 ns** |  **0.6699 ns** |  **0.7714 ns** |    **32.77 ns** |  **1.00** |    **0.00** |
|  gfoidlBase64 |   AVX2 |                                      Empty |       5 |    21.45 ns |  0.0786 ns |  0.0735 ns |    21.46 ns |  0.66 |    0.01 |
|               |        |                                            |         |             |            |            |             |       |         |
| BuffersBase64 |  SSSE3 |                       COMPlus_EnableAVX2=0 |       5 |    32.60 ns |  0.7028 ns |  0.6902 ns |    32.81 ns |  1.00 |    0.00 |
|  gfoidlBase64 |  SSSE3 |                       COMPlus_EnableAVX2=0 |       5 |    16.68 ns |  1.1249 ns |  2.8014 ns |    15.09 ns |  0.66 |    0.03 |
|               |        |                                            |         |             |            |            |             |       |         |
| BuffersBase64 | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |       5 |    31.06 ns |  0.6703 ns |  1.5669 ns |    31.84 ns |  1.00 |    0.00 |
|  gfoidlBase64 | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |       5 |    20.27 ns |  0.0737 ns |  0.0616 ns |    20.29 ns |  0.66 |    0.04 |
|               |        |                                            |         |             |            |            |             |       |         |
| **BuffersBase64** |   **AVX2** |                                      **Empty** |      **16** |    **48.07 ns** |  **1.0006 ns** |  **2.3973 ns** |    **48.64 ns** |  **1.00** |    **0.00** |
|  gfoidlBase64 |   AVX2 |                                      Empty |      16 |    24.65 ns |  0.1185 ns |  0.1050 ns |    24.64 ns |  0.50 |    0.03 |
|               |        |                                            |         |             |            |            |             |       |         |
| BuffersBase64 |  SSSE3 |                       COMPlus_EnableAVX2=0 |      16 |    46.18 ns |  0.9636 ns |  2.1352 ns |    47.30 ns |  1.00 |    0.00 |
|  gfoidlBase64 |  SSSE3 |                       COMPlus_EnableAVX2=0 |      16 |    24.22 ns |  0.2351 ns |  0.2199 ns |    24.14 ns |  0.54 |    0.03 |
|               |        |                                            |         |             |            |            |             |       |         |
| BuffersBase64 | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |      16 |    53.85 ns |  1.1210 ns |  2.6859 ns |    53.76 ns |  1.00 |    0.00 |
|  gfoidlBase64 | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |      16 |    34.82 ns |  0.2725 ns |  0.2549 ns |    34.94 ns |  0.68 |    0.04 |
|               |        |                                            |         |             |            |            |             |       |         |
| **BuffersBase64** |   **AVX2** |                                      **Empty** |    **1000** | **1,423.59 ns** | **28.3039 ns** | **48.8228 ns** | **1,447.99 ns** |  **1.00** |    **0.00** |
|  gfoidlBase64 |   AVX2 |                                      Empty |    1000 |   116.03 ns |  1.5311 ns |  1.3573 ns |   115.93 ns |  0.08 |    0.00 |
|               |        |                                            |         |             |            |            |             |       |         |
| BuffersBase64 |  SSSE3 |                       COMPlus_EnableAVX2=0 |    1000 | 1,449.96 ns | 23.5019 ns | 21.9836 ns | 1,455.84 ns |  1.00 |    0.00 |
|  gfoidlBase64 |  SSSE3 |                       COMPlus_EnableAVX2=0 |    1000 |   183.04 ns |  0.9453 ns |  0.8842 ns |   183.07 ns |  0.13 |    0.00 |
|               |        |                                            |         |             |            |            |             |       |         |
| BuffersBase64 | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |    1000 | 1,404.80 ns | 27.8234 ns | 56.8359 ns | 1,432.02 ns |  1.00 |    0.00 |
|  gfoidlBase64 | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |    1000 | 1,219.77 ns |  7.7461 ns |  7.2457 ns | 1,218.80 ns |  0.88 |    0.05 |
