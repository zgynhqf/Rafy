/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20101017
 * 说明：内存缓存提供器
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20101017
 * 修改文件 宋军瑞 20170112 -- 重构 Caching 模块。
 * 
*******************************************************/

using System.Runtime.Caching;

namespace Rafy.Utils.Caching
{
    /// <summary>
    /// 内存缓存提供器
    /// 
    /// 使用System.Runtime.Caching.MemoryCache作为内部实现。
    /// 由于MemoryCache不支持Region，所以这里使用RegionCache进行转换。
    /// </summary>
    public class MemoryCache : Cache
    {
        internal MemoryCache()
        {
            _regionCache = new RegionCache(System.Runtime.Caching.MemoryCache.Default);
        }

        public MemoryCache(System.Runtime.Caching.MemoryCache memoryCache)
        {
            _regionCache = new RegionCache(memoryCache);
        }

        private readonly RegionCache _regionCache;

        protected internal override StoredValue GetCacheItemCore(string region, string key)
        {
            var item = this._regionCache.GetCacheItem(key, region);

            return item?.Value as StoredValue;
        }

        protected internal override bool AddCore(string region, string key, StoredValue value)
        {
            return this._regionCache.Add(key, value, new CacheItemPolicy(), region);
        }

        protected internal override void RemoveCore(string region, string key)
        {
            this._regionCache.Remove(key, region);
        }

        protected internal override void ClearRegionCore(string region)
        {
            this._regionCache.RemoveRegion(region);
        }

        protected internal override void ClearCore()
        {
            this._regionCache.Clear();
        }
    }
}
