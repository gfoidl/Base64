``` ini

BenchmarkDotNet=v0.11.3, OS=Windows 10.0.17134.407 (1803/April2018Update/Redstone4)
Intel Core i7-7700HQ CPU 2.80GHz (Kaby Lake), 1 CPU, 8 logical and 4 physical cores
Frequency=2742187 Hz, Resolution=364.6724 ns, Timer=TSC
.NET Core SDK=3.0.100-preview-009765
  [Host]     : .NET Core 3.0.0-preview-27117-01 (CoreCLR 4.6.27116.05, CoreFX 4.7.18.56608), 64bit RyuJIT
  DefaultJob : .NET Core 3.0.0-preview-27117-01 (CoreCLR 4.6.27116.05, CoreFX 4.7.18.56608), 64bit RyuJIT


```
|                       Method |       Mean |     Error |    StdDev |
|----------------------------- |-----------:|----------:|----------:|
|         Base64UrlEncode_Data | 196.223 ns | 3.0274 ns | 2.6838 ns |
|         Base64UrlEncode_Guid |  53.401 ns | 0.2790 ns | 0.2329 ns |
|         Base64UrlDecode_Data | 241.765 ns | 2.2757 ns | 2.1287 ns |
|         Base64UrlDecode_Guid |  45.635 ns | 0.8086 ns | 0.7168 ns |
| GetArraySizeRequiredToEncode |   1.618 ns | 0.0344 ns | 0.0322 ns |
| GetArraySizeRequiredToDecode |   3.920 ns | 0.1027 ns | 0.0960 ns |
