using System;
using System.Buffers;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Threading.Tasks;

namespace gfoidl.Base64.Demo
{
    class Program
    {
        static async Task Main()
        {
            Func<Task>[] demos = { RunGuidEncoding, RunGuidDecoding, RunBufferChainEncoding, RunDetectEncoding, RunSequenceEncoding };

            foreach (Func<Task> demo in demos)
            {
                Console.Write($"{demo.Method.Name}...");
                await demo();
                Console.WriteLine("done");
            }
        }
        //---------------------------------------------------------------------
        private static Task RunGuidEncoding()
        {
            byte[] guid = Guid.NewGuid().ToByteArray();

            string guidBase64     = Base64.Default.Encode(guid);
            string guidBases64Url = Base64.Url.Encode(guid);

            int guidBase64EncodedLength = Base64.Default.GetEncodedLength(guid.Length);
            Span<byte> guidBase64UTF8   = stackalloc byte[guidBase64EncodedLength];
            OperationStatus status      = Base64.Default.Encode(guid, guidBase64UTF8, out int consumed, out int written);
            Debug.Assert(status   == OperationStatus.Done);
            Debug.Assert(consumed == guid.Length);
            Debug.Assert(written  == guidBase64UTF8.Length);

            int guidBase64UrlEncodedLength = Base64.Url.GetEncodedLength(guid.Length);
            Span<byte> guidBase64UrlUTF8   = stackalloc byte[guidBase64UrlEncodedLength];
            status                         = Base64.Url.Encode(guid, guidBase64UrlUTF8, out consumed, out written);
            Debug.Assert(status   == OperationStatus.Done);
            Debug.Assert(consumed == guid.Length);
            Debug.Assert(written  == guidBase64UrlUTF8.Length);

            return Task.CompletedTask;
        }
        //---------------------------------------------------------------------
        private static Task RunGuidDecoding()
        {
            Guid guid = Guid.NewGuid();

            string guidBase64    = Convert.ToBase64String(guid.ToByteArray());
            string guidBase64Url = guidBase64.Replace('+', '-').Replace('/', '_').TrimEnd('=');

            byte[] guidBase64Decoded    = Base64.Default.Decode(guidBase64);
            byte[] guidBase64UrlDecoded = Base64.Url.Decode(guidBase64Url);

            Debug.Assert(guid == new Guid(guidBase64Decoded));
            Debug.Assert(guid == new Guid(guidBase64UrlDecoded));

            int guidBase64DecodedLen    = Base64.Default.GetDecodedLength(guidBase64);
            int guidBase64UrlDecodedLen = Base64.Url.GetDecodedLength(guidBase64Url);

            Span<byte> guidBase64DecodedBuffer    = stackalloc byte[guidBase64DecodedLen];
            Span<byte> guidBase64UrlDecodedBuffer = stackalloc byte[guidBase64UrlDecodedLen];

            OperationStatus status = Base64.Default.Decode(guidBase64, guidBase64DecodedBuffer, out int consumed, out int written);
            Debug.Assert(status   == OperationStatus.Done);
            Debug.Assert(consumed == guidBase64.Length);
            Debug.Assert(written  == guidBase64DecodedBuffer.Length);

            status = Base64.Url.Decode(guidBase64Url, guidBase64UrlDecodedBuffer, out consumed, out written);
            Debug.Assert(status   == OperationStatus.Done);
            Debug.Assert(consumed == guidBase64Url.Length);
            Debug.Assert(written  == guidBase64UrlDecodedBuffer.Length);

            return Task.CompletedTask;
        }
        //---------------------------------------------------------------------
        private static Task RunBufferChainEncoding()
        {
            var rnd         = new Random();
            Span<byte> data = new byte[1000];
            rnd.NextBytes(data);

            // exact length could be computed by Base64.Default.GetEncodedLength, here for demo exzessive size
            Span<char> base64 = new char[5000];

            OperationStatus status = Base64.Default.Encode(data.Slice(0, 400), base64, out int consumed, out int written, isFinalBlock: false);
            Debug.Assert(status == OperationStatus.NeedMoreData);   // 400 is not a multiple of 3, so more data is needed
            status                 = Base64.Default.Encode(data.Slice(consumed), base64.Slice(written), out consumed, out int written1, isFinalBlock: true);
            Debug.Assert(status == OperationStatus.Done);

            base64 = base64.Slice(0, written + written1);

            Span<byte> decoded = new byte[5000];
            status             = Base64.Default.Decode(base64.Slice(0, 100), decoded, out consumed, out written, isFinalBlock: false);
            Debug.Assert(status == OperationStatus.Done);           // 100 encoded bytes can be decoded to 75 bytes, so Done
            status             = Base64.Default.Decode(base64.Slice(consumed), decoded.Slice(written), out consumed, out written1, isFinalBlock: true);
            Debug.Assert(status == OperationStatus.Done);

            decoded = decoded.Slice(0, written + written1);
            Debug.Assert(data.SequenceEqual(decoded));

            return Task.CompletedTask;
        }
        //---------------------------------------------------------------------
        private static Task RunDetectEncoding()
        {
            // Let's assume we don't know whether this string is base64 or base64Url
            string encodedString = "a-_9";

            Span<byte> data = stackalloc byte[Base64.Default.GetMaxDecodedLength(encodedString.Length)];

            EncodingType encodingType = Base64.DetectEncoding(encodedString);

            int written = 0;
            switch (encodingType)
            {
                case EncodingType.Base64:
                    Base64.Default.Decode(encodedString.AsSpan(), data, out int _, out written);
                    break;
                case EncodingType.Base64Url:
                    Base64.Url.Decode(encodedString.AsSpan(), data, out int _, out written);
                    break;
                case EncodingType.Unknown:
                    throw new InvalidOperationException("should not be here");
            }

            data = data.Slice(0, written);

            Debug.Assert(data.Length == 3);

            return Task.CompletedTask;
        }
        //---------------------------------------------------------------------
        private static async Task RunSequenceEncoding()
        {
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

            Debug.Assert(consumed == data.Length);
            Debug.Assert(written  == (data.Length + 2) / 3 * 4);
        }
    }
}
