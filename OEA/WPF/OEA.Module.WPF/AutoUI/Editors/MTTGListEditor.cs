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
using System.Windows;
using System.Windows.Input;
using Hardcodet.Wpf.GenericTreeView;
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

        public MTTGListEditor(IListEditorContext context)
            : base(context) { }

        /// <summary>
        /// 使用的控件
        /// </summary>
        public new MultiTypesTreeGrid Control
        {
            get { return base.Control as MultiTypesTreeGrid; }
        }

        public override CheckingMode CheckingMode
        {
            get { return this.Control.CheckingMode; }
            set { this.Control.CheckingMode = value; }
        }

        public override CheckingRowCascade CheckingRowCascade
        {
            get { return this.Control.CheckingRowCascade; }
            set { this.Control.CheckingRowCascade = value; }
        }

        public override Predicate<Entity> Filter
        {
            get { return this.Control.ItemFilter; }
            set { this.Control.ItemFilter = value; }
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
            var newTreeView = value as MultiTypesTreeGrid;
            if (newTreeView == null) throw new ArgumentException("value 必须继承自 MultiTypesTreeGrid。");

            var oldTreeView = this.Control;

            base.SetControl(value);

            this.AdaptEventsToContext(oldTreeView, newTreeView);
        }

        public override Entity Current
        {
            get { return this.Control.SelectedItem; }
            set { this.Control.SelectedItem = value; }
        }

        public override IList<Entity> SelectedEntities
        {
            get { return this.Control.SelectedItems; }
        }

        public override void SelectAll()
        {
            this.Control.SelectAll();
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
                motv.Columns.UpdateVisibilities(data[0]);

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

        private void AdaptEventsToContext(MultiTypesTreeGrid oldTreeView, MultiTypesTreeGrid newTreeView)
        {
            if (oldTreeView != null)
            {
                oldTreeView.SelectedItemChanged -= On_TreeView_SelectedItemChanged;
                oldTreeView.MouseDoubleClick -= On_TreeView_MouseDoubleClick;
            }

            var eventReporter = this.Context.EventReporter;
            if (eventReporter != null)
            {
                newTreeView.SelectedItemChanged += On_TreeView_SelectedItemChanged;
                newTreeView.MouseDoubleClick += On_TreeView_MouseDoubleClick;
            }
        }

        private void On_TreeView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (this._reportEvents)
            {
                this.Context.EventReporter.NotifyMouseDoubleClick(sender, e);
            }
        }

        private void On_TreeView_SelectedItemChanged(object sender, RoutedTreeItemEventArgs<Entity> e)
        {
            if (this._reportEvents)
            {
                var args = new SelectedEntityChangedEventArgs(e.NewItem, e.OldItem);
                var eventReporter = this.Context.EventReporter;
                eventReporter.NotifySelectedItemChanged(sender, args);
            }
        }

        #endregion
    }
}