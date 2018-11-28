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
| **BuffersBase64** |   **AVX2** |                                      **Empty** |       **5** |    **34.32 ns** |  **0.7292 ns** |  **1.8953 ns** |    **34.99 ns** |  **1.00** |    **0.00** |
|  gfoidlBase64 |   AVX2 |                                      Empty |       5 |    32.55 ns |  0.6937 ns |  1.6754 ns |    33.07 ns |  0.95 |    0.07 |
|               |        |                                            |         |             |            |            |             |       |         |
| BuffersBase64 |  SSSE3 |                       COMPlus_EnableAVX2=0 |       5 |    32.67 ns |  0.7933 ns |  2.3390 ns |    33.46 ns |  1.00 |    0.00 |
|  gfoidlBase64 |  SSSE3 |                       COMPlus_EnableAVX2=0 |       5 |    32.26 ns |  0.6873 ns |  1.9045 ns |    33.03 ns |  0.98 |    0.08 |
|               |        |                                            |         |             |            |            |             |       |         |
| BuffersBase64 | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |       5 |    32.32 ns |  0.7843 ns |  2.3124 ns |    32.64 ns |  1.00 |    0.00 |
|  gfoidlBase64 | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |       5 |    33.71 ns |  0.7191 ns |  1.0314 ns |    34.07 ns |  1.00 |    0.08 |
|               |        |                                            |         |             |            |            |             |       |         |
| **BuffersBase64** |   **AVX2** |                                      **Empty** |      **16** |    **47.39 ns** |  **1.0015 ns** |  **2.1558 ns** |    **48.03 ns** |  **1.00** |    **0.00** |
|  gfoidlBase64 |   AVX2 |                                      Empty |      16 |    37.61 ns |  0.7966 ns |  2.1672 ns |    38.43 ns |  0.81 |    0.05 |
|               |        |                                            |         |             |            |            |             |       |         |
| BuffersBase64 |  SSSE3 |                       COMPlus_EnableAVX2=0 |      16 |    52.04 ns |  0.7414 ns |  0.6935 ns |    52.22 ns |  1.00 |    0.00 |
|  gfoidlBase64 |  SSSE3 |                       COMPlus_EnableAVX2=0 |      16 |    37.75 ns |  0.8632 ns |  2.5453 ns |    38.02 ns |  0.78 |    0.06 |
|               |        |                                            |         |             |            |            |             |       |         |
| BuffersBase64 | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |      16 |    46.33 ns |  1.0803 ns |  3.1854 ns |    46.28 ns |  1.00 |    0.00 |
|  gfoidlBase64 | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |      16 |    54.86 ns |  1.1461 ns |  3.2699 ns |    55.04 ns |  1.20 |    0.13 |
|               |        |                                            |         |             |            |            |             |       |         |
| **BuffersBase64** |   **AVX2** |                                      **Empty** |    **1000** | **1,170.23 ns** | **23.4101 ns** | **56.5381 ns** | **1,182.42 ns** |  **1.00** |    **0.00** |
|  gfoidlBase64 |   AVX2 |                                      Empty |    1000 |   263.57 ns |  4.0891 ns |  3.6249 ns |   264.39 ns |  0.21 |    0.00 |
|               |        |                                            |         |             |            |            |             |       |         |
| BuffersBase64 |  SSSE3 |                       COMPlus_EnableAVX2=0 |    1000 | 1,218.84 ns | 18.3513 ns | 17.1658 ns | 1,224.33 ns |  1.00 |    0.00 |
|  gfoidlBase64 |  SSSE3 |                       COMPlus_EnableAVX2=0 |    1000 |   250.79 ns |  5.0717 ns | 13.3609 ns |   251.42 ns |  0.22 |    0.01 |
|               |        |                                            |         |             |            |            |             |       |         |
| BuffersBase64 | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |    1000 | 1,196.36 ns | 23.9491 ns | 44.9822 ns | 1,213.07 ns |  1.00 |    0.00 |
|  gfoidlBase64 | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |    1000 | 1,315.79 ns | 25.7476 ns | 47.7248 ns | 1,331.36 ns |  1.10 |    0.06 |
