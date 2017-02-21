/*******************************************************
 * 
 * 作者：宋军瑞
 * 创建时间：20170112
 * 说明：通用缓存框架中的接口
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 宋军瑞 20170112 -- 重构 Caching 模块。
 * 
*******************************************************/

using System;

namespace Rafy.Utils.Caching
{
    public interface ICache
    {
        /// <summary>
        /// 是否打开缓存功能。
        /// </summary>
        bool IsEnabled { get; set; }

        /// <summary>
        /// 通过 <paramref name="key"/> 和 <paramref name="region"/> 从缓存中获取缓存项。
        /// 如果不存在，则返回null。
        /// </summary>
        /// <param name="key">表示一个缓存的键。</param>
        /// <param name="region">表示缓存所属的域。</param>
        /// <returns>返回缓存项。</returns>
        object Get(string key, string region = null);

        /// <summary>
        /// 向缓存中添加一个缓存项。
        /// </summary>
        /// <param name="key">表示一个缓存的键。</param>
        /// <param name="value">表示一个缓存的值。</param>
        /// <param name="policy">缓存使用的策略（一般是过期策略）</param>
        /// <param name="region">表示缓存所属的域。</param>
        /// <returns>添加成功返回 true, 失败返回 false。</returns>
        bool Add(string key, object value, Policy policy, string region = null);

        /// <summary>
        /// 把指定项从缓存中移除。
        /// </summary>
        /// <param name="key">表示一个缓存的键。</param>
        /// <param name="region">表示缓存所属的域。</param>
        void Remove(string key, string region = null);

        /// <summary>
        /// 删除某个区域中的所有值。
        /// </summary>
        /// <param name="region">表示缓存所属的域。</param>
        void ClearRegion(string region);

        /// <summary>
        /// 清空所有缓存。
        /// </summary>
        void Clear();

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
        T Get<T>(string key, Func<T> ifNotExists, string regionName = null, Policy policy = null) where T : class;
    }
}