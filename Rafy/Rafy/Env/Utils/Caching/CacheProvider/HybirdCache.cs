/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20101017
 * 说明：综合使用内存和硬盘的缓存提供器。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20101017
 * 修改文件 宋军瑞 20170112 -- 重构 Caching 模块。
 * 
*******************************************************/

using Rafy.Threading;

namespace Rafy.Utils.Caching
{
    /// <summary>
    /// 综合使用内存和硬盘的缓存提供器。
    /// 一级缓存：内存
    /// 二级缓存：硬盘/数据库
    /// </summary>
    public class HybirdCache : Cache
    {
        /// <summary>
        /// 一级缓存：内存
        /// </summary>
        private Cache _memory;

        /// <summary>
        /// 二级缓存：硬盘
        /// </summary>
        private Cache _disk;

        /// <summary>
        /// 使用指定的 sqlce 文件来作为二级缓存的硬盘缓存。
        /// </summary>
        /// <param name="sqlceFile"></param>
        public HybirdCache(string sqlceFile) : this(new SQLCompactCache(sqlceFile)) { }

        /// <summary>
        /// 使用指定的硬盘缓存来构造二级缓存。
        /// </summary>
        /// <param name="diskCache"></param>
        public HybirdCache(Cache diskCache)
        {
            this._memory = new MemoryCache();
            this._disk = diskCache;
        }

        internal protected override StoredValue GetCacheItemCore(string region, string key)
        {
            var value = this._memory.GetCacheItemCore(region, key);

            if (value == null)
            {
                value = this._disk.GetCacheItemCore(region, key);
                if (value != null)
                {
                    this._memory.AddCore(region, key, value);
                }
            }

            return value;
        }

        internal protected override bool AddCore(string region, string key, StoredValue value)
        {
            //由于优先使用内存中的缓存，所以添加到硬盘缓存的操作可以异步执行。
            AsyncHelper.InvokeSafe(() =>
            {
                this._disk.AddCore(region, key, value);
            });

            return this._memory.AddCore(region, key, value);
        }

        internal protected override void RemoveCore(string region, string key)
        {
            this._memory.RemoveCore(region, key);
            this._disk.RemoveCore(region, key);
        }

        protected internal override void ClearRegionCore(string region)
        {
            this._memory.ClearRegionCore(region);
            this._disk.ClearRegionCore(region);
        }

        protected internal override void ClearCore()
        {
            this._memory.Clear();
            this._disk.Clear();
        }
    }
}
