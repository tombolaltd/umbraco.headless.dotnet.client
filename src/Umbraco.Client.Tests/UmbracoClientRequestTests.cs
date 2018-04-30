using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using RichardSzalay.MockHttp;
using Shouldly;
using System.Reflection;
using System.Linq;
using Umbraco.Caching;

namespace Umbraco.Client.Tests
{
    [TestFixture]
    public class UmbracoClientRequestTests
    {
        private Mock<ICacheProvider> mockCache;
        private MockHttpMessageHandler mockHttpMessageHandler;
        private UmbracoRequestSettings NoRetrySettings => new UmbracoRequestSettings { NumberOfRetries = 0 };

        private UmbracoClientRequest GetNewRequest()
        {
            return new UmbracoClientRequest(new Uri("http://my-umbraco/api/"), mockCache.Object, new HttpClient(mockHttpMessageHandler));
        }

        [SetUp]
        public void TestSetup()
        {
            mockCache = new Mock<ICacheProvider>();
            mockHttpMessageHandler = new MockHttpMessageHandler();
        }

        [Test]
        public void requests_look_in_the_cache_first()
        {
            // arrange
            RawContentResponse cacheOut = new RawContentResponse { Name = "Content name", RenderedContent = "<p>some content html</p>" };
            int contentId = It.IsAny<int>();
            var req = GetNewRequest();
            mockCache.Setup(c => c.TryGet(It.IsAny<string>(), out cacheOut)).Returns(true).Verifiable();

            // act
            var htmlString = req.GetPublishedContent(contentId);

            // assert
            mockCache.Verify(c => c.TryGet(It.IsAny<string>(), out cacheOut), Times.Once);
            mockHttpMessageHandler.VerifyNoOutstandingRequest();

            htmlString.ShouldNotBeNull();
            htmlString.ToString().ShouldBe(cacheOut.RenderedContent);
        }

        [Test]
        public async void raw_content_response_should_be_serializable()
        {
            var mockServerResponse = new
            {
                Id = 12,
                Name = "Page with meta tags",
                RenderedContent = "<p>some page information</p>",
                PropertyCollection = new List<KeyValuePair<string, string>> {
                    new KeyValuePair<string, string>("SEOTitle", "SEO PageTitle"),
                    new KeyValuePair<string, string>("SEODescription", "Testing the seo tab and tags"),
                    new KeyValuePair<string, string>("SEOKeywords", "seo, tabs,apis"),
                    new KeyValuePair<string, string>("SEORobots", "noindex"),
                    new KeyValuePair<string, string>("SEOMetaTags", "[\n  {\n    \"key\": \"revisit-after\",\n    \"value\": \"30 Days\"\n  },\n  {\n    \"key\": \"author\",\n    \"value\": \"umbraco\"\n  }\n]"),
                }
            };

            var response = JsonConvert.SerializeObject(mockServerResponse);
            mockHttpMessageHandler.When("*").Respond(HttpStatusCode.OK, "application/json", response);

            var rawobject = await GetNewRequest().GetPublishedContentIncludingMetadataAsync(It.IsAny<int>());
            Assert.IsTrue(rawobject.GetType().IsSerializable);

            foreach (var property in rawobject.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                Assert.IsTrue(property.GetType().IsSerializable);
            }
        }

        [Test]
        public async void content_with_no_visibility_restrictions_should_return_true_for_both_loggedin_and_loggedout()
        {
            var mockServerResponse = new
            {
                Id = 12,
                Name = "Page with no visibility property",
                RenderedContent = "<p>some page information</p>",
                PropertyCollection = new List<KeyValuePair<string, string>> {
                    new KeyValuePair<string, string>("SEOTitle", "SEO PageTitle"),
                    new KeyValuePair<string, string>("SEODescription", "Testing the seo tab and tags"),
                    new KeyValuePair<string, string>("SEOKeywords", "seo, tabs,apis"),
                    new KeyValuePair<string, string>("SEORobots", "noindex"),
                    new KeyValuePair<string, string>("SEOMetaTags", "[\n  {\n    \"key\": \"revisit-after\",\n    \"value\": \"30 Days\"\n  },\n  {\n    \"key\": \"author\",\n    \"value\": \"umbraco\"\n  }\n]"),
                }
            };

            var response = JsonConvert.SerializeObject(mockServerResponse);
            mockHttpMessageHandler.When("*").Respond(HttpStatusCode.OK, "application/json", response);
            var req = GetNewRequest();
            var res = await req.GetPublishedContentIncludingMetadataAsync(It.IsAny<int>());
            Assert.IsTrue(res.VisibleLoggedIn && res.VisibleLoggedOut);

        }

       
        
