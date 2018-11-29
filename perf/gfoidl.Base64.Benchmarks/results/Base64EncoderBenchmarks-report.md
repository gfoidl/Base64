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
|                       Method |    Job |                       EnvironmentVariables |       Mean |      Error |     StdDev |     Median |
|----------------------------- |------- |------------------------------------------- |-----------:|-----------:|-----------:|-----------:|
|                  Encode_Data |   AVX2 |                                      Empty | 343.726 ns | 10.0195 ns | 29.5426 ns | 340.054 ns |
|                  Encode_Guid |   AVX2 |                                      Empty |  97.281 ns |  1.3678 ns |  1.2125 ns |  97.415 ns |
|                  Decode_Data |   AVX2 |                                      Empty | 254.633 ns |  5.2029 ns | 11.4206 ns | 251.966 ns |
|                  Decode_Guid |   AVX2 |                                      Empty |  69.843 ns |  1.5052 ns |  2.5148 ns |  68.943 ns |
| GetArraySizeRequiredToEncode |   AVX2 |                                      Empty |   2.189 ns |  0.0371 ns |  0.0347 ns |   2.192 ns |
| GetArraySizeRequiredToDecode |   AVX2 |                                      Empty |   9.752 ns |  0.4464 ns |  0.9610 ns |   9.232 ns |
|                  Encode_Data |  SSSE3 |                       COMPlus_EnableAVX2=0 | 349.299 ns |  7.0900 ns | 16.7120 ns | 345.790 ns |
|                  Encode_Guid |  SSSE3 |                       COMPlus_EnableAVX2=0 |  95.425 ns |  2.0278 ns |  2.1697 ns |  96.069 ns |
|                  Decode_Data |  SSSE3 |                       COMPlus_EnableAVX2=0 | 278.415 ns |  5.6966 ns | 15.4980 ns | 274.103 ns |
|                  Decode_Guid |  SSSE3 |                       COMPlus_EnableAVX2=0 |  68.796 ns |  1.0800 ns |  0.9574 ns |  69.015 ns |
| GetArraySizeRequiredToEncode |  SSSE3 |                       COMPlus_EnableAVX2=0 |   1.647 ns |  0.0176 ns |  0.0156 ns |   1.648 ns |
| GetArraySizeRequiredToDecode |  SSSE3 |                       COMPlus_EnableAVX2=0 |   9.378 ns |  0.4928 ns |  0.9727 ns |   8.999 ns |
|                  Encode_Data | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 | 986.491 ns | 19.3651 ns | 19.0192 ns | 983.073 ns |
|                  Encode_Guid | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |  96.592 ns |  1.1198 ns |  0.9351 ns |  97.031 ns |
|                  Decode_Data | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 | 868.723 ns |  5.9746 ns |  5.5887 ns | 867.447 ns |
|                  Decode_Guid | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |  80.652 ns |  0.5235 ns |  0.4897 ns |  80.704 ns |
| GetArraySizeRequiredToEncode | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |   1.897 ns |  0.0186 ns |  0.0174 ns |   1.895 ns |
| GetArraySizeRequiredToDecode | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |  11.099 ns |  0.0664 ns |  0.0554 ns |  11.088 ns |
