``` ini

BenchmarkDotNet=v0.11.3, OS=ubuntu 16.04
Intel Xeon CPU 2.00GHz, 1 CPU, 2 logical cores and 1 physical core
.NET Core SDK=3.0.100-preview-009765
  [Host] : .NET Core 3.0.0-preview-27117-01 (CoreCLR 4.6.27116.05, CoreFX 4.7.18.56608), 64bit RyuJIT
  AVX2   : .NET Core 3.0.0-preview-27117-01 (CoreCLR 4.6.27116.05, CoreFX 4.7.18.56608), 64bit RyuJIT
  SSSE3  : .NET Core 3.0.0-preview-27117-01 (CoreCLR 4.6.27116.05, CoreFX 4.7.18.56608), 64bit RyuJIT
  Scalar : .NET Core 3.0.0-preview-27117-01 (CoreCLR 4.6.27116.05, CoreFX 4.7.18.56608), 64bit RyuJIT

Runtime=Core  

```
|                       Method |    Job |                       EnvironmentVariables |         Mean |      Error |     StdDev |       Median |
|----------------------------- |------- |------------------------------------------- |-------------:|-----------:|-----------:|-------------:|
|                  Encode_Data |   AVX2 |                                      Empty |   327.749 ns | 10.7769 ns | 29.6828 ns |   320.239 ns |
|                  Encode_Guid |   AVX2 |                                      Empty |    74.896 ns |  1.6372 ns |  3.4174 ns |    75.961 ns |
|                  Decode_Data |   AVX2 |                                      Empty |   449.276 ns |  7.1115 ns |  6.6521 ns |   450.256 ns |
|                  Decode_Guid |   AVX2 |                                      Empty |    65.070 ns |  1.4480 ns |  1.8313 ns |    66.323 ns |
| GetArraySizeRequiredToEncode |   AVX2 |                                      Empty |     3.656 ns |  0.0269 ns |  0.0238 ns |     3.656 ns |
| GetArraySizeRequiredToDecode |   AVX2 |                                      Empty |     9.877 ns |  0.0585 ns |  0.0518 ns |     9.863 ns |
|                  Encode_Data |  SSSE3 |                       COMPlus_EnableAVX2=0 |   383.231 ns |  8.3680 ns | 22.9074 ns |   380.496 ns |
|                  Encode_Guid |  SSSE3 |                       COMPlus_EnableAVX2=0 |    75.185 ns |  1.5573 ns |  1.4567 ns |    75.603 ns |
|                  Decode_Data |  SSSE3 |                       COMPlus_EnableAVX2=0 |   311.558 ns |  6.2258 ns |  9.5075 ns |   311.041 ns |
|                  Decode_Guid |  SSSE3 |                       COMPlus_EnableAVX2=0 |    66.622 ns |  0.5339 ns |  0.4733 ns |    66.712 ns |
| GetArraySizeRequiredToEncode |  SSSE3 |                       COMPlus_EnableAVX2=0 |     4.121 ns |  0.1312 ns |  0.1227 ns |     4.105 ns |
| GetArraySizeRequiredToDecode |  SSSE3 |                       COMPlus_EnableAVX2=0 |     9.415 ns |  0.0855 ns |  0.0799 ns |     9.393 ns |
|                  Encode_Data | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 | 1,052.649 ns | 20.3149 ns | 21.7367 ns | 1,059.519 ns |
|                  Encode_Guid | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |    88.602 ns |  1.6878 ns |  1.5788 ns |    88.952 ns |
|                  Decode_Data | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |   846.240 ns |  5.5065 ns |  5.1508 ns |   846.927 ns |
|                  Decode_Guid | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |    69.334 ns |  1.5263 ns |  4.4037 ns |    68.543 ns |
| GetArraySizeRequiredToEncode | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |     4.248 ns |  0.1679 ns |  0.1571 ns |     4.190 ns |
| GetArraySizeRequiredToDecode | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |     7.666 ns |  0.4751 ns |  1.1831 ns |     6.998 ns |
