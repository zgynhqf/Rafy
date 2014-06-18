/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130329 10:40
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130329 10:40
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Rafy.EntityObjectModel
{
    /// <summary>
    /// 属性列表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class PropertyCollection<T> : Collection<T>
        where T : Property
    {
        //列表会自动设置其中所有的项的 Parent 属性。
        private EntityType _parent;

        internal PropertyCollection(EntityType parent)
        {
            this._parent = parent;
        }

        protected override void InsertItem(int index, T item)
        {
            item.Parent = _parent;

            base.InsertItem(index, item);
        }

        protected override void SetItem(int index, T item)
        {
            item.Parent = _parent;

            base.SetItem(index, item);
        }

        protected override void RemoveItem(int index)
        {
            var item = this[index];
            item.Parent = null;

            base.RemoveItem(index);
        }

        protected override void ClearItems()
        {
            foreach (var item in this)
            {
                item.Parent = null;
            }

            base.ClearItems();
        }
    }
}
