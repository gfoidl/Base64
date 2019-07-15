using System;
using System.Text;
using SharpFuzz;

// see https://github.com/Metalnem/sharpfuzz-samples

namespace gfoidl.Base64.FuzzTests
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Fuzzing method must be given");
                Environment.Exit(1);
            }

            switch (args[0])
            {
                case "Base64_Default_String": Fuzzer.Run(Base64_Default_String); break;
                case "Base64_Url_String"    : Fuzzer.Run(Base64_Url_String); break;
                default:
                    Console.WriteLine($"Unknown fuzzing function: {args[0]}");
                    Environment.Exit(2);
                    throw null;
            }
        }
        //---------------------------------------------------------------------
        private static void Base64_Default_String(string input) => Base64_String(input, Base64.Default);
        private static void Base64_Url_String(string input)     => Base64_String(input, Base64.Url);
        //---------------------------------------------------------------------
        private static void Base64_String(string input, Base64 encoder)
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            string base64     = encoder.Encode(inputBytes);
            byte[] decoded    = encoder.Decode(base64);

            if (!inputBytes.AsSpan().SequenceEqual(decoded))
            {
                throw new Exception("Roundtripping failed");
            }
        }
    }
}
