using System;

namespace Umbraco.Client.HttpPolicies
{
    internal interface ITombolaUmbracoRequestPolicy
    {
        TimeSpan Timeout { get; }
        int NumberOfRetries { get; }
        TimeSpan RetryInterval { get; }
        int RetryIntervalModifier { get; }
        int[] HttpStatusCodesWorthRetrying { get; }
    }
}