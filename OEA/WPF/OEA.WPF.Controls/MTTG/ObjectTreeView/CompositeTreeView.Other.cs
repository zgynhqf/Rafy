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
    partial class CompositeTreeView
    {
        #region 右键选择功能

        #region SelectNodesOnRightClick DependencyProperty

        public static readonly DependencyProperty SelectNodesOnRightClickProperty = DependencyProperty.Register(
            "SelectNodesOnRightClick", typeof(bool), typeof(CompositeTreeView),
            new FrameworkPropertyMetadata(false)
            );

        /// <summary>
        /// If set to true, treeview items are automatically selected on right clicks,
        /// which simplifies context menu handling.
        /// </summary>
        [Category("TreeGrid")]
        [Description("Whether right-clicked nodes should be selected or not.")]
        public bool SelectNodesOnRightClick
        {
            get { return (bool)GetValue(SelectNodesOnRightClickProperty); }
            set { SetValue(SelectNodesOnRightClickProperty, value); }
        }

        #endregion

        /// <summary>
        /// Intercepts right mouse button clicks an checks whether a tree
        /// node was clicked. If this is the case, the node will be selected
        /// in case it's not selected an the <see cref="SelectNodesOnRightClick"/>
        /// dependency property is set.<br/>
        /// If the <see cref="NodeContextMenu"/> property is set and no custom
        /// context menu was assigned to the item, the <see cref="NodeContextMenu"/>
        /// will be opened with its <see cref="ContextMenu.PlacementTarget"/> property
        /// set to the clicked tree node. Right clicks on a <see cref="RootNode"/>
        /// will be ignored.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
        {
            //return if no node was clicked
            var os = e.OriginalSource as DependencyObject;
            var item = os.GetVisualParent<TreeViewItem>();
            if (item == null) return;

            //activate item if necessary
            if (this.SelectNodesOnRightClick && !item.IsSelected)
            {
                item.IsSelected = true;
            }

            //context menu handling: don't do anything if no context menu
            //was defined or one was assigned by custom code
            if (this.NodeContextMenu == null || item.ContextMenu != null) return;

            //also don't show a context menu if the root node was clicked
            if (ReferenceEquals(item, RootNode)) return;

            //temporarily assign the menu to the item - this ensures that
            //a the PlacementTarget property of the context menu points to
            //the item (can be evaluated in a click event or command handler)
            item.ContextMenu = NodeContextMenu;

            //open the context menu for the clicked item
            this.NodeContextMenu.PlacementTarget = item;
            this.NodeContextMenu.IsOpen = true;

            //mark as handled - let the event bubble on...
            e.Handled = true;

            //reset the context menu assignment
            item.ContextMenu = null;
        }

        #endregion

        #region 布局

        /// <summary>
        /// 获取当前的布局
        /// 
        /// 本布局是当前显示的布局。
        /// </summary>
        public TreeLayout GetTreeLayout()
        {
            var layout = new TreeLayout();
            object selected = this.SelectedItem;

            //set selected item
            if (selected != null) layout.SelectedItemId = GetId(selected);

            //if there is no tree yet, we're done
            if (this.Tree != null)
            {
                //get nodes of all expanded nodes
                this.GetExpandedNodes(layout.ExpandedNodeIds, this.Tree.Items);
            }

            return layout;
        }

        /// <summary>
        /// Recursively determines all expanded nodes of the tree, and
        /// stores the qualified IDs of the underlying items in a list.
        /// </summary>
        /// <param name="nodeIds">The list to be populated.</param>
        /// <param name="nodes">The tree nodes to be processed recursively.</param>
        private void GetExpandedNodes(List<int> nodeIds, ItemCollection nodes)
        {
            foreach (TreeViewItem treeNode in nodes)
            {
                //if we're having a dummy node, break
                if (this.IsDummyNode(treeNode)) break;

                if (treeNode.IsExpanded)
                {
                    object item = GetEntity(treeNode);
                    if (item != null) nodeIds.Add(GetId(item));
                }

                //process recursively (always, even if the item is collapsed!)
                this.GetExpandedNodes(nodeIds, treeNode.Items);
            }
        }

        #endregion

        #region 其它方法

        /// <summary>
        /// Gets an enumerator that provides recursive browsing through
        /// all nodes of the tree. Note that this enumerator may not return
        /// nodes for all elements in the bound <see cref="Items"/> collection
        /// if lazy loading is enabled, but traverses the tree's existing
        /// nodes (<see cref="TreeViewItem"/> instances).<br/>
        /// </summary>
        public IEnumerable<TreeViewItem> RecursiveNodeList
        {
            get
            {
                ItemCollection nodes = this.RootControl.Items;
                return TreeUtil.TraverseNodes(nodes);
            }
        }

        /// <summary>
        /// 最上层的 ItemsControl，可能是 TreeView 或者 RootNode。
        /// </summary>
        protected ItemsControl RootControl
        {
            get
            {
                var rootNode = this.RootNode;
                return rootNode == null ? this.Tree : rootNode as ItemsControl;
            }
        }

        /// <summary>
        /// Validates whether a given tree node is a dummy that was
        /// included in the tree to have its parent node render an
        /// expander.
        /// </summary>
        /// <param name="treeNode">The node to be inspected.</param>
        /// <returns>True if the node does not seem to be a regular
        /// node but a dummy that should not be processed any further.</returns>
        protected virtual bool IsDummyNode(TreeViewItem treeNode)
        {
            //this default implementation checks two things
            //1: it's not the root node
            //2: the Header property is null
            return TreeUtil.IsDummyItem(treeNode) && treeNode != this.RootNode;
        }

        #endregion
    }
}
