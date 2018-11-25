| Azure Pipelines |
| -- |
| [![Build Status](https://dev.azure.com/gh-gfoidl/github-Projects/_apis/build/status/github-Projects-CI)](https://dev.azure.com/gh-gfoidl/github-Projects/_build/latest?definitionId=5) |

## Note

This project is WIP, as hardware intrinsics in .NET Core are also WIP.

# gfoidl.Base64

A .NET library for base64 encoding / decoding, as well as base64Url support.
Encoding can be done to buffers of type `byte` (for UTF-8) or `char`.
Decoding can read from buffers of type `byte` (for UTF-8) or `char`.

Encoding / decoding supports buffer-chains, for example for very large data or when the data arrives in chunks.

In .NET Core 2.1+ encoding / decoding is done with SIMD-support:

| Framework | scalar | SSE | AVX |
| -- | -- | -- | -- |
| .NET Core 3.0 | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: |
| .NET Core 2.1 | :heavy_check_mark: | :heavy_check_mark: | :x: |
| .NET Standard 2.0 | :heavy_check_mark: | :x: | :x: |

If available AVX will "eat" up as much as possible, then SSE will "eat" up as much as possible,
finally scalar code processes the rest (including padding).

## Usage

Basically the entry to encoder / decoder is `Base64.Default` for _base64_, and `Base64.Url` for _base64Url_.

### Encoding

```c#
byte[] guid = Guid.NewGuid().ToByteArray();

string guidBase64 = Base64.Default.Encode(guid);
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

See [demo](./demo/gfoidl.Base64.Demo/Program.cs) for further examples.

## Acknowledgements

The scalar version of the base64 encoding / decoding is based on [System.Buffers.Tesxt.Base64](https://github.com/dotnet/corefx/tree/9c68db7fb016c6c9ae4d0f6152798d7ab1e38a37/src/System.Memory/src/System/Buffers/Text).

The scalar version of the base64Url encoding / decoding is based on https://github.com/aspnet/Extensions/pull/334 and https://github.com/aspnet/Extensions/pull/338.

Vectorized versions (SSE, AVX) for base64 encoding / decoding is based on https://github.com/aklomp/base64 (see also _Acknowledgements_ in that repository).

Vectorized versions (SSE, AVX) for base64Url encoding / decoding is based on https://github.com/aklomp/base64 (see _Acknowledgements_ in that repository).
For decoding (SSE, AVX) code is based on [Vector lookup (pshufb)](http://0x80.pl/notesen/2016-01-17-sse-base64-decoding.html#vector-lookup-pshufb) by Wojciech Mula.
