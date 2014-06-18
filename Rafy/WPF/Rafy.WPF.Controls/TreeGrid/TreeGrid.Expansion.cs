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

namespace Rafy.WPF.Controls
{
    /// <summary>
    /// 展开折叠功能。
    /// </summary>
    partial class TreeGrid
    {
        /// <summary>
        /// 本字段表示当前的树型控件的需要展开的所有节点。
        /// 
        /// 在某行被创建时，会根据这个字段来判断其是否需要在初始时即呈现展开状态。
        /// （不论是否使用了虚拟化，都使用本对象指导需要展开的节点列表。）
        /// </summary>
        internal TreeGridExpandedItems _renderExpansion = new TreeGridExpandedItems();

        #region ClearCollapsedNodes DependencyProperty

        public static readonly DependencyProperty ClearCollapsedNodesProperty = DependencyProperty.Register(
            "ClearCollapsedNodes", typeof(bool), typeof(TreeGrid),
            new FrameworkPropertyMetadata(true, (d, e) => (d as TreeGrid).OnClearCollapsedNodesChanged(e))
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
        public bool ClearCollapsedNodes
        {
            get { return (bool)GetValue(ClearCollapsedNodesProperty); }
            set { SetValue(ClearCollapsedNodesProperty, value); }
        }

        #endregion

        private void OnClearCollapsedNodesChanged(DependencyPropertyChangedEventArgs e)
        {
            if (this.IsInitialized) { this.Render(); }
        }

        #region API - 展开折叠节点

        /// <summary>
        /// 展开所有节点。
        /// 也就是说，就算 <see cref="IsLazyLoading"/> 的值是 true，也会生成所有的节点。
        /// </summary>
        public void ExpandAll()
        {
            //把当前生成的节点都展开。
            if (this.RootNode != null) this.RootNode.IsExpanded = true;
            foreach (var row in this.RecursiveRows) { row.IsExpanded = true; }

            //把所有的 Id 都加入到列表中，这样在延迟生成时，就可以再展开相应的节点。
            EachNode(item =>
            {
                _renderExpansion.Add(GetId(item));
                return false;
            });
        }

        /// <summary>
        /// 折叠所有结点
        /// </summary>
        /// <remarks>If <see cref="IsLazyLoading"/> is set to true,
        /// the footprint of the tree may be reduced by invoking
        /// <see cref="Render()"/>. This automatically discards all
        /// previously created nodes and only recreates the (visible)
        /// root nodes.</remarks>
        public void CollapseAll()
        {
            _renderExpansion.Clear();

            foreach (var item in this.RecursiveRows) { item.IsExpanded = false; }
        }

        /// <summary>
        /// 展开某个节点
        /// </summary>
        /// <param name="dataItem"></param>
        /// <param name="recur">是否递归展开node的子节点</param>
        public void Expand(object dataItem, bool recur = true)
        {
            var row = this.FindRow(dataItem);
            if (row != null)
            {
                row.IsExpanded = true;
            }
            else
            {
                _renderExpansion.Add(GetId(dataItem));
            }

            if (recur)
            {
                var children = this.GetChildItems(dataItem);
                foreach (var child in children)
                {
                    if (this.HasChildItems(child)) { this.Expand(child, true); }
                }
            }
        }

        /// <summary>
        /// 折叠指定的节点
        /// </summary>
        /// <param name="node"></param>
        /// <param name="recur"></param>
        public void Collapse(object dataItem, bool recur = true)
        {
            var row = this.FindRow(dataItem);
            if (row != null)
            {
                if (recur)
                {
                    for (int i = row.Items.Count - 1; i > 0; i--)
                    {
                        var item = row.Items[i];
                        if (this.HasChildItems(item))
                        {
                            this.Collapse(item, recur);
                        }
                    }
                }

                row.IsExpanded = false;
            }
        }

        /// <summary>
        /// 展开指定节点到其下面指定级别
        /// </summary>
        /// <param name="node"></param>
        /// <param name="depth"></param>
        public void ExpandToDepth(object node, int depth)
        {
            if (depth > 0)
            {
                this.Expand(node, false);

                //如果对象不在Items里面，则return
                var row = this.FindRow(node);
                if (row != null)
                {
                    foreach (var child in row.Items)
                    {
                        ExpandToDepth(child, depth - 1);
                    }
                }
            }
            else
            {
                this.Collapse(node);
            }
        }

        /// <summary>
        /// 尽量展开所有目标行的父行，并让目标行处于可视范围内。
        /// </summary>
        /// <param name="item">要显示的行对应的数据。</param>
        /// <returns>返回是否成功显示该数据对应的行。</returns>
        public bool ExpandToView(object item)
        {
            bool success = false;

            if (item != null)
            {
                //从最上层父开始搜索对应的生成的行，如果已经生成，则直接展开该行，
                //如果该行还没有生成，则在渲染布局中加入该id，该行在接下来生成时会自动展开。
                var parents = this.GetParentItemList(item);
                bool stopFindingRow = false;
                TreeGridRow lastParentRow = null;
                foreach (var parent in parents)
                {
                    _renderExpansion.Add(GetId(parent));

                    if (!stopFindingRow)
                    {
                        var parentRow = this.FindRow(parent);
                        if (parentRow == null)
                        {
                            stopFindingRow = true;
                        }
                        else
                        {
                            lastParentRow = parentRow;
                            parentRow.IsExpanded = true;
                        }
                    }
                }

                //没有停止搜索，说明所有行都已经生成。
                if (!stopFindingRow)
                {
                    var selectedRow = this.SelectedRow;
                    if (selectedRow != null)
                    {
                        selectedRow.BringIntoView();
                        success = true;
                    }
                }

                //如果目标行并没有成功显示，则把最后可见的父行显示出来。
                if (!success && lastParentRow != null)
                {
                    lastParentRow.BringIntoView();
                }
            }

            return success;
        }

        #endregion

        /// <summary>
        /// 在结点被收缩起来时，应该它的子结点的所有数据在_objectItems中对应的项都移除。
        /// </summary>
        /// <param name="treeNode"></param>
        private void OnNodeCollapsed(TreeGridRow treeNode)
        {
            //处理 LazyLoading

            //结点被折叠后事件。
            //如果懒加载开启后，则在折叠时清空所有子结点。

            //不处理根结点。
            if (treeNode != this.RootNode)
            {
                //更新布局
                var itemKey = GetId(treeNode.DataContext);
                _renderExpansion.Remove(itemKey);

                if (this.IsLazyLoading && this.ClearCollapsedNodes && treeNode.Items.Count > 0)
                {
                    //deregisters listeners for all ancestors
                    if (this.ObserveChildItems)
                    {
                        _monitor.RemoveNodes(treeNode.Items);
                    }

                    treeNode.ItemsSource = null;
                    TreeGridHelper.CreateDummyItem(treeNode);

                    //TODO do we have collapsed event if all childs are being filtered?
                    //if yes, we need to set the dummy nodes visibility depending
                    //on the filtered childs (visible if at least 1 visible child)
                }
            }
        }

        private void OnNodeExpanded(TreeGridRow treeNode)
        {
            //处理 LazyLoading
            //结点被展开后事件
            //完成懒加载时的结点创建工作，并设置各结点的 Filter 属性。

            object item = treeNode.DataContext;
            if (item != null)
            {
                //布局
                var itemKey = GetId(item);
                _renderExpansion.Add(itemKey);

                var nodeItems = treeNode.Items;

                //懒加载状态下，创建结点。
                if (this.IsLazyLoading)
                {
                    TreeGridHelper.ClearDummyChildNode(treeNode);

                    if (nodeItems.Count == 0)
                    {
                        var childItems = this.GetChildItems(item);

                        treeNode.ItemsSource = childItems;

                        //刷新以排序
                        if (nodeItems.NeedsRefresh) { nodeItems.Refresh(); }
                    }
                }

                if (nodeItems.Count == 0)
                {
                    //如果结点没有数据，则不需要展开它。
                    treeNode.IsExpanded = false;
                    _renderExpansion.Remove(itemKey);
                }
            }
        }

        /// <summary>
        /// 折叠起那些不是当前选择行或其父节点的所有行。
        /// </summary>
        private void ApplyAutoCollapse()
        {
            if (!this.AutoCollapse) return;

            object selected = this.SelectedItem;
            var itemsControl = this.RootItemsControl;

            if (selected == null)
            {
                //if we don't have a selected item, just collapse the
                //root items
                foreach (TreeGridRow node in TreeGridHelper.TraverseRows(itemsControl))
                {
                    node.IsExpanded = false;
                }
            }
            else
            {
                var parents = this.GetParentItemList(selected);
                foreach (object parent in parents)
                {
                    var parentNode = this.FindChildRow(parent, itemsControl);
                    if (parentNode == null)
                    {
                        string msg = "Cannot collapse item '{0}' - the item does not exist in the hierarchy of the tree's bound items.";
                        msg = String.Format(msg, GetId(parent));
                        throw new InvalidOperationException(msg);
                    }

                    foreach (TreeGridRow item in TreeGridHelper.TraverseRows(itemsControl))
                    {
                        //collapse all items that are no ancestors
                        if (item == parentNode) continue;
                        item.IsExpanded = false;
                    }

                    //go a level deeper
                    itemsControl = parentNode;
                }

                //finally collapse the item and its siblings
                foreach (TreeGridRow item in TreeGridHelper.TraverseRows(itemsControl))
                {
                    item.IsExpanded = false;
                }
            }
        }

        /// <summary>
        /// 获取当前的布局
        /// 
        /// 本布局是当前显示的布局。
        /// </summary>
        private TreeGridExpandedItems GetCurrentExpandedItems()
        {
            var items = new TreeGridExpandedItems();

            //get nodes of all expanded nodes
            foreach (var row in this.RecursiveRows)
            {
                if (row.IsExpanded)
                {
                    items.Add(GetId(row.DataContext));
                }
            }

            return items;
        }
    }

    /// <summary>
    /// 封装树的布局展开的结点集合信息。
    /// </summary>
    internal class TreeGridExpandedItems
    {
        /// <summary>
        /// 已经展开的节点的 id 列表。
        /// </summary>
        private List<object> _expandedItemIds = new List<object>();

        /// <summary>
        /// 判断某个 id 对应的节点是否已经处于展开状态。
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool IsExpanded(object id)
        {
            return _expandedItemIds.Contains(id);
        }

        /// <summary>
        /// 添加一个展开节点的 Id。
        /// 
        /// 本方法不会添加重复项。
        /// </summary>
        /// <param name="id"></param>
        public bool Add(object id)
        {
            if (!_expandedItemIds.Contains(id))
            {
                _expandedItemIds.Add(id);
                return true;
            }

            return false;
        }

        public void Remove(object id)
        {
            _expandedItemIds.Remove(id);
        }

        public void Clear()
        {
            _expandedItemIds.Clear();
        }
    }
}