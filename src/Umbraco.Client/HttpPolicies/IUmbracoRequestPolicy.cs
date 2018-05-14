using System;

namespace Umbraco.Client.HttpPolicies
{
    internal interface IUmbracoRequestPolicy
    {
        TimeSpan Timeout { get; }
        int NumberOfRetries { get; }
        TimeSpan RetryInterval { get; }
        int RetryIntervalModifier { get; }
        int[] HttpStatusCodesWorthRetrying { get; }
    }
}