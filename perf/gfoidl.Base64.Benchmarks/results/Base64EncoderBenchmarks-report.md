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
|                  Encode_Data |   AVX2 |                                      Empty |   394.5091 ns | 13.1254 ns | 38.4944 ns |   384.2983 ns |
|                  Encode_Guid |   AVX2 |                                      Empty |   100.8085 ns |  1.5477 ns |  1.3720 ns |   100.8078 ns |
|                  Decode_Data |   AVX2 |                                      Empty |   314.8039 ns |  6.4383 ns | 13.5805 ns |   314.3586 ns |
|                  Decode_Guid |   AVX2 |                                      Empty |    70.6434 ns |  1.2812 ns |  1.1985 ns |    71.0590 ns |
| GetArraySizeRequiredToEncode |   AVX2 |                                      Empty |     0.9639 ns |  0.3771 ns |  0.7871 ns |     0.5216 ns |
| GetArraySizeRequiredToDecode |   AVX2 |                                      Empty |    12.3219 ns |  0.0856 ns |  0.0801 ns |    12.2913 ns |
|                  Encode_Data |  SSSE3 |                       COMPlus_EnableAVX2=0 |   424.3073 ns |  8.6030 ns | 23.8389 ns |   424.6142 ns |
|                  Encode_Guid |  SSSE3 |                       COMPlus_EnableAVX2=0 |   103.5616 ns |  2.2008 ns |  2.2601 ns |   104.8407 ns |
|                  Decode_Data |  SSSE3 |                       COMPlus_EnableAVX2=0 |   324.9873 ns |  5.8966 ns |  5.5156 ns |   325.9970 ns |
|                  Decode_Guid |  SSSE3 |                       COMPlus_EnableAVX2=0 |    70.3384 ns |  1.0169 ns |  0.9512 ns |    70.5477 ns |
| GetArraySizeRequiredToEncode |  SSSE3 |                       COMPlus_EnableAVX2=0 |     1.6589 ns |  0.1223 ns |  0.1084 ns |     1.6808 ns |
| GetArraySizeRequiredToDecode |  SSSE3 |                       COMPlus_EnableAVX2=0 |    11.0733 ns |  0.0861 ns |  0.0805 ns |    11.0406 ns |
|                  Encode_Data | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 | 1,088.7753 ns | 20.5433 ns | 18.2111 ns | 1,093.6047 ns |
|                  Encode_Guid | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |   100.1396 ns |  0.4920 ns |  0.4602 ns |   100.2871 ns |
|                  Decode_Data | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |   870.5556 ns | 12.6404 ns | 10.5553 ns |   869.7844 ns |
|                  Decode_Guid | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |    82.4976 ns |  0.8588 ns |  0.8033 ns |    82.7266 ns |
| GetArraySizeRequiredToEncode | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |     1.9484 ns |  0.0604 ns |  0.0565 ns |     1.9595 ns |
| GetArraySizeRequiredToDecode | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |    11.2192 ns |  0.0683 ns |  0.0639 ns |    11.2108 ns |
