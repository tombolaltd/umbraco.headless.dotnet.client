using System;
using System.Net.Http;
using Umbraco.Caching;
using Umbraco.Client.Connections;
using Umbraco.Client.EventArgs;

namespace Umbraco.Client
{
    /// <summary>
    /// Represents a connection to the Umbraco CMS
    /// </summary>
    public sealed class UmbracoConnection : IDisposable
    {
        private readonly HttpClient httpClient;
        private readonly TargetProvider targetProvider;
        private ICacheProvider cache;


        /// <summary>
        /// A log event which is fired whenever a failure or success in the client connection occurs.
        /// </summary>
        public event ConnectionLogEventHandler Log;

        /// <summary>
        /// An event which is fired after every endpoint ping attempt.
        /// </summary>
        public event EventHandler<UmbracoTargetPingEventArgs> OnUmbracoTargetPing;

        /// <summary>
        /// Creates a new Tombola Umbraco Client connection.
        /// </summary>
        public UmbracoConnection(string primaryUmbracoApiAddress)
            : this(new Target(primaryUmbracoApiAddress))
        {            
        }

        /// <summary>
        /// Creates a new Tombola Umbraco Client connection.
        /// </summary>
        public UmbracoConnection(Uri primaryUmbracoApiAddress)
            : this(new Target(primaryUmbracoApiAddress))
        {
        }

        /// <summary>
        /// Creates a new Tombola Umbraco Client connection. 
        /// </summary>
        public UmbracoConnection(Target target) 
            : this(target, new UmbracoConnectionSettings())
        {
        }

        /// <summary>
        /// Creates a new Tombola Umbraco Client connection. 
        /// </summary>
        public UmbracoConnection(Uri primaryUmbracoApiAddress, UmbracoConnectionSettings connectionSettings)
            : this(new Target(primaryUmbracoApiAddress), connectionSettings)
        {
        }

        /// <summary>
        /// Creates a new Tombola Umbraco Client connection. 
        /// </summary>
        public UmbracoConnection(Target primaryTarget, UmbracoConnectionSettings connectionSettings)
            : this(primaryTarget, null, connectionSettings, new HttpClient())
        {
        }

        /// <summary>
        /// Creates a new Tombola Umbraco Client connection. 
        /// </summary>
        public UmbracoConnection(Target primaryTarget, Target secondaryTarget)
            : this(primaryTarget, secondaryTarget, new UmbracoConnectionSettings())
        {
        }

        /// <summary>
        /// Creates a new Tombola Umbraco Client connection. 
        /// </summary>
        public UmbracoConnection(Target primaryTarget, Target secondaryTarget, UmbracoConnectionSettings connectionSettings)
            : this(primaryTarget, secondaryTarget, connectionSettings, new HttpClient())
        {
        }

        /// <summary>
        /// Creates a new Tombola Umbraco Client connection.
        /// </summary>
        public UmbracoConnection(Target primaryTarget, UmbracoConnectionSettings connectionSettings, ICacheProvider cache)
            : this(primaryTarget, null, connectionSettings, cache)
        {
        }

        /// <summary>
        /// Creates a new Tombola Umbraco Client connection.
        /// </summary>
        public UmbracoConnection(Target primaryTarget, Target secondaryTarget, UmbracoConnectionSettings connectionSettings, ICacheProvider cache)
            : this(primaryTarget, secondaryTarget, connectionSettings, new HttpClient(), cache)
        {
        }


        /// <summary>
        /// Creates a new Tombola Umbraco Client connection. 
        /// </summary>
        /// <exception cref="ArgumentNullException">thrown if any of the parameters are not given (are null)</exception> 
        public UmbracoConnection(Uri primaryUmbracoApiAddress, UmbracoConnectionSettings connectionSettings, HttpClient httpClient, ICacheProvider cache = null)
            : this(new Target(primaryUmbracoApiAddress), null, connectionSettings, httpClient, cache)
        {
        }

