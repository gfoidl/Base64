``` ini

BenchmarkDotNet=v0.12.0, OS=Windows 10.0.18362
Intel Core i7-7700HQ CPU 2.80GHz (Kaby Lake), 1 CPU, 8 logical and 4 physical cores
  [Host]     : .NET Framework 4.8 (4.8.4121.0), X64 RyuJIT
  DefaultJob : .NET Framework 4.8 (4.8.4121.0), X64 RyuJIT


```
|          Method | DataLen |        Mean |     Error |    StdDev | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------- |-------- |------------:|----------:|----------:|------:|--------:|-------:|------:|------:|----------:|
| **ConvertToBase64** |       **5** |    **27.02 ns** |  **0.350 ns** |  **0.328 ns** |  **1.00** |    **0.00** | **0.0153** |     **-** |     **-** |      **48 B** |
|    gfoidlBase64 |       5 |    71.45 ns |  0.749 ns |  0.701 ns |  2.64 |    0.04 | 0.0153 |     - |     - |      48 B |
|                 |         |             |           |           |       |         |        |       |       |           |
| **ConvertToBase64** |      **16** |    **47.38 ns** |  **0.428 ns** |  **0.379 ns** |  **1.00** |    **0.00** | **0.0255** |     **-** |     **-** |      **80 B** |
|    gfoidlBase64 |      16 |    88.56 ns |  1.825 ns |  2.731 ns |  1.85 |    0.05 | 0.0254 |     - |     - |      80 B |
|                 |         |             |           |           |       |         |        |       |       |           |
| **ConvertToBase64** |    **1000** | **1,694.93 ns** | **31.599 ns** | **28.012 ns** |  **1.00** |    **0.00** | **0.8602** |     **-** |     **-** |    **2714 B** |
|    gfoidlBase64 |    1000 | 1,708.36 ns | 16.448 ns | 13.735 ns |  1.01 |    0.01 | 2.1725 |     - |     - |    6844 B |
