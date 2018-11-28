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
|                  Encode_Data |   AVX2 |                                      Empty |   388.580 ns | 13.9119 ns | 41.0197 ns |   377.519 ns |
|                  Encode_Guid |   AVX2 |                                      Empty |    80.174 ns |  0.8314 ns |  0.7777 ns |    80.115 ns |
|                  Decode_Data |   AVX2 |                                      Empty |   325.777 ns |  6.5769 ns | 14.9790 ns |   330.007 ns |
|                  Decode_Guid |   AVX2 |                                      Empty |    67.746 ns |  1.4880 ns |  1.3918 ns |    68.059 ns |
| GetArraySizeRequiredToEncode |   AVX2 |                                      Empty |     3.323 ns |  0.0428 ns |  0.0379 ns |     3.318 ns |
| GetArraySizeRequiredToDecode |   AVX2 |                                      Empty |     9.936 ns |  0.0473 ns |  0.0442 ns |     9.926 ns |
|                  Encode_Data |  SSSE3 |                       COMPlus_EnableAVX2=0 |   446.492 ns | 10.9494 ns | 32.1126 ns |   447.804 ns |
|                  Encode_Guid |  SSSE3 |                       COMPlus_EnableAVX2=0 |    74.918 ns |  1.6525 ns |  3.1838 ns |    75.787 ns |
|                  Decode_Data |  SSSE3 |                       COMPlus_EnableAVX2=0 |   322.080 ns |  6.5755 ns | 17.6646 ns |   323.422 ns |
|                  Decode_Guid |  SSSE3 |                       COMPlus_EnableAVX2=0 |    67.153 ns |  0.7258 ns |  0.6789 ns |    67.274 ns |
| GetArraySizeRequiredToEncode |  SSSE3 |                       COMPlus_EnableAVX2=0 |     2.990 ns |  0.0367 ns |  0.0325 ns |     2.985 ns |
| GetArraySizeRequiredToDecode |  SSSE3 |                       COMPlus_EnableAVX2=0 |     9.251 ns |  0.0458 ns |  0.0429 ns |     9.259 ns |
|                  Encode_Data | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 | 1,126.864 ns | 11.7734 ns | 11.0128 ns | 1,127.098 ns |
|                  Encode_Guid | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |    90.103 ns |  1.7346 ns |  1.6225 ns |    89.768 ns |
|                  Decode_Data | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |   861.098 ns | 16.5828 ns | 20.3652 ns |   862.125 ns |
|                  Decode_Guid | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |    70.948 ns |  1.5615 ns |  4.3785 ns |    70.676 ns |
| GetArraySizeRequiredToEncode | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |     4.527 ns |  0.0314 ns |  0.0279 ns |     4.521 ns |
| GetArraySizeRequiredToDecode | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |     9.552 ns |  0.0693 ns |  0.0648 ns |     9.553 ns |
