``` ini

BenchmarkDotNet=v0.11.3, OS=ubuntu 16.04
Intel Xeon CPU 2.00GHz, 1 CPU, 2 logical cores and 1 physical core
.NET Core SDK=3.0.100-preview-009765
  [Host]     : .NET Core 3.0.0-preview-27117-01 (CoreCLR 4.6.27116.05, CoreFX 4.7.18.56608), 64bit RyuJIT
  DefaultJob : .NET Core 3.0.0-preview-27117-01 (CoreCLR 4.6.27116.05, CoreFX 4.7.18.56608), 64bit RyuJIT


```
|                                   Method | DataLen |         Mean |       Error |      StdDev |       Median | Ratio | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
|----------------------------------------- |-------- |-------------:|------------:|------------:|-------------:|------:|------------:|------------:|------------:|--------------------:|
| **ConvertFromBase64StringWithStringReplace** |       **5** |    **237.45 ns** |   **3.0669 ns** |   **2.7187 ns** |    **238.33 ns** |  **1.00** |      **0.0234** |           **-** |           **-** |               **160 B** |
|                          gfoidlBase64Url |       5 |     58.55 ns |   0.6626 ns |   0.5874 ns |     58.74 ns |  0.25 |      0.0046 |           - |           - |                32 B |
|                                          |         |              |             |             |              |       |             |             |             |                     |
| **ConvertFromBase64StringWithStringReplace** |      **16** |    **338.44 ns** |   **6.8924 ns** |  **11.5156 ns** |    **333.32 ns** |  **1.00** |      **0.0348** |           **-** |           **-** |               **232 B** |
|                          gfoidlBase64Url |      16 |     64.11 ns |   1.6066 ns |   3.6264 ns |     62.52 ns |  0.19 |      0.0058 |           - |           - |                40 B |
|                                          |         |              |             |             |              |       |             |             |             |                     |
| **ConvertFromBase64StringWithStringReplace** |    **1000** | **11,191.99 ns** | **173.6015 ns** | **153.8932 ns** | **11,166.18 ns** |  **1.00** |      **1.8158** |      **0.1068** |           **-** |             **11856 B** |
|                          gfoidlBase64Url |    1000 |    461.76 ns |   9.1884 ns |  17.7029 ns |    464.00 ns |  0.04 |      0.1607 |      0.0005 |           - |              1024 B |
