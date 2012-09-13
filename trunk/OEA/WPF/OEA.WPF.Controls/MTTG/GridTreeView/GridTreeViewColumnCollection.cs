/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：2011
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 2011
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Collections.Specialized;

namespace OEA.Module.WPF.Controls
{
    public class GridTreeViewColumnCollection : GridViewColumnCollection
    {
        private GridTreeView _owner;

        private List<GridTreeViewColumn> _unvisibleColumns = new List<GridTreeViewColumn>();

        protected internal GridTreeView Owner
        {
            get { return this._owner; }
            internal set { this._owner = value; }
        }

        public IList<GridTreeViewColumn> UnvisibleColumns
        {
            get { return this._unvisibleColumns.AsReadOnly(); }
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Replace)
            {
                foreach (TreeGridColumn column in e.NewItems)
                {
                    column.Owner = this.Owner.TreeGrid;

                    column._ownerCollection = this;

                    //如果该列不可见，就在设置完 _ownerCollection 后删除它。
                    if (!column.IsVisible)
                    {
                        if (!this._unvisibleColumns.Contains(column))
                        {
                            this.SetVisibility(column, false);
                        }
                    }
                }
            }

            base.OnCollectionChanged(e);
        }

        internal void SetVisibility(GridTreeViewColumn column, bool isVisible)
        {
            if (isVisible)
            {
                if (column._oldIndexAsUnvisible >= 0 && column._oldIndexAsUnvisible < this.Count)
                {
                    this.Insert(column._oldIndexAsUnvisible, column);
                }
                else
                {
                    this.Add(column);
                }

                //最后再移除，OnCollectionChanged 事件中会使用此变量防止重入
                this._unvisibleColumns.Remove(column);
            }
            else
            {
                column._oldIndexAsUnvisible = this.IndexOf(column);

                this.RemoveAt(column._oldIndexAsUnvisible);

                this._unvisibleColumns.Add(column);
            }
        }
    }
}