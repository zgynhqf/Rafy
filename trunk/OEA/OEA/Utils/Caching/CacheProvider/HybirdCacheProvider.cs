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
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OEA.Threading;

namespace OEA.Utils.Caching
{
    /// <summary>
    /// 综合使用内存和硬盘的缓存提供器。
    /// 一级缓存：内存
    /// 二级缓存：硬盘/数据库
    /// </summary>
    public class HybirdCacheProvider : CacheProvider
    {
        /// <summary>
        /// 一级缓存：内存
        /// </summary>
        private CacheProvider _memory;

        /// <summary>
        /// 二级缓存：硬盘
        /// </summary>
        private CacheProvider _disk;

        private IHybirdCacheProviderService _service;

        public HybirdCacheProvider(string dbFile, IHybirdCacheProviderService service)
        {
            if (service == null) throw new ArgumentNullException("service");

            this._memory = new MemoryCacheProvider();
            this._disk = new SQLCompactProvider(dbFile, service);
            this._service = service;
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
            //异步添加到硬盘上。
            this._service.AsyncInvokeSave(() =>
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

        protected internal override void ClearCore()
        {
            this._memory.Clear();
            this._disk.Clear();
        }
    }

    public interface IHybirdCacheProviderService : ISQLCompactSerializer
    {
        void AsyncInvokeSave(Action action);
    }
}
