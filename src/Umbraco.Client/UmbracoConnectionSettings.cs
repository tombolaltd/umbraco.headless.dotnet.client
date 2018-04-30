using System;

namespace Umbraco.Client
{
    public class UmbracoConnectionSettings
    {
        public TimeSpan CacheSettingsRetryInterval { get; } = TimeSpan.FromSeconds(10);
        public TimeSpan GlobalRequestTimeout { get; } = TimeSpan.FromSeconds(10);
    }
}