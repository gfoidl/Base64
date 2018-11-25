| Azure Pipelines |  
| -- |  
| [![Build Status](https://dev.azure.com/gh-gfoidl/github-Projects/_apis/build/status/github-Projects-CI)](https://dev.azure.com/gh-gfoidl/github-Projects/_build/latest?definitionId=5) |  

# gfoidl.Base64

A library for base64 encoding / decoding, as well as base64url support.  
For .NET Core 2.1+ encoding / decoding is done with SIMD-support.

| Framework | scalar | SSE | AVX |  
| -- | -- | -- | -- |  
| .NET Core 3.0 | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: |  
| .NET Core 2.1 | :heavy_check_mark: | :heavy_check_mark: | :x: |  
| .NET Standard 2.0 | :heavy_check_mark: | :x: | :x: |  

## Note

This project is WIP, as hardware intrinsics in .NET Core are also WIP.

## Acknowledgements

The scalar version of the base64 encoding / decoding is based on [System.Buffers.Tesxt.Base64](https://github.com/dotnet/corefx/tree/9c68db7fb016c6c9ae4d0f6152798d7ab1e38a37/src/System.Memory/src/System/Buffers/Text).  

The scalar version of the base64Url encoding / decoding is based on https://github.com/aspnet/Extensions/pull/334 and https://github.com/aspnet/Extensions/pull/338.

Vectorized versions (SSE, AVX) for base64 encoding / decoding is based on https://github.com/aklomp/base64 (see also _Acknowledgements_ in that repository).

Vectorized versions (SSE, AVX) for base64Url encoding / decoding is based on https://github.com/aklomp/base64 (see _Acknowledgements_ in that repository).  
For decoding (SSE, AVX) code is based on [Vector lookup (pshufb)](http://0x80.pl/notesen/2016-01-17-sse-base64-decoding.html#vector-lookup-pshufb) by Wojciech Mula.
