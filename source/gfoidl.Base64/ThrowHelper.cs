using System;
using System.Buffers;
using System.Diagnostics;

namespace gfoidl.Base64
{
    internal static class ThrowHelper
    {
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
            return new ArgumentOutOfRangeException(GetArgumentName(argument), GetRessource(ressource));
        }
        //---------------------------------------------------------------------
        private static FormatException GetMalformdedInputException(int urlEncodedLen)
        {
            return new FormatException(string.Format(Strings.MalformedInput, urlEncodedLen));
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
                    return new FormatException(Strings.InvalidInput);
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
        private static string GetRessource(ExceptionRessource ressource)
        {
            Debug.Assert(Enum.IsDefined(typeof(ExceptionRessource), ressource),
                $"The enum value is not defined, please check the {nameof(ExceptionRessource)} enum.");

            return Strings.ResourceManager.GetString(ressource.ToString());
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
        EncodedLengthOutOfRange
    }
}
