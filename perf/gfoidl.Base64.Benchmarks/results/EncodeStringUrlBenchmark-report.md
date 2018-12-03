``` ini

BenchmarkDotNet=v0.11.3, OS=ubuntu 16.04
Intel Xeon CPU 2.00GHz, 1 CPU, 2 logical cores and 1 physical core
.NET Core SDK=3.0.100-preview-009765
  [Host]     : .NET Core 3.0.0-preview-27117-01 (CoreCLR 4.6.27116.05, CoreFX 4.7.18.56608), 64bit RyuJIT
  DefaultJob : .NET Core 3.0.0-preview-27117-01 (CoreCLR 4.6.27116.05, CoreFX 4.7.18.56608), 64bit RyuJIT


```
|          Method | DataLen |        Mean |      Error |      StdDev |      Median | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
|---------------- |-------- |------------:|-----------:|------------:|------------:|------:|--------:|------------:|------------:|------------:|--------------------:|
| **ConvertToBase64** |       **5** |   **127.36 ns** |   **1.019 ns** |   **0.9032 ns** |   **127.64 ns** |  **1.00** |    **0.00** |      **0.0117** |           **-** |           **-** |                **80 B** |
| gfoidlBase64Url |       5 |    64.76 ns |   1.393 ns |   1.8118 ns |    64.43 ns |  0.51 |    0.02 |      0.0058 |           - |           - |                40 B |
|                 |         |             |            |             |             |       |         |             |             |             |                     |
| **ConvertToBase64** |      **16** |   **186.39 ns** |   **3.860 ns** |   **8.5543 ns** |   **182.84 ns** |  **1.00** |    **0.00** |      **0.0219** |           **-** |           **-** |               **144 B** |
| gfoidlBase64Url |      16 |    72.90 ns |   1.594 ns |   2.7923 ns |    72.66 ns |  0.39 |    0.02 |      0.0110 |           - |           - |                72 B |
|                 |         |             |            |             |             |       |         |             |             |             |                     |
| **ConvertToBase64** |    **1000** | **7,270.87 ns** | **139.271 ns** | **185.9226 ns** | **7,244.97 ns** |  **1.00** |    **0.00** |      **1.6861** |      **0.0229** |           **-** |             **10784 B** |
| gfoidlBase64Url |    1000 |   539.53 ns |  13.882 ns |  40.9317 ns |   535.55 ns |  0.08 |    0.00 |      0.4349 |      0.0038 |           - |              2760 B |
