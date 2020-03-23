``` ini

BenchmarkDotNet=v0.12.0, OS=Windows 10.0.18362
Intel Core i7-7700HQ CPU 2.80GHz (Kaby Lake), 1 CPU, 8 logical and 4 physical cores
  [Host]     : .NET Framework 4.8 (4.8.4121.0), X64 RyuJIT
  DefaultJob : .NET Framework 4.8 (4.8.4121.0), X64 RyuJIT


```
|        Method | DataLen |        Mean |     Error |    StdDev |      Median | Ratio | RatioSD | Gen 0 | Gen 1 | Gen 2 | Allocated |
|-------------- |-------- |------------:|----------:|----------:|------------:|------:|--------:|------:|------:|------:|----------:|
| **BuffersBase64** |       **5** |    **35.53 ns** |  **0.721 ns** |  **0.830 ns** |    **35.53 ns** |  **1.00** |    **0.00** |     **-** |     **-** |     **-** |         **-** |
|  gfoidlBase64 |       5 |    36.72 ns |  0.746 ns |  0.766 ns |    36.68 ns |  1.03 |    0.04 |     - |     - |     - |         - |
|               |         |             |           |           |             |       |         |       |       |       |           |
| **BuffersBase64** |      **16** |    **60.60 ns** |  **1.274 ns** |  **3.508 ns** |    **58.99 ns** |  **1.00** |    **0.00** |     **-** |     **-** |     **-** |         **-** |
|  gfoidlBase64 |      16 |    47.73 ns |  0.718 ns |  0.672 ns |    47.56 ns |  0.77 |    0.06 |     - |     - |     - |         - |
|               |         |             |           |           |             |       |         |       |       |       |           |
| **BuffersBase64** |    **1000** | **1,136.12 ns** | **26.037 ns** | **40.536 ns** | **1,122.01 ns** |  **1.00** |    **0.00** |     **-** |     **-** |     **-** |         **-** |
|  gfoidlBase64 |    1000 | 1,035.82 ns | 15.145 ns | 11.824 ns | 1,037.03 ns |  0.90 |    0.05 |     - |     - |     - |         - |
