``` ini

BenchmarkDotNet=v0.12.0, OS=Windows 10.0.18362
Intel Core i7-7700HQ CPU 2.80GHz (Kaby Lake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=5.0.100-preview.1.20167.6
  [Host] : .NET Core 3.1.2 (CoreCLR 4.700.20.6602, CoreFX 4.700.20.6702), X64 RyuJIT
  AVX2   : .NET Core 3.1.2 (CoreCLR 4.700.20.6602, CoreFX 4.700.20.6702), X64 RyuJIT
  SSSE3  : .NET Core 3.1.2 (CoreCLR 4.700.20.6602, CoreFX 4.700.20.6702), X64 RyuJIT
  Scalar : .NET Core 3.1.2 (CoreCLR 4.700.20.6602, CoreFX 4.700.20.6702), X64 RyuJIT


```
|          Method |    Job | EnvironmentVariables | DataLen |        Mean |     Error |    StdDev |      Median | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------- |------- |--------------------- |-------- |------------:|----------:|----------:|------------:|------:|--------:|-------:|------:|------:|----------:|
| **ConvertToBase64** |   **AVX2** |                **Empty** |       **5** |    **57.32 ns** |  **1.214 ns** |  **2.425 ns** |    **56.31 ns** |  **1.00** |    **0.00** | **0.0254** |     **-** |     **-** |      **80 B** |
| gfoidlBase64Url |   AVX2 |                Empty |       5 |    28.87 ns |  0.805 ns |  0.827 ns |    28.70 ns |  0.49 |    0.02 | 0.0127 |     - |     - |      40 B |
|                 |        |                      |         |             |           |           |             |       |         |        |       |       |           |
| ConvertToBase64 |  SSSE3 |  COMPlus_EnableAVX=0 |       5 |    58.70 ns |  1.295 ns |  2.200 ns |    58.25 ns |  1.00 |    0.00 | 0.0254 |     - |     - |      80 B |
| gfoidlBase64Url |  SSSE3 |  COMPlus_EnableAVX=0 |       5 |    28.53 ns |  0.648 ns |  0.887 ns |    28.26 ns |  0.48 |    0.02 | 0.0127 |     - |     - |      40 B |
|                 |        |                      |         |             |           |           |             |       |         |        |       |       |           |
| ConvertToBase64 | Scalar |  COMPlus_EnableSSE=0 |       5 |    55.67 ns |  0.755 ns |  0.706 ns |    55.38 ns |  1.00 |    0.00 | 0.0254 |     - |     - |      80 B |
| gfoidlBase64Url | Scalar |  COMPlus_EnableSSE=0 |       5 |    27.26 ns |  0.602 ns |  0.740 ns |    27.11 ns |  0.49 |    0.01 | 0.0127 |     - |     - |      40 B |
|                 |        |                      |         |             |           |           |             |       |         |        |       |       |           |
| **ConvertToBase64** |   **AVX2** |                **Empty** |      **16** |   **102.31 ns** |  **2.469 ns** |  **2.744 ns** |   **101.29 ns** |  **1.00** |    **0.00** | **0.0688** |     **-** |     **-** |     **216 B** |
| gfoidlBase64Url |   AVX2 |                Empty |      16 |    31.49 ns |  0.289 ns |  0.271 ns |    31.35 ns |  0.31 |    0.01 | 0.0229 |     - |     - |      72 B |
|                 |        |                      |         |             |           |           |             |       |         |        |       |       |           |
| ConvertToBase64 |  SSSE3 |  COMPlus_EnableAVX=0 |      16 |   117.45 ns |  1.149 ns |  1.075 ns |   117.36 ns |  1.00 |    0.00 | 0.0687 |     - |     - |     216 B |
| gfoidlBase64Url |  SSSE3 |  COMPlus_EnableAVX=0 |      16 |    31.30 ns |  0.423 ns |  0.396 ns |    31.20 ns |  0.27 |    0.00 | 0.0229 |     - |     - |      72 B |
|                 |        |                      |         |             |           |           |             |       |         |        |       |       |           |
| ConvertToBase64 | Scalar |  COMPlus_EnableSSE=0 |      16 |   103.61 ns |  1.119 ns |  1.047 ns |   103.63 ns |  1.00 |    0.00 | 0.0688 |     - |     - |     216 B |
| gfoidlBase64Url | Scalar |  COMPlus_EnableSSE=0 |      16 |    34.51 ns |  0.387 ns |  0.362 ns |    34.51 ns |  0.33 |    0.00 | 0.0229 |     - |     - |      72 B |
|                 |        |                      |         |             |           |           |             |       |         |        |       |       |           |
| **ConvertToBase64** |   **AVX2** |                **Empty** |    **1000** | **4,438.78 ns** | **39.616 ns** | **37.056 ns** | **4,427.60 ns** |  **1.00** |    **0.00** | **3.4332** |     **-** |     **-** |   **10784 B** |
| gfoidlBase64Url |   AVX2 |                Empty |    1000 |   351.02 ns |  5.020 ns |  4.192 ns |   351.90 ns |  0.08 |    0.00 | 0.8793 |     - |     - |    2760 B |
|                 |        |                      |         |             |           |           |             |       |         |        |       |       |           |
| ConvertToBase64 |  SSSE3 |  COMPlus_EnableAVX=0 |    1000 | 5,081.33 ns | 59.782 ns | 55.920 ns | 5,071.99 ns |  1.00 |    0.00 | 3.4332 |     - |     - |   10784 B |
| gfoidlBase64Url |  SSSE3 |  COMPlus_EnableAVX=0 |    1000 |   412.16 ns |  4.878 ns |  4.562 ns |   412.91 ns |  0.08 |    0.00 | 0.8793 |     - |     - |    2760 B |
|                 |        |                      |         |             |           |           |             |       |         |        |       |       |           |
| ConvertToBase64 | Scalar |  COMPlus_EnableSSE=0 |    1000 | 4,434.99 ns | 45.224 ns | 40.090 ns | 4,434.15 ns |  1.00 |    0.00 | 3.4332 |     - |     - |   10784 B |
| gfoidlBase64Url | Scalar |  COMPlus_EnableSSE=0 |    1000 | 1,067.00 ns | 10.978 ns |  9.732 ns | 1,068.90 ns |  0.24 |    0.00 | 0.8793 |     - |     - |    2760 B |
