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
    /// 界面渲染、界面元素构造相关方法
    /// </summary>
    partial class CompositeTreeView
    {
        /// <summary>
        /// 是否树型控件已经被渲染
        /// </summary>
        private bool _isTreeRendered = false;

        /// <summary>
        /// 保证树已经呈现。
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            //如果需要，创建一个默认的树控件，并第一次渲染它。
            if (!this._isTreeRendered)
            {
                if (this.Tree == null)
                {
                    //设置默认的树控件，同时也会触发渲染过程。
                    this.Tree = this.CreateTreeView();
                }
                else
                {
                    this.ApplySorting();

                    //everything has been created, but the tree has not
                    //been rendered yet, because IsInitialized was false
                    //-> render now
                    this.Refresh(null);
                }
            }
        }

        #region 渲染、创建

        /// <summary>
        /// 使用当前布局重新刷新整棵树。
        /// </summary>
        public void Refresh()
        {
            this.RenderTree(this.GetTreeLayout());
        }

        /// <summary>
        /// 使用指定的布局重新刷新整棵树。
        /// </summary>
        /// <param name="layout">.</param>
        public void Refresh(TreeLayout layout)
        {
            this.RenderTree(layout);
        }

        /// <summary>
        /// 渲染整个树，并显示为指定的布局。
        /// </summary>
        /// <param name="layout">树型控件的布局。如果传入 null，表示简单渲染，不需要布局。</param>
        protected virtual void RenderTree(TreeLayout layout)
        {
            var tree = this.Tree;
            if (tree == null) return;

            this._monitor.Clear();

            try
            {
                //在清空树时，阻止监听树的事件。
                this._ignoreTreeViewEvents = true;

                #region 如果有自定义根结点，则清空并展开它。

                var root = this.RootNode;
                if (root != null)
                {
                    root.IsExpanded = true;
                    tree.Items.Add(root);
                }

                #endregion

                #region 创建树的根结点列表
                //根据 layout，如果结点处于展开状态，则递归生成子结点。

                var rootList = new List<TreeViewItem>();
                var items = this.Items;
                if (items != null)
                {
                    foreach (object item in items)
                    {
                        this.CreateItemNode(item, rootList, layout);
                    }
                }

                #endregion

                #region 把根结点列表直接绑定到 ItemsSource 属性上。

                var rootControl = this.RootControl;

                //rootItem 的数据使用 ItemsSource 来进行生成，这样，就可以使用 GroupDescriptions 来实现分组功能。
                rootControl.ItemsSource = rootList;

                //为了应用排序、过滤，需要保证根结点视图已经被刷新。
                rootControl.Items.Refresh();

                #endregion

                #region 设置选择项

                TreeViewItem selectedNode = null;
                if (layout != null)
                {
                    var itemId = layout.SelectedItemId;
                    selectedNode = this.TryFindItemNode(tree.Items, itemId, true);
                }

                if (selectedNode == null)
                {
                    this.SelectedItem = null;
                }
                else
                {
                    this.SelectedItem = GetEntity(selectedNode);
                }

                #endregion

                #region 设置焦点

                if (selectedNode != null && tree.IsKeyboardFocusWithin)
                {
                    //if the tree has the focus, it will automatically select the root node once
                    //the tree is rendered - prevent this by explicitely setting the focus
                    //to root, than to the selected item
                    //-> the order of *both* selects is needed, depending on the currently selected item
                    //(direct child of root or not makes a difference)
                    if (root != null) Keyboard.Focus(root);
                    Keyboard.Focus(selectedNode);
                }

                #endregion
            }
            finally
            {
                this._ignoreTreeViewEvents = false;
            }

            //保存或者创建一个新的目标布局
            this._destinationLayout = layout ?? new TreeLayout();
            this._isTreeRendered = true;
        }

        /// <summary>
        /// 为指定的数据使用指定的布局创建一个结点，并加入到指定的父列表对象中。
        /// </summary>
        /// <param name="item"></param>
        /// <param name="parentNodes">包含这个结点的父结点列表</param>
        /// <param name="layout">
        /// 在生成过程中，根据指定的这个布局，设置展开状态，并可能递归生成子结点。
        /// 如果是 null，则表示无须任何布局。
        /// </param>
        protected internal void CreateItemNode(object item, IList parentNodes, TreeLayout layout)
        {
            #region 创建结点

            var treeNode = this.CreateItem(item);
            SetEntity(treeNode, item);
            this.ApplyNodeStyle(treeNode, item);

            #endregion

            #region 计算该结点的状态、数据

            var itemKey = GetId(item);
            bool isExpanded = layout != null && layout.IsNodeExpanded(itemKey);
            bool renderChilds = isExpanded || !this.IsLazyLoading;
            bool hasChilds;

            //only invoke GetChildItems directly if we *need* the
            //collection. This is the case if the node is expanded or
            //collection monitoring is active. Otherwise, HasChildItems
            //might be more efficient
            ICollection<object> childItems = null;
            if (renderChilds || this.ObserveChildItems)
            {
                childItems = this.GetChildItems(item);
                hasChilds = childItems.Count > 0;
            }
            else
            {
                //invoke the potentially cheaper operation
                hasChilds = this.HasChildItems(item);
            }

            #endregion

            #region 处理 Expansion、懒加载创建子结点

            if (renderChilds)
            {
                //render childs if the node is expanded according to the
                //layout information, or if lazy loading is not active
                foreach (object childItem in childItems)
                {
                    this.CreateItemNode(childItem, treeNode.Items, layout);
                }

                if (isExpanded) treeNode.IsExpanded = true;
            }
            else if (hasChilds && treeNode.Items.Count == 0)
            {
                //if the item has child nodes which we don't need to create right
                //now (not expanded and lazy loading is active), insert
                //a dummy node which results in an expansion indicator
                treeNode.Items.Add(this.CreateDummyItem());
            }

            #endregion

            #region 处理 Selection

            if (layout != null && itemKey.Equals(layout.SelectedItemId) || item == SelectedItem)
            {
                //select the item and notify
                this.SelectedRowOnCreated(treeNode);
            }

            #endregion

            #region 过滤、排序

            //finally, if we should monitor the child collection, register it
            if (this.ObserveChildItems) this._monitor.RegisterItem(itemKey, childItems);

            //filter / sort node contents
            this.ApplyFilter(treeNode, item);
            this.ApplySorting(treeNode, item);

            #endregion

            #region 加入父列表

            if (treeNode.Parent is HeaderedItemsControl)
            {
                (treeNode.Parent as HeaderedItemsControl).Items.Remove(treeNode);
            }
            parentNodes.Add(treeNode);

            #endregion
        }

        /// <summary>
        /// 在创建时选中某行，由于此时并不会发生 TreeView 的 SelecteItemChanged 事件，
        /// 所以子类可以重写此方法来执行特定的逻辑。
        /// </summary>
        /// <param name="row"></param>
        internal virtual void SelectedRowOnCreated(TreeViewItem row)
        {
            row.IsSelected = true;
        }

        protected internal TreeViewItem CreateDummyItem()
        {
            return this.CreateItem(null);
        }

        /// <summary>
        /// Creates an empty <see cref="TreeViewItem"/>
        /// which will represent a given item. The default
        /// method just returns an empty <see cref="TreeViewItem"/>
        /// instance. Override it in order to further customize
        /// the item, or return a custom class that derives from
        /// <see cref="TreeViewItem"/>.
        /// </summary>
        /// <param name="item">The item which will be represented
        /// by the returned <see cref="TreeViewItem"/>.</param>
        /// <returns>A <see cref="TreeViewItem"/> which will represent
        /// the submitted <paramref name="item"/>.</returns>
        protected virtual TreeViewItem CreateItem(object item)
        {
            return new TreeViewItem();
        }

        /// <summary>
        /// 子类重写此方法生成一个树型控件。
        /// </summary>
        /// <returns></returns>
        protected virtual TreeView CreateTreeView()
        {
            return new TreeView();
        }

        /// <summary>
        /// 为指定的结点应用 <see cref="TreeNodeStyle"/> 样式。
        /// 
        /// 子类重写此方法用于应用自定义样式。
        /// </summary>
        /// <param name="treeNode">The node to be styled.</param>
        /// <param name="item">The bound item that is represented by the <paramref name="treeNode"/> .</param>
        protected virtual void ApplyNodeStyle(TreeViewItem treeNode, object item)
        {
            var style = this.TreeNodeStyle;
            if (style != null) treeNode.Style = style;
        }

        #endregion

        #region Header 约定

        /// <summary>
        /// 获取指定行所对应的实体对象
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected static object GetEntity(TreeViewItem item)
        {
            return item.Header;
        }

        protected static void SetEntity(TreeViewItem item, object entity)
        {
            item.Header = entity;
            item.DataContext = entity;//赋值DataContext，否则TreeGridColumn生成的控件绑定有问题
        }

        /// <summary>
        /// 由于本控件的设计方案为生成 TreeViewItem，然后添加到 Items(ItemCollection) 集合中，
        /// 所以模型的属性不能直接被绑定到 TreeViewItem 上，而是需要通过此方法进行转换。
        /// </summary>
        /// <param name="modelProperty"></param>
        /// <returns></returns>
        protected static string BindableEntityProperty(string modelProperty)
        {
            return "Header." + modelProperty;
        }

        #endregion
    }
}
