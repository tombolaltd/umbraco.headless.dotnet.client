using NUnit.Framework;
using Shouldly;
using Umbraco.Client.Connections;

namespace Umbraco.Client.Tests.Connections
{
    [TestFixture]
    public class TargetTests
    {
        [Test]
        [TestCase("http://test/api/", "ping")]
        [TestCase("http://test/api/", "/ping")]
        [TestCase("http://test/api", "ping")]
        [TestCase("http://test/api", "/ping")]
        public void regardless_or_trailing_or_prepending_slashes_ping_url_should_be_valid(string url, string ping)
        {
            var expected = "http://test/api/ping";
            var t = new Target(url, ping);

            t.Ping.AbsoluteUri.ShouldBe(expected);
        }

        [Test(Description = "With our current setup; /content can be used as a default ping endpoint")]
        public void when_no_ping_argument_is_supplied_add_a_default()
        {
            var url = "http://test/api";
            var expected = "http://test/api/content";
            var t = new Target(url);

            t.Ping.AbsoluteUri.ShouldBe(expected);
        }
    }
}
