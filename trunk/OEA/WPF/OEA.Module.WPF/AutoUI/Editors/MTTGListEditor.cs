/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20111215
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20111215
 * 
*******************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using OEA.Library;
using OEA.Module.WPF.Controls;

namespace OEA.Module.WPF.Editors
{
    /// <summary>
    /// 树型的列表编辑器
    /// </summary>
    public class MTTGListEditor : ListEditor
    {
        private IEnumerable<SortDescription> _lastSortDescriptions;

        private IEnumerable<string> _lastRootGroupDescriptions;

        public MTTGListEditor(IListEditorContext context) : base(context) { }

        /// <summary>
        /// 使用的 MTTG 控件
        /// </summary>
        public new TreeGrid Control
        {
            get { return base.Control as TreeGrid; }
        }

        public override CheckingMode CheckingMode
        {
            get { return this.Control.IsCheckingRow ? CheckingMode.CheckingRow : CheckingMode.None; }
            set { this.Control.IsCheckingRow = value == CheckingMode.CheckingRow; }
        }

        public override CheckingRowCascade CheckingRowCascade
        {
            get { return this.Control.CheckingRowCascade; }
            set { this.Control.CheckingRowCascade = value; }
        }

        public override Predicate<Entity> Filter
        {
            get { return this.Control.ItemFilter; }
            set { this.Control.ItemFilter = e => value(e as Entity); }
        }

        /// <summary>
        /// 排序字段
        /// </summary>
        public override IEnumerable<SortDescription> SortDescriptions
        {
            get { return this._lastSortDescriptions; }
            set
            {
                //记录最后一次使用的排序描述，在数据变化时，此值需要再次起作用。
                this._lastSortDescriptions = value;
                this.Control.NodeSortDescriptions = value;
            }
        }

        public override IEnumerable<string> RootGroupDescriptions
        {
            get { return this._lastRootGroupDescriptions; }
            set
            {
                this._lastRootGroupDescriptions = value;
                this.Control.RootGroupDescriptions = value;
            }
        }

        internal override void SetControl(FrameworkElement value)
        {
            var newTreeView = value as TreeGrid;
            if (newTreeView == null) throw new ArgumentException("value 必须继承自 TreeGrid。");

            var oldTreeView = this.Control;

            base.SetControl(value);

            this.AdaptEventsToContext(oldTreeView, newTreeView);
        }

        public override Entity Current
        {
            get { return this.Control.SelectedItem as Entity; }
            set { this.Control.SelectedItem = value; }
        }

        public override IList<Entity> SelectedEntities
        {
            get
            {
                var ctrl = this.Control;
                if (ctrl.IsCheckingRow)
                {
                    return new EntityListAdapter { InnerList = ctrl.CheckingModel.SelectedItems };
                }
                else
                {
                    return new EntityListAdapter { InnerList = ctrl.SelectionModel.SelectedItems };
                }
            }
        }

        public override void SelectAll()
        {
            var ctrl = this.Control;
            if (ctrl.IsCheckingRow)
            {
                ctrl.CheckingModel.SelectAll();
            }
            else
            {
                ctrl.SelectionModel.SelectAll();
            }
        }

        protected override void OnIsReadOnlyChanged()
        {
            base.OnIsReadOnlyChanged();

            this.Control.IsReadOnly = this.IsReadOnly;
        }

        protected override void OnContextDataChanged()
        {
            var motv = this.Control;

            var data = this.Context.Data;
            try
            {
                this._reportEvents = false;

                motv.ItemsSource = data;
            }
            finally
            {
                this._reportEvents = true;
            }

            if (data != null && data.Count > 0)
            {
                //重置列隐藏
                (motv.Columns as TreeColumnCollection).UpdateVisibilities(data[0]);

                //重置排序：由于用户点击列头等操作可能使得控件本身的排序方案发生改变，所以这里需要还原这个设置。
                this.SortDescriptions = this._lastSortDescriptions;

                //重置分组
                if (this._lastRootGroupDescriptions != null)
                {
                    this.RootGroupDescriptions = this._lastRootGroupDescriptions;
                }
                else
                {
                    var g = this.Context.Meta.GroupBy;
                    if (g != null)
                    {
                        this.RootGroupDescriptions = new string[] { g.Name };
                    }
                }

                //设置过滤
                var hasFilter = data as IHasViewFilter;
                if (hasFilter != null)
                {
                    motv.ItemFilter = e => hasFilter.ViewModelFilter(e);
                }
            }

            motv.IsReadOnly = this.IsReadOnly;
        }

