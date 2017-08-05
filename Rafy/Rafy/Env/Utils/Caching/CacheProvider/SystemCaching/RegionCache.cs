/*******************************************************
 * 
 * 作者：许保同
 * 创建日期：20170805
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 许保同 20170805 17:00
 * 
*******************************************************/

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
