``` ini

BenchmarkDotNet=v0.12.0, OS=Windows 10.0.18362
Intel Core i7-7700HQ CPU 2.80GHz (Kaby Lake), 1 CPU, 8 logical and 4 physical cores
  [Host]     : .NET Framework 4.8 (4.8.4121.0), X64 RyuJIT
  DefaultJob : .NET Framework 4.8 (4.8.4121.0), X64 RyuJIT


```
|                                   Method | DataLen |        Mean |     Error |    StdDev | Ratio |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|----------------------------------------- |-------- |------------:|----------:|----------:|------:|-------:|------:|------:|----------:|
| **ConvertFromBase64StringWithStringReplace** |       **5** |   **107.43 ns** |  **2.200 ns** |  **1.950 ns** |  **1.00** | **0.0535** |     **-** |     **-** |     **168 B** |
|                          gfoidlBase64Url |       5 |    58.52 ns |  0.800 ns |  0.709 ns |  0.54 | 0.0101 |     - |     - |      32 B |
|                                          |         |             |           |           |       |        |       |       |           |
| **ConvertFromBase64StringWithStringReplace** |      **16** |   **188.27 ns** |  **3.795 ns** |  **5.066 ns** |  **1.00** | **0.0763** |     **-** |     **-** |     **241 B** |
|                          gfoidlBase64Url |      16 |    69.77 ns |  0.520 ns |  0.461 ns |  0.37 | 0.0126 |     - |     - |      40 B |
|                                          |         |             |           |           |       |        |       |       |           |
| **ConvertFromBase64StringWithStringReplace** |    **1000** | **6,460.87 ns** | **91.833 ns** | **81.407 ns** |  **1.00** | **2.0599** |     **-** |     **-** |    **6492 B** |
|                          gfoidlBase64Url |    1000 | 1,231.36 ns | 12.711 ns | 11.268 ns |  0.19 | 0.3262 |     - |     - |    1027 B |
