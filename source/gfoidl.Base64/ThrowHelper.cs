using System;
using System.Buffers;
using System.Diagnostics;
using System.Resources;

namespace gfoidl.Base64
{
    internal static class ThrowHelper
    {
        private static readonly Lazy<ResourceManager> s_resources;
        //---------------------------------------------------------------------
        static ThrowHelper()
        {
            string ns = typeof(ThrowHelper).Namespace;
            s_resources = new Lazy<ResourceManager>(() => new ResourceManager($"{ns}.Strings", typeof(ThrowHelper).Assembly));
        }
        //---------------------------------------------------------------------
        public static void ThrowArgumentOutOfRangeException(ExceptionArgument argument) => throw GetArgumentOutOfRangeException(argument);
        public static void ThrowMalformedInputException(int urlEncodedLen)              => throw GetMalformdedInputException(urlEncodedLen);
        public static void ThrowForOperationNotDone(OperationStatus status)             => throw GetExceptionForOperationNotDone(status);
        //---------------------------------------------------------------------
        public static void ThrowArgumentOutOfRangeException(ExceptionArgument argument, ExceptionRessource ressource)
        {
            throw GetArgumentOutOfRangeException(argument, ressource);
        }
        //---------------------------------------------------------------------
        private static Exception GetArgumentOutOfRangeException(ExceptionArgument argument)
        {
            return new ArgumentOutOfRangeException(GetArgumentName(argument));
        }
        //---------------------------------------------------------------------
        private static Exception GetArgumentOutOfRangeException(ExceptionArgument argument, ExceptionRessource ressource)
        {
            return new ArgumentOutOfRangeException(GetArgumentName(argument), GetResource(ressource));
        }
        //---------------------------------------------------------------------
        private static FormatException GetMalformdedInputException(int urlEncodedLen)
        {
            return new FormatException(string.Format(GetResource(ExceptionRessource.MalformedInput), urlEncodedLen));
        }
        //---------------------------------------------------------------------
        private static Exception GetExceptionForOperationNotDone(OperationStatus status)
        {
            switch (status)
            {
                case OperationStatus.DestinationTooSmall:
                    //return new InvalidOperationException(Strings.DestinationTooSmall);
                    return new InvalidOperationException("should not be here");
                case OperationStatus.InvalidData:
                    return new FormatException(GetResource(ExceptionRessource.InvalidInput));
                default:
                    throw new NotSupportedException();
            }
        }
        //---------------------------------------------------------------------
        private static string GetArgumentName(ExceptionArgument argument)
        {
            Debug.Assert(Enum.IsDefined(typeof(ExceptionArgument), argument),
                $"The enum value is not defined, please check the {nameof(ExceptionArgument)} enum.");

            return argument.ToString();
        }
        //---------------------------------------------------------------------
        private static string GetResource(ExceptionRessource ressource)
        {
            Debug.Assert(Enum.IsDefined(typeof(ExceptionRessource), ressource),
                $"The enum value is not defined, please check the {nameof(ExceptionRessource)} enum.");

            return s_resources.Value.GetString(ressource.ToString());
        }
    }
    //-------------------------------------------------------------------------
    internal enum ExceptionArgument
    {
        length,
        encodedLength
    }
    //---------------------------------------------------------------------
    internal enum ExceptionRessource
    {
        EncodedLengthOutOfRange,
        InvalidInput,
        MalformedInput
    }
}
