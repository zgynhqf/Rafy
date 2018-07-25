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
 * 编辑文件 崔化栋 20180502 14:00
 * 
*******************************************************/

#if NS2
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
#endif
#if NET45
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web;
#endif

namespace Rafy.Utils.Caching
{
    /// <summary>
    /// 定义一个 Http 请求缓存。使用场景：在每次 Http 请求中使用。
    /// </summary>
    public class PerHttpRequestCache : Cache
    {
#if NET45
        private readonly HttpContextBase _context;
#endif
#if NS2
        private readonly HttpContext _context;
#endif
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
#if NET45
        public PerHttpRequestCache(HttpContextBase context)
#endif
#if NS2
        public PerHttpRequestCache(HttpContext context)
#endif
        {
            this._context = context;
        }

        /// <summary>
        /// 获取 <see cref="HttpContextBase"/> 实例的 Items.
        /// </summary>
#if NET45
        protected virtual IDictionary GetItems()
#endif
#if NS2
        protected virtual IDictionary<object, object> GetItems()
#endif
        {
#if NET45
            if (this._context == null && HttpContext.Current == null) return null;

            if (this._context != null) return _context.Items;
            if (HttpContext.Current != null) return HttpContext.Current.Items;
#endif
#if NS2
            if (this._context == null) return null;

            if (this._context != null) return _context.Items;
#endif

            return null;
        }

        protected override StoredValue GetCacheItemCore(string region, string key)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException($"parameter {nameof(key)} can not be null or empty.");

            var items = this.GetItems();

            return (StoredValue)items?[key];
        }

        protected override bool AddCore(string region, string key, StoredValue value)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException($"parameter {nameof(key)} can not be null or empty.");

            var items = this.GetItems();
            if (items == null) return false;

            if (value == null) return false;

#if NET45
            if (items.Contains(key))
#endif
#if NS2
            if (items.ContainsKey(key))
#endif
            {
                items[key] = value;
            }
            else
            {
                items.Add(key, value);
            }

            return true;
        }

        protected override void RemoveCore(string region, string key)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException($"parameter {nameof(key)} can not be null or empty.");

            var items = this.GetItems();

            items?.Remove(key);
        }

        protected override void ClearRegionCore(string region)
        {
            if (string.IsNullOrWhiteSpace(region)) throw new ArgumentException($"parameter {nameof(region)} can not be null or empty.");

            var items = this.GetItems();
            if (items == null)
            {
                return;
            }

            var regex = new Regex(region, RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var keysToRemove = new List<string>();

#if NET45
            var enumerator = items.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (regex.IsMatch(enumerator.Key.ToString()))
                {
                    keysToRemove.Add(enumerator.Key.ToString());
                }
            }
#endif
#if NS2
            foreach (var item in items)
            {
                if (regex.IsMatch(item.Key.ToString()))
                {
                    keysToRemove.Add(item.Key.ToString());
                }
            }
#endif

            foreach (var key in keysToRemove)
            {
                items.Remove(key);
            }
        }

        protected override void ClearCore()
        {
            var items = this.GetItems();
            if (items == null)
            {
                return;
            }


            var keysToRemove = new List<string>();

#if NET45
            var enumerator = items.GetEnumerator();
            while (enumerator.MoveNext())
            {
                keysToRemove.Add(enumerator.Key.ToString());
            }
#endif
#if NS2
            foreach (var item in items)
            {
                keysToRemove.Add(item.Key.ToString());
            }
#endif

            foreach (var key in keysToRemove)
            {
                items.Remove(key);
            }
        }
    }
}
