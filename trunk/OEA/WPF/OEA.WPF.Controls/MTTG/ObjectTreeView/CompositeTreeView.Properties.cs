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
    /// 一些依赖属性的定义。
    /// </summary>
    [DefaultProperty("Items")]
    [DefaultEvent("SelectedItemChanged")]
    partial class CompositeTreeView
    {
        #region RootNode DependencyProperty

        public static readonly DependencyProperty RootNodeProperty = DependencyProperty.Register(
            "RootNode", typeof(TreeViewItem), typeof(CompositeTreeView),
            new FrameworkPropertyMetadata(null, (d, e) => (d as CompositeTreeView).OnRootNodeChanged(e))
            );

        /// <summary>
        /// 一个自定义根结点。
        /// 如果声明了该根结点，则 Items 中所有的结点都将作为该根结点了子结点，否则将会直接生成为 TreeView 的子结点。
        /// </summary>
        [Category("TreeGrid")]
        [Description("A virtual root node that displays the assigned items.")]
        public TreeViewItem RootNode
        {
            get { return (TreeViewItem)GetValue(RootNodeProperty); }
            set { SetValue(RootNodeProperty, value); }
        }

        private void OnRootNodeChanged(DependencyPropertyChangedEventArgs e)
        {
            if (!this.IsInitialized) return;

            this.Refresh(this.GetTreeLayout());

            //apply sorting and filtering on root or tree (if the new value is null)
            this.ApplySorting(e.NewValue as TreeViewItem, null);
        }

        #endregion

        #region IsLazyLoading DependencyProperty

        public static readonly DependencyProperty IsLazyLoadingProperty = DependencyProperty.Register(
            "IsLazyLoading", typeof(bool), typeof(CompositeTreeView),
            new FrameworkPropertyMetadata(true, (d, e) => (d as CompositeTreeView).OnIsLazyLoadingChanged(e))
            );

        /// <summary>
        /// Gets or sets whether tree nodes are being created on demand. If set to
        /// true (default value), nodes are being created as soon as they are going
        /// to be displayed the first time because their parent node is being expanded.
        /// </summary>
        [Category("TreeGrid")]
        [Description("Whether to delay creation of child nodes until a parent node is expanded.")]
        public bool IsLazyLoading
        {
            get { return (bool)GetValue(IsLazyLoadingProperty); }
            set { SetValue(IsLazyLoadingProperty, value); }
        }

        private void OnIsLazyLoadingChanged(DependencyPropertyChangedEventArgs e)
        {
            if (this.IsInitialized) this.Refresh();
        }

        #endregion

        #region ObserveRootItems DependencyProperty

        public static readonly DependencyProperty ObserveRootItemsProperty = DependencyProperty.Register(
            "ObserveRootItems", typeof(bool), typeof(CompositeTreeView),
            new FrameworkPropertyMetadata(true, (d, e) => (d as CompositeTreeView).OnObserveRootItemsChanged(e))
            );

        /// <summary>
        /// Whether the bound <see cref="Items"/> collection should be observed for changes of its contents. Defaults to true.
        /// </summary>
        /// <remarks>This property only control whether the collection's contents should be observed. A replacement of the
        /// <see cref="Items"/> collection itself always results in a refresh.</remarks>
        [Category("TreeGrid")]
        [Description("Whether to observe the bound Items collection for changed contents.")]
        public bool ObserveRootItems
        {
            get { return (bool)GetValue(ObserveRootItemsProperty); }
            set { SetValue(ObserveRootItemsProperty, value); }
        }

        protected virtual void OnObserveRootItemsChanged(DependencyPropertyChangedEventArgs e)
        {
            if (this.IsInitialized)
            {
                bool newValue = (bool)e.NewValue;
                if (newValue && this.Items != null)
                {
                    //if the tree was updated in order to start observing, the tree may already have
                    //changed - make a refresh
                    this.Refresh();
                }
            }
        }

        #endregion

        #region ObserveChildItems DependencyProperty

        public static readonly DependencyProperty ObserveChildItemsProperty = DependencyProperty.Register(
            "ObserveChildItems", typeof(bool), typeof(CompositeTreeView),
            new FrameworkPropertyMetadata(true, (d, e) => (d as CompositeTreeView).OnObserveChildItemsChanged(e))
            );

        /// <summary>
        /// If set to true, the control observes not only the directly bound
        /// <see cref="Items"/> collection, but also all rendered child
        /// collections for changes in order to reflect updates of the data
        /// source.<br/>
        /// If this property is set to false, you can always update the
        /// tree by invoking <see cref="Refresh()"/> or one of its overloads.
        /// This property also does not affect the behaviour of the main
        /// <see cref="Items"/> property. Changing <see cref="Items"/> always
        /// recreates and fully updates the tree.
        /// </summary>
        [Category("TreeGrid")]
        [Description("Whether to monitor child collections of rendered nodes for changes or not.")]
        public bool ObserveChildItems
        {
            get { return (bool)GetValue(ObserveChildItemsProperty); }
            set { SetValue(ObserveChildItemsProperty, value); }
        }

        private void OnObserveChildItemsChanged(DependencyPropertyChangedEventArgs e)
        {
            if (this.IsInitialized) this.Refresh();
        }

        #endregion

        #region TreeStyle DependencyProperty

        public static readonly DependencyProperty TreeStyleProperty = DependencyProperty.Register(
            "TreeStyle", typeof(Style), typeof(CompositeTreeView),
            new FrameworkPropertyMetadata(null, (d, e) => (d as CompositeTreeView).OnTreeStyleChanged(e))
            );

        /// <summary>
        /// The style to be attached to the control's <see cref="TreeProperty"/>.
        /// </summary>
        [Category("TreeGrid")]
        [Description("The style to be attached to the internal TreeView control.")]
        public Style TreeStyle
        {
            get { return (Style)GetValue(TreeStyleProperty); }
            set { SetValue(TreeStyleProperty, value); }
        }

        private void OnTreeStyleChanged(DependencyPropertyChangedEventArgs e)
        {
            var tree = this.Tree;
            var newValue = (Style)e.NewValue;
            if (tree != null && newValue != null) tree.Style = newValue;
        }

        #endregion

        #region TreeNodeStyle DependencyProperty

        /// <summary>
        /// A style which is explicitly applied to every tree node except
        /// the custom <see cref="RootNode"/>.
        /// </summary>
        public static readonly DependencyProperty TreeNodeStyleProperty = DependencyProperty.Register(
            "TreeNodeStyle", typeof(Style), typeof(CompositeTreeView),
            new FrameworkPropertyMetadata(null, (d, e) => (d as CompositeTreeView).OnTreeNodeStyleChanged(e))
            );

        /// <summary>
        /// A property wrapper for the <see cref="TreeNodeStyleProperty"/>
        /// dependency property:<br/>
        /// A style which is explicitly applied to every <see cref="TreeViewItem"/>
        /// except the custom <see cref="RootNode"/>.
        /// </summary>
        [Category("TreeGrid")]
        [Description("The style to be assigned to the tree's item nodes.")]
        public Style TreeNodeStyle
        {
            get { return (Style)GetValue(TreeNodeStyleProperty); }
            set { SetValue(TreeNodeStyleProperty, value); }
        }

        private void OnTreeNodeStyleChanged(DependencyPropertyChangedEventArgs e)
        {
            //don't do anything if the control is being created
            if (!this.IsInitialized || this.Tree == null) return;

            //assign the style to every node of the tree
            foreach (TreeViewItem node in this.RecursiveNodeList)
            {
                this.ApplyNodeStyle(node, GetEntity(node));
            }
        }

        #endregion

        #region NodeContextMenu DependencyProperty

        public static readonly DependencyProperty NodeContextMenuProperty = DependencyProperty.Register(
            "NodeContextMenu", typeof(ContextMenu), typeof(CompositeTreeView),
            new FrameworkPropertyMetadata(null)
            );

        /// <summary>
        /// Defines a context menu to be assigned to open on all nodes of
        /// the tree. If this property is set, the context menu will be
        /// displayed if a node of the tree is being right-clicked, *and*
        /// no custom context menu has been assigned to the node.<br/>
        /// When handling menu-related events, the clicked node that
        /// caused the event can be determined by accessing the menu's
        /// <see cref="ContextMenu.PlacementTarget"/> property.
        /// </summary>
        [Category("TreeGrid")]
        [Description("A custom context menu which is displayed for all nodes.")]
        public ContextMenu NodeContextMenu
        {
            get { return (ContextMenu)GetValue(NodeContextMenuProperty); }
            set { SetValue(NodeContextMenuProperty, value); }
        }

        #endregion

        #region AutoCollapse DependencyProperty

        public static readonly DependencyProperty AutoCollapseProperty = DependencyProperty.Register(
            "AutoCollapse", typeof(bool), typeof(CompositeTreeView),
            new FrameworkPropertyMetadata(false, (d, e) => (d as CompositeTreeView).OnAutoCollapseChanged(e))
            );

        /// <summary>
        /// 如果设置为 true，则不是 <see cref="SelectedItem"/> 的父结点都会被折叠起来。
        /// </summary>
        [Category("TreeGrid")]
        [Description("Collapses all nodes that are not needed to show the selected item.")]
        public bool AutoCollapse
        {
            get { return (bool)GetValue(AutoCollapseProperty); }
            set { SetValue(AutoCollapseProperty, value); }
        }

        private void OnAutoCollapseChanged(DependencyPropertyChangedEventArgs e)
        {
            if (this.IsInitialized)
            {
                bool newValue = (bool)e.NewValue;
                //only react if the value is being set to true
                if (newValue) { this.ApplyAutoCollapse(); }
            }
        }

        #endregion
    }
}