/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130412 19:10
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130412 19:10
 * 
*******************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace Rafy
{
    /// <summary>
    /// 这个类解决了基类在 Clear 时，不提供所有 OldItems 的问题。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class NotifyChangedCollection<T> : ObservableCollection<T>, INotifyChangedCollection
    {
        private IList _clearedItems;

        public NotifyChangedCollection() { }

        public NotifyChangedCollection(IEnumerable<T> collection) : base(collection) { }

        public NotifyChangedCollection(List<T> list) : base(list) { }

        ///// <summary>
        ///// 被清空的项
        ///// </summary>
        //public IList ClearedItems
        //{
        //    get { return _clearedItems; }
        //}

        /// <summary>
        /// 获取被清空的项，并清空这个缓存。
        /// </summary>
        public IList PopClearedItems()
        {
            var items = _clearedItems;
            _clearedItems = null;
            return items;
        }

        protected override void ClearItems()
        {
            if (this.Count == 0)
            {
                _clearedItems = null;
            }
            else
            {
                _clearedItems = this.ToArray();
            }

            base.ClearItems();
        }
    }
}