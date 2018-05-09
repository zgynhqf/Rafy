/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20101017
 * 说明：通用缓存框架中的缓存抽象类。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20101017
 * 修改文件 宋军瑞 20170112 -- 重构 Caching 模块。
 * 
*******************************************************/

using System;

namespace Rafy.Utils.Caching
{
    /// <summary>
    /// 通用缓存框架中的抽象类。
    /// </summary>
    public abstract class Cache : ICache
    {
        private static ICache _cache;
        private static readonly object _syncRoot = new object();

        /// <summary>
        /// <see cref="Cache"/> 的默认实现。采用单例模式。
        /// </summary>
        public static ICache Default
        {
            get
            {
                if (_cache == null)
                {
                    lock (_syncRoot)
                    {
                        if (_cache == null)
                        {
                            _cache = new MemoryCache();
                        }
                    }
                }

                return _cache;
            }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));

                _cache = value;
            }
        }

        /// <summary>
        /// 获取或设置是否打开缓存功能。
        /// </summary>
        public virtual bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 通过 <paramref name="key"/> 和 <paramref name="region"/> 从缓存中获取缓存项。
        /// 如果不存在，则返回null。
        /// </summary>
        /// <param name="key">表示一个缓存的键。</param>
        /// <param name="region">表示缓存所属的域。</param>
        /// <returns>返回缓存项。</returns>
        public object Get(string key, string region = null)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException($"parameter {nameof(key)} can not be null or empty.");

            if (!this.IsEnabled) return null;

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
        /// 向缓存中添加一个缓存项。
        /// </summary>
        /// <param name="key">表示一个缓存的键。</param>
        /// <param name="value">表示一个缓存的值。</param>
        /// <param name="policy">缓存使用的策略（一般是过期策略）</param>
        /// <param name="region">表示缓存所属的域。</param>
        /// <returns>添加成功返回 true, 失败返回 false。</returns>
        public bool Add(string key, object value, Policy policy, string region = null)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException($"parameter {nameof(key)} can not be null or empty.");

            if (!this.IsEnabled) return false;

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
        /// <param name="key">表示一个缓存的键。</param>
        /// <param name="region">表示缓存所属的域。</param>
        public void Remove(string key, string region = null)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException($"parameter {nameof(key)} can not be null or empty.");

            if (!this.IsEnabled) return;

            this.RemoveCore(region, key);
        }

        /// <summary>
        /// 删除某个区域中的所有值。
        /// </summary>
        /// <param name="region">表示缓存所属的域。</param>
        public void ClearRegion(string region)
        {
            if (!this.IsEnabled) return;

            this.ClearRegionCore(region);
        }

        /// <summary>
        /// 清空所有缓存。
        /// </summary>
        public void Clear()
        {
            if (!this.IsEnabled) return;

            this.ClearCore();
        }


        /// <summary>
        /// 从缓存中获取指定的值。
        /// <para>尝试使用缓存获取，如果不存在，则调用ifNotExists函数获取返回值，并添加到缓存中。</para>
        /// </summary>
        /// <typeparam name="T">表示缓存项的类型。</typeparam>
        /// <param name="key">表示一个缓存的键。</param>
        /// <param name="ifNotExists">表示如果未取到缓存，执行的委托。</param>
        /// <param name="regionName">表示缓存所属的域。</param>
        /// <param name="policy">表示缓存策略。</param>
        /// <returns></returns>
        public T Get<T>(string key, Func<T> ifNotExists, string regionName = null, Policy policy = null)
            where T : class
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException($"parameter {nameof(key)} can not be null or empty.");

            if (!this.IsEnabled) return default(T);

            var result = this.Get(key, regionName) as T;

            if (result != null) return result;

            lock (this)
            {
                result = this.Get(key, regionName) as T;
                if (result != null) return result;

                result = ifNotExists();

                if (result != null)
                {
                    this.Add(key, result, policy, regionName);
                }
            }

            return result;
        }

        #region 子类实现以下方法

        protected internal abstract StoredValue GetCacheItemCore(string region, string key);

        protected internal abstract bool AddCore(string region, string key, StoredValue value);

        protected internal abstract void RemoveCore(string region, string key);

        protected internal abstract void ClearRegionCore(string region);

        protected internal abstract void ClearCore();

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
