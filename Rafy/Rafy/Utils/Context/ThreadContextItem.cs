/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20211113
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20211113 22:14
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Text;

namespace Rafy
{
    /// <summary>
    /// 用于定义在线程上下文中的一个项。
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public class ThreadContextItem<TValue> : ContextItem<TValue>
    {
        [ThreadStatic]
        private static IDictionary<string, object> _items;

        public ThreadContextItem(string key, TValue defaultValue = default) : base(key, defaultValue) { }

        protected override IDictionary<string, object> ContextDataContainer
        {
            get
            {
                var res = _items;
                if (res == null)
                {
                    res = new Dictionary<string, object>();
                    _items = res;
                }
                return res;
            }
        }
    }
}