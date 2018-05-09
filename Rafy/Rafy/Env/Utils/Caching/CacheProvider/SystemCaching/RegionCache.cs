﻿/*******************************************************
 * 
 * 作者：许保同
 * 创建日期：20170805
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 许保同 20170805 17:00
 * 编辑文件 崔化栋 20180502 14:00
 * 
*******************************************************/

#if NET45
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Caching;

namespace Rafy.Utils.Caching
{
    /// <summary>
    /// 使用装饰模式，把一个不支持Region的ObjectCache变为支持的。
    /// </summary>
    public class RegionCache : ObjectCache
    {
        private static readonly string SPLITTER = "_:_";

        private ObjectCache _innerCache;

        public ObjectCache InnerCache
        {
            get { return _innerCache; }
        }

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

        internal void Clear()
        {
            var dic = _innerCache as IEnumerable<KeyValuePair<string, object>>;
            foreach (var kv in dic)
            {
                this._innerCache.Remove(kv.Key);
            }
        }

        internal void RemoveRegion(string regionName)
        {
            var dic = _innerCache as IEnumerable<KeyValuePair<string, object>>;
            foreach (var kv in dic)
            {
                string key = kv.Key;
                string region = null;
                DisableRegionInKey(ref key, ref region);
                if (region == regionName)
                {
                    this._innerCache.Remove(kv.Key);
                }
            }
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

        private static void DisableRegionInKey(ref string key, ref string regionName)
        {
            var index = key.IndexOf(SPLITTER);
            if (index >= 0)
            {
                regionName = key.Substring(0, index);
                key = key.Substring(index + SPLITTER.Length);
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
                if (index >= 0)
                {
                    item.RegionName = all.Substring(0, index);
                    item.Key = all.Substring(index + SPLITTER.Length);
                }
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
#endif

#if NS2
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Rafy.Utils.Caching
{

    public class RegionCache
    {
        private static readonly Microsoft.Extensions.Caching.Memory.MemoryCache _defaultMemoryCache;
        private static readonly string SPLITTER = "_:_";

        static RegionCache()
        {
            var memoryCacheOptions = new MemoryCacheOptions();
            var defaultMemoryCacheOptions = Options.Create<MemoryCacheOptions>(memoryCacheOptions);
            _defaultMemoryCache = new Microsoft.Extensions.Caching.Memory.MemoryCache(defaultMemoryCacheOptions);
        }

        private Microsoft.Extensions.Caching.Memory.MemoryCache _innerCache;

        private List<string> _keys;

        public RegionCache() : this(_defaultMemoryCache) { }

        public RegionCache(Microsoft.Extensions.Caching.Memory.MemoryCache innerCache)
        {
            if (innerCache == null) throw new ArgumentNullException("innerCache");
            _innerCache = innerCache;
            _keys = new List<string>();
        }

        public Microsoft.Extensions.Caching.Memory.MemoryCache InnerCache
        {
            get { return _innerCache; }
        }

        internal void Clear()
        {
            foreach (string key in _keys)
            {
                _innerCache.Remove(key);
            }
            _keys.Clear();
        }

        internal void RemoveRegion(string regionName)
        {
            for (int i = _keys.Count- 1; i >= 0; i--)
            {
                var key = _keys[i];

                string keyString = key;
                string region = null;
                DisableRegionInKey(ref keyString, ref region);

                if (region == regionName)
                {
                    _innerCache.Remove(key);
                    _keys.Remove(key);
                }
            }
        }

        public object Remove(string key, string regionName = null)
        {
            EnableRegionInKey(ref key, ref regionName);
            object retVal = _innerCache.Get(key);
            _innerCache.Remove(key);
            _keys.Remove(key);
            return retVal;
        }

        public object Get(string key, string regionName = null)
        {
            EnableRegionInKey(ref key, ref regionName);
            object retVal = _innerCache.Get(key);
            return retVal;
        }

        internal bool Add(string key, StoredValue value, string region)
        {
            try
            {
                EnableRegionInKey(ref key, ref region);
                _innerCache.Set<StoredValue>(key, value);
                _keys.Add(key);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private static void EnableRegionInKey(ref string key, ref string regionName)
        {
            if (regionName != null)
            {
                key = regionName + SPLITTER + key;
                regionName = null;
            }
        }

        private static void DisableRegionInKey(ref string key, ref string regionName)
        {
            var index = key.IndexOf(SPLITTER);
            if (index >= 0)
            {
                regionName = key.Substring(0, index);
                key = key.Substring(index + SPLITTER.Length);
            }
        }
    }
}
#endif
