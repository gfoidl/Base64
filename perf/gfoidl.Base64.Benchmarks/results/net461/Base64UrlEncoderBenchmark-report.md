``` ini

BenchmarkDotNet=v0.12.0, OS=Windows 10.0.18362
Intel Core i7-7700HQ CPU 2.80GHz (Kaby Lake), 1 CPU, 8 logical and 4 physical cores
  [Host]     : .NET Framework 4.8 (4.8.4121.0), X64 RyuJIT
  DefaultJob : .NET Framework 4.8 (4.8.4121.0), X64 RyuJIT


```
|                       Method |       Mean |      Error |     StdDev |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|----------------------------- |-----------:|-----------:|-----------:|-------:|------:|------:|----------:|
|                  Encode_Data | 930.680 ns | 14.1025 ns | 11.7762 ns | 1.0939 |     - |     - |    3443 B |
|                  Encode_Guid |  96.448 ns |  0.4192 ns |  0.3921 ns | 0.0229 |     - |     - |      72 B |
|                  Decode_Data | 662.351 ns | 10.0879 ns |  8.4239 ns | 0.1678 |     - |     - |     530 B |
|                  Decode_Guid |  65.796 ns |  0.6604 ns |  0.5854 ns | 0.0126 |     - |     - |      40 B |
| GetArraySizeRequiredToEncode |   3.471 ns |  0.0464 ns |  0.0411 ns |      - |     - |     - |         - |
| GetArraySizeRequiredToDecode |  13.537 ns |  0.1367 ns |  0.1212 ns |      - |     - |     - |         - |
|          GetMaxDecodedLength |   3.027 ns |  0.0626 ns |  0.0585 ns |      - |     - |     - |         - |
