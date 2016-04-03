using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Caching;

namespace Rafy.Utils.Caching
{
    /// <summary>
    /// ObjectCache的一般实现
    /// </summary>
    public abstract class NormalProvider : ObjectCache
    {
        #region Simple Implementation

        public override object AddOrGetExisting(string key, object value, CacheItemPolicy policy, string regionName = null)
        {
            var item = GetCacheItem(key, regionName);

            if (item == null)
            {
                Set(new CacheItem(key, value, regionName), policy);
                return value;
            }

            return item.Value;
        }

        public override CacheItem AddOrGetExisting(CacheItem value, CacheItemPolicy policy)
        {
            var item = GetCacheItem(value.Key, value.RegionName);

            if (item == null)
            {
                Set(value, policy);
                item = value;
            }

            return item;
        }

        public override object AddOrGetExisting(string key, object value, DateTimeOffset absoluteExpiration, string regionName = null)
        {
            var item = GetCacheItem(key, regionName);

            if (item == null)
            {
                item = new CacheItem(key, value, regionName);
                var policy = new CacheItemPolicy()
                {
                    AbsoluteExpiration = absoluteExpiration
                };
                Set(item, policy);
            }

            return item.Value;
        }

        public override bool Contains(string key, string regionName = null)
        {
            var value = GetCacheItem(key, regionName);
            return value != null;
        }

        public override object Get(string key, string regionName = null)
        {
            var item = GetCacheItem(key, regionName);
            if (item != null)
            {
                return item.Value;
            }
            return null;
        }

        public override void Set(string key, object value, CacheItemPolicy policy, string regionName = null)
        {
            if (key == null) throw new ArgumentNullException("key");
            if (value == null) throw new ArgumentNullException("value");

            var item = new CacheItem(key, value, regionName);

            this.Set(item, policy);
        }

        public override void Set(string key, object value, DateTimeOffset absoluteExpiration, string regionName = null)
        {
            if (key == null) throw new ArgumentNullException("key");
            if (value == null) throw new ArgumentNullException("value");

            var item = new CacheItem(key, value, regionName);
            var policy = new CacheItemPolicy();
            policy.AbsoluteExpiration = absoluteExpiration;

            this.Set(item, policy);
        }

        public override object this[string key]
        {
            get
            {
                return Get(key);
            }
            set
            {
                Set(key, value, null);
            }
        }

        public override long GetCount(string regionName = null)
        {
            return GetValues(regionName).Count;
        }

        public override string Name
        {
            get
            {
                return this.GetType().Name;
            }
        }

        #endregion
    }
}
