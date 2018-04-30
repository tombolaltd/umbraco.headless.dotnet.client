using System.Collections.Generic;
using System.Web;
using NUnit.Framework;
using Shouldly;

namespace Umbraco.Client.Tests
{
    [TestFixture]
    public class RawContentResponseTests
    {
        [Test]
        public void referencing_RenderedContentHtmlString_with_empty_RenderedContent_should_return_an_empty_HtmlString()
        {
            var contentResponse = new RawContentResponse
            {
                RenderedContent = string.Empty
            };

            contentResponse.RenderedContentHtmlString.ShouldNotBeNull();
            contentResponse.RenderedContentHtmlString.ShouldBeAssignableTo<HtmlString>();
            contentResponse.RenderedContentHtmlString.ToHtmlString().ShouldBe(new HtmlString(string.Empty).ToHtmlString());
        }

        [Test]
        public void RawContentResponse_Empty_returns_an_empty_RawContentResponse()
        {
            var emptyContentExplicit = RawContentResponse.Empty;

            emptyContentExplicit.RenderedContent.ShouldBe(string.Empty);
            emptyContentExplicit.Id.ShouldBe(default(int));
            emptyContentExplicit.Name.ShouldBe(string.Empty);
        }

        [Test]
        public void RawContentResponse_Empty_returns_an_empty_MetaTagCollection()
        {
            var emptyContentExplicit = RawContentResponse.Empty;

            emptyContentExplicit.MetaTagCollection.ShouldNotBe(null);
            emptyContentExplicit.RenderedMetaTags.ShouldBe(string.Empty);
        }

        [Test]
        public void RawContentResponse_Empty_returns_empty_MetaTagProperties()
        {
            var emptyContentExplicit = RawContentResponse.Empty;

            emptyContentExplicit.MetaTagPageTitle().ShouldBe(string.Empty);
            emptyContentExplicit.MetaTagPageDescription().ShouldBe(string.Empty);
        }

        [Test]
        public void RawContentResponse_WithMeta_returns_valid_MetaTagProperties()
        {
            var meta = new List<KeyValuePair<string, string>>();
            meta.Add(new KeyValuePair<string, string>("keywords", "test, this, works"));
            meta.Add(new KeyValuePair<string, string>("title", "METAPAGETITLE"));
            meta.Add(new KeyValuePair<string, string>("pagetitle", "FAKE"));
            meta.Add(new KeyValuePair<string, string>("description", "METAPAGEDESC"));
            meta.Add(new KeyValuePair<string, string>("pagedesc", "FAKE"));

            var testContentResponse = RawContentResponse.Empty;
            testContentResponse.MetaTagCollection = meta;

            testContentResponse.MetaTagPageTitle().ShouldBe("METAPAGETITLE");
            testContentResponse.MetaTagPageDescription().ShouldBe("METAPAGEDESC");
        }
    }
}
