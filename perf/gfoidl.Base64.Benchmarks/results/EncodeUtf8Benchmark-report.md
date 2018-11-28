``` ini

BenchmarkDotNet=v0.11.3, OS=ubuntu 16.04
Intel Xeon CPU 2.00GHz, 1 CPU, 2 logical cores and 1 physical core
.NET Core SDK=3.0.100-preview-009765
  [Host]     : .NET Core 3.0.0-preview-27117-01 (CoreCLR 4.6.27116.05, CoreFX 4.7.18.56608), 64bit RyuJIT
  DefaultJob : .NET Core 3.0.0-preview-27117-01 (CoreCLR 4.6.27116.05, CoreFX 4.7.18.56608), 64bit RyuJIT


```
|             Method | DataLen |        Mean |      Error |    StdDev |      Median | Ratio | RatioSD |
|------------------- |-------- |------------:|-----------:|----------:|------------:|------:|--------:|
|      **BuffersBase64** |       **5** |    **31.46 ns** |  **0.6757 ns** |  **1.593 ns** |    **32.04 ns** |  **1.00** |    **0.00** |
|       gfoidlBase64 |       5 |    54.57 ns |  1.1289 ns |  2.280 ns |    55.17 ns |  1.75 |    0.12 |
| gfoidlBase64Static |       5 |    45.77 ns |  0.9627 ns |  2.619 ns |    46.46 ns |  1.47 |    0.10 |
|                    |         |             |            |           |             |       |         |
|      **BuffersBase64** |      **16** |    **48.27 ns** |  **1.0087 ns** |  **2.894 ns** |    **48.46 ns** |  **1.00** |    **0.00** |
|       gfoidlBase64 |      16 |    58.42 ns |  1.1932 ns |  1.326 ns |    59.24 ns |  1.14 |    0.08 |
| gfoidlBase64Static |      16 |    49.49 ns |  1.0289 ns |  2.782 ns |    50.56 ns |  1.02 |    0.07 |
|                    |         |             |            |           |             |       |         |
|      **BuffersBase64** |    **1000** | **1,434.80 ns** | **27.7913 ns** | **32.005 ns** | **1,447.29 ns** |  **1.00** |    **0.00** |
|       gfoidlBase64 |    1000 |   155.91 ns |  3.1503 ns |  3.371 ns |   157.52 ns |  0.11 |    0.00 |
| gfoidlBase64Static |    1000 |   149.82 ns |  3.0348 ns |  7.995 ns |   152.06 ns |  0.10 |    0.01 |
