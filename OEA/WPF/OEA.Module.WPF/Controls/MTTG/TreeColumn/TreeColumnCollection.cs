/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：201202
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 201202
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using OEA.Module.WPF.Editors;
using OEA.ManagedProperty;
using OEA.Library;

namespace OEA.Module.WPF.Controls
{
    public class TreeColumnCollection : GridTreeViewColumnCollection
    {
        /// <summary>
        /// 通过属性查找对应的列
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public TreeColumn FindByProperty(IManagedProperty property)
        {
            return this.Cast<TreeColumn>()
                .FirstOrDefault(c => c.Meta.PropertyMeta.ManagedProperty == property);
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (TreeColumn item in e.NewItems) { item.Owner = this.Owner.TreeGrid; }
            }

            base.OnCollectionChanged(e);
        }

        internal void UpdateVisibilities(Entity data)
        {
            foreach (TreeColumn column in this.ToArray()) { column.UpdateVisibility(data); }
            foreach (TreeColumn column in this.UnvisibleColumns.ToArray()) { column.UpdateVisibility(data); }
        }
    }
}