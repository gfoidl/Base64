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
| **BuffersBase64** |   **AVX2** |                                      **Empty** |       **5** |    **32.45 ns** |  **0.6913 ns** |  **1.8211 ns** |    **33.08 ns** |  **1.00** |    **0.00** |
|  gfoidlBase64 |   AVX2 |                                      Empty |       5 |    33.65 ns |  0.7173 ns |  1.3116 ns |    34.19 ns |  1.04 |    0.06 |
|               |        |                                            |         |             |            |            |             |       |         |
| BuffersBase64 |  SSSE3 |                       COMPlus_EnableAVX2=0 |       5 |    32.35 ns |  0.6966 ns |  1.5143 ns |    32.99 ns |  1.00 |    0.00 |
|  gfoidlBase64 |  SSSE3 |                       COMPlus_EnableAVX2=0 |       5 |    34.40 ns |  0.5167 ns |  0.4833 ns |    34.66 ns |  1.07 |    0.07 |
|               |        |                                            |         |             |            |            |             |       |         |
| BuffersBase64 | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |       5 |    32.62 ns |  0.7304 ns |  2.1537 ns |    33.05 ns |  1.00 |    0.00 |
|  gfoidlBase64 | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |       5 |    31.77 ns |  0.6807 ns |  1.9531 ns |    31.90 ns |  0.98 |    0.07 |
|               |        |                                            |         |             |            |            |             |       |         |
| **BuffersBase64** |   **AVX2** |                                      **Empty** |      **16** |    **51.02 ns** |  **1.0391 ns** |  **0.9719 ns** |    **51.09 ns** |  **1.00** |    **0.00** |
|  gfoidlBase64 |   AVX2 |                                      Empty |      16 |    39.39 ns |  0.8295 ns |  1.4309 ns |    39.99 ns |  0.78 |    0.03 |
|               |        |                                            |         |             |            |            |             |       |         |
| BuffersBase64 |  SSSE3 |                       COMPlus_EnableAVX2=0 |      16 |    47.64 ns |  0.9973 ns |  1.8732 ns |    47.29 ns |  1.00 |    0.00 |
|  gfoidlBase64 |  SSSE3 |                       COMPlus_EnableAVX2=0 |      16 |    39.37 ns |  0.6135 ns |  0.5739 ns |    39.58 ns |  0.82 |    0.04 |
|               |        |                                            |         |             |            |            |             |       |         |
| BuffersBase64 | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |      16 |    45.80 ns |  0.9569 ns |  2.7763 ns |    46.42 ns |  1.00 |    0.00 |
|  gfoidlBase64 | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |      16 |    48.72 ns |  1.0088 ns |  2.3178 ns |    49.24 ns |  1.05 |    0.07 |
|               |        |                                            |         |             |            |            |             |       |         |
| **BuffersBase64** |   **AVX2** |                                      **Empty** |    **1000** | **1,171.03 ns** | **23.4219 ns** | **50.4180 ns** | **1,199.57 ns** |  **1.00** |    **0.00** |
|  gfoidlBase64 |   AVX2 |                                      Empty |    1000 |   408.45 ns |  7.9724 ns |  9.1811 ns |   412.42 ns |  0.35 |    0.02 |
|               |        |                                            |         |             |            |            |             |       |         |
| BuffersBase64 |  SSSE3 |                       COMPlus_EnableAVX2=0 |    1000 | 1,194.00 ns | 23.6957 ns | 45.6537 ns | 1,212.59 ns |  1.00 |    0.00 |
|  gfoidlBase64 |  SSSE3 |                       COMPlus_EnableAVX2=0 |    1000 |   255.66 ns |  5.1737 ns | 12.0933 ns |   261.52 ns |  0.22 |    0.01 |
|               |        |                                            |         |             |            |            |             |       |         |
| BuffersBase64 | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |    1000 | 1,198.89 ns | 23.8594 ns | 26.5197 ns | 1,211.51 ns |  1.00 |    0.00 |
|  gfoidlBase64 | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |    1000 | 1,319.62 ns | 26.0255 ns | 50.7607 ns | 1,351.98 ns |  1.10 |    0.04 |
