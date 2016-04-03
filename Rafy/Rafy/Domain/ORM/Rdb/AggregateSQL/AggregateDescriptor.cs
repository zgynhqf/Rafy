/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20101230
 * 说明：聚合加载描述器
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20101230
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.Domain.ORM
{
    /// <summary>
    /// 聚合加载描述器。
    /// 
    /// 目前只包含一些聚合加载选项“AggregateSQLItem”
    /// </summary>
    internal class AggregateDescriptor
    {
        private LinkedList<LoadOptionItem> _items = new LinkedList<LoadOptionItem>();

        /// <summary>
        /// 所有的AggregateSQLItem
        /// </summary>
        internal LinkedList<LoadOptionItem> Items
        {
            get
            {
                return _items;
            }
        }

        /// <summary>
        /// 直接加载的实体类型
        /// </summary>
        internal Type DirectlyQueryType
        {
            get
            {
                return this._items.First.Value.OwnerType;
            }
        }

        /// <summary>
        /// 追加一个聚合加载选项
        /// </summary>
        /// <param name="item"></param>
        internal void AddItem(LoadOptionItem item)
        {
            this._items.AddLast(item);
        }
    }
}