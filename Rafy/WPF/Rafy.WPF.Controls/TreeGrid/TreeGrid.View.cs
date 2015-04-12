/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120912 14:35
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120912 14:35
 * 
*******************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Rafy.WPF.Controls
{
    /// <summary>
    /// 与视图相关的方法集。
    /// </summary>
    partial class TreeGrid
    {
        #region NodeSortDescriptions DependencyProperty

        public static readonly DependencyProperty NodeSortDescriptionsProperty = DependencyProperty.Register(
            "NodeSortDescriptions", typeof(IEnumerable<SortDescription>), typeof(TreeGrid),
            new FrameworkPropertyMetadata(null, (d, e) => (d as TreeGrid).OnNodeSortDescriptionsChanged(e))
            );

        /// <summary>
        /// 所有结点使用的排序规则
        /// </summary>
        public IEnumerable<SortDescription> NodeSortDescriptions
        {
            get { return (IEnumerable<SortDescription>)GetValue(NodeSortDescriptionsProperty); }
            set { SetValue(NodeSortDescriptionsProperty, value); }
        }

        #endregion

        #region ItemFilter DependencyProperty

        public static readonly DependencyProperty ItemFilterProperty = DependencyProperty.Register(
            "ItemFilter", typeof(Predicate<object>), typeof(TreeGrid),
            new FrameworkPropertyMetadata(null, (d, e) => (d as TreeGrid).OnItemFilterChanged(e))
            );

        /// <summary>
        /// 所有层级都会使用到的过滤委托。默认为 null。
        /// </summary>
        public Predicate<object> ItemFilter
        {
            get { return (Predicate<object>)GetValue(ItemFilterProperty); }
            set { SetValue(ItemFilterProperty, value); }
        }

        #endregion

        #region 排序

        private TreeGridColumn _lastSortColumn;

        protected virtual void OnNodeSortDescriptionsChanged(DependencyPropertyChangedEventArgs e)
        {
            if (!this.IsInitialized) return;

            this.ApplySorting();

            //所有结点都需要被排序
            foreach (TreeGridRow item in this.RecursiveRows)
            {
                this.ApplySorting(item);
            }
        }

        /// <summary>
        /// How to: Sort a GridView Column When a Header Is Clicked
        /// http://msdn.microsoft.com/zh-cn/library/ms745786(v=VS.85).aspx
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnColumnHeaderClick(RoutedEventArgs e)
        {
            var header = e.OriginalSource as TreeGridColumnHeader;
            if (header == null || header.Column == null) return;

            var treeColumn = header.Column as TreeGridColumn;
            if (treeColumn == null) return;

            //如果是一些没有属性列（例如“选择”列），不需要执行排序。
            var property = treeColumn.SortingProperty ?? treeColumn.PropertyName;
            if (property == null) return;

            var ascending = treeColumn.SortDirection == TreeGridColumnSortDirection.Ascending;
            if (this._lastSortColumn != treeColumn)
            {
                ascending = true;
            }
            else
            {
                ascending = !ascending;
            }

            var sort = new SortDescription(property, ascending ? ListSortDirection.Ascending : ListSortDirection.Descending);
            this.NodeSortDescriptions = new SortDescription[] { sort };

            if (_lastSortColumn != null && _lastSortColumn != treeColumn)
            {
                _lastSortColumn.SortDirection = TreeGridColumnSortDirection.None;
            }
            _lastSortColumn = treeColumn;
            _lastSortColumn.SortDirection = ascending ? TreeGridColumnSortDirection.Ascending : TreeGridColumnSortDirection.Descending;

            //处理掉这个事件，包含这个 TreeGrid 的外层 TreeGrid 才不会继续排序。
            e.Handled = true;
        }

        /// <summary>
        /// 对整个树进行排序。
        /// </summary>
        private void ApplySorting()
        {
            this.ApplySorting(this.RootNode);
        }

        /// <summary>
        /// Copies the <see cref="SortDescription"/> elements of
        /// the <see cref="NodeSortDescriptions"/> collection to
        /// a currently processed tree node. This method is being
        /// invoked during the initialization of a given node, and
        /// if the <see cref="NodeSortDescriptions"/> property 
        /// is changed at runtime.<br/>
        /// If the <see cref="ItemCollection.SortDescriptions"/>
        /// collection of the submitted <paramref name="node"/> is
        /// not empty, it will be cleared.<br/>
        /// This method is always being invoked, even if the
        /// <see cref="NodeSortDescriptions"/> dependency property
        /// is null. If you want to apply a custom sorting mechanism,
        /// simply override this method.
        /// </summary>
        /// <param name="node">The currently processed node, if any.
        /// This parameter is null if sort parameters should be set
        /// on the tree's <see cref="ItemsControl.Items"/>
        /// collection itself.
        /// </param>
        private void ApplySorting(TreeGridRow node)
        {
            //check whether we're sorting on node or tree level
            var items = node == null ? this.Items : node.Items;

            using (items.DeferRefresh())
            {
                //clear existing sort directions, if there are any
                items.SortDescriptions.Clear();

                //copy new sort directions
                if (this.NodeSortDescriptions != null)
                {
                    foreach (var sd in this.NodeSortDescriptions)
                    {
                        items.SortDescriptions.Add(sd);
                    }
                }
            }
        }

        #endregion

        #region 过滤

        protected virtual void OnItemFilterChanged(DependencyPropertyChangedEventArgs e)
        {
            if (!this.IsInitialized) return;

            //整颗树也需要过滤。
            this.ApplyFilter(this);
            //start at root level
            foreach (var row in this.RecursiveRows)
            {
                this.ApplyFilter(row);
            }
        }

        /// <summary>
        /// Applies the current <see cref="ItemFilter"/> on a given tree node.
        /// </summary>
        /// <param name="treeNode">The node to be filtered.</param>
        private void ApplyFilter(ItemsControl node)
        {
            node.Items.Filter = this.ItemFilter;
        }

        #endregion

        #region 分组

        private IEnumerable<string> _rootGroupDescriptions;

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

            var rootControl = this.RootItemsControl;

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

                this.IsVirtualizing = false;
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
                        groups.Add(new PropertyGroupDescription(mp));
                    }
                }
            }
        }

        private bool HasGrouped
        {
            get { return this._rootGroupDescriptions != null && this._rootGroupDescriptions.Any(); }
        }

        #endregion

        #region 行号

        private const int NumberWidth = 8;//一个数字的宽度。

        #region AutoCalcRowHeaderWith DependencyProperty

        public static readonly DependencyProperty AutoCalcRowHeaderWidthProperty = DependencyProperty.Register(
            "AutoCalcRowHeaderWidth", typeof(bool), typeof(TreeGrid),
            new PropertyMetadata(true)
            );

        /// <summary>
        /// 是否自动计算 RowHeader 的宽度。
        /// </summary>
        public bool AutoCalcRowHeaderWidth
        {
            get { return (bool)this.GetValue(AutoCalcRowHeaderWidthProperty); }
            set { this.SetValue(AutoCalcRowHeaderWidthProperty, value); }
        }

        #endregion

        #region RowHeaderWidth DependencyProperty

        public static readonly DependencyProperty RowHeaderWidthProperty = DependencyProperty.Register(
            "RowHeaderWidth", typeof(int), typeof(TreeGrid)
            );

        /// <summary>
        /// 行号的左缩进量
        /// </summary>
        public int RowHeaderWidth
        {
            get { return (int)this.GetValue(RowHeaderWidthProperty); }
            set { this.SetValue(RowHeaderWidthProperty, value); }
        }

        #endregion

        private void AutoCalculateRowHeaderWidth(object dataSource)
        {
            var list = dataSource as IList;
            if (list != null)
            {
                var c = list.Count;
                if (c == 0)
                {
                    this.RowHeaderWidth = 0;
                }
                //else if (c < 10)
                //{
                //    this.RowHeaderWidth = NumberWidth;
                //}
                else if (c < 100)
                {
                    this.RowHeaderWidth = NumberWidth * 2;
                }
                else if (c < 1000)
                {
                    this.RowHeaderWidth = NumberWidth * 3;
                }
                else if (c < 10000)
                {
                    this.RowHeaderWidth = NumberWidth * 4;
                }
                else if (c < 100000)
                {
                    this.RowHeaderWidth = NumberWidth * 5;
                }
                else
                {
                    this.RowHeaderWidth = NumberWidth * 6;
                }
            }
            else
            {
                this.RowHeaderWidth = 0;
            }
        }

        private void RefreshRowNo(TreeGridRow row)
        {
            //目前为了简单起见，只支持表格模式下有行号。（因为在树型模式下，行号应该使用递归遍历后计算生成。）
            if (this.OnlyGridMode)
            {
                //不能直接使用 DataItemsSource，这是因为可能界面会对集合进行排序。
                //var item = GetEntity(row);
                //var rowNo = this.DataItemsSource.IndexOf(item) + 1;
                //row.RowNo = rowNo;

                row.RowNo = this.ItemContainerGenerator.IndexFromContainer(row) + 1;
            }
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
                if (column.State != ColumnMeasureState.SpecificWidth)
                {
                    column.RequestDataWidth();
                }
            }
        }

        #endregion
    }
}