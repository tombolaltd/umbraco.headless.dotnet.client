using System.Net.Http;
using Umbraco.Client.Connections;

namespace Umbraco.Client.EventArgs
{
    /// <summary>
    /// Represents an event payload of an umbraco ping attempt
    /// </summary>
    public class UmbracoTargetPingEventArgs
    {
        /// <summary>
        /// Represents an event payload of an umbraco ping attempt
        /// </summary>
        /// <param name="target"></param>
        /// <param name="response"></param>
        public UmbracoTargetPingEventArgs(Target target, HttpResponseMessage response)
        {
            Target = target;
            Response = response;
        }

        /// <summary>
        /// The target which is associated with the UmbracoConnection
        /// </summary>
        public Target Target { get; }


        /// <summary>
        /// The ping response message
        /// </summary>
        public HttpResponseMessage Response { get; }
    }
}
