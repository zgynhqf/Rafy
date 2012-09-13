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

namespace OEA.Module.WPF.Controls
{
    /// <summary>
    /// 展开折叠功能。
    /// </summary>
    partial class CompositeTreeView
    {
        #region 静态函数入口

        private static void TreeViewItem_Expanded(object sender, RoutedEventArgs e)
        {
            var item = e.OriginalSource as TreeViewItem;
            if (!TreeUtil.IsDummyItem(item)) { (sender as CompositeTreeView).OnNodeExpanded(item); }
        }

        private static void TreeViewItem_Collapsed(object sender, RoutedEventArgs e)
        {
            var item = e.OriginalSource as TreeViewItem;
            if (!TreeUtil.IsDummyItem(item)) { (sender as CompositeTreeView).OnNodeCollapsed(item); }
        }

        #endregion

        /// <summary>
        /// 本字段表示当前的树型控件的需要展示的内部布局，在展开结点并生成子结点时使用。
        /// （不论是否使用了虚拟化，都使用本对象跟踪已展开的结点列表。）
        /// </summary>
        private TreeLayout _destinationLayout = new TreeLayout();

        #region ClearCollapsedNodes DependencyProperty

        public static readonly DependencyProperty ClearCollapsedNodesProperty = DependencyProperty.Register(
            "ClearCollapsedNodes", typeof(bool), typeof(CompositeTreeView),
            new FrameworkPropertyMetadata(true, (d, e) => (d as CompositeTreeView).OnClearCollapsedNodesChanged(e))
            );

        /// <summary>
        /// If enabled along with lazy loading, the tree automatically
        /// discards tree nodes if their parent is being collapsed.
        /// This keeps the memory footprint at a minimum (only visible nodes exist
        /// in memory), but re-expanding a node also requires recreation of its
        /// child nodes. This property defaults to true.<br/>
        /// Important: This feature is only applied if <see cref="IsLazyLoading"/>
        /// is true as well.
        /// </summary>
        [Category("TreeGrid")]
        [Description("Removes collapsed nodes from the tree if lazy loading is active.")]
        public bool ClearCollapsedNodes
        {
            get { return (bool)GetValue(ClearCollapsedNodesProperty); }
            set { SetValue(ClearCollapsedNodesProperty, value); }
        }

        private void OnClearCollapsedNodesChanged(DependencyPropertyChangedEventArgs e)
        {
            if (!this.IsInitialized) return;
            this.Refresh();
        }

        #endregion

        /// <summary>
        /// 结点被折叠后事件。
        /// 如果懒加载开启后，则在折叠时清空所有子结点。
        /// </summary>
        /// <param name="treeNode"></param>
        protected virtual void OnNodeCollapsed(TreeViewItem treeNode)
        {
            //不处理根结点。
            if (treeNode == this.RootNode) return;

            //更新布局
            var itemKey = GetId(GetEntity(treeNode));
            this._destinationLayout.ExpandedNodeIds.Remove(itemKey);

            if (this.IsLazyLoading && this.ClearCollapsedNodes && treeNode.Items.Count > 0)
            {
                //deregisters listeners for all ancestors
                if (this.ObserveChildItems)
                {
                    this._monitor.RemoveNodes(treeNode.Items);
                }

                //clear items and insert dummy
                treeNode.Items.Clear();
                treeNode.Items.Add(this.CreateDummyItem());

                //TODO do we have collapsed event if all childs are being filtered?
                //if yes, we need to set the dummy nodes visibility depending
                //on the filtered childs (visible if at least 1 visible child)
            }
        }

        /// <summary>
        /// 结点被展开后事件
        /// 完成懒加载时的结点创建工作，并设置各结点的 Filter 属性。
        /// </summary>
        protected virtual void OnNodeExpanded(TreeViewItem treeNode)
        {
            object item = GetEntity(treeNode);
            if (item == null) return;

            //布局
            var itemKey = GetId(item);
            if (!this._destinationLayout.ExpandedNodeIds.Contains(itemKey)) this._destinationLayout.ExpandedNodeIds.Add(itemKey);

            var nodeItems = treeNode.Items;

            //懒加载状态下，创建结点。
            if (this.IsLazyLoading)
            {
                TreeUtil.ClearDummyChildNode(treeNode);

                var childItems = this.GetChildItems(item);
                if (nodeItems.Count == 0)
                {
                    foreach (var childItem in childItems)
                    {
                        //根据需要的布局来生成递归子结点，
                        this.CreateItemNode(childItem, nodeItems, this._destinationLayout);
                    }

                    //刷新以排序
                    if (nodeItems.NeedsRefresh) { nodeItems.Refresh(); }
                }
            }

            if (nodeItems.Count == 0)
            {
                //如果结点没有数据，则不需要展开它。
                treeNode.IsExpanded = false;
                this._destinationLayout.ExpandedNodeIds.Remove(itemKey);
            }
        }

        /// <summary>
        /// Expands all nodes of the tree. This means that nodes
        /// for all items will be created even if <see cref="IsLazyLoading"/>
        /// is set to true.
        /// </summary>
        public virtual void ExpandAll()
        {
            if (this.RootNode != null) this.RootNode.IsExpanded = true;

            foreach (TreeViewItem item in this.RootControl.Items)
            {
                item.ExpandSubtree();
            }
        }

        /// <summary>
        /// Collapses all nodes of the tree. 
        /// </summary>
        /// <remarks>If <see cref="IsLazyLoading"/> is set to true,
        /// the footprint of the tree may be reduced by invoking
        /// <see cref="Refresh()"/>. This automatically discards all
        /// previously created nodes and only recreates the (visible)
        /// root nodes.</remarks>
        public virtual void CollapseAll()
        {
            foreach (TreeViewItem item in this.RecursiveNodeList)
            {
                item.IsExpanded = false;
            }
        }

        /// <summary>
        /// Collapses all tree nodes that are not direct ancestors of
        /// the currently selected item's node. This method is being
        /// invoked every time the <see cref="SelectedItem"/> property
        /// is being changed, even if <see cref="AutoCollapse"/> is
        /// false.
        /// </summary>
        protected virtual void ApplyAutoCollapse()
        {
            if (Tree == null || !AutoCollapse) return;

            object selected = SelectedItem;
            ItemCollection items = this.RootControl.Items;

            if (selected == null)
            {
                //if we don't have a selected item, just collapse the
                //root items
                foreach (TreeViewItem node in items)
                {
                    node.IsExpanded = false;
                }
            }
            else
            {
                var parents = GetParentItemList(selected);
                foreach (object parent in parents)
                {
                    var parentKey = GetId(parent);
                    var parentNode = TryFindItemNode(items, parentKey, false);

                    if (parentNode == null)
                    {
                        string msg = "Cannot collapse item '{0}' - the item does not exist in the hierarchy of the tree's bound items.";
                        msg = String.Format(msg, parentKey);
                        throw new InvalidOperationException(msg);
                    }

                    foreach (TreeViewItem item in items)
                    {
                        //collapse all items that are no ancestors
                        if (item == parentNode) continue;
                        item.IsExpanded = false;
                    }

                    //go a level deeper
                    items = parentNode.Items;
                }

                //finally collapse the item and its siblings
                foreach (TreeViewItem item in items)
                {
                    item.IsExpanded = false;
                }
            }
        }
    }
}
