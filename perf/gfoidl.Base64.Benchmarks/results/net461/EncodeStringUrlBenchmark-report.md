``` ini

BenchmarkDotNet=v0.12.0, OS=Windows 10.0.18362
Intel Core i7-7700HQ CPU 2.80GHz (Kaby Lake), 1 CPU, 8 logical and 4 physical cores
  [Host]     : .NET Framework 4.8 (4.8.4121.0), X64 RyuJIT
  DefaultJob : .NET Framework 4.8 (4.8.4121.0), X64 RyuJIT


```
|          Method | DataLen |        Mean |     Error |     StdDev |      Median | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------- |-------- |------------:|----------:|-----------:|------------:|------:|--------:|-------:|------:|------:|----------:|
| **ConvertToBase64** |       **5** |    **68.88 ns** |  **0.481 ns** |   **0.427 ns** |    **68.97 ns** |  **1.00** |    **0.00** | **0.0381** |     **-** |     **-** |     **120 B** |
| gfoidlBase64Url |       5 |    71.95 ns |  0.843 ns |   0.748 ns |    71.73 ns |  1.04 |    0.01 | 0.0126 |     - |     - |      40 B |
|                 |         |             |           |            |             |       |         |        |       |       |           |
| **ConvertToBase64** |      **16** |   **171.08 ns** |  **1.935 ns** |   **1.716 ns** |   **171.03 ns** |  **1.00** |    **0.00** | **0.0842** |     **-** |     **-** |     **265 B** |
| gfoidlBase64Url |      16 |    84.48 ns |  0.968 ns |   0.858 ns |    84.21 ns |  0.49 |    0.01 | 0.0229 |     - |     - |      72 B |
|                 |         |             |           |            |             |       |         |        |       |       |           |
| **ConvertToBase64** |    **1000** | **5,204.16 ns** | **94.801 ns** |  **79.163 ns** | **5,184.30 ns** |  **1.00** |    **0.00** | **3.4485** |     **-** |     **-** |   **10876 B** |
| gfoidlBase64Url |    1000 | 2,014.86 ns | 89.253 ns | 257.514 ns | 1,924.19 ns |  0.40 |    0.04 | 2.1706 |     - |     - |    6838 B |
