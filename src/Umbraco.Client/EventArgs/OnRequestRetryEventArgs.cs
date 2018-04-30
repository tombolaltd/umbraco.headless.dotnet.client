using System;
using System.Net;
using System.Net.Http;

namespace Umbraco.Client.EventArgs
{
    /// <summary>
    /// Event argument payload which is used upon request failure
    /// </summary>
    public class OnRequestFailureEventArgs : System.EventArgs
    {
        /// <summary>
        /// The failed response message
        /// </summary>
        public HttpResponseMessage ResponseMessage { get; set; }

        /// <summary>
        /// The failed request status code
        /// </summary>
        public HttpStatusCode HttpStatusCode => ResponseMessage.StatusCode;

        /// <summary>
        /// An exception attached to the failed response
        /// </summary>
        public Exception RequestException { get; set; }
    }
}
