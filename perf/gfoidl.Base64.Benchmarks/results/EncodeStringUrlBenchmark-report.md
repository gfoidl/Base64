``` ini

BenchmarkDotNet=v0.12.0, OS=Windows 10.0.18362
Intel Core i7-7700HQ CPU 2.80GHz (Kaby Lake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=5.0.100-preview.1.20167.6
  [Host]     : .NET Core 3.1.2 (CoreCLR 4.700.20.6602, CoreFX 4.700.20.6702), X64 RyuJIT
  DefaultJob : .NET Core 3.1.2 (CoreCLR 4.700.20.6602, CoreFX 4.700.20.6702), X64 RyuJIT


```
|          Method | DataLen |        Mean |      Error |     StdDev | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------- |-------- |------------:|-----------:|-----------:|------:|--------:|-------:|------:|------:|----------:|
| **ConvertToBase64** |       **5** |    **57.45 ns** |   **1.251 ns** |   **2.288 ns** |  **1.00** |    **0.00** | **0.0254** |     **-** |     **-** |      **80 B** |
| gfoidlBase64Url |       5 |    28.71 ns |   0.314 ns |   0.279 ns |  0.48 |    0.02 | 0.0127 |     - |     - |      40 B |
|                 |         |             |            |            |       |         |        |       |       |           |
| **ConvertToBase64** |      **16** |   **125.32 ns** |   **2.589 ns** |   **2.543 ns** |  **1.00** |    **0.00** | **0.0687** |     **-** |     **-** |     **216 B** |
| gfoidlBase64Url |      16 |    34.23 ns |   0.708 ns |   0.662 ns |  0.27 |    0.01 | 0.0229 |     - |     - |      72 B |
|                 |         |             |            |            |       |         |        |       |       |           |
| **ConvertToBase64** |    **1000** | **5,422.61 ns** | **105.032 ns** | **132.832 ns** |  **1.00** |    **0.00** | **3.4332** |     **-** |     **-** |   **10784 B** |
| gfoidlBase64Url |    1000 |   370.64 ns |   7.129 ns |   8.756 ns |  0.07 |    0.00 | 0.8793 |     - |     - |    2760 B |
