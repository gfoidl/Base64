namespace gfoidl.Base64.Internal
{
    public sealed partial class Base64Encoder : Base64EncoderImpl
    {
        private const byte EncodingPad = (byte)'=';     // '=', for padding
    }
}
