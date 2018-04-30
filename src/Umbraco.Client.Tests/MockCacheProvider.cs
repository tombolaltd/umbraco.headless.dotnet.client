using System;
using Umbraco.Caching;

namespace Umbraco.Client.Tests
{
    internal class MockCacheProvider : ICacheProvider
    {
        public event EventHandler<CacheMissEventArgs> OnCacheMiss;

        public bool TryGet<T>(object key, out T item)
        {
            throw new NotImplementedException();
        }

        public void Add<T>(object key, T value)
        {
            throw new NotImplementedException();
        }

        public void Add<T>(string key, T value, TimeSpan slidingExpiration)
        {
            throw new NotImplementedException();
        }

        public void Add<T>(string key, T value, DateTime fixedExpiration)
        {
            throw new NotImplementedException();
        }

        public void Remove(object key)
        {
            throw new NotImplementedException();
        }
    }
}