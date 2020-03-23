using System;
using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Resources;

namespace gfoidl.Base64
{
    internal static class ThrowHelper
    {
        private static readonly Lazy<ResourceManager> s_resources;
        //---------------------------------------------------------------------
#pragma warning disable CA1810 // Initialize reference type static fields inline
        static ThrowHelper()
#pragma warning restore CA1810 // Initialize reference type static fields inline
        {
            string? ns  = typeof(ThrowHelper).Namespace;
            Debug.Assert(ns != null);
            s_resources = new Lazy<ResourceManager>(() => new ResourceManager($"{ns}.Strings", typeof(ThrowHelper).Assembly));
        }
        //---------------------------------------------------------------------
        [DoesNotReturn] public static void ThrowArgumentNullException(ExceptionArgument argument)       => throw GetArgumentNullException(argument);
        [DoesNotReturn] public static void ThrowArgumentOutOfRangeException(ExceptionArgument argument) => throw GetArgumentOutOfRangeException(argument);
        [DoesNotReturn] public static void ThrowMalformedInputException(int urlEncodedLen)              => throw GetMalformdedInputException(urlEncodedLen);
        [DoesNotReturn] public static void ThrowForOperationNotDone(OperationStatus status)             => throw GetExceptionForOperationNotDone(status);
        //---------------------------------------------------------------------
        [DoesNotReturn]
        public static void ThrowArgumentOutOfRangeException(ExceptionArgument argument, ExceptionRessource ressource)
        {
            throw GetArgumentOutOfRangeException(argument, ressource);
        }
        //---------------------------------------------------------------------
        private static Exception GetArgumentNullException(ExceptionArgument argument)
        {
            return new ArgumentNullException(GetArgumentName(argument));
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
            => status switch
            {
                OperationStatus.DestinationTooSmall => new InvalidOperationException("should not be here"), // new InvalidOperationException(Strings.DestinationTooSmall);
                OperationStatus.InvalidData         => new FormatException(GetResource(ExceptionRessource.InvalidInput)),
                _                                   => throw new NotSupportedException(),
            };
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

#if NETCOREAPP
            string? tmp = s_resources.Value.GetString(ressource.ToString());
            Debug.Assert(tmp != null);
            return tmp;
#else
            return s_resources.Value.GetString(ressource.ToString());
#endif
        }
    }
    //-------------------------------------------------------------------------
    internal enum ExceptionArgument
    {
        encoder,
        encodedLength,
        length,
        writer
    }
    //---------------------------------------------------------------------
    internal enum ExceptionRessource
    {
        EncodedLengthOutOfRange,
        InvalidInput,
        MalformedInput
    }
}