        /// <summary>
        /// 根据rootPid绑定数据
        /// </summary>
        /// <param name="rootPid">
        /// 如果这个值不是null，则这个值表示绑定的所有根节点的父id。
        /// </param>
        public override void BindData(int? rootPid)
        {
            try
            {
                this._reportEvents = false;

                this.Control.BindData(rootPid);
            }
            finally
            {
                this._reportEvents = true;
            }
        }

        public override void RefreshControl()
        {
            try
            {
                this._reportEvents = false;

                this.Control.Refresh();
            }
            finally
            {
                this._reportEvents = true;
            }
        }

        #region AdaptEventsToContext

        private bool _reportEvents = true;

        private void AdaptEventsToContext(TreeGrid oldTreeView, TreeGrid newTreeView)
        {
            if (oldTreeView != null)
            {
                oldTreeView.SelectedItemChanged -= On_TreeView_SelectedItemChanged;
                oldTreeView.MouseDoubleClick -= On_TreeView_MouseDoubleClick;
                oldTreeView.CheckChanged -= On_TreeView_CheckChanged;
            }

            var eventReporter = this.Context.EventReporter;
            if (eventReporter != null)
            {
                newTreeView.SelectedItemChanged += On_TreeView_SelectedItemChanged;
                newTreeView.MouseDoubleClick += On_TreeView_MouseDoubleClick;
                newTreeView.CheckChanged += On_TreeView_CheckChanged;
            }
        }

        private void On_TreeView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (this._reportEvents)
            {
                this.Context.EventReporter.NotifyMouseDoubleClick(sender, e);
            }
        }

        private void On_TreeView_SelectedItemChanged(object sender, SelectedItemChangedEventArgs e)
        {
            if (this._reportEvents)
            {
                var args = new SelectedEntityChangedEventArgs(e.NewItem as Entity, e.OldItem as Entity);
                var eventReporter = this.Context.EventReporter;
                eventReporter.NotifySelectedItemChanged(sender, args);
            }
        }

        private void On_TreeView_CheckChanged(object sender, CheckChangedEventArgs e)
        {
            if (this._reportEvents)
            {
                var eventReporter = this.Context.EventReporter;
                eventReporter.NotifyCheckChanged(sender, e);
            }
        }

        #endregion

        #region EntityListAdapter

        private class EntityListAdapter : IList<Entity>, IList
        {
            internal IList InnerList;

            public int IndexOf(Entity item)
            {
                return this.InnerList.IndexOf(item);
            }

            public void Insert(int index, Entity item)
            {
                this.InnerList.Insert(index, item);
            }

            public void RemoveAt(int index)
            {
                this.InnerList.RemoveAt(index);
            }

            public Entity this[int index]
            {
                get
                {
                    return this.InnerList[index] as Entity;
                }
                set
                {
                    this.InnerList[index] = value;
                }
            }

            public void Add(Entity item)
            {
                this.InnerList.Add(item);
            }

            public void Clear()
            {
                this.InnerList.Clear();
            }

            public bool Contains(Entity item)
            {
                return this.InnerList.Contains(item);
            }

            public void CopyTo(Entity[] array, int arrayIndex)
            {
                this.InnerList.CopyTo(array, arrayIndex);
            }

            public int Count
            {
                get { return this.InnerList.Count; }
            }

            public bool IsReadOnly
            {
                get { return this.InnerList.IsReadOnly; }
            }

            public bool Remove(Entity item)
            {
                var index = this.InnerList.IndexOf(item);
                if (index >= 0)
                {
                    this.InnerList.Remove(item);
                    return true;
                }
                return false;
            }

            public IEnumerator<Entity> GetEnumerator()
            {
                return this.InnerList.Cast<Entity>().GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.InnerList.GetEnumerator();
            }

            public int Add(object value)
            {
                return this.InnerList.Add(value);
            }

            public bool Contains(object value)
            {
                return this.InnerList.Contains(value);
            }

            public int IndexOf(object value)
            {
                return this.InnerList.IndexOf(value);
            }

            public void Insert(int index, object value)
            {
                this.InnerList.Insert(index, value);
            }

            public bool IsFixedSize
            {
                get { return this.InnerList.IsFixedSize; }
            }

            public void Remove(object value)
            {
                this.InnerList.Remove(value);
            }

            object IList.this[int index]
            {
                get
                {
                    return this.InnerList[index];
                }
                set
                {
                    this.InnerList[index] = value;
                }
            }

            public void CopyTo(Array array, int index)
            {
                this.InnerList.CopyTo(array, index);
            }

            public bool IsSynchronized
            {
                get { return this.InnerList.IsSynchronized; }
            }

            public object SyncRoot
            {
                get { return this.InnerList.SyncRoot; }
            }
        }

        #endregion
    }
}