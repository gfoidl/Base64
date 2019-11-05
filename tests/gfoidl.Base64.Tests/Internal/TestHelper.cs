using System;
using System.Collections.Generic;
using System.Linq;

namespace gfoidl.Base64.Tests.Internal
{
    public static class TestHelper
    {
        public static IEnumerable<byte> GetInvalidBytes(ReadOnlySpan<sbyte> map)
        {
            var indicesOfInvalidBytes = FindAllIndicesOf<sbyte>(map.ToArray(), -1);

            // Won't work, due to
            // InvalidCastException: Unable to cast object of type 'System.Int32' to type 'System.Byte'
            //return indicesOfInvalidBytes
            //    .Cast<byte>()
            //    .ToArray();

            return indicesOfInvalidBytes.Select(i => (byte)i);
        }
        //---------------------------------------------------------------------
        public static IEnumerable<int> FindAllIndicesOf<T>(this IEnumerable<T> source, T value)
            where T : IEquatable<T>
            => source
                .Select((item, index) => item.Equals(value) ? index : -1)
                .Where(index => index != -1);
    }
}
