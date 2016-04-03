/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20101017
 * 说明：通用缓存框架中的提供器
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

namespace Rafy.Utils.Caching
{
    /// <summary>
    /// 通用缓存框架中的提供器
    /// </summary>
    public abstract class CacheProvider
    {
        /// <summary>
        /// 通过key和region从缓存中获取缓存项。
        /// 如果不存在，则返回null。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="region"></param>
        /// <returns></returns>
        public object GetCacheItem(string key, string region = null)
        {
            var wrapper = this.GetCacheItemCore(region, key);

            if (wrapper != null)
            {
                //如果存在检测器，则需要进行检测后才能保证缓存没有失效。
                var checker = wrapper.Checker;
                if (checker != null)
                {
                    checker.Check();
                    if (checker.HasChanged)
                    {
                        this.Remove(key, region);
                        wrapper.Value = null;
                    }
                }
                return wrapper.Value;
            }

            return null;
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
            if (value == null) return false;
            ChangeChecker checker = null;
            if (policy != null)
            {
                checker = policy.Checker;
            }

            return this.AddCore(region, key, new StoredValue()
            {
                Value = value,
                Checker = checker
            });
        }

        /// <summary>
        /// 把指定项从缓存中移除。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="region"></param>
        public void Remove(string key, string region = null)
        {
            this.RemoveCore(region, key);
        }

        /// <summary>
        /// 删除某个区域中的所有值。
        /// </summary>
        /// <param name="region"></param>
        public void ClearRegion(string region)
        {
            this.ClearRegionCore(region);
        }

        /// <summary>
        /// 清空所有缓存。
        /// </summary>
        public void Clear()
        {
            this.ClearCore();
        }

        #region 子类实现以下方法

        internal protected abstract StoredValue GetCacheItemCore(string region, string key);

        internal protected abstract bool AddCore(string region, string key, StoredValue value);

        internal protected abstract void RemoveCore(string region, string key);

        internal protected abstract void ClearRegionCore(string region);

        internal protected abstract void ClearCore();

        #endregion
    }

    /// <summary>
    /// 存储在缓存中的对象
    /// </summary>
    public class StoredValue
    {
        /// <summary>
        /// 真实的缓存值。
        /// </summary>
        public object Value;

        /// <summary>
        /// 缓存值获取时需要进行的检测。
        /// </summary>
        public ChangeChecker Checker;
    }
}
