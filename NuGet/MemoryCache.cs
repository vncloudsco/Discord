namespace NuGet
{
    using System;
    using System.Collections.Concurrent;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal sealed class MemoryCache : IDisposable
    {
        private static readonly Lazy<MemoryCache> _instance = new Lazy<MemoryCache>(() => new MemoryCache());
        private static readonly TimeSpan _cleanupInterval = TimeSpan.FromSeconds(10.0);
        private readonly ConcurrentDictionary<object, CacheItem> _cache = new ConcurrentDictionary<object, CacheItem>();
        private readonly Timer _timer;

        internal MemoryCache()
        {
            this._timer = new Timer(new TimerCallback(this.RemoveExpiredEntries), null, _cleanupInterval, _cleanupInterval);
        }

        public void Dispose()
        {
            if (this._timer != null)
            {
                this._timer.Dispose();
            }
        }

        internal T GetOrAdd<T>(object cacheKey, Func<T> factory, TimeSpan expiration, bool absoluteExpiration = false) where T: class
        {
            CacheItem item = new CacheItem(factory, expiration, absoluteExpiration);
            CacheItem orAdd = this._cache.GetOrAdd(cacheKey, item);
            orAdd.UpdateUsage(expiration);
            return (T) orAdd.Value;
        }

        internal void Remove(object cacheKey)
        {
            CacheItem item;
            this._cache.TryRemove(cacheKey, out item);
        }

        private void RemoveExpiredEntries(object state)
        {
            foreach (object obj2 in this._cache.Keys)
            {
                CacheItem item;
                if (this._cache.TryGetValue(obj2, out item) && item.Expired)
                {
                    this._cache.TryRemove(obj2, out item);
                }
            }
        }

        internal bool TryGetValue<T>(object cacheKey, out T value) where T: class
        {
            CacheItem item;
            if (this._cache.TryGetValue(cacheKey, out item))
            {
                value = (T) item.Value;
                return true;
            }
            value = default(T);
            return false;
        }

        internal static MemoryCache Instance =>
            _instance.Value;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MemoryCache.<>c <>9 = new MemoryCache.<>c();

            internal MemoryCache <.cctor>b__13_0() => 
                new MemoryCache();
        }

        private sealed class CacheItem
        {
            private readonly Lazy<object> _valueFactory;
            private readonly bool _absoluteExpiration;
            private long _expires;

            public CacheItem(Func<object> valueFactory, TimeSpan expires, bool absoluteExpiration)
            {
                this._valueFactory = new Lazy<object>(valueFactory);
                this._absoluteExpiration = absoluteExpiration;
                this._expires = DateTime.UtcNow.Ticks + expires.Ticks;
            }

            public void UpdateUsage(TimeSpan slidingExpiration)
            {
                if (!this._absoluteExpiration)
                {
                    this._expires = DateTime.UtcNow.Ticks + slidingExpiration.Ticks;
                }
            }

            public object Value =>
                this._valueFactory.Value;

            public bool Expired =>
                (DateTime.UtcNow.Ticks > Interlocked.Read(ref this._expires));
        }
    }
}

