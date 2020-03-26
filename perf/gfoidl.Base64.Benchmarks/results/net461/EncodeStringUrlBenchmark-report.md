``` ini

BenchmarkDotNet=v0.12.0, OS=Windows 10.0.18362
Intel Core i7-7700HQ CPU 2.80GHz (Kaby Lake), 1 CPU, 8 logical and 4 physical cores
  [Host]     : .NET Framework 4.8 (4.8.4121.0), X64 RyuJIT
  DefaultJob : .NET Framework 4.8 (4.8.4121.0), X64 RyuJIT


```
|          Method | DataLen |        Mean |     Error |    StdDev | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------- |-------- |------------:|----------:|----------:|------:|--------:|-------:|------:|------:|----------:|
| **ConvertToBase64** |       **5** |    **68.38 ns** |  **0.704 ns** |  **0.658 ns** |  **1.00** |    **0.00** | **0.0381** |     **-** |     **-** |     **120 B** |
| gfoidlBase64Url |       5 |    71.73 ns |  1.627 ns |  1.359 ns |  1.05 |    0.02 | 0.0126 |     - |     - |      40 B |
|                 |         |             |           |           |       |         |        |       |       |           |
| **ConvertToBase64** |      **16** |   **170.44 ns** |  **4.023 ns** |  **3.763 ns** |  **1.00** |    **0.00** | **0.0842** |     **-** |     **-** |     **265 B** |
| gfoidlBase64Url |      16 |    84.56 ns |  1.758 ns |  1.954 ns |  0.50 |    0.02 | 0.0229 |     - |     - |      72 B |
|                 |         |             |           |           |       |         |        |       |       |           |
| **ConvertToBase64** |    **1000** | **5,059.78 ns** | **73.259 ns** | **61.175 ns** |  **1.00** |    **0.00** | **3.4485** |     **-** |     **-** |   **10876 B** |
| gfoidlBase64Url |    1000 | 1,701.66 ns | 29.158 ns | 25.848 ns |  0.34 |    0.01 | 2.1706 |     - |     - |    6837 B |
