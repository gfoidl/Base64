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
|          Method |    Job |                       EnvironmentVariables | DataLen |        Mean |      Error |      StdDev |      Median | Ratio | RatioSD |
|---------------- |------- |------------------------------------------- |-------- |------------:|-----------:|------------:|------------:|------:|--------:|
| **ConvertToBase64** |   **AVX2** |                                      **Empty** |       **5** |    **49.53 ns** |  **0.6290 ns** |   **0.5883 ns** |    **49.65 ns** |  **1.00** |    **0.00** |
|    gfoidlBase64 |   AVX2 |                                      Empty |       5 |    61.03 ns |  1.3615 ns |   2.9885 ns |    60.59 ns |  1.31 |    0.04 |
|                 |        |                                            |         |             |            |             |             |       |         |
| ConvertToBase64 |  SSSE3 |                       COMPlus_EnableAVX2=0 |       5 |    46.60 ns |  1.0633 ns |   2.3561 ns |    46.85 ns |  1.00 |    0.00 |
|    gfoidlBase64 |  SSSE3 |                       COMPlus_EnableAVX2=0 |       5 |    65.57 ns |  0.4514 ns |   0.4001 ns |    65.48 ns |  1.36 |    0.05 |
|                 |        |                                            |         |             |            |             |             |       |         |
| ConvertToBase64 | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |       5 |    48.34 ns |  0.9455 ns |   0.8844 ns |    48.63 ns |  1.00 |    0.00 |
|    gfoidlBase64 | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |       5 |    63.63 ns |  0.2880 ns |   0.2405 ns |    63.67 ns |  1.31 |    0.01 |
|                 |        |                                            |         |             |            |             |             |       |         |
| **ConvertToBase64** |   **AVX2** |                                      **Empty** |      **16** |    **76.04 ns** |  **1.7215 ns** |   **3.9554 ns** |    **76.30 ns** |  **1.00** |    **0.00** |
|    gfoidlBase64 |   AVX2 |                                      Empty |      16 |    98.50 ns |  2.0201 ns |   1.8896 ns |    98.91 ns |  1.24 |    0.05 |
|                 |        |                                            |         |             |            |             |             |       |         |
| ConvertToBase64 |  SSSE3 |                       COMPlus_EnableAVX2=0 |      16 |    81.04 ns |  1.3466 ns |   1.2596 ns |    81.34 ns |  1.00 |    0.00 |
|    gfoidlBase64 |  SSSE3 |                       COMPlus_EnableAVX2=0 |      16 |   101.13 ns |  2.1350 ns |   2.2845 ns |   100.79 ns |  1.25 |    0.03 |
|                 |        |                                            |         |             |            |             |             |       |         |
| ConvertToBase64 | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |      16 |    80.78 ns |  1.6500 ns |   1.9001 ns |    81.50 ns |  1.00 |    0.00 |
|    gfoidlBase64 | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |      16 |    96.92 ns |  1.8007 ns |   1.6844 ns |    96.78 ns |  1.20 |    0.04 |
|                 |        |                                            |         |             |            |             |             |       |         |
| **ConvertToBase64** |   **AVX2** |                                      **Empty** |    **1000** | **2,663.27 ns** | **28.1948 ns** |  **26.3734 ns** | **2,662.35 ns** |  **1.00** |    **0.00** |
|    gfoidlBase64 |   AVX2 |                                      Empty |    1000 |   576.20 ns | 24.2293 ns |  68.7346 ns |   549.36 ns |  0.23 |    0.02 |
|                 |        |                                            |         |             |            |             |             |       |         |
| ConvertToBase64 |  SSSE3 |                       COMPlus_EnableAVX2=0 |    1000 | 2,617.46 ns | 51.1691 ns |  47.8636 ns | 2,614.29 ns |  1.00 |    0.00 |
|    gfoidlBase64 |  SSSE3 |                       COMPlus_EnableAVX2=0 |    1000 |   654.67 ns | 13.2162 ns |  34.5845 ns |   648.14 ns |  0.26 |    0.01 |
|                 |        |                                            |         |             |            |             |             |       |         |
| ConvertToBase64 | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |    1000 | 2,656.95 ns | 53.1329 ns | 116.6280 ns | 2,622.63 ns |  1.00 |    0.00 |
|    gfoidlBase64 | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |    1000 | 2,017.40 ns | 39.2783 ns |  40.3359 ns | 2,017.04 ns |  0.74 |    0.03 |
