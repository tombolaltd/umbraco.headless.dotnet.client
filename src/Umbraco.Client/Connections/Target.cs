using System;

namespace Umbraco.Client.Connections
{
    /// <summary>
    /// A Target represents an umbraco api endpoint
    /// </summary>
    public class Target
    {
        private const string DEFAULT_PING_PATH = "content";

        /// <summary>
        /// A Target represents an umbraco api endpoint
        /// </summary>
        /// <param name="url">An umbraco api endpoint</param>
        public Target(string url, string ping = null)
            : this(new Uri(url.TrimEnd('/') + "/"), ping)
        {
        }

        /// <summary>
        /// A Target represents an umbraco api endpoint
        /// </summary>
        /// <param name="url">An umbraco api endpoint</param>
        public Target(Uri url, string ping = null)
        {
            if (url == null)
                throw new ArgumentNullException(nameof(url));

            Url = url;

            if (string.IsNullOrEmpty(ping))
            {
                Ping = new Uri(url, DEFAULT_PING_PATH);
            }
            else
            {
                Ping = new Uri(url, ping.TrimStart('/'));
            }
        }

        /// <summary>
        /// The URI object associated with the target
        /// </summary>
        public Uri Url { get; }

        /// <summary>
        /// The endpoint in which is used as a target healthcheck. This might be different from the base url.
        /// </summary>
        public Uri Ping { get; }

        /// <summary>
        /// A boolean flag which determines if the target is alive and active or not.
        /// </summary>
        public bool Alive { get; private set; }

        /// <summary>
        /// Allows the setting of the 'Alive' property.
        /// </summary>
        /// <param name="alive"></param>
        public void SetAliveStatus(bool alive)
        {
            Alive = alive;
        }
    }
}
