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
| **BuffersBase64** |   **AVX2** |                                      **Empty** |       **5** |    **36.00 ns** |  **0.6956 ns** |  **0.6507 ns** |    **36.15 ns** |  **1.00** |    **0.00** |
|  gfoidlBase64 |   AVX2 |                                      Empty |       5 |    34.18 ns |  0.7233 ns |  1.3406 ns |    34.72 ns |  0.97 |    0.04 |
|               |        |                                            |         |             |            |            |             |       |         |
| BuffersBase64 |  SSSE3 |                       COMPlus_EnableAVX2=0 |       5 |    35.78 ns |  0.4632 ns |  0.4333 ns |    35.89 ns |  1.00 |    0.00 |
|  gfoidlBase64 |  SSSE3 |                       COMPlus_EnableAVX2=0 |       5 |    34.07 ns |  0.6178 ns |  0.5779 ns |    34.20 ns |  0.95 |    0.02 |
|               |        |                                            |         |             |            |            |             |       |         |
| BuffersBase64 | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |       5 |    36.00 ns |  0.7350 ns |  0.7864 ns |    36.20 ns |  1.00 |    0.00 |
|  gfoidlBase64 | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |       5 |    33.79 ns |  0.7049 ns |  1.0550 ns |    33.88 ns |  0.94 |    0.04 |
|               |        |                                            |         |             |            |            |             |       |         |
| **BuffersBase64** |   **AVX2** |                                      **Empty** |      **16** |    **46.30 ns** |  **0.9739 ns** |  **2.4071 ns** |    **47.36 ns** |  **1.00** |    **0.00** |
|  gfoidlBase64 |   AVX2 |                                      Empty |      16 |    39.26 ns |  0.8170 ns |  1.4093 ns |    39.79 ns |  0.85 |    0.05 |
|               |        |                                            |         |             |            |            |             |       |         |
| BuffersBase64 |  SSSE3 |                       COMPlus_EnableAVX2=0 |      16 |    49.16 ns |  1.0296 ns |  1.7203 ns |    48.78 ns |  1.00 |    0.00 |
|  gfoidlBase64 |  SSSE3 |                       COMPlus_EnableAVX2=0 |      16 |    38.40 ns |  0.8045 ns |  1.3661 ns |    38.94 ns |  0.78 |    0.04 |
|               |        |                                            |         |             |            |            |             |       |         |
| BuffersBase64 | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |      16 |    50.87 ns |  0.5463 ns |  0.5111 ns |    51.07 ns |  1.00 |    0.00 |
|  gfoidlBase64 | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |      16 |    48.22 ns |  1.1283 ns |  3.1825 ns |    49.10 ns |  1.02 |    0.07 |
|               |        |                                            |         |             |            |            |             |       |         |
| **BuffersBase64** |   **AVX2** |                                      **Empty** |    **1000** | **1,197.79 ns** | **23.2928 ns** | **37.6136 ns** | **1,211.29 ns** |  **1.00** |    **0.00** |
|  gfoidlBase64 |   AVX2 |                                      Empty |    1000 |   179.10 ns |  3.6289 ns |  6.8159 ns |   182.23 ns |  0.15 |    0.01 |
|               |        |                                            |         |             |            |            |             |       |         |
| BuffersBase64 |  SSSE3 |                       COMPlus_EnableAVX2=0 |    1000 | 1,205.04 ns | 24.0066 ns | 30.3607 ns | 1,215.72 ns |  1.00 |    0.00 |
|  gfoidlBase64 |  SSSE3 |                       COMPlus_EnableAVX2=0 |    1000 |   256.70 ns |  5.1126 ns |  8.6815 ns |   260.10 ns |  0.21 |    0.01 |
|               |        |                                            |         |             |            |            |             |       |         |
| BuffersBase64 | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |    1000 | 1,223.15 ns | 10.9624 ns | 10.2543 ns | 1,223.58 ns |  1.00 |    0.00 |
|  gfoidlBase64 | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |    1000 | 1,334.13 ns | 26.4728 ns | 55.2585 ns | 1,360.19 ns |  1.10 |    0.04 |
