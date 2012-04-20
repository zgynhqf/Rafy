using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.Module.WPF.Editors;
using System.Windows.Controls;
using System.Windows;
using System.ComponentModel;
using OEA.Library;
using System.Windows.Data;
using OEA.ManagedProperty;
using OEA.MetaModel;
using OEA.MetaModel.View;
using System.Windows.Media;
using Hardcodet.Wpf.GenericTreeView;
using System.Windows.Input;

namespace OEA.Module.WPF.Controls
{
    public partial class MultiTypesTreeGrid
    {
        private void OnViewConstruct()
        {
            base.Tree.AddHandler(GridTreeViewColumnHeader.ClickEvent, (RoutedEventHandler)this.OnColumnHeaderClick);

            if (!this._rootIsTree)
            {
                //这个控件不需要支持多类型对象了，如果不是树型，直接是 Grid 模式
                this.Tree.OnlyGridMode = true;
                //this.Tree.OnlyGridMode = this.RootEntityViewMeta.TreeChildEntity == null;
            }

            KeyboardNavigation.SetTabNavigation(this, KeyboardNavigationMode.Contained);
        }

        #region 排序

        private TreeColumn _lastSortColumn;

        private bool _ascending;

        /// <summary>
        /// How to: Sort a GridView Column When a Header Is Clicked
        /// http://msdn.microsoft.com/zh-cn/library/ms745786(v=VS.85).aspx
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnColumnHeaderClick(object sender, RoutedEventArgs e)
        {
            var header = e.OriginalSource as GridTreeViewColumnHeader;
            if (header == null || header.Column == null) return;

            var treeColumn = header.Column as TreeColumn;
            if (treeColumn == null) return;

            //如果是一些没有属性列（例如“选择”列），不需要执行排序。
            var property = treeColumn.Meta;
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
            var sort = new SortDescription(treeColumn.Meta.Name, this._ascending ? ListSortDirection.Ascending : ListSortDirection.Descending);
            this.NodeSortDescriptions = new SortDescription[] { sort };

            this._lastSortColumn = treeColumn;

            //处理掉这个事件，包含这个列表的父列表才不会继续排序。
            e.Handled = true;
        }

        #endregion

        #region 过滤

        private bool PassFilter(Entity nonRootItem)
        {
            var filter = this.ItemFilter;
            return filter == null || filter(nonRootItem);
        }

        private Predicate<object> _viewFilter;

        protected override void OnItemFilterChanged(DependencyPropertyChangedEventArgs e)
        {
            this._viewFilter = null;
            if (e.NewValue != null) this._viewFilter = o => this.ItemFilter(o as Entity);

            if (!this.IsInitialized || this.Tree == null) return;

            //整颗树也需要过滤。
            this.ApplyFilter(this.Tree, null);

            base.OnItemFilterChanged(e);
        }

        protected internal override void ApplyFilter(ItemsControl node, Entity item)
        {
            Predicate<object> filter = null;
            if (this.ItemFilter != null)
            {
                filter = o =>
                {
                    var entity = GetEntity(o as GridTreeViewRow);
                    return entity == null || this.ItemFilter(entity);
                };
            }

            node.Items.Filter = filter;
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
                        ContainerStyle = OEAStyles.GroupContainerStyle
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

        protected override void RefreshCore(TreeLayout layout)
        {
            this._entityRows.Clear();

            base.RefreshCore(layout);

            this.RequestDataWidthes();

            if (!this._suppressRowNoRefresh) { this.TryRefreshRowNo(); }
        }

        private void RefreshRowNo_OnCollapsedOrExpanded()
        {
            if (this._suppressRowNoRefresh) return;

            this.TryRefreshRowNo();
        }

        private void TryRefreshRowNo()
        {
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
            foreach (TreeColumn column in this.Columns)
            {
                column.RequestDataWidth();
            }
        }

        #endregion
    }
}