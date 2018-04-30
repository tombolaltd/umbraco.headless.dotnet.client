using System;

namespace Umbraco.Caching.CacheValueTypes
{
    [Serializable]
    public class CustomQueryCacheValue
    {
        public string ContentId { get; set; }
        public string CustomQueryKey { get; set; }
    }
}
