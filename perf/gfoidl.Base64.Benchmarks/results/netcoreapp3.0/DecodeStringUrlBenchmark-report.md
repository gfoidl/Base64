``` ini

BenchmarkDotNet=v0.12.0, OS=Windows 10.0.18362
Intel Core i7-7700HQ CPU 2.80GHz (Kaby Lake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=5.0.100-preview.1.20167.6
  [Host] : .NET Core 3.1.2 (CoreCLR 4.700.20.6602, CoreFX 4.700.20.6702), X64 RyuJIT
  AVX2   : .NET Core 3.1.2 (CoreCLR 4.700.20.6602, CoreFX 4.700.20.6702), X64 RyuJIT
  SSSE3  : .NET Core 3.1.2 (CoreCLR 4.700.20.6602, CoreFX 4.700.20.6702), X64 RyuJIT
  Scalar : .NET Core 3.1.2 (CoreCLR 4.700.20.6602, CoreFX 4.700.20.6702), X64 RyuJIT


```
|                                   Method |    Job | EnvironmentVariables | DataLen |        Mean |      Error |     StdDev |      Median | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|----------------------------------------- |------- |--------------------- |-------- |------------:|-----------:|-----------:|------------:|------:|--------:|-------:|------:|------:|----------:|
| **ConvertFromBase64StringWithStringReplace** |   **AVX2** |                **Empty** |       **5** |   **117.36 ns** |   **2.471 ns** |   **5.104 ns** |   **115.76 ns** |  **1.00** |    **0.00** | **0.0508** |     **-** |     **-** |     **160 B** |
|                          gfoidlBase64Url |   AVX2 |                Empty |       5 |    25.31 ns |   0.573 ns |   0.614 ns |    25.25 ns |  0.21 |    0.01 | 0.0102 |     - |     - |      32 B |
|                                          |        |                      |         |             |            |            |             |       |         |        |       |       |           |
| ConvertFromBase64StringWithStringReplace |  SSSE3 |  COMPlus_EnableAVX=0 |       5 |   114.12 ns |   1.123 ns |   1.051 ns |   114.10 ns |  1.00 |    0.00 | 0.0508 |     - |     - |     160 B |
|                          gfoidlBase64Url |  SSSE3 |  COMPlus_EnableAVX=0 |       5 |    25.74 ns |   0.596 ns |   0.946 ns |    25.82 ns |  0.22 |    0.01 | 0.0102 |     - |     - |      32 B |
|                                          |        |                      |         |             |            |            |             |       |         |        |       |       |           |
| ConvertFromBase64StringWithStringReplace | Scalar |  COMPlus_EnableSSE=0 |       5 |   138.08 ns |   2.909 ns |   8.577 ns |   136.61 ns |  1.00 |    0.00 | 0.0508 |     - |     - |     160 B |
|                          gfoidlBase64Url | Scalar |  COMPlus_EnableSSE=0 |       5 |    23.15 ns |   0.696 ns |   2.040 ns |    22.02 ns |  0.17 |    0.02 | 0.0102 |     - |     - |      32 B |
|                                          |        |                      |         |             |            |            |             |       |         |        |       |       |           |
| **ConvertFromBase64StringWithStringReplace** |   **AVX2** |                **Empty** |      **16** |   **221.48 ns** |   **4.583 ns** |  **13.440 ns** |   **216.11 ns** |  **1.00** |    **0.00** | **0.0737** |     **-** |     **-** |     **232 B** |
|                          gfoidlBase64Url |   AVX2 |                Empty |      16 |    33.07 ns |   0.744 ns |   1.398 ns |    32.95 ns |  0.14 |    0.01 | 0.0127 |     - |     - |      40 B |
|                                          |        |                      |         |             |            |            |             |       |         |        |       |       |           |
| ConvertFromBase64StringWithStringReplace |  SSSE3 |  COMPlus_EnableAVX=0 |      16 |   193.73 ns |   3.550 ns |   3.321 ns |   192.95 ns |  1.00 |    0.00 | 0.0737 |     - |     - |     232 B |
|                          gfoidlBase64Url |  SSSE3 |  COMPlus_EnableAVX=0 |      16 |    29.95 ns |   0.682 ns |   0.785 ns |    29.76 ns |  0.16 |    0.00 | 0.0127 |     - |     - |      40 B |
|                                          |        |                      |         |             |            |            |             |       |         |        |       |       |           |
| ConvertFromBase64StringWithStringReplace | Scalar |  COMPlus_EnableSSE=0 |      16 |   220.88 ns |   4.467 ns |   8.606 ns |   220.83 ns |  1.00 |    0.00 | 0.0739 |     - |     - |     232 B |
|                          gfoidlBase64Url | Scalar |  COMPlus_EnableSSE=0 |      16 |    33.98 ns |   0.780 ns |   1.885 ns |    33.65 ns |  0.16 |    0.01 | 0.0127 |     - |     - |      40 B |
|                                          |        |                      |         |             |            |            |             |       |         |        |       |       |           |
| **ConvertFromBase64StringWithStringReplace** |   **AVX2** |                **Empty** |    **1000** | **6,400.76 ns** | **126.115 ns** | **220.881 ns** | **6,386.61 ns** |  **1.00** |    **0.00** | **2.0523** |     **-** |     **-** |    **6464 B** |
|                          gfoidlBase64Url |   AVX2 |                Empty |    1000 |   287.45 ns |   5.711 ns |  11.274 ns |   284.12 ns |  0.04 |    0.00 | 0.3262 |     - |     - |    1024 B |
|                                          |        |                      |         |             |            |            |             |       |         |        |       |       |           |
| ConvertFromBase64StringWithStringReplace |  SSSE3 |  COMPlus_EnableAVX=0 |    1000 | 6,440.57 ns | 128.400 ns | 333.729 ns | 6,390.41 ns |  1.00 |    0.00 | 2.0523 |     - |     - |    6464 B |
|                          gfoidlBase64Url |  SSSE3 |  COMPlus_EnableAVX=0 |    1000 |   335.84 ns |   6.968 ns |  17.861 ns |   328.71 ns |  0.05 |    0.00 | 0.3262 |     - |     - |    1024 B |
|                                          |        |                      |         |             |            |            |             |       |         |        |       |       |           |
| ConvertFromBase64StringWithStringReplace | Scalar |  COMPlus_EnableSSE=0 |    1000 | 6,630.50 ns |  52.305 ns |  43.677 ns | 6,635.65 ns |  1.00 |    0.00 | 2.0523 |     - |     - |    6464 B |
|                          gfoidlBase64Url | Scalar |  COMPlus_EnableSSE=0 |    1000 | 1,003.51 ns |   6.618 ns |   6.190 ns | 1,004.05 ns |  0.15 |    0.00 | 0.3262 |     - |     - |    1024 B |
