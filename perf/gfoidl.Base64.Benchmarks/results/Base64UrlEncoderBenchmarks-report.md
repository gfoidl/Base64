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
|                  Encode_Data |   AVX2 |                                      Empty |   300.289 ns |  6.1001 ns | 16.5957 ns |   293.529 ns |
|                  Encode_Guid |   AVX2 |                                      Empty |    76.114 ns |  0.7742 ns |  0.7242 ns |    76.330 ns |
|                  Decode_Data |   AVX2 |                                      Empty |   265.520 ns |  5.4690 ns | 10.1371 ns |   263.705 ns |
|                  Decode_Guid |   AVX2 |                                      Empty |    59.972 ns |  1.4369 ns |  4.0056 ns |    60.334 ns |
| GetArraySizeRequiredToEncode |   AVX2 |                                      Empty |     3.574 ns |  0.0955 ns |  0.0847 ns |     3.550 ns |
| GetArraySizeRequiredToDecode |   AVX2 |                                      Empty |    10.014 ns |  0.1010 ns |  0.0944 ns |    10.008 ns |
|                  Encode_Data |  SSSE3 |                       COMPlus_EnableAVX2=0 |   358.641 ns |  6.9517 ns |  6.8275 ns |   358.333 ns |
|                  Encode_Guid |  SSSE3 |                       COMPlus_EnableAVX2=0 |    75.173 ns |  0.5356 ns |  0.4473 ns |    75.191 ns |
|                  Decode_Data |  SSSE3 |                       COMPlus_EnableAVX2=0 |   286.618 ns |  5.8500 ns | 12.5928 ns |   287.010 ns |
|                  Decode_Guid |  SSSE3 |                       COMPlus_EnableAVX2=0 |    65.419 ns |  1.0784 ns |  0.9560 ns |    65.646 ns |
| GetArraySizeRequiredToEncode |  SSSE3 |                       COMPlus_EnableAVX2=0 |     4.032 ns |  0.0450 ns |  0.0421 ns |     4.032 ns |
| GetArraySizeRequiredToDecode |  SSSE3 |                       COMPlus_EnableAVX2=0 |     9.444 ns |  0.1554 ns |  0.1453 ns |     9.364 ns |
|                  Encode_Data | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 | 1,004.000 ns | 19.8874 ns | 27.2220 ns | 1,004.760 ns |
|                  Encode_Guid | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |    81.298 ns |  1.7699 ns |  4.2406 ns |    82.072 ns |
|                  Decode_Data | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |   848.201 ns | 16.0536 ns | 16.4859 ns |   842.457 ns |
|                  Decode_Guid | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |    75.590 ns |  2.9382 ns |  5.8678 ns |    72.440 ns |
| GetArraySizeRequiredToEncode | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |     4.066 ns |  0.0138 ns |  0.0129 ns |     4.066 ns |
| GetArraySizeRequiredToDecode | Scalar | COMPlus_EnableAVX2=0,COMPlus_EnableSSSE3=0 |     7.739 ns |  0.4746 ns |  1.0317 ns |     7.163 ns |
