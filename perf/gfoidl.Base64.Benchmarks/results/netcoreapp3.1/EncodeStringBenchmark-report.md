``` ini

BenchmarkDotNet=v0.12.0, OS=Windows 10.0.18362
Intel Core i7-7700HQ CPU 2.80GHz (Kaby Lake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=5.0.100-preview.1.20167.6
  [Host] : .NET Core 3.1.2 (CoreCLR 4.700.20.6602, CoreFX 4.700.20.6702), X64 RyuJIT
  AVX2   : .NET Core 3.1.2 (CoreCLR 4.700.20.6602, CoreFX 4.700.20.6702), X64 RyuJIT
  SSSE3  : .NET Core 3.1.2 (CoreCLR 4.700.20.6602, CoreFX 4.700.20.6702), X64 RyuJIT
  Scalar : .NET Core 3.1.2 (CoreCLR 4.700.20.6602, CoreFX 4.700.20.6702), X64 RyuJIT


```
|          Method |    Job | EnvironmentVariables | DataLen |        Mean |     Error |    StdDev | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------- |------- |--------------------- |-------- |------------:|----------:|----------:|------:|--------:|-------:|------:|------:|----------:|
| **ConvertToBase64** |   **AVX2** |                **Empty** |       **5** |    **23.77 ns** |  **0.378 ns** |  **0.335 ns** |  **1.00** |    **0.00** | **0.0127** |     **-** |     **-** |      **40 B** |
|    gfoidlBase64 |   AVX2 |                Empty |       5 |    24.26 ns |  0.213 ns |  0.199 ns |  1.02 |    0.01 | 0.0127 |     - |     - |      40 B |
|                 |        |                      |         |             |           |           |       |         |        |       |       |           |
| ConvertToBase64 |  SSSE3 |  COMPlus_EnableAVX=0 |       5 |    24.55 ns |  0.621 ns |  0.715 ns |  1.00 |    0.00 | 0.0127 |     - |     - |      40 B |
|    gfoidlBase64 |  SSSE3 |  COMPlus_EnableAVX=0 |       5 |    24.60 ns |  0.542 ns |  0.507 ns |  1.00 |    0.02 | 0.0127 |     - |     - |      40 B |
|                 |        |                      |         |             |           |           |       |         |        |       |       |           |
| ConvertToBase64 | Scalar |  COMPlus_EnableSSE=0 |       5 |    23.93 ns |  0.340 ns |  0.318 ns |  1.00 |    0.00 | 0.0127 |     - |     - |      40 B |
|    gfoidlBase64 | Scalar |  COMPlus_EnableSSE=0 |       5 |    27.02 ns |  0.823 ns |  2.360 ns |  1.12 |    0.12 | 0.0127 |     - |     - |      40 B |
|                 |        |                      |         |             |           |           |       |         |        |       |       |           |
| **ConvertToBase64** |   **AVX2** |                **Empty** |      **16** |    **42.17 ns** |  **0.989 ns** |  **1.387 ns** |  **1.00** |    **0.00** | **0.0229** |     **-** |     **-** |      **72 B** |
|    gfoidlBase64 |   AVX2 |                Empty |      16 |    37.23 ns |  0.993 ns |  2.834 ns |  0.92 |    0.06 | 0.0229 |     - |     - |      72 B |
|                 |        |                      |         |             |           |           |       |         |        |       |       |           |
| ConvertToBase64 |  SSSE3 |  COMPlus_EnableAVX=0 |      16 |    41.97 ns |  0.807 ns |  0.715 ns |  1.00 |    0.00 | 0.0229 |     - |     - |      72 B |
|    gfoidlBase64 |  SSSE3 |  COMPlus_EnableAVX=0 |      16 |    34.63 ns |  0.600 ns |  0.561 ns |  0.82 |    0.02 | 0.0229 |     - |     - |      72 B |
|                 |        |                      |         |             |           |           |       |         |        |       |       |           |
| ConvertToBase64 | Scalar |  COMPlus_EnableSSE=0 |      16 |    42.37 ns |  0.512 ns |  0.479 ns |  1.00 |    0.00 | 0.0229 |     - |     - |      72 B |
|    gfoidlBase64 | Scalar |  COMPlus_EnableSSE=0 |      16 |    39.13 ns |  0.810 ns |  0.758 ns |  0.92 |    0.02 | 0.0229 |     - |     - |      72 B |
|                 |        |                      |         |             |           |           |       |         |        |       |       |           |
| **ConvertToBase64** |   **AVX2** |                **Empty** |    **1000** | **1,571.76 ns** | **11.677 ns** | **10.923 ns** |  **1.00** |    **0.00** | **0.8583** |     **-** |     **-** |    **2696 B** |
|    gfoidlBase64 |   AVX2 |                Empty |    1000 |   376.48 ns |  5.348 ns |  5.003 ns |  0.24 |    0.00 | 0.8793 |     - |     - |    2760 B |
|                 |        |                      |         |             |           |           |       |         |        |       |       |           |
| ConvertToBase64 |  SSSE3 |  COMPlus_EnableAVX=0 |    1000 | 1,562.76 ns | 15.192 ns | 13.467 ns |  1.00 |    0.00 | 0.8583 |     - |     - |    2696 B |
|    gfoidlBase64 |  SSSE3 |  COMPlus_EnableAVX=0 |    1000 |   449.36 ns |  8.947 ns | 15.193 ns |  0.29 |    0.01 | 0.8793 |     - |     - |    2760 B |
|                 |        |                      |         |             |           |           |       |         |        |       |       |           |
| ConvertToBase64 | Scalar |  COMPlus_EnableSSE=0 |    1000 | 1,611.21 ns | 32.101 ns | 46.038 ns |  1.00 |    0.00 | 0.8583 |     - |     - |    2696 B |
|    gfoidlBase64 | Scalar |  COMPlus_EnableSSE=0 |    1000 | 1,130.69 ns | 19.930 ns | 22.152 ns |  0.71 |    0.02 | 0.8793 |     - |     - |    2760 B |
