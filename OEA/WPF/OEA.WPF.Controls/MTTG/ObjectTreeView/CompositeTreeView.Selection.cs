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
    /// 树结点的选择事件处理
    /// 
    /// 基类的此方法以一种防止 TreeView 重入的方式来同步 this.SelectedItem 到 TreeView.SelecteItem 上。
    /// 并在最后调用 this.OnSelectedItemChanged 方法，以抛出路由事件。
    /// 
    /// 注意这三个方法的顺序：
    /// OnTreeViewSelectedItemChanged
    /// OnSelectedItemPropertyChanged
    /// RaiseSelectedItemChanged
    /// </summary>
    partial class CompositeTreeView
    {
        #region SelectedItem DependencyProperty

        /// <summary>
        /// Gets or sets the currently selected item, if any. Setting a null
        /// reference deselects the currently selected node.
        /// </summary>
        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
            "SelectedItem", typeof(object), typeof(CompositeTreeView),
            new FrameworkPropertyMetadata(null, (o, e) => (o as CompositeTreeView).OnSelectedItemPropertyChanged(e))
            );

        /// <summary>
        /// A property wrapper for the <see cref="SelectedItemProperty"/>
        /// dependency property:<br/>
        /// Gets or sets the currently selected item, if any. Setting a null
        /// reference deselects the currently selected node.
        /// </summary>
        [Category("TreeGrid")]
        [Description("The tree's currently selected item, if any.")]
        public object SelectedItem
        {
            get { return (object)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        #endregion

        #region SelectedItemChanged event

        public static RoutedEvent SelectedItemChangedEvent = EventManager.RegisterRoutedEvent(
            "SelectedItemChanged", RoutingStrategy.Bubble, typeof(EventHandler<SelectedItemChangedEventArgs>), typeof(CompositeTreeView)
            );

        /// <summary>
        /// 选择项变更路由事件。
        /// 当 <see cref="SelectedItem"/> 属性被变更时，此事件会被触发。
        /// </summary>
        [Category("TreeGrid")]
        [Description("Raised if the SelectedItem property is changed either programmatically or through the user interface.")]
        public event EventHandler<SelectedItemChangedEventArgs> SelectedItemChanged
        {
            add { AddHandler(SelectedItemChangedEvent, value); }
            remove { RemoveHandler(SelectedItemChangedEvent, value); }
        }

        #endregion

        /// <summary>
        /// 是否忽略 TreeView 的 SelectionChanged 事件。（在重建树时使用。）
        /// </summary>
        private bool _ignoreTreeViewEvents = false;

        /// <summary>
        /// 重置当前选项。
        /// 直接选中 <see cref="RootNode"/> 或者直接清空所有选项。
        /// </summary>
        protected virtual void ResetNodeSelection()
        {
            //none of the Items should be selected - but we'll select a root node
            //if available
            if (this.RootNode != null)
            {
                this.RootNode.IsSelected = true;
            }
            else
            {
                var current = this.Tree.SelectedItem as TreeViewItem;
                //if we have no root to select, clear the current selection
                if (current != null) current.IsSelected = false;
            }
        }

        /// <summary>
        /// TreeView.SelectedItemChanged 事件的静态处理函数。
        /// </summary>
        private static void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var tree = sender as CompositeTreeView;
            if (!tree._ignoreTreeViewEvents)
            {
                tree.OnTreeViewSelectedItemChanged(e);
            }

            //直接处理掉本函数，停止继续冒泡。
            e.Handled = true;
        }

        /// <summary>
        /// 整个选择的源头：TreeView 的 SelectedItemChanged 事件。
        /// 当树型控件被点击时，使用选中项同步本对象的 <see cref="SelectedItem"/> 属性。
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnTreeViewSelectedItemChanged(RoutedPropertyChangedEventArgs<object> e)
        {
            var node = e.NewValue as TreeViewItem;
            if (node != null)
            {
                this.SelectedItem = GetEntity(node);
            }
            else
            {
                this.SelectedItem = null;
            }
        }

        /// <summary>
        /// 在 SelectedItem 属性变更时，同步 TreeView 控件的选中项。
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnSelectedItemPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            var tree = this.Tree;
            if (!this.IsInitialized || tree == null) return;

            //展开根结点
            if (this.RootNode != null) this.RootNode.IsExpanded = true;

            try
            {
                this._ignoreTreeViewEvents = true;

                var newItem = e.NewValue;

                #region 选中结点

                if (newItem == null)
                {
                    this.ResetNodeSelection();
                }
                else
                {
                    var selectedItem = this.Tree.SelectedItem as TreeViewItem;
                    //该项已经被处于选中状态，那么直接显示即可。
                    if (selectedItem != null && GetEntity(selectedItem) == newItem)
                    {
                        selectedItem.BringIntoView();
                    }
                    //没有被选中时，直接把该结点选中即可。
                    else
                    {
                        try
                        {
                            //保证指定结点及其父结点都已经被创建并展开后，直接选中该结点。
                            selectedItem = this.EnsureNodeIsVisible(newItem);
                            selectedItem.IsSelected = true;
                            selectedItem.BringIntoView();
                        }
                        catch
                        {
                            //发生异常，重设选项后再抛出。
                            this.SelectedItem = null;
                            throw;
                        }
                    }
                }

                #endregion

                #region 更新布局

                var itemKey = newItem == null ? 0 : GetId(newItem);
                this._destinationLayout.SelectedItemId = itemKey;

                //收缩所有其它的结点
                this.ApplyAutoCollapse();

                #endregion

                #region 触发路由事件

                object oldItem = (object)e.OldValue;
                if (oldItem != newItem)
                {
                    this.RaiseSelectedItemChanged(new SelectedItemChangedEventArgs(this)
                    {
                        OldItem = oldItem,
                        NewItem = newItem
                    });
                }

                #endregion
            }
            finally
            {
                this._ignoreTreeViewEvents = false;
            }
        }

        /// <summary>
        /// 子类重写此方法实现选择项变更逻辑处理。
        /// </summary>
        /// <param name="e"></param>
        protected virtual void RaiseSelectedItemChanged(SelectedItemChangedEventArgs e)
        {
            this.RaiseEvent(e);
        }

        /// <summary>
        /// Expands all nodes that represent ancestors of a given item
        /// in order to make sure the node that represents
        /// <paramref name="dataItem"/> is visible (and thus created as well).
        /// </summary>
        /// <param name="dataItem">The item to make available.</param>
        /// <returns>The tree node that represent <paramref name="dataItem"/>.</returns>
        /// <exception cref="InvalidOperationException">If the item's
        /// ancestor list does not lead back to the root items because
        /// the item does not belong to the tree, or the tree's rendered
        /// nodes and bound data are out of sync.</exception>
        protected virtual TreeViewItem EnsureNodeIsVisible(object dataItem)
        {
            var itemKey = GetId(dataItem);

            //找到所有父结点并展开，这样传入的 item 对应的结点才算可见。
            var parentList = this.GetParentItemList(dataItem);

            var items = this.RootControl.Items;
            foreach (object parent in parentList)
            {
                var parentKey = GetId(parent);

                //找到并展开父结点
                var parentNode = this.TryFindItemNode(items, parentKey, false);
                if (parentNode == null) { throw new InvalidOperationException(string.Format("结点 {0} 及它的父结点 {1} 不在树结构内. 数据和 UI 结点已经不同步。", itemKey, parentKey)); }
                parentNode.IsExpanded = true;

                //继续下一层结点
                items = parentNode.Items;
            }

            //所有父已经展开，则选中该结点。
            var itemNode = this.TryFindItemNode(items, itemKey, false);
            if (itemNode == null)
            {
                //this could be the case if we received a top level item, but the
                //tree's Items collection does not contain it.
                throw new InvalidOperationException(string.Format("无法选中结点 '{0}' - 它并不在树的结构内。", itemKey));
            }

            return itemNode;
        }
    }
}
