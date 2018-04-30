using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Umbraco.Client.EventArgs;

namespace Umbraco.Client.Connections
{
    class TargetProvider : IDisposable
    {
        private const int PRIMARY_PING_DELAY = 10000;
        private const int SECONDARY_PING_DELAY = 20000;
        private const int PING_EXCEPTION_RETRY_DELAY = 10000;

        private readonly HttpClient httpClient;
        private readonly Target primaryTarget;
        private readonly Target secondaryTarget;
        private readonly CancellationTokenSource tokenSource;

        public TargetProvider(Target primaryTarget, HttpClient httpClient)
            : this(primaryTarget, null, httpClient)
        {
        }

        public TargetProvider(Target primaryTarget, Target secondaryTarget, HttpClient httpClient)
        {
            if (primaryTarget == null)
                throw new ArgumentNullException(nameof(primaryTarget), "You need to supply at least a primary target");
            if (httpClient == null)
                throw new ArgumentNullException(nameof(httpClient));

            this.primaryTarget = primaryTarget;
            this.secondaryTarget = secondaryTarget;
            this.httpClient = httpClient;
            this.tokenSource = new CancellationTokenSource();
        }

        public Target ActiveTarget {
            get
            {
                if (secondaryTarget == null)
                {
                    return primaryTarget;
                }
                if (!primaryTarget.Alive && !secondaryTarget.Alive)
                {
                    return primaryTarget;
                }

                return primaryTarget.Alive ? primaryTarget : secondaryTarget;
            }
        }

        public event EventHandler<UmbracoTargetPingEventArgs> OnFailedPing;
        public event EventHandler<UmbracoTargetPingEventArgs> OnSuccessPing;
        public event EventHandler<Exception> OnException;

        public void Begin()
        {
            // http://blog.stephencleary.com/2013/11/taskrun-etiquette-examples-using.html
            // Task.Run isn't ideal here, but Application_Start should not be blocked and cannot be decorated with async
            // The only other alternative is to .Wait() which will block.
            var pingTask = Task.Run(() =>
            {
                try
                {
                    BeginPingOnTarget(primaryTarget, PRIMARY_PING_DELAY, tokenSource.Token).ConfigureAwait(false);

                    if (secondaryTarget != null)
                    {
                        BeginPingOnTarget(secondaryTarget, SECONDARY_PING_DELAY, tokenSource.Token).ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    OnException?.Invoke(this, ex);
                }
            }, tokenSource.Token);
            pingTask.ConfigureAwait(false);
        }

        public void Dispose()
        {
            tokenSource.Cancel();
        }

        private async Task BeginPingOnTarget(Target target, int delay, CancellationToken token)
        {
            while(!token.IsCancellationRequested)
            {
                try
                {
                    var res = await httpClient.GetAsync(target.Ping, token);

                    if (res.StatusCode == HttpStatusCode.OK)
                    {
                        target.SetAliveStatus(true);
                        OnSuccessPing?.Invoke(this, new UmbracoTargetPingEventArgs(target, res));
                    }
                    else
                    {
                        target.SetAliveStatus(false);
                        OnFailedPing?.Invoke(this, new UmbracoTargetPingEventArgs(target, res));
                    }

                    await Task.Delay(delay, token);
                }
                catch(ArgumentNullException nullEx)
                {
                    OnException?.Invoke(this, nullEx);
                    await Task.Delay(PING_EXCEPTION_RETRY_DELAY, token);
                }
                catch(Exception)
                {
                    OnFailedPing?.Invoke(this, new UmbracoTargetPingEventArgs(target, null));
                    await Task.Delay(PING_EXCEPTION_RETRY_DELAY, token);
                }
            }   
        }
    }
}
