``` ini

BenchmarkDotNet=v0.12.0, OS=Windows 10.0.18362
Intel Core i7-7700HQ CPU 2.80GHz (Kaby Lake), 1 CPU, 8 logical and 4 physical cores
  [Host]     : .NET Framework 4.8 (4.8.4121.0), X64 RyuJIT
  DefaultJob : .NET Framework 4.8 (4.8.4121.0), X64 RyuJIT


```
|                       Method |       Mean |     Error |    StdDev |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|----------------------------- |-----------:|----------:|----------:|-------:|------:|------:|----------:|
|                  Encode_Data | 918.029 ns | 7.2174 ns | 6.0269 ns | 1.0958 |     - |     - |    3451 B |
|                  Encode_Guid |  95.461 ns | 0.4049 ns | 0.3589 ns | 0.0254 |     - |     - |      80 B |
|                  Decode_Data | 650.626 ns | 8.7997 ns | 8.2313 ns | 0.1678 |     - |     - |     530 B |
|                  Decode_Guid |  82.348 ns | 0.9401 ns | 0.8794 ns | 0.0126 |     - |     - |      40 B |
| GetArraySizeRequiredToEncode |   2.252 ns | 0.0556 ns | 0.0520 ns |      - |     - |     - |         - |
| GetArraySizeRequiredToDecode |  25.775 ns | 0.1683 ns | 0.1574 ns |      - |     - |     - |         - |
|          GetMaxDecodedLength |   2.243 ns | 0.0586 ns | 0.0520 ns |      - |     - |     - |         - |
