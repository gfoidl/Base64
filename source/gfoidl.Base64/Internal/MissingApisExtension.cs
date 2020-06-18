#if NET45
namespace gfoidl.Base64.Internal
{
    internal static class Array
    {
        public static T[] Empty<T>() => EmptyArray<T>.s_value;
        //---------------------------------------------------------------------
        private static class EmptyArray<T>
        {
            public static readonly T[] s_value = new T[0];
        }
    }
}
#endif
//-------------------------------------------------------------------------
#if NETSTANDARD2_0 || NET45
namespace System.Diagnostics.CodeAnalysis
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    internal sealed class DoesNotReturnAttribute : Attribute { }
}
#endif
