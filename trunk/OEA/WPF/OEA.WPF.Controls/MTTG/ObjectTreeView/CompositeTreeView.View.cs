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
using System.Windows.Input;

namespace OEA.Module.WPF.Controls
{
    /// <summary>
    /// 与视图相关的方法集。
    /// </summary>
    partial class CompositeTreeView
    {
        #region NodeSortDescriptions DependencyProperty

        public static readonly DependencyProperty NodeSortDescriptionsProperty = DependencyProperty.Register(
            "NodeSortDescriptions", typeof(IEnumerable<SortDescription>), typeof(CompositeTreeView),
            new FrameworkPropertyMetadata(null, (d, e) => (d as CompositeTreeView).OnNodeSortDescriptionsChanged(e))
            );

        /// <summary>
        /// 所有结点使用的排序规则
        /// </summary>
        [Category("TreeGrid")]
        [Description("Sorting directives for the tree's nodes.")]
        public IEnumerable<SortDescription> NodeSortDescriptions
        {
            get { return (IEnumerable<SortDescription>)GetValue(NodeSortDescriptionsProperty); }
            set { SetValue(NodeSortDescriptionsProperty, value); }
        }

        #endregion

        #region ItemFilter DependencyProperty

        public static readonly DependencyProperty ItemFilterProperty = DependencyProperty.Register(
            "ItemFilter", typeof(Predicate<object>), typeof(CompositeTreeView),
            new FrameworkPropertyMetadata(null, (d, e) => (d as CompositeTreeView).OnItemFilterChanged(e))
            );

        /// <summary>
        /// 所有层级都会使用到的过滤委托。默认为 null。
        /// </summary>
        [Category("TreeGrid")]
        [Description("Filter expression for bound items.")]
        public Predicate<object> ItemFilter
        {
            get { return (Predicate<object>)GetValue(ItemFilterProperty); }
            set { SetValue(ItemFilterProperty, value); }
        }

        #endregion

        #region 排序

        protected virtual void OnNodeSortDescriptionsChanged(DependencyPropertyChangedEventArgs e)
        {
            if (!this.IsInitialized || this.Tree == null) return;

            this.PrepareControlSortDescriptions();

            try
            {
                //TreeView 在被排序时会发生 SelectedItemChanged 事件，这里需要过滤掉。
                this._ignoreTreeViewEvents = true;

                this.ApplySorting();
            }
            finally
            {
                this._ignoreTreeViewEvents = false;
            }

            //所有结点都需要被排序
            foreach (TreeViewItem item in this.RecursiveNodeList)
            {
                this.ApplySorting(item, GetEntity(item));
            }
        }

        /// <summary>
        /// SortDescription 中使用的属性名称是数据的属性名称，但是这里需要转换为可绑定到行控件上的属性，即每个属性前都加上 “Header.”。
        /// </summary>
        private IEnumerable<SortDescription> _controlSortDescriptions;

        private void PrepareControlSortDescriptions()
        {
            var newValue = this.NodeSortDescriptions;
            if (newValue != null)
            {
                this._controlSortDescriptions = newValue.Select(rd => new SortDescription(BindableEntityProperty(rd.PropertyName), rd.Direction));
            }
            else
            {
                this._controlSortDescriptions = null;
            }
        }

        /// <summary>
        /// 对整个树进行排序。
        /// </summary>
        private void ApplySorting()
        {
            this.ApplySorting(this.RootNode, null);
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
        /// <param name="item">The item that is being represented
        /// by the node. This parameter is null if sort parameters
        /// should be set on the tree's <see cref="ItemsControl.Items"/>
        /// collection itself, or on the <see cref="RootNode"/>.</param>
        private void ApplySorting(TreeViewItem node, object item)
        {
            //check whether we're sorting on node or tree level
            var items = node == null ? this.Tree.Items : node.Items;
            //clear existing sort directions, if there are any
            items.SortDescriptions.Clear();

            //copy new sort directions
            if (this._controlSortDescriptions != null)
            {
                using (items.DeferRefresh())
                {
                    foreach (var sd in this._controlSortDescriptions)
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
            if (!this.IsInitialized || this.Tree == null) return;

            if (e.NewValue == null)
            {
                //explicitely clear the filter (refreshing only operates on
                //visible nodes, not collapsed ones
                this.ClearFilter();
            }
            else
            {
                this.RefreshFilter();
            }
        }

        /// <summary>
        /// Clears the filter on <em>all</em> created tree
        /// nodes. This is the case if the <see cref="ItemFilter"/>
        /// dependency property is set to null.
        /// </summary>
        protected void ClearFilter()
        {
            //we cannot just recurse - even dummy nodes are affected by the filter!
            ItemCollection nodes = this.RootControl.Items;
            foreach (TreeViewItem node in nodes)
            {
                this.ClearFilter(node);
            }
        }

        /// <summary>
        /// Recursively reverts filtered items starting at a
        /// given node.
        /// </summary>
        /// <param name="treeNode">The node to be reverted.</param>
        protected void ClearFilter(TreeViewItem treeNode)
        {
            //get the underlying item, if possible
            object item = GetEntity(treeNode);
            this.ApplyFilter(treeNode, item);

            foreach (TreeViewItem childNode in treeNode.Items)
            {
                this.ClearFilter(childNode);
            }
        }

        /// <summary>
        /// Applies the <see cref="ItemFilter"/> Predicate<object> on all currently
        /// visible tree nodes as well as direct descendants of collapsed
        /// nodes (in order to determine whether to show or hide a node
        /// expander). This method can be invoked in order to re-evaluate
        /// a filter, and it's also invoked if the <see cref="ItemFilter"/>
        /// dependency property is changed. This method is also being invoked
        /// if the <see cref="ItemFilter"/> dependency property has been
        /// set to <c>null</c>.
        /// </summary>
        public void RefreshFilter()
        {
            //start at root level
            var nodes = this.RootControl.Items;
            foreach (TreeViewItem node in nodes)
            {
                this.Filter(node);
            }
        }

        /// <summary>
        /// Recursively evaluates a branch of the tree against the currently
        /// active filter (if there is one).
        /// </summary>
        /// <param name="treeNode"></param>
        public void Filter(TreeViewItem treeNode)
        {
            //get the underlying item, if possible
            object item = GetEntity(treeNode);

            //apply the filter (submits a null reference if the node does
            //not represent a bound item)
            this.ApplyFilter(treeNode, item);

            //process all regular child nodes (ignore whether expanded or not)
            foreach (TreeViewItem childNode in treeNode.Items)
            {
                this.Filter(childNode);
            }
        }

        /// <summary>
        /// Applies the current <see cref="ItemFilter"/> on a given tree node.
        /// </summary>
        /// <param name="treeNode">The node to be filtered.</param>
        /// <param name="item">The item that was bound to the node. If the
        /// node does not represent a bound item (because of custom injected
        /// nodes), this parameter is null.</param>
        /// <returns>The result of the filter evaluation (true if the item is
        /// accepted and not filtered).</returns>
        protected void ApplyFilter(ItemsControl node, object item)
        {
            Predicate<object> filter = null;
            if (this.ItemFilter != null)
            {
                filter = o =>
                {
                    var entity = GetEntity(o as TreeViewItem);
                    return entity == null || this.ItemFilter(entity);
                };
            }

            node.Items.Filter = filter;
        }

        #endregion
    }
}