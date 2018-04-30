using System;
using Umbraco.Client.HttpPolicies;

namespace Umbraco.Client
{
    /// <summary>
    /// Various settings used by an umbraco request
    /// </summary>
    public class UmbracoRequestSettings : ITombolaUmbracoRequestPolicy
    {
        /// <summary>
        /// Dictates how long before an umbraco request will timeout.
        /// Default is 4 seconds
        /// </summary>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(4);

        /// <summary>
        /// Dictates how many retries a request should make, before failing completely.
        /// Default is 3
        /// </summary>
        public int NumberOfRetries { get; set; } = 3;

        /// <summary>
        /// How long should the request wait after failing, before trying again.
        /// Default is 500 milliseconds
        /// </summary>
        public TimeSpan RetryInterval { get; set; } = TimeSpan.FromMilliseconds(500);

        /// <summary>
        /// A value to modify the retry interval by.
        /// With each failed request, the retry interval will be multiplied by this value.
        /// Default is 2
        /// </summary>
        public int RetryIntervalModifier { get; set; } = 2;

        /// <summary>
        /// An array of status codes in which, if encountered; will eligible for retry
        /// </summary>
        public int[] HttpStatusCodesWorthRetrying { get; set; } = {408, 500, 502, 503, 504};
    }
}