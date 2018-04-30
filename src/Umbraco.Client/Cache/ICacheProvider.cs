using System;

namespace Umbraco.Caching
{
    public interface ICacheProvider
    {
        bool TryGet<T>(object key, out T item);
        void Add<T>(object key, T value);
        void Add<T>(string key, T value, TimeSpan slidingExpiration);
        void Add<T>(string key, T value, DateTime fixedExpiration);
        void Remove(object key);
        event EventHandler<CacheMissEventArgs> OnCacheMiss;
    }
}
