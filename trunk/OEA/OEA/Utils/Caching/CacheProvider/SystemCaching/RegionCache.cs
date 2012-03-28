using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Caching;

namespace OEA.Utils.Caching
{
    /// <summary>
    /// 使用装饰模式，把一个不支持Region的ObjectCache变为支持的。
    /// </summary>
    public class RegionCache : ObjectCache
    {
        private static readonly string SPLITTER = "_:_";

        private ObjectCache _innerCache;

        public RegionCache(ObjectCache innerCache)
        {
            if (innerCache == null) throw new ArgumentNullException("innerCache");

            this._innerCache = innerCache;
        }

        public override CacheItem GetCacheItem(string key, string regionName = null)
        {
            EnableRegionInKey(ref key, ref regionName);

            var item = this._innerCache.GetCacheItem(key, regionName);

            DisableRegionInKey(item);

            return item;
        }

        public override bool Add(CacheItem item, CacheItemPolicy policy)
        {
            EnableRegionInKey(item);

            return this._innerCache.Add(item, policy);
        }

        public override void Set(CacheItem item, CacheItemPolicy policy)
        {
            EnableRegionInKey(item);

            this._innerCache.Set(item, policy);
        }

        public override object Remove(string key, string regionName = null)
        {
            EnableRegionInKey(ref key, ref regionName);

            return this._innerCache.Remove(key, regionName);
        }

        private static void EnableRegionInKey(ref string key, ref string regionName)
        {
            if (regionName != null)
            {
                key = regionName + SPLITTER + key;
                regionName = null;
            }
        }

        private static void EnableRegionInKey(CacheItem item)
        {
            if (item.RegionName != null)
            {
                item.Key = item.RegionName + SPLITTER + item.Key;
                item.RegionName = null;
            }
        }

        private static void DisableRegionInKey(CacheItem item)
        {
            if (item != null)
            {
                var all = item.Key;
                var index = all.IndexOf(SPLITTER);
                var region = all.Substring(0, index);
                var key = all.Substring(index + SPLITTER.Length);

                item.RegionName = region;
                item.Key = key;
            }
        }

        public override object AddOrGetExisting(string key, object value, CacheItemPolicy policy, string regionName = null)
        {
            EnableRegionInKey(ref key, ref regionName);

            return this._innerCache.AddOrGetExisting(key, value, policy, regionName);
        }

        #region NotSupported

        public override CacheItem AddOrGetExisting(CacheItem value, CacheItemPolicy policy)
        {
            throw new NotImplementedException();
        }

        public override object AddOrGetExisting(string key, object value, DateTimeOffset absoluteExpiration, string regionName = null)
        {
            throw new NotImplementedException();
        }

        public override bool Contains(string key, string regionName = null)
        {
            throw new NotImplementedException();
        }

        public override CacheEntryChangeMonitor CreateCacheEntryChangeMonitor(IEnumerable<string> keys, string regionName = null)
        {
            throw new NotImplementedException();
        }

        public override DefaultCacheCapabilities DefaultCacheCapabilities
        {
            get { throw new NotImplementedException(); }
        }

        public override object Get(string key, string regionName = null)
        {
            throw new NotImplementedException();
        }

        public override long GetCount(string regionName = null)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public override IDictionary<string, object> GetValues(IEnumerable<string> keys, string regionName = null)
        {
            throw new NotImplementedException();
        }

        public override string Name
        {
            get { throw new NotImplementedException(); }
        }

        public override void Set(string key, object value, CacheItemPolicy policy, string regionName = null)
        {
            throw new NotImplementedException();
        }

        public override void Set(string key, object value, DateTimeOffset absoluteExpiration, string regionName = null)
        {
            throw new NotImplementedException();
        }

        public override object this[string key]
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        #endregion
    }
}
