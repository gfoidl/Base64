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
| **ConvertFromBase64CharArray** |   **AVX2** |                                      **Empty** |       **5** |   **100.53 ns** |   **1.3053 ns** |   **1.2210 ns** |  **1.00** |    **0.00** |
|               gfoidlBase64 |   AVX2 |                                      Empty |       5 |    60.76 ns |   0.2119 ns |   0.1982 ns |  0.60 |    0.01 |
|                            |        |                                            |         |             |             |             |       |         |
| ConvertFromBase64CharArray |  SSSE3 |                       COMPlus_EnableAVX2=0 |       5 |    95.26 ns |   2.0371 ns |   4.6395 ns |  1.00 |    0.00 |
|               gfoidlBase64 |  SSSE3 |                       COMPlus_EnableAVX2=0 |       5 |    58.89 ns |   0.2136 ns |   0.1998 ns |  0.59 |    0.02 |
|                            |        |                                            |         |             |             |             |       |         |
| ConvertFromBase64CharArray | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |       5 |    96.36 ns |   2.0669 ns |   3.5098 ns |  1.00 |    0.00 |
|               gfoidlBase64 | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |       5 |    57.22 ns |   1.2784 ns |   2.1359 ns |  0.59 |    0.03 |
|                            |        |                                            |         |             |             |             |       |         |
| **ConvertFromBase64CharArray** |   **AVX2** |                                      **Empty** |      **16** |   **156.96 ns** |   **3.2402 ns** |   **4.4352 ns** |  **1.00** |    **0.00** |
|               gfoidlBase64 |   AVX2 |                                      Empty |      16 |    66.57 ns |   1.4604 ns |   2.4400 ns |  0.43 |    0.01 |
|                            |        |                                            |         |             |             |             |       |         |
| ConvertFromBase64CharArray |  SSSE3 |                       COMPlus_EnableAVX2=0 |      16 |   157.12 ns |   0.9189 ns |   0.8595 ns |  1.00 |    0.00 |
|               gfoidlBase64 |  SSSE3 |                       COMPlus_EnableAVX2=0 |      16 |    66.80 ns |   0.3296 ns |   0.2922 ns |  0.42 |    0.00 |
|                            |        |                                            |         |             |             |             |       |         |
| ConvertFromBase64CharArray | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |      16 |   155.75 ns |   3.2355 ns |   5.4942 ns |  1.00 |    0.00 |
|               gfoidlBase64 | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |      16 |    78.15 ns |   1.3654 ns |   1.1402 ns |  0.48 |    0.01 |
|                            |        |                                            |         |             |             |             |       |         |
| **ConvertFromBase64CharArray** |   **AVX2** |                                      **Empty** |    **1000** | **5,337.24 ns** |  **42.3704 ns** |  **39.6333 ns** |  **1.00** |    **0.00** |
|               gfoidlBase64 |   AVX2 |                                      Empty |    1000 |   403.99 ns |   8.1154 ns |   7.5912 ns |  0.08 |    0.00 |
|                            |        |                                            |         |             |             |             |       |         |
| ConvertFromBase64CharArray |  SSSE3 |                       COMPlus_EnableAVX2=0 |    1000 | 5,312.29 ns | 101.7631 ns | 121.1417 ns |  1.00 |    0.00 |
|               gfoidlBase64 |  SSSE3 |                       COMPlus_EnableAVX2=0 |    1000 |   432.14 ns |   8.6192 ns |  16.8111 ns |  0.08 |    0.00 |
|                            |        |                                            |         |             |             |             |       |         |
| ConvertFromBase64CharArray | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |    1000 | 5,316.13 ns | 100.5839 ns | 107.6236 ns |  1.00 |    0.00 |
|               gfoidlBase64 | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |    1000 | 1,649.40 ns |  32.5025 ns |  45.5639 ns |  0.31 |    0.01 |