        [Test]
        public void never_cache_empty_content()
        {
            int contentId = It.IsAny<int>();
            mockCache.Setup(c => c.Add(It.IsAny<string>(), It.IsAny<object>())).Verifiable();
            mockHttpMessageHandler.When("*").Respond(HttpStatusCode.OK, "application/json", JsonConvert.SerializeObject(new { name = "Content name", renderedContent = "" }));
            var req = GetNewRequest();

            req.GetContentRegardlessOfPublishedStatus(contentId);

            mockCache.Verify(c => c.Add(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        }

        [Test]
        public void if_the_request_isnt_cached_it_will_go_to_the_server()
        {
            // arrange
            RawContentResponse cacheOut = new RawContentResponse { RenderedContent = "<p>some content html</p>" };
            string goalContent = "<p>some content html</p>";
            object response = new { renderedContent = HttpUtility.HtmlEncode(goalContent) };
            int contentId = It.IsAny<int>();
            var req = GetNewRequest();
            mockCache.Setup(c => c.TryGet(It.IsAny<string>(), out cacheOut)).Returns(false);
            mockHttpMessageHandler
                .When("*")
                .Respond(HttpStatusCode.OK, "application/json", JsonConvert.SerializeObject(response));

            // act
            var htmlString = req.GetContentRegardlessOfPublishedStatus(contentId);

            // assert
            htmlString.ShouldNotBeNull();
            htmlString.ToString().ShouldBe(goalContent);
        }

        [Test]
        public void optionally_set_a_flag_on_the_request_to_bypass_the_cache()
        {
            // arrange
            object cacheOut;
            mockHttpMessageHandler
                .When("*")
                .Respond(HttpStatusCode.OK, "application/json", JsonConvert.SerializeObject(new { renderedContent = "<p>some content html</p>" }));
            var req = GetNewRequest();

            // act
            var result = req.GetContentRegardlessOfPublishedStatus(It.IsAny<int>());

            // assert
            mockCache.Verify(c => c.TryGet(It.IsAny<string>(), out cacheOut), Times.Never);
            result.ShouldNotBeNull();
        }

        [Test]
        public void if_a_content_request_fails_it_should_not_throw_an_exception()
        {
            mockHttpMessageHandler.When("*").Respond(HttpStatusCode.InternalServerError);
            var req = GetNewRequest().WithSettings(NoRetrySettings);

            Should.NotThrow(() => req.GetContentRegardlessOfPublishedStatus(It.IsAny<int>()));
        }

        [Test]
        public void a_404_response_is_still_valid_and_should_be_returned_without_retries()
        {
            var eventRaised = false;
            mockHttpMessageHandler
                .When("*")
                .Respond(HttpStatusCode.NotFound);
            var req = GetNewRequest();
            req.OnRequestRetry += (sender, args) => { eventRaised = true; };

            var res = req.GetContentRegardlessOfPublishedStatus(1);

            eventRaised.ShouldBeFalse();
            res.ToString().ShouldBeEmpty();
        }

        [Test]
        public void no_available_cache_should_not_blow_up_the_request()
        {
            mockHttpMessageHandler
                .When("*")
                .Respond(HttpStatusCode.OK, "application/json", JsonConvert.SerializeObject(new { renderedContent = "<p>some content html</p>" }));
            var request = new UmbracoClientRequest(new Uri("http://my-umbraco/api/"), null, new HttpClient(mockHttpMessageHandler));

            Should.NotThrow(() => request.GetContentRegardlessOfPublishedStatus(It.IsAny<int>()));
        }

        [Test]
        public void no_available_cache_should_go_to_the_server_for_content()
        {
            mockHttpMessageHandler
                .When("*")
                .Respond(HttpStatusCode.OK, "application/json", JsonConvert.SerializeObject(new { renderedContent = "<p>some content html</p>" }));
            var request = new UmbracoClientRequest(new Uri("http://my-umbraco/api/"), null, new HttpClient(mockHttpMessageHandler));

            var result = request.GetContentRegardlessOfPublishedStatus(It.IsAny<int>());

            result.ShouldNotBeNull();
            result.ToString().ShouldBe("<p>some content html</p>");
        }

        [Test]
        public async Task a_request_for_content_with_page_meta_data_should_return_more_than_just_the_content_markup()
        {
            mockHttpMessageHandler
                .When("*")
                .Respond(HttpStatusCode.OK, "application/json", JsonConvert.SerializeObject(new { name = "Content name", renderedContent = "<p>some content html</p>" }));
            var request = GetNewRequest();

            var result = await request.GetPublishedContentIncludingMetadataAsync(It.IsAny<int>(), true);

            result.ShouldNotBeNull();
            result.RenderedContentHtmlString.ToString().ShouldBe("<p>some content html</p>");
            result.Name.ShouldBe("Content name");
        }

        [Test]
        public void when_bypass_cache_is_true_we_shouldnt_add_the_returning_result_to_the_cache_either()
        {
            mockHttpMessageHandler
                .When("*")
                .Respond(HttpStatusCode.OK, "application/json", JsonConvert.SerializeObject(new { name = "Content name", renderedContent = "<p>some content html</p>" }));
            mockCache.Setup(c => c.Add(It.IsAny<string>(), It.IsAny<object>())).Verifiable();
            var request = GetNewRequest();

            request.GetContentRegardlessOfPublishedStatus(It.IsAny<int>());

            mockCache.Verify(c => c.Add(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        }

        [Test]
        public async Task GetPublishedDescendantsOfFolder_should_return_descendant_content()
        {
            RawContentResponse cacheOut = MockRawContentResponse1;
            var mockHttpResponse = new
            {
                origin = It.IsAny<int>(),
                descendants = new[] { MockRawContentResponse1.Id },
                publishedOnly = true,
                descendantCount = 1
            };
            mockHttpMessageHandler
                .When("*")
                .Respond(HttpStatusCode.OK, "application/json", JsonConvert.SerializeObject(mockHttpResponse));
            mockCache.Object.Add(MockRawContentResponse1.Id.ToString(), MockRawContentResponse1);
            mockCache.Setup(c => c.TryGet(It.IsAny<string>(), out cacheOut)).Returns(true);
            var request = GetNewRequest();

            IDictionary<string, RawContentResponse> response = await request.GetPublishedDescendantsOfFolderAsync(It.IsAny<int>());

            response.ShouldNotBeNull();
            response.Count.ShouldBe(1);
            response.ShouldContainKey("Content response 1");
            response["Content response 1"].ShouldBeOfType<RawContentResponse>();
        }

        [Test]
        public async Task GetPublishedDescendantsOfFolder_should_return_descendant_content_byURL()
        {
            RawContentResponse cacheOut = MockRawContentResponse1;
            var mockHttpResponse = new
            {
                origin = It.IsAny<int>(),
                descendants = new[] { MockRawContentResponse1.Id },
                publishedOnly = true,
                descendantCount = 1
            };
            mockHttpMessageHandler
                .When("*")
                .Respond(HttpStatusCode.OK, "application/json", JsonConvert.SerializeObject(mockHttpResponse));
            mockCache.Object.Add(MockRawContentResponse1.Id.ToString(), MockRawContentResponse1);
            mockCache.Setup(c => c.TryGet(It.IsAny<string>(), out cacheOut)).Returns(true);
            var request = GetNewRequest();

            IDictionary<string, RawContentResponse> response = await request.GetPublishedDescendantsOfFolderAsync("testURL");

            response.ShouldNotBeNull();
            response.Count.ShouldBe(1);
            response.ShouldContainKey("Content response 1");
            response["Content response 1"].ShouldBeOfType<RawContentResponse>();
        }

        [Test]
        public async Task GetPublishedDescendantsOfFolder_should_return_an_empty_dictionary_if_no_published_descendants_are_found()
        {
            var mockHttpResponse = new
            {
                origin = It.IsAny<int>(),
                descendants = new int[0],
                publishedOnly = true,
                descendantCount = 0
            };
            mockHttpMessageHandler
                .When("*")
                .Respond(HttpStatusCode.OK, "application/json", JsonConvert.SerializeObject(mockHttpResponse));
            var request = GetNewRequest();

            IDictionary<string, RawContentResponse> response = await request.GetPublishedDescendantsOfFolderAsync(It.IsAny<int>(), bypassCache: true);

            response.ShouldNotBeNull();
            response.ShouldBeEmpty();
        }

        [Test]
        public async Task the_request_for_descendant_ids_should_be_cached()
        {
            object cacheOut;
            var mockHttpResponse = new
            {
                origin = It.IsAny<int>(),
                descendants = new int[] { },
                publishedOnly = true,
                descendantCount = 0
            };
            mockHttpMessageHandler
                .When("*")
                .Respond(HttpStatusCode.OK, "application/json", JsonConvert.SerializeObject(mockHttpResponse));
            var request = GetNewRequest();

            await request.GetPublishedDescendantsOfFolderAsync(It.IsAny<int>());

            mockCache.Verify(c => c.TryGet(It.IsAny<string>(), out cacheOut), Times.Exactly(1));
        }

        [Test]
        public async Task GetPublishedDescendantsOfFolder_will_check_the_cache_for_each_id_first()
        {
            RawContentResponse cacheOut;
            var mockHttpResponse = new
            {
                origin = It.IsAny<int>(),
                descendants = new[] { MockRawContentResponse1.Id, MockRawContentResponse2.Id },
                publishedOnly = true,
                descendantCount = 2
            };
            mockHttpMessageHandler
                .When("*")
                .Respond(HttpStatusCode.OK, "application/json", JsonConvert.SerializeObject(mockHttpResponse));
            var request = GetNewRequest();

            IDictionary<string, RawContentResponse> response = await request.GetPublishedDescendantsOfFolderAsync(It.IsAny<int>(), bypassCache: false);

            mockCache.Verify(c => c.TryGet(It.IsAny<string>(), out cacheOut), Times.Exactly(2));
        }

        [Test]
        public async Task GetPublishedContentOfTreePicker_should_return_descendant_content()
        {
            RawContentResponse cacheOut = MockRawContentResponse1;
            var mockHttpResponse = new
            {
                origin = It.IsAny<int>(),
                descendants = new[] { MockRawContentResponse1.Id },
                publishedOnly = true,
                descendantCount = 1
            };
            mockHttpMessageHandler
                .When("*")
                .Respond(HttpStatusCode.OK, "application/json", JsonConvert.SerializeObject(mockHttpResponse));
            mockCache.Object.Add(MockRawContentResponse1.Id.ToString(), MockRawContentResponse1);
            mockCache.Setup(c => c.TryGet(It.IsAny<string>(), out cacheOut)).Returns(true);
            var request = GetNewRequest();

            IDictionary<string, RawContentResponse> response = await request.GetPublishedContentOfTreePickerAsync(It.IsAny<int>(), It.IsAny<string>());

            response.ShouldNotBeNull();
            response.Count.ShouldBe(1);
            response.ShouldContainKey("Content response 1");
            response["Content response 1"].ShouldBeOfType<RawContentResponse>();
        }

        [Test]
        public async Task GetPublishedContentOfTreePicker_should_return_an_empty_dictionary_if_no_published_descendants_are_found()
        {
            var mockHttpResponse = new
            {
                origin = It.IsAny<int>(),
                descendants = new int[0],
                publishedOnly = true,
                descendantCount = 0
            };
            mockHttpMessageHandler
                .When("*")
                .Respond(HttpStatusCode.OK, "application/json", JsonConvert.SerializeObject(mockHttpResponse));
            var request = GetNewRequest();

            IDictionary<string, RawContentResponse> response = await request.GetPublishedContentOfTreePickerAsync(It.IsAny<int>(), It.IsAny<string>(), bypassCache: true);

            response.ShouldNotBeNull();
            response.ShouldBeEmpty();
        }

        [Test]
        public async Task GetPublishedContentOfTreePicker_will_check_the_cache_for_each_id_first()
        {
            RawContentResponse cacheOut;
            var mockHttpResponse = new
            {
                origin = It.IsAny<int>(),
                descendants = new[] { MockRawContentResponse1.Id, MockRawContentResponse2.Id },
                publishedOnly = true,
                descendantCount = 2
            };
            mockHttpMessageHandler
                .When("*")
                .Respond(HttpStatusCode.OK, "application/json", JsonConvert.SerializeObject(mockHttpResponse));
            var request = GetNewRequest();

            IDictionary<string, RawContentResponse> response = await request.GetPublishedContentOfTreePickerAsync(It.IsAny<int>(), It.IsAny<string>(), bypassCache: false);

            mockCache.Verify(c => c.TryGet(It.IsAny<string>(), out cacheOut), Times.Exactly(2));
        }

        [Test]
        public async Task the_request_for_Content_Of_Tree_Picker_ids_should_be_cached()
        {
            object cacheOut;
            var mockHttpResponse = new
            {
                origin = It.IsAny<int>(),
                descendants = new int[] { },
                publishedOnly = true,
                descendantCount = 0
            };
            mockHttpMessageHandler
                .When("*")
                .Respond(HttpStatusCode.OK, "application/json", JsonConvert.SerializeObject(mockHttpResponse));
            var request = GetNewRequest();

            await request.GetPublishedContentOfTreePickerAsync(It.IsAny<int>(), It.IsAny<string>());

            mockCache.Verify(c => c.TryGet(It.IsAny<string>(), out cacheOut), Times.Once);
        }

        [Test]
        public async Task query_request_should_map_a_custom_query_to_a_response_type_T()
        {
            // arrange
            var mockHttpResponse = new
            {
                id = 1,
                name = "Content name",
                renderedContent = "<p>some content html</p>",
                properties = new
                {
                    pageTitle = "title",
                    pageContent = "page content"
                }
            };
            mockHttpMessageHandler
                .When("*")
                .Respond(HttpStatusCode.OK, "application/json", JsonConvert.SerializeObject(mockHttpResponse));
            var request = GetNewRequest();

            // act
            MockCustomQueryResponse response = await request.QueryAsync(new MockUmbracoQueryA(1));

            // assert
            response.Id.ShouldBe(1);
            response.Name.ShouldBe("Content name");
            response.Rest.ShouldNotBeNull();
        }

        [Test]
        public async Task query_request_should_check_the_cache_first()
        {
            // arrange
            MockCustomQueryResponse cacheOut;
            var request = GetNewRequest();

            // act
            MockCustomQueryResponse response = await request.QueryAsync(new MockUmbracoQueryA(1));

            // assert
            mockCache.Verify(c => c.TryGet(It.IsAny<string>(), out cacheOut), Times.Exactly(1));
        }

        [Test]
        public async Task query_request_with_the_cache_bypass_set_to_true_should_not_attempt_to_get_or_save_response_in_cache()
        {
            // arrange
            MockCustomQueryResponse cacheOut;
            var mockHttpResponse = new
            {
                id = 1,
                name = "Content name",
                renderedContent = "<p>some content html</p>",
                properties = new
                {
                    pageTitle = "title",
                    pageContent = "page content"
                }
            };
            mockHttpMessageHandler
                .When("*")
                .Respond(HttpStatusCode.OK, "application/json", JsonConvert.SerializeObject(mockHttpResponse));
            var request = GetNewRequest();

            // act
            MockCustomQueryResponse response = await request.QueryAsync(new MockUmbracoQueryA(1), bypassCache: true);

            // assert
            mockCache.Verify(c => c.TryGet(It.IsAny<string>(), out cacheOut), Times.Never);
            mockCache.Verify(c => c.Add(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        }

        [Test]
        public async Task query_returning_meta_properties_has_meta_string_propulated()
        {
            var mockServerResponse = new
            {
                Id = 12,
                Name = "Page with meta tags",
                RenderedContent = "<p>some page information</p>",
                PropertyCollection = new List<KeyValuePair<string, string>> {
                    new KeyValuePair<string, string>("SEOTitle", "SEO PageTitle"),
                    new KeyValuePair<string, string>("SEODescription", "Testing the seo tab and tags"),
                    new KeyValuePair<string, string>("SEOKeywords", "seo, tabs,apis"),
                    new KeyValuePair<string, string>("SEORobots", "noindex"),
                    new KeyValuePair<string, string>("SEOMetaTags", "[\n  {\n    \"key\": \"revisit-after\",\n    \"value\": \"30 Days\"\n  },\n  {\n    \"key\": \"author\",\n    \"value\": \"umbraco\"\n  }\n]"),
                }
            };
            var responseBody = JsonConvert.SerializeObject(mockServerResponse);
            mockHttpMessageHandler
                .When("*").Respond(HttpStatusCode.OK, "application/json", responseBody);

            var req = GetNewRequest();
            var result = await req.GetPublishedContentIncludingMetadataAsync(It.IsAny<int>());

            result.MetaTagCollection.Count.ShouldBeGreaterThanOrEqualTo(1);
            result.RenderedMetaTagsHtmlString.ToHtmlString().Length.ShouldNotBe(0);
        }


        [Test]
        public async Task query_returning_no_meta_properties_has_page_name_as_title()
        {
            var mockServerResponse = new
            {
                Id = 12,
                Name = "Page with meta tags",
                RenderedContent = "<p>some page information</p>",
                PropertyCollection = new List<KeyValuePair<string, string>>()
            };
            var responseBody = JsonConvert.SerializeObject(mockServerResponse);
            mockHttpMessageHandler
                .When("*").Respond(HttpStatusCode.OK, "application/json", responseBody);

            var req = GetNewRequest();
            var result = await req.GetPublishedContentIncludingMetadataAsync(It.IsAny<int>());

            result.MetaTagCollection.Count.ShouldBeGreaterThanOrEqualTo(1);
            result.MetaTagCollection.Where(x => x.Key == "title").SingleOrDefault().Value.ShouldBe(mockServerResponse.Name);
            result.RenderedMetaTagsHtmlString.ToHtmlString().Length.ShouldBeGreaterThan(1);
        }

        #region Mock content responses

        private RawContentResponse MockRawContentResponse1 => new RawContentResponse
        {
            Id = 1001,
            Name = "Content response 1",
            RenderedContent = "<p>content response 1</p>"
        };
        private RawContentResponse MockRawContentResponse2 => new RawContentResponse
        {
            Id = 1002,
            Name = "Content response 2",
            RenderedContent = "<p>content response 2</p>"
        };

        #endregion

        [Serializable]
        class MockCustomQueryResponse
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public object Rest { get; set; }
        }

        class MockUmbracoQueryA : IUmbracoCustomQuery<MockCustomQueryResponse>
        {
            public MockUmbracoQueryA(int id)
            {
                AssociatedContentId = id;
            }

            public int AssociatedContentId { get; }
            public MockCustomQueryResponse EmptyResponse => new MockCustomQueryResponse();

            public Guid UniqueQueryId { get; } = new Guid("017E9289-91CC-4D6F-956C-1E18310AF36C");

            public string RelativeRequestUrl()
            {
                return $"/test/{AssociatedContentId}";
            }

            public MockCustomQueryResponse MapResponse(string jsonResponse)
            {
                var responseInstance = JsonConvert.DeserializeObject<MockCustomQueryResponse>(jsonResponse);
                responseInstance.Rest = JsonConvert.DeserializeObject(jsonResponse);

                return responseInstance;
            }
        }
    }

}
