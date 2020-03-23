``` ini

BenchmarkDotNet=v0.12.0, OS=Windows 10.0.18362
Intel Core i7-7700HQ CPU 2.80GHz (Kaby Lake), 1 CPU, 8 logical and 4 physical cores
  [Host]     : .NET Framework 4.8 (4.8.4121.0), X64 RyuJIT
  DefaultJob : .NET Framework 4.8 (4.8.4121.0), X64 RyuJIT


```
|                     Method | DataLen |        Mean |     Error |    StdDev | Ratio |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|--------------------------- |-------- |------------:|----------:|----------:|------:|-------:|------:|------:|----------:|
| **ConvertFromBase64CharArray** |       **5** |    **34.21 ns** |  **0.239 ns** |  **0.224 ns** |  **1.00** | **0.0102** |     **-** |     **-** |      **32 B** |
|               gfoidlBase64 |       5 |    64.96 ns |  0.521 ns |  0.407 ns |  1.90 | 0.0101 |     - |     - |      32 B |
|                            |         |             |           |           |       |        |       |       |           |
| **ConvertFromBase64CharArray** |      **16** |    **75.28 ns** |  **0.295 ns** |  **0.261 ns** |  **1.00** | **0.0126** |     **-** |     **-** |      **40 B** |
|               gfoidlBase64 |      16 |    79.27 ns |  0.811 ns |  0.719 ns |  1.05 | 0.0126 |     - |     - |      40 B |
|                            |         |             |           |           |       |        |       |       |           |
| **ConvertFromBase64CharArray** |    **1000** | **3,270.70 ns** | **16.197 ns** | **15.150 ns** |  **1.00** | **0.3242** |     **-** |     **-** |    **1027 B** |
|               gfoidlBase64 |    1000 | 1,192.49 ns | 12.818 ns | 11.990 ns |  0.36 | 0.3262 |     - |     - |    1027 B |
