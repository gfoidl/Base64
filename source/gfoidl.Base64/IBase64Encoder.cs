using System;
using System.Buffers;

namespace gfoidl.Base64
{
    public interface IBase64Encoder
    {
        int GetEncodedLength(int sourceLength);
        int GetDecodedLength(ReadOnlySpan<byte> encoded);
        int GetDecodedLength(ReadOnlySpan<char> encoded);

        OperationStatus Encode(ReadOnlySpan<byte> data, Span<byte> encoded, out int consumed, out int written, bool isFinalBlock = true);
        OperationStatus Encode(ReadOnlySpan<byte> data, Span<char> encoded, out int consumed, out int written, bool isFinalBlock = true);

        OperationStatus Decode(ReadOnlySpan<byte> encoded, Span<byte> data, out int consumed, out int written);
        OperationStatus Decode(ReadOnlySpan<char> encoded, Span<byte> data, out int consumed, out int written);

        string Encode(ReadOnlySpan<byte> data);
        byte[] Decode(ReadOnlySpan<char> encoded);
    }
}
