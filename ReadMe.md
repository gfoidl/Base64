| Azure Pipelines | Code Coverage | NuGet |
| -- | -- | -- |
| [![Build Status](https://dev.azure.com/gh-gfoidl/github-Projects/_apis/build/status/github-Projects-CI)](https://dev.azure.com/gh-gfoidl/github-Projects/_build/latest?definitionId=5)| [![codecov](https://codecov.io/gh/gfoidl/Base64/branch/master/graph/badge.svg)](https://codecov.io/gh/gfoidl/Base64) | [![NuGet](https://img.shields.io/nuget/v/gfoidl.Base64.svg?style=flat-square)](https://www.nuget.org/packages/gfoidl.Base64/) |

# gfoidl.Base64

A .NET library for base64 encoding / decoding, as well as base64Url support.
Encoding can be done to buffers of type `byte` (for UTF-8) or `char`.
Decoding can read from buffers of type `byte` (for UTF-8) or `char`.

Encoding / decoding supports buffer-chains, for example for very large data or when the data arrives in chunks.

In .NET Core 3.0 onwards encoding / decoding is done with SIMD-support:

| Framework | scalar | SSSE3 | AVX2 |
| -- | -- | -- | -- |
| .NET Core 3.0 | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: |
| .NET Standard 2.0 / .NET 4.5 | :heavy_check_mark: | :x: | :x: |

If available AVX will "eat" up as much as possible, then SSE will "eat" up as much as possible,
finally scalar code processes the rest (including padding).

## Usage

Basically the entry to encoder / decoder is `Base64.Default` for _base64_, and `Base64.Url` for _base64Url_.

See [demo](./demo/gfoidl.Base64.Demo/Program.cs) for further examples.

### Encoding

```c#
byte[] guid = Guid.NewGuid().ToByteArray();

string guidBase64     = Base64.Default.Encode(guid);
string guidBases64Url = Base64.Url.Encode(guid);
```

or `Span<byte>` based (for UTF-8 encoded output):

```c#
int guidBase64EncodedLength = Base64.Default.GetEncodedLength(guid.Length);
Span<byte> guidBase64UTF8   = stackalloc byte[guidBase64EncodedLength];
OperationStatus status      = Base64.Default.Encode(guid, guidBase64UTF8, out int consumed, out int written);

int guidBase64UrlEncodedLength = Base64.Url.GetEncodedLength(guid.Length);
Span<byte> guidBase64UrlUTF8   = stackalloc byte[guidBase64UrlEncodedLength];
status                         = Base64.Url.Encode(guid, guidBase64UrlUTF8, out consumed, out written);
```

### Decoding

```c#
Guid guid = Guid.NewGuid();

string guidBase64    = Convert.ToBase64String(guid.ToByteArray());
string guidBase64Url = guidBase64.Replace('+', '-').Replace('/', '_').TrimEnd('=');

byte[] guidBase64Decoded    = Base64.Default.Decode(guidBase64);
byte[] guidBase64UrlDecoded = Base64.Url.Decode(guidBase64Url);
```

or `Span<char>` based:

```c#
int guidBase64DecodedLen    = Base64.Default.GetDecodedLength(guidBase64);
int guidBase64UrlDecodedLen = Base64.Url.GetDecodedLength(guidBase64Url);

Span<byte> guidBase64DecodedBuffer    = stackalloc byte[guidBase64DecodedLen];
Span<byte> guidBase64UrlDecodedBuffer = stackalloc byte[guidBase64UrlDecodedLen];

OperationStatus status = Base64.Default.Decode(guidBase64, guidBase64DecodedBuffer, out int consumed, out int written);
status                 = Base64.Url.Decode(guidBase64Url, guidBase64UrlDecodedBuffer, out consumed, out written);
```

### Buffer chains

Buffer chains are handy when for encoding / decoding

* very large data
* data arrives is chunks, e.g. by reading from a (buffered) stream / pipeline
* the size of data is initially unknown
* ...

```c#
var rnd         = new Random();
Span<byte> data = new byte[1000];
rnd.NextBytes(data);

// exact length could be computed by Base64.Default.GetEncodedLength, here for demo exzessive size
Span<char> base64 = new char[5000];

OperationStatus status = Base64.Default.Encode(data.Slice(0, 400), base64, out int consumed, out int written, isFinalBlock: false);
status                 = Base64.Default.Encode(data.Slice(consumed), base64.Slice(written), out consumed, out int written1, isFinalBlock: true);

base64 = base64.Slice(0, written + written1);

Span<byte> decoded = new byte[5000];
status             = Base64.Default.Decode(base64.Slice(0, 100), decoded, out consumed, out written, isFinalBlock: false);
status             = Base64.Default.Decode(base64.Slice(consumed), decoded.Slice(written), out consumed, out written1, isFinalBlock: true);

decoded = decoded.Slice(0, written + written1);
```

### ReadOnlySequence / IBufferWriter

Encoding / decoding with `ReadOnlySequence<byte>` and `IBufferWriter<byte>` can be used together with `System.IO.Pipelines`.

```c#
var pipeOptions = PipeOptions.Default;
var pipe        = new Pipe(pipeOptions);

var rnd  = new Random(42);
var data = new byte[4097];
rnd.NextBytes(data);

pipe.Writer.Write(data);
await pipe.Writer.CompleteAsync();

ReadResult readResult = await pipe.Reader.ReadAsync();

var resultPipe = new Pipe();
Base64.Default.Encode(readResult.Buffer, resultPipe.Writer, out long consumed, out long written);
await resultPipe.Writer.CompleteAsync();
```

## (Functional) Comparison to classes in .NET

### General

.NET provides the classes [System.Convert](https://docs.microsoft.com/en-us/dotnet/api/system.convert) and [System.Buffers.Text.Base64](https://docs.microsoft.com/en-us/dotnet/api/system.buffers.text.base64)
for base64 operations.

base64Url isn't supported, so hacky solutions like
```c#
string base64 = Convert.ToBase64String(data);
string base64Url = base64.Replace('+', '-').Replace('/', '_').TrimEnd('=');
```
are needed. This isn't ideal, as there are avoidable allocations and several iterations over the encoded string (see [here](perf/gfoidl.Base64.Benchmarks/results/netcoreapp3.0/EncodeStringUrlBenchmark-report.md) and [here](perf/gfoidl.Base64.Benchmarks/results/netcoreapp3.0/DecodeStringUrlBenchmark-report.md) for benchmark results).

_gfoidl.Base64_ supports encoding / decoding to / from base64Url in a direct way.
Encoding `byte[] -> byte[]` for UTF-8 is supported, as well as `byte[] -> char[]`.
Decoding `byte[] -> byte[]` for UTF-8 is supported, as well as `char[] -> byte[]`.

Further SIMD isn't utilized in the .NET classes.
(Note: I've opened an [issue](https://github.com/dotnet/corefx/issues/32365) to add SIMD-support to these classes).

### Convert.ToBase64XYZ / Convert.FromBase64XYZ

These methods only support `byte[] -> char[]` as types for encoding,
and `char[] -> byte[]` as types for decoding, where `char[]` can also be `string` or `(ReadOnly)Span<char>`.

To support UTF-8 another method call like
```c#
byte[] utf8Encoded = Encoding.ASCII.GetBytes(base64String);
```
is needed.

An potential advantage of this class is that it allows the insertion of line-breaks (cf. [Base64FormattingOptions.InsertLineBreaks](https://docs.microsoft.com/en-us/dotnet/api/system.base64formattingoptions)).

### System.Buffers.Text.Base64

This class only supports `byte[] -> byte[]` for encoding / decoding. So in order to get a `string`
`Encoding` has to be used.

An potential advantage of this class is the support for in-place encoding / decoding (cf.
[Base64.EncodeToUtf8InPlace](https://docs.microsoft.com/en-us/dotnet/api/system.buffers.text.base64.encodetoutf8inplace),
[Base64.DecodeFromUtf8InPlace](https://docs.microsoft.com/en-us/dotnet/api/system.buffers.text.base64.decodefromutf8inplace)
)

## Benchmarks

For all benchmarks see [results](/perf/gfoidl.Base64.Benchmarks/results).

Performance gain depends, among a lot of other things, on the workload size, so here no table will with superior results will be shown.

[Direct encoding to a string](perf/gfoidl.Base64.Benchmarks/results/netcoreapp3.0/EncodeStringBenchmark-report.md) is for small inputs slower than `Convert.ToBase64String` (has less overhead, and can write to string-buffer in a direct way).
But the larger the workload, the better this library works. For data-length of 1000 speedup can be ~5x with AVX2 encoding.

[Direct decoding from a string](perf/gfoidl.Base64.Benchmarks/results/netcoreapp3.0/DecodeStringBenchmark-report.md) is generally (a lot) faster than `Convert.ConvertFromBase64CharArray`, also depending on workload size, but in the benchmark the speedup is from 1.5 to 12x.

For UTF-8 [encoding](perf/gfoidl.Base64.Benchmarks/results/netcoreapp3.0/EncodeUtf8Benchmark-report.md) and [decoding](perf/gfoidl.Base64.Benchmarks/results/netcoreapp3.0/DecodeUtf8Benchmark-report.md)
speedups for input-length 1000 can be in the height of 5 to 12x.

**Note:** please measure / profile in your real usecase, as this are just micro-benchmarks.


## Acknowledgements

The scalar version of the base64 encoding / decoding is based on [System.Buffers.Text.Base64](https://github.com/dotnet/corefx/tree/9c68db7fb016c6c9ae4d0f6152798d7ab1e38a37/src/System.Memory/src/System/Buffers/Text).

The scalar version of the base64Url encoding / decoding is based on https://github.com/aspnet/Extensions/pull/334 and https://github.com/aspnet/Extensions/pull/338.

Vectorized versions (SSE, AVX) for base64 encoding / decoding is based on https://github.com/aklomp/base64 (see also _Acknowledgements_ in that repository).

Vectorized versions (SSE, AVX) for base64Url encoding / decoding is based on https://github.com/aklomp/base64 (see _Acknowledgements_ in that repository).
For decoding (SSE, AVX) code is based on [Vector lookup (pshufb)](http://0x80.pl/notesen/2016-01-17-sse-base64-decoding.html#vector-lookup-pshufb) by Wojciech Mula.


## Development channel

To get packages from the development channel use a `nuget.config` similar to this one:
```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
    <packageSources>
        <add key="gfoidl-public" value="https://pkgs.dev.azure.com/gh-gfoidl/github-Projects/_packaging/gfoidl-public/nuget/v3/index.json" />
    </packageSources>
</configuration>
```
