using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using OEA.Module.WPF.Controls;

namespace OEA.Module.WPF.Controls
{
    public partial class TreeGrid
    {
        #region 排序

        private TreeGridColumn _lastSortColumn;

        private bool _ascending;

        /// <summary>
        /// How to: Sort a GridView Column When a Header Is Clicked
        /// http://msdn.microsoft.com/zh-cn/library/ms745786(v=VS.85).aspx
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnColumnHeaderClick(RoutedEventArgs e)
        {
            var header = e.OriginalSource as GridTreeViewColumnHeader;
            if (header == null || header.Column == null) return;

            var treeColumn = header.Column as TreeGridColumn;
            if (treeColumn == null) return;

            //如果是一些没有属性列（例如“选择”列），不需要执行排序。
            var property = treeColumn.SortingProperty;
            if (property == null) return;

            if (this._lastSortColumn != treeColumn)
            {
                this._ascending = true;
            }
            else
            {
                this._ascending = !this._ascending;
            }

            //motv.NodeSortDescriptions 中的属性必须加上 Header.
            //详见：http://www.codeproject.com/KB/WPF/versatile_treeview.aspx#sorting
            var sort = new SortDescription(property, this._ascending ? ListSortDirection.Ascending : ListSortDirection.Descending);
            this.NodeSortDescriptions = new SortDescription[] { sort };

            this._lastSortColumn = treeColumn;

            //处理掉这个事件，包含这个列表的父列表才不会继续排序。
            e.Handled = true;
        }

        #endregion

        #region 过滤

        private Predicate<object> _viewFilter;

        protected override void OnItemFilterChanged(DependencyPropertyChangedEventArgs e)
        {
            this._viewFilter = null;
            if (e.NewValue != null) this._viewFilter = o => this.ItemFilter(o as object);

            if (!this.IsInitialized || this.Tree == null) return;

            //整颗树也需要过滤。
            this.ApplyFilter(this.Tree, null);

            base.OnItemFilterChanged(e);
        }

        #endregion

        #region GroupingStyle DependencyProperty

        public static readonly DependencyProperty GroupingStyleProperty = DependencyProperty.Register(
            "GroupingStyle", typeof(Style), typeof(TreeGrid)
            );

        public Style GroupingStyle
        {
            get { return (Style)this.GetValue(GroupingStyleProperty); }
            set { this.SetValue(GroupingStyleProperty, value); }
        }

        #endregion

        #region 分组

        private IEnumerable<string> _rootGroupDescriptions;

        public IEnumerable<string> RootGroupDescriptions
        {
            get { return this._rootGroupDescriptions; }
            set
            {
                if (this._rootGroupDescriptions != value)
                {
                    this._rootGroupDescriptions = value;

                    this.OnRootGroupDescriptionsChanged();
                }
            }
        }

        private void OnRootGroupDescriptionsChanged()
        {
            var value = this._rootGroupDescriptions;

            var rootControl = this.RootControl;

            //如果需要分组，则初始化分组样式，并关闭 UI 虚拟化。
            var isGrouping = value != null && value.Any();
            if (isGrouping)
            {
                if (rootControl.GroupStyle.Count == 0)
                {
                    rootControl.GroupStyle.Add(new GroupStyle
                    {
                        ContainerStyle = this.GroupingStyle
                    });
                }

                this.Tree.IsGridVirtualizing = false;
            }

            //修改 GroupDescriptions 实现分组。
            var items = rootControl.Items;
            using (items.DeferRefresh())
            {
                var groups = items.GroupDescriptions;

                groups.Clear();

                if (isGrouping)
                {
                    foreach (var mp in value)
                    {
                        var property = BindableEntityProperty(mp);
                        groups.Add(new PropertyGroupDescription(property));
                    }
                }
            }
        }

        #endregion

        #region 行号

        /*********************** 代码块解释 *********************************
         * 在以下情况重新生成行号：
         * DataBound、Sorted、CollapseAll、ExpandAll、NodeCollapsed、NodeExpanded
        **********************************************************************/

        protected override void OnNodeSortDescriptionsChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnNodeSortDescriptionsChanged(e);

            this.TryRefreshRowNo();
        }

        private bool _suppressRowNoRefresh = false;

        public override void CollapseAll()
        {
            try
            {
                this._suppressRowNoRefresh = true;

                base.CollapseAll();

                this.RefreshRowNo();
            }
            finally
            {
                this._suppressRowNoRefresh = false;
            }
        }

        public override void ExpandAll()
        {
            try
            {
                //由于 Expand 每一行都会发生 OnNodeExpanded 事件，而这个事件处理函数中也会刷新行号，
                //所以这里需要把行号刷新的功能禁用，最后统一刷新。
                this._suppressRowNoRefresh = true;

                base.ExpandAll();

                this.RefreshRowNo();
            }
            finally
            {
                this._suppressRowNoRefresh = false;
            }
        }

        protected override void RenderTree(TreeLayout layout)
        {
            this._entityRows.Clear();

            base.RenderTree(layout);

            this.RequestDataWidthes();

            this.TryRefreshRowNo();
        }

        private void RefreshRowNo_OnCollapsedOrExpanded()
        {
            this.TryRefreshRowNo();
        }

        private void TryRefreshRowNo()
        {
            if (this._suppressRowNoRefresh) return;

            //已经开始生成行号了，或者是在简单表格的状态下，都需要刷新行号。
            var tree = this.Tree;
            if (tree.HasRowNo || tree.OnlyGridMode) { this.RefreshRowNo(); }
        }

        /// <summary>
        /// 遍历所有树型节点，设置行号
        /// 注意：目会把当前已经生成的行设置行号
        /// </summary>
        private void RefreshRowNo()
        {
            int rowNo = 0;
            foreach (GridTreeViewRow row in this.RecursiveNodeList) { row.RowNo = ++rowNo; }

            this.Tree.HasRowNo = true;
        }

        #endregion

        #region 自动列宽

        /// <summary>
        /// 对所有列都重新计算动态宽度。
        /// </summary>
        private void RequestDataWidthes()
        {
            foreach (TreeGridColumn column in this.Columns)
            {
                column.RequestDataWidth();
            }
        }

        #endregion
    }
}