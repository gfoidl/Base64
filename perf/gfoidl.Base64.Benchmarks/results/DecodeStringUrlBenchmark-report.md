``` ini

BenchmarkDotNet=v0.12.0, OS=Windows 10.0.18362
Intel Core i7-7700HQ CPU 2.80GHz (Kaby Lake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=5.0.100-preview.1.20167.6
  [Host]     : .NET Core 3.1.2 (CoreCLR 4.700.20.6602, CoreFX 4.700.20.6702), X64 RyuJIT
  DefaultJob : .NET Core 3.1.2 (CoreCLR 4.700.20.6602, CoreFX 4.700.20.6702), X64 RyuJIT


```
|                                   Method | DataLen |        Mean |     Error |    StdDev |      Median | Ratio |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|----------------------------------------- |-------- |------------:|----------:|----------:|------------:|------:|-------:|------:|------:|----------:|
| **ConvertFromBase64StringWithStringReplace** |       **5** |   **116.99 ns** |  **2.690 ns** |  **5.960 ns** |   **114.27 ns** |  **1.00** | **0.0508** |     **-** |     **-** |     **160 B** |
|                          gfoidlBase64Url |       5 |    24.95 ns |  0.721 ns |  0.740 ns |    24.58 ns |  0.21 | 0.0102 |     - |     - |      32 B |
|                                          |         |             |           |           |             |       |        |       |       |           |
| **ConvertFromBase64StringWithStringReplace** |      **16** |   **225.88 ns** |  **4.506 ns** |  **3.762 ns** |   **224.90 ns** |  **1.00** | **0.0737** |     **-** |     **-** |     **232 B** |
|                          gfoidlBase64Url |      16 |    28.59 ns |  0.745 ns |  0.732 ns |    28.45 ns |  0.13 | 0.0127 |     - |     - |      40 B |
|                                          |         |             |           |           |             |       |        |       |       |           |
| **ConvertFromBase64StringWithStringReplace** |    **1000** | **5,992.90 ns** | **48.504 ns** | **45.370 ns** | **5,980.04 ns** |  **1.00** | **2.0523** |     **-** |     **-** |    **6464 B** |
|                          gfoidlBase64Url |    1000 |   275.18 ns |  5.572 ns |  5.212 ns |   273.59 ns |  0.05 | 0.3262 |     - |     - |    1024 B |
