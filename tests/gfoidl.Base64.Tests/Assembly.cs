using System;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace gfoidl.Base64.Tests
{
    [TestFixture]
    public class Assembly
    {
        [Test]
        public void StrongName_is_correct()
        {
            Type type           = typeof(Base64);
            string assemblyName = type.Assembly.FullName;

            Match match = Regex.Match(assemblyName, @"^gfoidl\.Base64, Version=[^,]*, Culture=neutral, PublicKeyToken=(.*)$");

            Assert.Multiple(() =>
            {
                Assert.IsTrue(match.Success);

                string publicKeyToken = match.Groups[1].Value;
                Assert.AreEqual("6a1c26790d4ba8ae", publicKeyToken);
            });
        }
    }
}
