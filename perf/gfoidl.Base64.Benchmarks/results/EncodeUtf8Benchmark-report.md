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
| **BuffersBase64** |   **AVX2** |                                      **Empty** |       **5** |    **32.09 ns** |  **0.6820 ns** |  **1.0817 ns** |    **32.20 ns** |  **1.00** |    **0.00** |
|  gfoidlBase64 |   AVX2 |                                      Empty |       5 |    20.98 ns |  0.3997 ns |  0.3543 ns |    20.83 ns |  0.65 |    0.03 |
|               |        |                                            |         |             |            |            |             |       |         |
| BuffersBase64 |  SSSE3 |                       COMPlus_EnableAVX2=0 |       5 |    33.09 ns |  0.7068 ns |  0.9436 ns |    33.08 ns |  1.00 |    0.00 |
|  gfoidlBase64 |  SSSE3 |                       COMPlus_EnableAVX2=0 |       5 |    21.28 ns |  0.0662 ns |  0.0619 ns |    21.31 ns |  0.64 |    0.02 |
|               |        |                                            |         |             |            |            |             |       |         |
| BuffersBase64 | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |       5 |    31.12 ns |  0.6713 ns |  1.8376 ns |    31.93 ns |  1.00 |    0.00 |
|  gfoidlBase64 | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |       5 |    16.43 ns |  1.0679 ns |  2.3215 ns |    15.14 ns |  0.53 |    0.07 |
|               |        |                                            |         |             |            |            |             |       |         |
| **BuffersBase64** |   **AVX2** |                                      **Empty** |      **16** |    **47.58 ns** |  **1.0751 ns** |  **3.1699 ns** |    **47.60 ns** |  **1.00** |    **0.00** |
|  gfoidlBase64 |   AVX2 |                                      Empty |      16 |    24.73 ns |  0.1111 ns |  0.1039 ns |    24.73 ns |  0.47 |    0.02 |
|               |        |                                            |         |             |            |            |             |       |         |
| BuffersBase64 |  SSSE3 |                       COMPlus_EnableAVX2=0 |      16 |    45.88 ns |  1.1820 ns |  3.4851 ns |    46.17 ns |  1.00 |    0.00 |
|  gfoidlBase64 |  SSSE3 |                       COMPlus_EnableAVX2=0 |      16 |    24.42 ns |  0.3894 ns |  0.3643 ns |    24.49 ns |  0.48 |    0.03 |
|               |        |                                            |         |             |            |            |             |       |         |
| BuffersBase64 | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |      16 |    47.21 ns |  1.1896 ns |  3.5076 ns |    47.94 ns |  1.00 |    0.00 |
|  gfoidlBase64 | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |      16 |    29.98 ns |  0.9369 ns |  2.2627 ns |    29.04 ns |  0.63 |    0.04 |
|               |        |                                            |         |             |            |            |             |       |         |
| **BuffersBase64** |   **AVX2** |                                      **Empty** |    **1000** | **1,435.40 ns** | **27.5412 ns** | **35.8114 ns** | **1,448.48 ns** |  **1.00** |    **0.00** |
|  gfoidlBase64 |   AVX2 |                                      Empty |    1000 |   113.94 ns |  0.9781 ns |  0.9149 ns |   113.64 ns |  0.08 |    0.00 |
|               |        |                                            |         |             |            |            |             |       |         |
| BuffersBase64 |  SSSE3 |                       COMPlus_EnableAVX2=0 |    1000 | 1,409.03 ns | 27.8595 ns | 55.6384 ns | 1,440.17 ns |  1.00 |    0.00 |
|  gfoidlBase64 |  SSSE3 |                       COMPlus_EnableAVX2=0 |    1000 |   182.31 ns |  0.7308 ns |  0.6478 ns |   182.19 ns |  0.13 |    0.00 |
|               |        |                                            |         |             |            |            |             |       |         |
| BuffersBase64 | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |    1000 | 1,406.43 ns | 28.0366 ns | 51.9677 ns | 1,423.30 ns |  1.00 |    0.00 |
|  gfoidlBase64 | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |    1000 | 1,212.79 ns |  8.1814 ns |  7.2526 ns | 1,212.09 ns |  0.86 |    0.03 |