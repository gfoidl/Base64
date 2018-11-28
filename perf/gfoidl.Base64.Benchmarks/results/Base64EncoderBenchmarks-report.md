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
|                       Method |    Job |                       EnvironmentVariables |          Mean |      Error |     StdDev |        Median |
|----------------------------- |------- |------------------------------------------- |--------------:|-----------:|-----------:|--------------:|
|                  Encode_Data |   AVX2 |                                      Empty |   318.8990 ns |  8.4442 ns | 22.6849 ns |   313.8722 ns |
|                  Encode_Guid |   AVX2 |                                      Empty |    97.8886 ns |  2.0248 ns |  2.4866 ns |    98.6319 ns |
|                  Decode_Data |   AVX2 |                                      Empty |   399.1581 ns |  7.9936 ns | 10.1093 ns |   397.3372 ns |
|                  Decode_Guid |   AVX2 |                                      Empty |    70.2316 ns |  1.4042 ns |  1.3135 ns |    70.6731 ns |
| GetArraySizeRequiredToEncode |   AVX2 |                                      Empty |     0.9382 ns |  0.3441 ns |  0.7480 ns |     0.5322 ns |
| GetArraySizeRequiredToDecode |   AVX2 |                                      Empty |     9.7242 ns |  0.5175 ns |  1.1029 ns |     9.2419 ns |
|                  Encode_Data |  SSSE3 |                       COMPlus_EnableAVX2=0 |   356.4452 ns | 10.6504 ns | 29.8648 ns |   352.1573 ns |
|                  Encode_Guid |  SSSE3 |                       COMPlus_EnableAVX2=0 |    97.7851 ns |  1.4565 ns |  1.2162 ns |    98.1725 ns |
|                  Decode_Data |  SSSE3 |                       COMPlus_EnableAVX2=0 |   289.5804 ns |  6.5173 ns | 16.1091 ns |   289.7305 ns |
|                  Decode_Guid |  SSSE3 |                       COMPlus_EnableAVX2=0 |    66.5454 ns |  1.4732 ns |  3.0094 ns |    67.0437 ns |
| GetArraySizeRequiredToEncode |  SSSE3 |                       COMPlus_EnableAVX2=0 |     1.6149 ns |  0.0244 ns |  0.0229 ns |     1.6181 ns |
| GetArraySizeRequiredToDecode |  SSSE3 |                       COMPlus_EnableAVX2=0 |    10.9105 ns |  0.0488 ns |  0.0432 ns |    10.9042 ns |
|                  Encode_Data | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 | 1,081.1050 ns | 23.4213 ns | 69.0584 ns | 1,056.4079 ns |
|                  Encode_Guid | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |    91.7359 ns |  2.0360 ns |  4.3828 ns |    92.0605 ns |
|                  Decode_Data | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |   862.0597 ns | 17.0732 ns | 22.7922 ns |   869.3793 ns |
|                  Decode_Guid | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |    79.3121 ns |  1.7365 ns |  2.5453 ns |    78.4045 ns |
| GetArraySizeRequiredToEncode | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |     1.8900 ns |  0.0328 ns |  0.0307 ns |     1.8761 ns |
| GetArraySizeRequiredToDecode | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |    11.2511 ns |  0.1092 ns |  0.1022 ns |    11.2652 ns |
