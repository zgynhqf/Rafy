/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20101017
 * 说明：缓存模块为上层提供的API。
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
using System.Runtime.Caching;

namespace OEA.Utils.Caching
{
    /// <summary>
    /// 缓存子系统
    /// </summary>
    public class Cache
    {
        private CacheProvider _cacheProvider;

        /// <summary>
        /// 构造函数。
        /// </summary>
        /// <param name="cacheProvider">
        /// 本缓存模块需要指定提供器。
        /// </param>
        public Cache(CacheProvider cacheProvider)
        {
            if (cacheProvider == null) throw new ArgumentNullException("cacheProvider");
            this._cacheProvider = cacheProvider;
            this.IsEnabled = true;
        }

        /// <summary>
        /// 是否打开缓存功能？
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 通过key和region从缓存中获取缓存项。
        /// 如果不存在，则返回null。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="regionName"></param>
        /// <returns></returns>
        public object Get(string key, string regionName = null)
        {
            if (!this.IsEnabled) return null;

            return this._cacheProvider.GetCacheItem(key, regionName);
        }

        /// <summary>
        /// 向缓存中添加一项。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="policy">缓存使用的策略（一般是过期策略）</param>
        /// <param name="region"></param>
        /// <returns></returns>
        public bool Add(string key, object value, Policy policy, string region = null)
        {
            if (!this.IsEnabled) return false;

            return this._cacheProvider.Add(key, value, policy, region);
        }

        public void Remove(string key, string region = null)
        {
            if (this.IsEnabled)
            {
                this._cacheProvider.Remove(key, region);
            }
        }

        /// <summary>
        /// 从缓存中获取指定的值。
        /// 
        /// 尝试使用缓存获取，如果不存在，则调用ifNotExists函数获取返回值，并添加到缓存中。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="ifNotExists"></param>
        /// <param name="regionName"></param>
        /// <param name="policy"></param>
        /// <returns></returns>
        public T Get<T>(string key, Func<T> ifNotExists, string regionName = "GlobalCache", Policy policy = null)
            where T : class
        {
            var result = this.Get(key, regionName) as T;

            if (result == null)
            {
                result = ifNotExists();

                if (result != null)
                {
                    this.Add(key, result, policy, regionName);
                }
            }

            return result;
        }
    }
}