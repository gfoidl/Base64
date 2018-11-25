``` ini

BenchmarkDotNet=v0.11.3, OS=Windows 10.0.17134.407 (1803/April2018Update/Redstone4)
Intel Core i7-7700HQ CPU 2.80GHz (Kaby Lake), 1 CPU, 8 logical and 4 physical cores
Frequency=2742187 Hz, Resolution=364.6724 ns, Timer=TSC
.NET Core SDK=3.0.100-preview-009765
  [Host]     : .NET Core 3.0.0-preview-27117-01 (CoreCLR 4.6.27116.05, CoreFX 4.7.18.56608), 64bit RyuJIT
  DefaultJob : .NET Core 3.0.0-preview-27117-01 (CoreCLR 4.6.27116.05, CoreFX 4.7.18.56608), 64bit RyuJIT


```
|                       Method |       Mean |     Error |    StdDev |     Median |
|----------------------------- |-----------:|----------:|----------:|-----------:|
|         Base64UrlEncode_Data | 209.211 ns | 4.2732 ns | 9.9039 ns | 205.359 ns |
|         Base64UrlEncode_Guid |  57.885 ns | 0.9840 ns | 0.8723 ns |  57.663 ns |
|         Base64UrlDecode_Data | 283.293 ns | 4.9543 ns | 4.6342 ns | 283.710 ns |
|         Base64UrlDecode_Guid |  47.787 ns | 0.4418 ns | 0.4133 ns |  47.898 ns |
| GetArraySizeRequiredToEncode |   3.010 ns | 0.0588 ns | 0.0550 ns |   2.995 ns |
| GetArraySizeRequiredToDecode |   3.366 ns | 0.0554 ns | 0.0518 ns |   3.343 ns |
