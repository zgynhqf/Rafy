/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20101017
 * 说明：用于 Http 每个请求的缓存提供器。
 * 运行环境：.NET 4.5
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 宋军瑞 20170112 -- 重构 Caching 模块。
 * 
*******************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web;

namespace Rafy.Utils.Caching
{
    /// <summary>
    /// 定义一个 Http 请求缓存。使用场景：在每次 Http 请求中使用。
    /// </summary>
    public class PerHttpRequestCache : Cache
    {
        private readonly HttpContextBase _context;

        /// <summary>
        /// 初始化 <see cref="PerHttpRequestCache"/> 类的新实例.
        /// </summary>
        public PerHttpRequestCache()
        {

        }

        /// <summary>
        /// 初始化 <see cref="PerHttpRequestCache"/> 类的新实例.
        /// </summary>
        /// <param name="context">Context</param>
        public PerHttpRequestCache(HttpContextBase context)
        {
            this._context = context;
        }

        /// <summary>
        /// 获取 <see cref="HttpContextBase"/> 实例的 Items.
        /// </summary>
        protected virtual IDictionary GetItems()
        {
            if (this._context == null && HttpContext.Current == null) return null;

            if (this._context != null) return _context.Items;
            if (HttpContext.Current != null) return HttpContext.Current.Items;

            return null;
        }

        protected internal override StoredValue GetCacheItemCore(string region, string key)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException($"parameter {nameof(key)} can not be null or empty.");

            var items = this.GetItems();

            return (StoredValue)items?[key];
        }

        protected internal override bool AddCore(string region, string key, StoredValue value)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException($"parameter {nameof(key)} can not be null or empty.");

            var items = this.GetItems();
            if (items == null) return false;

            if (value == null) return false;

            if (items.Contains(key))
            {
                items[key] = value;
            }
            else
            {
                items.Add(key, value);
            }

            return true;
        }

        protected internal override void RemoveCore(string region, string key)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException($"parameter {nameof(key)} can not be null or empty.");

            var items = this.GetItems();

            items?.Remove(key);
        }

        protected internal override void ClearRegionCore(string region)
        {
            if (string.IsNullOrWhiteSpace(region)) throw new ArgumentException($"parameter {nameof(region)} can not be null or empty.");

            var items = this.GetItems();
            if (items == null)
            {
                return;
            }

            var enumerator = items.GetEnumerator();
            var regex = new Regex(region, RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var keysToRemove = new List<string>();
            while (enumerator.MoveNext())
            {
                if (regex.IsMatch(enumerator.Key.ToString()))
                {
                    keysToRemove.Add(enumerator.Key.ToString());
                }
            }

            foreach (var key in keysToRemove)
            {
                items.Remove(key);
            }
        }

        protected internal override void ClearCore()
        {
            var items = this.GetItems();
            if (items == null)
            {
                return;
            }

            var enumerator = items.GetEnumerator();
            var keysToRemove = new List<string>();
            while (enumerator.MoveNext())
            {
                keysToRemove.Add(enumerator.Key.ToString());
            }

            foreach (var key in keysToRemove)
            {
                items.Remove(key);
            }
        }
    }
}
