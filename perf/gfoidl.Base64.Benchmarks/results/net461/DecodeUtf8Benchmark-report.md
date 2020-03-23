``` ini

BenchmarkDotNet=v0.12.0, OS=Windows 10.0.18362
Intel Core i7-7700HQ CPU 2.80GHz (Kaby Lake), 1 CPU, 8 logical and 4 physical cores
  [Host]     : .NET Framework 4.8 (4.8.4121.0), X64 RyuJIT
  DefaultJob : .NET Framework 4.8 (4.8.4121.0), X64 RyuJIT


```
|        Method | DataLen |        Mean |    Error |   StdDev | Ratio | RatioSD | Gen 0 | Gen 1 | Gen 2 | Allocated |
|-------------- |-------- |------------:|---------:|---------:|------:|--------:|------:|------:|------:|----------:|
| **BuffersBase64** |       **5** |    **33.71 ns** | **0.345 ns** | **0.306 ns** |  **1.00** |    **0.00** |     **-** |     **-** |     **-** |         **-** |
|  gfoidlBase64 |       5 |    40.35 ns | 0.891 ns | 1.189 ns |  1.21 |    0.04 |     - |     - |     - |         - |
|               |         |             |          |          |       |         |       |       |       |           |
| **BuffersBase64** |      **16** |    **44.03 ns** | **0.916 ns** | **0.857 ns** |  **1.00** |    **0.00** |     **-** |     **-** |     **-** |         **-** |
|  gfoidlBase64 |      16 |    47.83 ns | 0.342 ns | 0.303 ns |  1.09 |    0.02 |     - |     - |     - |         - |
|               |         |             |          |          |       |         |       |       |       |           |
| **BuffersBase64** |    **1000** |   **921.87 ns** | **8.057 ns** | **7.143 ns** |  **1.00** |    **0.00** |     **-** |     **-** |     **-** |         **-** |
|  gfoidlBase64 |    1000 | 1,101.14 ns | 4.228 ns | 3.530 ns |  1.19 |    0.01 |     - |     - |     - |         - |
