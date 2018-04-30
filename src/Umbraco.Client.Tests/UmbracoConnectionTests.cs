using System;
using System.Net;
using System.Net.Http;
using Moq;
using NUnit.Framework;
using RichardSzalay.MockHttp;
using Shouldly;
using Umbraco.Caching;
using Umbraco.Client.Connections;

namespace Umbraco.Client.Tests
{
    [TestFixture]
    public class UmbracoConnectionTests
    {
        private MockHttpMessageHandler mockHttpMessageHandler;

        [SetUp]
        public void TestSetup()
        {
            mockHttpMessageHandler = new MockHttpMessageHandler();
        }

        private UmbracoConnection GetUmbracoConnection()
        {
            var options = new UmbracoConnectionSettings();
            var httpClient = new HttpClient(mockHttpMessageHandler);
            return new UmbracoConnection(new Uri("http://my-umbraco"), options, httpClient, Mock.Of<ICacheProvider>());
        }

        private UmbracoConnection GetUmbracoConnectionUsingTargets(string primary, string secondary)
        {
            var p = new Target(primary);
            var s = new Target(secondary);
            var options = new UmbracoConnectionSettings();
            var httpClient = new HttpClient(mockHttpMessageHandler);
            return new UmbracoConnection(p, s, options, httpClient, Mock.Of<ICacheProvider>());
        }

        [Test]
        public void connection_initialisation_should_not_throw_an_exception()
        {
            mockHttpMessageHandler.When("*").Respond(HttpStatusCode.InternalServerError);
            var connection = GetUmbracoConnection();

            Should.NotThrow(() => connection.Initialize());
        }

        [Test]
        public void asking_for_a_new_request_returns_a_new_request_object()
        {
            var connection = GetUmbracoConnection();

            var req1 = connection.NewRequest();
            var req2 = connection.NewRequest();

            req1.ShouldNotBeSameAs(req2);
        }

    }
}