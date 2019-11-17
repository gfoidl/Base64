#pragma warning disable GF0001 // Cross assembly pubternal reference

using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using gfoidl.Base64.Internal;
using SharpFuzz;

// see https://github.com/Metalnem/sharpfuzz-samples

namespace gfoidl.Base64.FuzzTests
{
    class Program
    {
        static void Main(string[] args)
        {
            // To verify a crash switch to Debug in the configuration manager and uncomment the following lines
            //var stream = File.OpenRead(@".\findings\crashes\id%3A000001,sig%3A02,src%3A000000,op%3Ahavoc,rep%3A64");
            //Base64_Url_Decode(stream);
            //return;

            if (args.Length < 1)
            {
                Console.WriteLine("Fuzzing method must be given");
                Environment.Exit(1);
            }

            switch (args[0])
            {
                case "Base64_Default_Decode": Fuzzer.Run(Base64_Default_Decode); break;
                case "Base64_Url_Decode"    : Fuzzer.Run(Base64_Url_Decode)    ; break;
                default:
                    Console.WriteLine($"Unknown fuzzing function: {args[0]}");
                    Environment.Exit(2);
                    throw null;
            }

        }
        //---------------------------------------------------------------------
        private static void Base64_Default_Decode(Stream stream) => Base64_Decode(stream, Base64.Default);
        private static void Base64_Url_Decode    (Stream stream) => Base64_Decode(stream, Base64.Url);
        //---------------------------------------------------------------------
        private static void Base64_Decode(Stream stream, Base64 encoder)
        {
            PipeReader pipeReader = PipeReader.Create(stream, new StreamPipeReaderOptions(leaveOpen: true));

            while (true)
            {
                ReadResult readResult = pipeReader.ReadAsync().GetAwaiter().GetResult();

                if (readResult.IsCompleted || readResult.IsCanceled)
                    break;

                ReadOnlySequence<byte> buffer = readResult.Buffer;

                if (buffer.Length > int.MaxValue)
                    return;

                if (buffer.IsSingleSegment)
                {
                    Base64_Decode(buffer.FirstSpan, encoder);
                }
                else
                {
                    byte[] arrayToReturnToPool = ArrayPool<byte>.Shared.Rent((int)buffer.Length);
                    try
                    {
                        buffer.CopyTo(arrayToReturnToPool);
                        Base64_Decode(arrayToReturnToPool.AsSpan(0, (int)buffer.Length), encoder);
                    }
                    finally
                    {
                        ArrayPool<byte>.Shared.Return(arrayToReturnToPool);
                    }
                }

                pipeReader.AdvanceTo(buffer.End);
            }

            pipeReader.Complete();
        }
        //---------------------------------------------------------------------
        private static void Base64_Decode(ReadOnlySpan<byte> encoded, Base64 encoder)
        {
            byte[]? dataArrayFromPool = null;
            try
            {
                dataArrayFromPool = ArrayPool<byte>.Shared.Rent(encoder.GetMaxDecodedLength(encoded.Length));

                OperationStatus status = encoder.Decode(encoded, dataArrayFromPool, out int consumed, out int written);

                if (ContainsInvalidData(encoded, encoder) && status != OperationStatus.InvalidData)
                    throw new Exception("contains invalid data -- not detected");

                if (status == OperationStatus.Done)
                {
                    if (consumed != encoded.Length)
                        throw new Exception("consumed != encoded.Length");
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                // exception is allowed to happen
            }
            catch (FormatException) when (encoder is Base64UrlEncoder)
            {
                // exception is allowed to happen
            }
            finally
            {
                if (dataArrayFromPool != null)
                {
                    ArrayPool<byte>.Shared.Return(dataArrayFromPool);
                }
            }
        }
        //---------------------------------------------------------------------
        private static bool ContainsInvalidData(ReadOnlySpan<byte> encoded, Base64 encoder)
        {
            ReadOnlySpan<sbyte> decodingMap = default;

            if (encoder is Base64Encoder)
            {
                decodingMap = Base64Encoder.DecodingMap;

                // Check for padding at the end
                int paddingCount = 0;

                if (encoded.Length > 1 && encoded[^1] == Base64Encoder.EncodingPad) paddingCount++;
                if (encoded.Length > 2 && encoded[^2] == Base64Encoder.EncodingPad) paddingCount++;

                encoded = encoded[0..^paddingCount];
            }
            else if (encoder is Base64UrlEncoder)
            {
                decodingMap = Base64UrlEncoder.DecodingMap;
            }

            for (int i = 0; i < encoded.Length; ++i)
            {
                byte e = encoded[i];

                if (decodingMap[e] == -1) return true;
            }

            return false;
        }
    }
}

#pragma warning restore GF0001 // Cross assembly pubternal reference
