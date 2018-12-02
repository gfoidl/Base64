using System;
using System.Linq;
using NUnit.Framework;

namespace gfoidl.Base64.Tests.Base64Tests
{
    [TestFixture(typeof(byte))]
    [TestFixture(typeof(char))]
    public class DetectEncoding<T> where T : IEquatable<T>
    {
        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void EncodedSpan_length_lt_4___Unknown(int encodedLength)
        {
            T[] encoded = new T[encodedLength];

            EncodingType actual = Base64.DetectEncoding<T>(encoded);

            Assert.AreEqual(EncodingType.Unknown, actual);
        }
        //---------------------------------------------------------------------
        [Test]
        public void No_special_characters___Base64()
        {
            T a;

            if (typeof(T) == typeof(byte))
            {
                a = (T)(object)(byte)'a';
            }
            else if (typeof(T) == typeof(char))
            {
                a = (T)(object)'a';
            }
            else
            {
                throw new NotSupportedException(); // just in case new types are introduced in the future
            }

            T[] encoded = { a, a, a, a };

            EncodingType actual = Base64.DetectEncoding<T>(encoded);

            Assert.AreEqual(EncodingType.Base64, actual);
        }
        //---------------------------------------------------------------------
        [Test]
        public void Plus___Base64()
        {
            T a;
            T plus;

            if (typeof(T) == typeof(byte))
            {
                a    = (T)(object)(byte)'a';
                plus = (T)(object)(byte)'+';
            }
            else if (typeof(T) == typeof(char))
            {
                a    = (T)(object)'a';
                plus = (T)(object)'+';
            }
            else
            {
                throw new NotSupportedException(); // just in case new types are introduced in the future
            }

            T[] encoded = { a, a, plus, a };

            EncodingType actual = Base64.DetectEncoding<T>(encoded);

            Assert.AreEqual(EncodingType.Base64, actual);
        }
        //---------------------------------------------------------------------
        [Test]
        public void Slash___Base64()
        {
            T a;
            T slash;

            if (typeof(T) == typeof(byte))
            {
                a     = (T)(object)(byte)'a';
                slash = (T)(object)(byte)'/';
            }
            else if (typeof(T) == typeof(char))
            {
                a     = (T)(object)'a';
                slash = (T)(object)'/';
            }
            else
            {
                throw new NotSupportedException(); // just in case new types are introduced in the future
            }

            T[] encoded = { a, a, slash, a };

            EncodingType actual = Base64.DetectEncoding<T>(encoded);

            Assert.AreEqual(EncodingType.Base64, actual);
        }
        //---------------------------------------------------------------------
        [Test]
        public void No_plus_and_no_slash_but_padding___Base64()
        {
            T a;
            T padding;

            if (typeof(T) == typeof(byte))
            {
                a       = (T)(object)(byte)'a';
                padding = (T)(object)(byte)'=';
            }
            else if (typeof(T) == typeof(char))
            {
                a       = (T)(object)'a';
                padding = (T)(object)'=';
            }
            else
            {
                throw new NotSupportedException(); // just in case new types are introduced in the future
            }

            T[] encoded = { a, a, padding, a };

            EncodingType actual = Base64.DetectEncoding<T>(encoded);

            Assert.AreEqual(EncodingType.Base64, actual);
        }
        //---------------------------------------------------------------------
        [Test]
        public void Minus___Base64Url()
        {
            T a;
            T minus;

            if (typeof(T) == typeof(byte))
            {
                a     = (T)(object)(byte)'a';
                minus = (T)(object)(byte)'-';
            }
            else if (typeof(T) == typeof(char))
            {
                a     = (T)(object)'a';
                minus = (T)(object)'-';
            }
            else
            {
                throw new NotSupportedException(); // just in case new types are introduced in the future
            }

            T[] encoded = { a, a, minus, a };

            EncodingType actual = Base64.DetectEncoding<T>(encoded);

            Assert.AreEqual(EncodingType.Base64Url, actual);
        }
        //---------------------------------------------------------------------
        [Test]
        public void Underscore___Base64Url()
        {
            T a;
            T underscore;

            if (typeof(T) == typeof(byte))
            {
                a          = (T)(object)(byte)'a';
                underscore = (T)(object)(byte)'_';
            }
            else if (typeof(T) == typeof(char))
            {
                a          = (T)(object)'a';
                underscore = (T)(object)'_';
            }
            else
            {
                throw new NotSupportedException(); // just in case new types are introduced in the future
            }

            T[] encoded = { a, a, underscore, a };

            EncodingType actual = Base64.DetectEncoding<T>(encoded);

            Assert.AreEqual(EncodingType.Base64Url, actual);
        }
        //---------------------------------------------------------------------
        [Test]
        public void Mix___Unknown([Values('+', '/')] char arg1, [Values('-', '_')] char arg2)
        {
            T a;
            T a1;
            T a2;

            if (typeof(T) == typeof(byte))
            {
                a = (T)(object)(byte)'a';
                a1 = (T)(object)(byte)arg1;
                a2 = (T)(object)(byte)arg2;
            }
            else if (typeof(T) == typeof(char))
            {
                a = (T)(object)'a';
                a1 = (T)(object)arg1;
                a2 = (T)(object)arg2;
            }
            else
            {
                throw new NotSupportedException(); // just in case new types are introduced in the future
            }

            T[] encoded = { a1, a, a2, a };

            EncodingType actual = Base64.DetectEncoding<T>(encoded);

            Assert.AreEqual(EncodingType.Unknown, actual);
        }
        //---------------------------------------------------------------------
        [Test]
        public void Mix_and_fast___Base64Url([Values('+', '/')] char arg1, [Values('-', '_')] char arg2)
        {
            T a;
            T a1;
            T a2;

            if (typeof(T) == typeof(byte))
            {
                a  = (T)(object)(byte)'a';
                a1 = (T)(object)(byte)arg1;
                a2 = (T)(object)(byte)arg2;
            }
            else if (typeof(T) == typeof(char))
            {
                a  = (T)(object)'a';
                a1 = (T)(object)arg1;
                a2 = (T)(object)arg2;
            }
            else
            {
                throw new NotSupportedException(); // just in case new types are introduced in the future
            }

            T[] encoded = { a1, a, a2, a };

            EncodingType actual = Base64.DetectEncoding<T>(encoded, fast: true);

            Assert.AreEqual(EncodingType.Base64Url, actual);
        }
    }
}