        /// <summary>
        /// Creates a new Tombola Umbraco Client connection. 
        /// </summary>
        /// <exception cref="ArgumentNullException">thrown if any of the parameters are not given (are null)</exception> 
        public UmbracoConnection(Target primaryTarget, Target secondaryTarget, UmbracoConnectionSettings connectionSettings, HttpClient httpClient, ICacheProvider cache = null)
        {
            if (primaryTarget == null)
                throw new ArgumentNullException(nameof(primaryTarget));
            if (connectionSettings == null)
                throw new ArgumentNullException(nameof(connectionSettings));
            if (httpClient == null)
                throw new ArgumentNullException(nameof(httpClient));

            // Configure HttpClient          
            httpClient.Timeout = connectionSettings.GlobalRequestTimeout;
            httpClient.DefaultRequestHeaders.Add("X-Tombola-ApiKey", "UB1eS3LZQMs9k6xXqAMMiZ1uvhhh2Gck");
            this.httpClient = httpClient;

            // Configure cache
            this.cache = cache;
            if (cache != null)
            {
                cache.OnCacheMiss += Cache_OnCacheMiss;
            }

            // Create target provider
            this.targetProvider = new TargetProvider(primaryTarget, secondaryTarget, httpClient);

            ConnectionStatus = ConnectionStatus.UnInitialized;
        }

        private void Cache_OnCacheMiss(object sender, CacheMissEventArgs e)
        {
            // let the client log these if necessary
            Log?.Invoke(this, new WarningLogEventArgs()
            {
                Message = "Cache Miss",
                Data = e
            });
        }

        /// <summary>
        /// The status the connection is currently in
        /// </summary>
        public ConnectionStatus ConnectionStatus { get; private set; }

        /// <summary>
        /// Initializes the connection with umbraco, retreiving cache settings.
        /// </summary>
        public UmbracoConnectionInitializationResult Initialize()
        {
            if (cache != null && ConnectionStatus == ConnectionStatus.Initialized)
                return new UmbracoConnectionInitializationResult(true);
            if (ConnectionStatus == ConnectionStatus.Pending)
                return new UmbracoConnectionInitializationResult(false) {Message = "UmbracoConnection status currently in a pending state"};

            ConnectionStatus = ConnectionStatus.Pending;

            try
            {
                targetProvider.OnSuccessPing += TargetProvider_OnSuccessPing;
                targetProvider.OnFailedPing += TargetProvider_OnFailurePing;
                targetProvider.OnException += TargetProvider_OnExceptionPing;
                targetProvider.Begin();

                return new UmbracoConnectionInitializationResult(true);
            }
            catch (Exception ex)
            {
                var message = "UmbracoConnection failed to initialize";

                ConnectionStatus = ConnectionStatus.UnInitialized;
                Log?.Invoke(this, new FailureLogEventArgs { Message = message, Exception = ex });
                Dispose();

                return new UmbracoConnectionInitializationResult(false)
                {
                    Message = message,
                    Exception = ex
                };
            }
        }

        /// <summary>
        /// Creates a new tombola CMS request scope
        /// </summary>
        public UmbracoClientRequest NewRequest()
        {
            return new UmbracoClientRequest(targetProvider.ActiveTarget.Url, cache, httpClient);
        }

        /// <summary>
        /// Correctly disposes the connection, disposing the internal httpClient which the connection consumes.
        /// Unsubscribes from all events.
        /// </summary>
        public void Dispose()
        {
            targetProvider.Dispose();
            httpClient.Dispose();
            targetProvider.OnSuccessPing -= TargetProvider_OnSuccessPing;
            targetProvider.OnFailedPing -= TargetProvider_OnFailurePing;
            targetProvider.OnException -= TargetProvider_OnExceptionPing;
        }

        private void TargetProvider_OnSuccessPing(object sender, UmbracoTargetPingEventArgs payload)
        {
            Log?.Invoke(sender, new SuccessLogEventArgs { Data = payload, Message = "Umbraco target ping successful" });
        }

        private void TargetProvider_OnFailurePing(object sender, UmbracoTargetPingEventArgs payload)
        {
            Log?.Invoke(sender, new WarningLogEventArgs { Data = payload, Message = "Umbraco target ping failure" });
        }

        private void TargetProvider_OnExceptionPing(object sender, Exception ex)
        {
            Log?.Invoke(sender, new FailureLogEventArgs { Exception = ex, Message = "Umbraco target provider threw an exception" });
        }

        /// <summary>
        /// A delegate which describes the signature of a connection log event handler.
        /// </summary>
        public delegate void ConnectionLogEventHandler(object sender, LogEventArgs args);
    }

    /// <summary>
    /// Represents an umbraco connection initialisation state.
    /// </summary>
    public enum ConnectionStatus
    {
        /// <summary>
        /// The connection is uninitialised.
        /// </summary>
        UnInitialized,
        /// <summary>
        /// The connection is pending.
        /// Most likely the connection is in the process of being initialised.
        /// </summary>
        Pending,
        /// <summary>
        /// The connection has been successfully initialised.
        /// </summary>
        Initialized
    }
}
