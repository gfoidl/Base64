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
|          Method |    Job |                       EnvironmentVariables | DataLen |        Mean |      Error |      StdDev | Ratio | RatioSD |
|---------------- |------- |------------------------------------------- |-------- |------------:|-----------:|------------:|------:|--------:|
| **ConvertToBase64** |   **AVX2** |                                      **Empty** |       **5** |    **47.91 ns** |  **1.1043 ns** |   **1.7192 ns** |  **1.00** |    **0.00** |
|    gfoidlBase64 |   AVX2 |                                      Empty |       5 |    62.53 ns |  1.3961 ns |   3.2633 ns |  1.34 |    0.06 |
|                 |        |                                            |         |             |            |             |       |         |
| ConvertToBase64 |  SSSE3 |                       COMPlus_EnableAVX2=0 |       5 |    49.46 ns |  0.4253 ns |   0.3770 ns |  1.00 |    0.00 |
|    gfoidlBase64 |  SSSE3 |                       COMPlus_EnableAVX2=0 |       5 |    60.37 ns |  1.3564 ns |   3.6440 ns |  1.33 |    0.04 |
|                 |        |                                            |         |             |            |             |       |         |
| ConvertToBase64 | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |       5 |    48.09 ns |  1.1097 ns |   2.0291 ns |  1.00 |    0.00 |
|    gfoidlBase64 | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |       5 |    63.88 ns |  1.4262 ns |   2.2205 ns |  1.35 |    0.07 |
|                 |        |                                            |         |             |            |             |       |         |
| **ConvertToBase64** |   **AVX2** |                                      **Empty** |      **16** |    **77.63 ns** |  **1.6853 ns** |   **4.1971 ns** |  **1.00** |    **0.00** |
|    gfoidlBase64 |   AVX2 |                                      Empty |      16 |    93.16 ns |  2.0081 ns |   4.9636 ns |  1.20 |    0.08 |
|                 |        |                                            |         |             |            |             |       |         |
| ConvertToBase64 |  SSSE3 |                       COMPlus_EnableAVX2=0 |      16 |    83.11 ns |  1.7592 ns |   1.7277 ns |  1.00 |    0.00 |
|    gfoidlBase64 |  SSSE3 |                       COMPlus_EnableAVX2=0 |      16 |    99.74 ns |  1.9136 ns |   1.7900 ns |  1.20 |    0.03 |
|                 |        |                                            |         |             |            |             |       |         |
| ConvertToBase64 | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |      16 |    79.43 ns |  1.7309 ns |   3.6887 ns |  1.00 |    0.00 |
|    gfoidlBase64 | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |      16 |    95.56 ns |  2.0506 ns |   3.4262 ns |  1.20 |    0.06 |
|                 |        |                                            |         |             |            |             |       |         |
| **ConvertToBase64** |   **AVX2** |                                      **Empty** |    **1000** | **2,687.16 ns** | **53.2854 ns** | **120.2740 ns** |  **1.00** |    **0.00** |
|    gfoidlBase64 |   AVX2 |                                      Empty |    1000 |   664.29 ns | 22.3031 ns |  65.7612 ns |  0.26 |    0.02 |
|                 |        |                                            |         |             |            |             |       |         |
| ConvertToBase64 |  SSSE3 |                       COMPlus_EnableAVX2=0 |    1000 | 2,754.64 ns | 13.4394 ns |  12.5713 ns |  1.00 |    0.00 |
|    gfoidlBase64 |  SSSE3 |                       COMPlus_EnableAVX2=0 |    1000 |   770.20 ns | 22.0646 ns |  65.0578 ns |  0.30 |    0.01 |
|                 |        |                                            |         |             |            |             |       |         |
| ConvertToBase64 | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |    1000 | 2,753.22 ns | 43.5268 ns |  38.5853 ns |  1.00 |    0.00 |
|    gfoidlBase64 | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |    1000 | 2,106.60 ns | 41.5595 ns |  38.8748 ns |  0.77 |    0.02 |
