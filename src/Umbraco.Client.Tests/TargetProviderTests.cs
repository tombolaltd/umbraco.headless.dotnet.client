using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using RichardSzalay.MockHttp;
using Shouldly;
using Umbraco.Client.Connections;
using Umbraco.Client.EventArgs;

namespace Umbraco.Client.Tests
{
    [TestFixture]
    public class TargetProviderTests
    {
        private MockHttpMessageHandler mockHttpRequestHandler;

        [SetUp]
        public void Setup()
        {
            this.mockHttpRequestHandler = new MockHttpMessageHandler();
        }

        private TargetProvider CreateNewProvider(string primary, string secondary)
        {
            var primaryTarget = new Target(primary);
            var secondaryTarget = new Target(secondary);
            return new TargetProvider(primaryTarget, secondaryTarget, new HttpClient(mockHttpRequestHandler));
        }

        private TargetProvider CreateNewProvider(Target primary, Target secondary)
        {
            return new TargetProvider(primary, secondary, new HttpClient(mockHttpRequestHandler));
        }

        [Test]
        public void before_any_ping_attempts_ActiveTarget_should_be_primary()
        {
            var primaryTarget = new Target("http://primary");
            var secondaryTarget = new Target("http://secondary");
            var provider = CreateNewProvider(primaryTarget, secondaryTarget);

            provider.ActiveTarget.ShouldBe(primaryTarget);
            provider.ActiveTarget.Url.AbsoluteUri.ShouldBe("http://primary/");
        }

        [Test]
        public void if_the_primary_connection_becomes_unavailable_switch_to_a_secondary()
        {
            var assert = false;
            var autoEvent = new AutoResetEvent(false);

            var primaryTarget = new Target("http://primary", "/content");
            var secondaryTarget = new Target("http://secondary", "/content");
            mockHttpRequestHandler.When("http://primary/content").Respond(HttpStatusCode.InternalServerError);
            mockHttpRequestHandler.When("http://secondary/content").Respond(HttpStatusCode.OK);
            var provider = CreateNewProvider(primaryTarget, secondaryTarget);

            provider.OnFailedPing += (sender, payload) =>
            {
                if (provider.ActiveTarget == secondaryTarget)
                {
                    assert = true;
                }
                autoEvent.Set();
            };

            provider.Begin();

            autoEvent.WaitOne(2000);
            autoEvent.Dispose();
            assert.ShouldBe(true);
        }

        [Test]
        public async Task if_both_primary_and_secondary_connections_are_unavailable_choose_primary()
        {
            var primaryTarget = new Target("http://primary");
            var secondaryTarget = new Target("http://secondary");
            var provider = CreateNewProvider(primaryTarget, secondaryTarget);

            var activeTarget = provider.ActiveTarget;

            primaryTarget.Alive.ShouldBe(false);
            secondaryTarget.Alive.ShouldBe(false);
            activeTarget.ShouldBe(primaryTarget);
        }

        [Test]
        public void provider_should_log_successful_ping_requests()
        {
            var onSuccessEventCalled = false;
            var autoEvent = new AutoResetEvent(false);

            var primaryTarget = new Target("http://primary");
            var secondaryTarget = new Target("http://secondary");
            mockHttpRequestHandler.When("*").Respond(HttpStatusCode.OK);
            var provider = CreateNewProvider(primaryTarget, secondaryTarget);

            provider.OnSuccessPing += (sender, payload) =>
            {
                onSuccessEventCalled = true;
                autoEvent.Set();
            };

            provider.Begin();

            autoEvent.WaitOne(2000);
            autoEvent.Dispose();
            onSuccessEventCalled.ShouldBe(true);
        }

        [Test]
        public void provider_should_log_each_failed_ping_request()
        {
            var onFailedEventCalled = false;
            var autoEvent = new AutoResetEvent(false);

            var primaryTarget = new Target("http://primary");
            var secondaryTarget = new Target("http://secondary");
            mockHttpRequestHandler.When("*").Respond(HttpStatusCode.InternalServerError);
            var provider = CreateNewProvider(primaryTarget, secondaryTarget);

            provider.OnFailedPing += (sender, payload) =>
            {
                onFailedEventCalled = true;
                autoEvent.Set();
            };

            provider.Begin();

            autoEvent.WaitOne(2000);
            autoEvent.Dispose();
            onFailedEventCalled.ShouldBe(true);
        }

        [Test]
        public void exceptions_which_occur_in_connection_checks_should_be_logged_and_swallowed()
        {
            var onExceptionEventFired = false;
            var primaryTarget = new Target("http://primary");
            var secondaryTarget = new Target("http://secondary");
            var provider = CreateNewProvider(primaryTarget, secondaryTarget);

            provider.OnException += (sender, ex) =>
            {
                onExceptionEventFired = true;
            };

            Assert.DoesNotThrow(() => provider.Begin());
            onExceptionEventFired.ShouldBe(false);
        }

        [Test]
        public void once_provider_dispose_is_called_all_ping_attempts_should_sease()
        {
            var eventFired = false;
            var primaryTarget = new Target("http://primary");
            var secondaryTarget = new Target("http://secondary");
            var provider = CreateNewProvider(primaryTarget, secondaryTarget);

            EventHandler<UmbracoTargetPingEventArgs> pingEventFiredFn = (sender, payload) =>
            {
                eventFired = true;
            };
            EventHandler<Exception> exceptionEventFiredFn = (sender, ex) =>
            {
                eventFired = true;
            };

            provider.Begin();
            provider.Dispose();

            provider.OnFailedPing += pingEventFiredFn;
            provider.OnSuccessPing += pingEventFiredFn;
            provider.OnException += exceptionEventFiredFn;

            eventFired.ShouldBe(false);
        }

        [Test]
        public void provider_should_make_ping_requests_using_the_ping_endpoint()
        {
            var assert = false;
            var autoEvent = new AutoResetEvent(false);
            var primaryTarget = new Target("http://primary", "/ping");
            var secondaryTarget = new Target("http://secondary");
            mockHttpRequestHandler.When("http://primary/ping").Respond(HttpStatusCode.OK);
            var provider = CreateNewProvider(primaryTarget, secondaryTarget);

            provider.OnSuccessPing += (sender, payload) =>
            {
                if(payload.Target.Ping.OriginalString == "http://primary/ping")
                {
                    assert = true;
                }
                autoEvent.Set();
            };

            provider.Begin();

            autoEvent.WaitOne(2000);
            autoEvent.Dispose();
            assert.ShouldBe(true);
        }

        [Test]
        public void if_no_ping_endpoint_is_provided_it_should_use_the_base_url_plus_content()
        {
            var assert = false;
            var autoEvent = new AutoResetEvent(false);
            var primaryTarget = new Target("http://primary");
            var secondaryTarget = new Target("http://secondary");
            mockHttpRequestHandler.When("http://primary/content").Respond(HttpStatusCode.OK);
            var provider = CreateNewProvider(primaryTarget, secondaryTarget);

            provider.OnSuccessPing += (sender, payload) =>
            {
                if (payload.Target.Ping.OriginalString == "http://primary/content")
                {
                    assert = true;
                }
                autoEvent.Set();
            };

            provider.Begin();

            autoEvent.WaitOne(2000);
            autoEvent.Dispose();
            assert.ShouldBe(true);
        }
    }
}
