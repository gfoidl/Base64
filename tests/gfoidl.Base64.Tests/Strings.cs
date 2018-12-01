using System.Globalization;
using NUnit.Framework;

namespace gfoidl.Base64.Tests
{
    [TestFixture]
    public class Strings
    {
        [Test]
        public void Just_to_get_100_in_coverage()
        {
            Assert.Multiple(() =>
            {
                var sut = new gfoidl.Base64.Strings();
                Assert.IsNotNull(sut);

                gfoidl.Base64.Strings.Culture = CultureInfo.InvariantCulture;

                var actual = gfoidl.Base64.Strings.Culture;
                Assert.AreSame(CultureInfo.InvariantCulture, actual);
            });
        }
    }
}
