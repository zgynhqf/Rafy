/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110621
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：2.1.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100621
 * 2.0.0 和 SelectionDataGrid 控件整合，并支持过滤、排序、分组、CheckingMode等。 胡庆访 20111121
 *      MultiObjectTreeView 重命名为 MultiTypesTreeGrid。 胡庆访 20111122
 * 2.1.0 支持 UIV。 胡庆访 20111213
 * 2.2.0 抽取为独立的模块，与 Rafy 元数据解耦。 胡庆访 20120820
 * 2.3.0 TreeGrid 类与 GridTreeView 类型合并，直接从 TreeView继承。 胡庆访 20120926
 * 2.4.0 TreeGrid 类与 CompositeTreeView 类型合并。 胡庆访 20120927
 * 2.5.0 TreeGrid 使用 ItemContainerGenerator 对应的模式来实现行的生成。 胡庆访 20121005
 * 2.6.0 TreeGrid 不再依赖 GridView，MS 所有代码全部拷出。 胡庆访 20121011
 * 2.7.0 TreeGrid 支持列虚拟化。 胡庆访 20121017
 * 
*******************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Rafy.WPF.Controls;

namespace Rafy.WPF.Controls
{
    /// <summary>
    /// 树型的列表编辑控件。
    /// 
    /// 本控件整合了两个开源控件（二者相互间没有关系，互不依赖！）：
    /// GridTreeView 作为底层用于 TreeView 显示表格的控件。
    /// ObjectTreeView 则是可以绑定任意对象的、有方便 API 的 TreeView 控件。
    /// 链接：
    /// * http://www.codeproject.com/KB/WPF/versatile_treeview.aspx
    ///     CodeProject A Versatile TreeView for WPF_ Free source code and programming help
    /// * http://blogs.msdn.com/b/atc_avalon_team/archive/2006/03/01/541206.aspx
    ///     GridTreeView: Show Hierarchy Data with Details in Columns
    /// 
    /// 本控件还支持以下功能：
    /// * 多类型合并显示。
    ///     不同对象生成的列根据类型属性名称来对应。
    /// * 过滤、排序。
    /// * 根对象分组。
    /// * CheckingMode。
    /// * 行号。
    /// * 使用 RootPId 过滤根对象。
    /// * 表格模式下的 UI Virtualization
    /// * 树的子节点的 Data Virtualizaiton。（子类可重写 HasChildItems、GetChildItems 两个方法，默认没有使用。）
    /// </summary>
    public abstract partial class TreeGrid : ItemsControl, IWeakEventListener
    {
        static TreeGrid()
        {
            //Property
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TreeGrid), new FrameworkPropertyMetadata(typeof(TreeGrid)));
            KeyboardNavigation.ControlTabNavigationProperty.OverrideMetadata(typeof(TreeGrid), new FrameworkPropertyMetadata(KeyboardNavigationMode.Contained));
            VirtualizingStackPanel.IsVirtualizingProperty.OverrideMetadata(typeof(TreeGrid), new FrameworkPropertyMetadata(BooleanBoxes.False));

            //Commands
            CommandManager.RegisterClassCommandBinding(typeof(TreeGrid), new CommandBinding(CommitEditCommand, (o, e) => (o as TreeGrid).OnCommitEditCommandExecuted(e)));

            //Event
            EventManager.RegisterClassHandler(typeof(TreeGrid), TreeGridColumnHeader.ClickEvent, new RoutedEventHandler((sender, e) => (sender as TreeGrid).OnColumnHeaderClick(e)));
            EventManager.RegisterClassHandler(typeof(TreeGrid), TreeGridRow.ExpandedEvent, new RoutedEventHandler(TreeGridRow_Expanded));
            EventManager.RegisterClassHandler(typeof(TreeGrid), TreeGridRow.CollapsedEvent, new RoutedEventHandler(TreeGridRow_Collapsed));
            //EventManager.RegisterClassHandler(typeof(TreeGrid), TreeViewItem.SelectedEvent, new RoutedEventHandler(TreeGridRow_Selected));
            //EventManager.RegisterClassHandler(typeof(TreeGrid), TreeViewItem.UnselectedEvent, new RoutedEventHandler(TreeGridRow_Unselected));
        }

        public TreeGrid()
        {
            this._monitor = new ItemMonitor(this);

            this.Columns = new TreeGridColumnCollection();

            this.OnSelectionConstruct();
        }

        #region 路由命令

        /// <summary>
        /// 表格内某个元素可使用此命令提交当前的编辑状态。
        /// </summary>
        public static readonly RoutedCommand CommitEditCommand = new RoutedCommand("CommitEdit", typeof(TreeGrid));

        #endregion

        #region 静态处理函数

        private static void TreeGridRow_Expanded(object sender, RoutedEventArgs e)
        {
            var row = e.OriginalSource as TreeGridRow;
            if (!TreeGridHelper.IsDummyItem(row)) { (sender as TreeGrid).OnNodeExpanded(row); }
        }

        private static void TreeGridRow_Collapsed(object sender, RoutedEventArgs e)
        {
            var row = e.OriginalSource as TreeGridRow;
            if (!TreeGridHelper.IsDummyItem(row)) { (sender as TreeGrid).OnNodeCollapsed(row); }
        }

        //private static void TreeGridRow_Selected(object sender, RoutedEventArgs e)
        //{
        //    var row = e.OriginalSource as TreeGridRow;
        //    if (!TreeUtil.IsDummyItem(row)) { (sender as TreeGrid).ResponseRowSelected(row); }
        //}

        //private static void TreeGridRow_Unselected(object sender, RoutedEventArgs e)
        //{
        //    var row = e.OriginalSource as TreeGridRow;
        //    if (!TreeUtil.IsDummyItem(row)) { (sender as TreeGrid).ResponseRowUnselected(row); }
        //}

        #endregion

        #region 样式属性

        #region NoDataString DependencyProperty

        public static readonly DependencyProperty NoDataTextProperty = DependencyProperty.Register(
            "NoDataText", typeof(string), typeof(TreeGrid),
            new PropertyMetadata("No Data")
            );

        /// <summary>
        /// 没有数据时显示的文本。
        /// </summary>
        public string NoDataText
        {
            get { return (string)this.GetValue(NoDataTextProperty); }
            set { this.SetValue(NoDataTextProperty, value); }
        }

        #endregion

        #region AlternatingRowBackground DependencyProperty

        public static readonly DependencyProperty AlternatingRowBackgroundProperty = DependencyProperty.Register(
            "AlternatingRowBackground", typeof(Brush), typeof(TreeGrid)
            );

        /// <summary>
        /// 偶数行的背景色。
        /// 
        /// 注意，此属性只在 GridMode 下可用。
        /// </summary>
        public Brush AlternatingRowBackground
        {
            get { return (Brush)this.GetValue(AlternatingRowBackgroundProperty); }
            set { this.SetValue(AlternatingRowBackgroundProperty, value); }
        }

        #endregion

        #region RowStyle DependencyProperty

        public static readonly DependencyProperty RowStyleProperty = DependencyProperty.Register(
            "RowStyle", typeof(Style), typeof(TreeGrid),
            new FrameworkPropertyMetadata(null, (d, e) => (d as TreeGrid).OnTreeNodeStyleChanged(e))
            );

        /// <summary>
        /// 所有行的样式。
        /// </summary>
        public Style RowStyle
        {
            get { return (Style)GetValue(RowStyleProperty); }
            set { SetValue(RowStyleProperty, value); }
        }

        private void OnTreeNodeStyleChanged(DependencyPropertyChangedEventArgs e)
        {
            //don't do anything if the control is being created
            if (!this.IsInitialized) return;

            //assign the style to every node of the tree
            foreach (TreeGridRow node in this.RecursiveRows)
            {
                this.ApplyNodeStyle(node);
            }
        }

        #endregion

        #region AllowsColumnReorder DependencyProperty

        public static readonly DependencyProperty AllowsColumnReorderProperty = DependencyProperty.Register(
            "AllowsColumnReorder", typeof(bool), typeof(TreeGrid),
            new FrameworkPropertyMetadata(BooleanBoxes.True)
            );

        public bool AllowsColumnReorder
        {
            get { return (bool)this.GetValue(AllowsColumnReorderProperty); }
            set { this.SetValue(AllowsColumnReorderProperty, value); }
        }

        #endregion

        #region ColumnHeaderContainerStyle DependencyProperty

        public static readonly DependencyProperty ColumnHeaderContainerStyleProperty = DependencyProperty.Register(
            "ColumnHeaderContainerStyle", typeof(Style), typeof(TreeGrid)
            );

        public Style ColumnHeaderContainerStyle
        {
            get { return (Style)this.GetValue(ColumnHeaderContainerStyleProperty); }
            set { this.SetValue(ColumnHeaderContainerStyleProperty, value); }
        }

        #endregion

        #region ColumnHeaderContextMenu DependencyProperty

        public static readonly DependencyProperty ColumnHeaderContextMenuProperty = DependencyProperty.Register(
            "ColumnHeaderContextMenu", typeof(ContextMenu), typeof(TreeGrid)
            );

        public ContextMenu ColumnHeaderContextMenu
        {
            get { return (ContextMenu)this.GetValue(ColumnHeaderContextMenuProperty); }
            set { this.SetValue(ColumnHeaderContextMenuProperty, value); }
        }

        #endregion

        #region ColumnHeaderStringFormat DependencyProperty

        public static readonly DependencyProperty ColumnHeaderStringFormatProperty = DependencyProperty.Register(
            "ColumnHeaderStringFormat", typeof(string), typeof(TreeGrid)
            );

        public string ColumnHeaderStringFormat
        {
            get { return (string)this.GetValue(ColumnHeaderStringFormatProperty); }
            set { this.SetValue(ColumnHeaderStringFormatProperty, value); }
        }

        #endregion

        #region ColumnHeaderTemplate DependencyProperty

        public static readonly DependencyProperty ColumnHeaderTemplateProperty = DependencyProperty.Register(
            "ColumnHeaderTemplate", typeof(DataTemplate), typeof(TreeGrid),
            new FrameworkPropertyMetadata((d, e) => (d as TreeGrid).OnColumnHeaderTemplateChanged(e))
            );

        public DataTemplate ColumnHeaderTemplate
        {
            get { return (DataTemplate)this.GetValue(ColumnHeaderTemplateProperty); }
            set { this.SetValue(ColumnHeaderTemplateProperty, value); }
        }

        private void OnColumnHeaderTemplateChanged(DependencyPropertyChangedEventArgs e)
        {
            TreeGridHelper.CheckTemplateAndTemplateSelector("GridViewColumnHeader", GridView.ColumnHeaderTemplateProperty, GridView.ColumnHeaderTemplateSelectorProperty, this);
        }

        #endregion

        #region ColumnHeaderTemplateSelector DependencyProperty

        public static readonly DependencyProperty ColumnHeaderTemplateSelectorProperty = DependencyProperty.Register(
            "ColumnHeaderTemplateSelector", typeof(DataTemplateSelector), typeof(TreeGrid),
            new FrameworkPropertyMetadata((d, e) => (d as TreeGrid).OnColumnHeaderTemplateSelectorChanged(e))
            );

        public DataTemplateSelector ColumnHeaderTemplateSelector
        {
            get { return (DataTemplateSelector)this.GetValue(ColumnHeaderTemplateSelectorProperty); }
            set { this.SetValue(ColumnHeaderTemplateSelectorProperty, value); }
        }

        private void OnColumnHeaderTemplateSelectorChanged(DependencyPropertyChangedEventArgs e)
        {
            TreeGridHelper.CheckTemplateAndTemplateSelector("GridViewColumnHeader", GridView.ColumnHeaderTemplateProperty, GridView.ColumnHeaderTemplateSelectorProperty, this);
        }

        #endregion

        #region ColumnHeaderToolTip DependencyProperty

        public static readonly DependencyProperty ColumnHeaderToolTipProperty = DependencyProperty.Register(
            "ColumnHeaderToolTip", typeof(object), typeof(TreeGrid)
            );

        public object ColumnHeaderToolTip
        {
            get { return (object)this.GetValue(ColumnHeaderToolTipProperty); }
            set { this.SetValue(ColumnHeaderToolTipProperty, value); }
        }

        #endregion

        #endregion

        #region 其它属性

        #region RootNode DependencyProperty

        public static readonly DependencyProperty RootNodeProperty = DependencyProperty.Register(
            "RootNode", typeof(TreeGridRow), typeof(TreeGrid),
            new FrameworkPropertyMetadata(null, (d, e) => (d as TreeGrid).OnRootNodeChanged(e))
            );

        /// <summary>
        /// 一个自定义根结点。
        /// 如果声明了该根结点，则 Items 中所有的结点都将作为该根结点了子结点，否则将会直接生成为 TreeView 的子结点。
        /// </summary>
        public TreeGridRow RootNode
        {
            get { return (TreeGridRow)GetValue(RootNodeProperty); }
            set { SetValue(RootNodeProperty, value); }
        }

        private void OnRootNodeChanged(DependencyPropertyChangedEventArgs e)
        {
            if (this.IsInitialized)
            {
                this.Render();

                //apply sorting and filtering on root or tree (if the new value is null)
                this.ApplySorting(e.NewValue as TreeGridRow);
            }
        }

        #endregion

        #region IsLazyLoading DependencyProperty

        public static readonly DependencyProperty IsLazyLoadingProperty = DependencyProperty.Register(
            "IsLazyLoading", typeof(bool), typeof(TreeGrid),
            new FrameworkPropertyMetadata(true, (d, e) => (d as TreeGrid).OnIsLazyLoadingChanged(e))
            );

        /// <summary>
        /// 获取或设置值表示是否树节点将在需要时才被创建。
        /// 如果被设置为 true（默认值），那么节点会在其父节点被展开后、它即将被显示之前被创建。
        /// </summary>
        public bool IsLazyLoading
        {
            get { return (bool)GetValue(IsLazyLoadingProperty); }
            set { SetValue(IsLazyLoadingProperty, value); }
        }

        private void OnIsLazyLoadingChanged(DependencyPropertyChangedEventArgs e)
        {
            if (this.IsInitialized) this.Render();
        }

        #endregion

        #region ObserveRootItems DependencyProperty

        public static readonly DependencyProperty ObserveRootItemsProperty = DependencyProperty.Register(
            "ObserveRootItems", typeof(bool), typeof(TreeGrid),
            new FrameworkPropertyMetadata(true, (d, e) => (d as TreeGrid).OnObserveRootItemsChanged(e))
            );

        /// <summary>
        /// Whether the bound <see cref="ItemsSource"/> collection should be observed for changes of its contents. Defaults to true.
        /// </summary>
        /// <remarks>This property only control whether the collection's contents should be observed. A replacement of the
        /// <see cref="ItemsSource"/> collection itself always results in a refresh.</remarks>
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
                if (newValue && this.ItemsSource != null)
                {
                    //if the tree was updated in order to start observing, the tree may already have
                    //changed - make a refresh
                    this.Render();
                }
            }
        }

        #endregion

        #region ObserveChildItems DependencyProperty

        /// <summary>
        /// 此属性如果为真，GetChildItems 方法返回值必须实现 INotifyCollectionChanged 接口。
        /// 目前，object.ChildrenNodes 无法进行监听，所以我们选择不实现自动模式，而是直接调用 Refresh 接口。
        /// 相关 API 定义，请查看 ObserveChildItems 属性文档。
        /// </summary>
        public static readonly DependencyProperty ObserveChildItemsProperty = DependencyProperty.Register(
            "ObserveChildItems", typeof(bool), typeof(TreeGrid),
            new FrameworkPropertyMetadata(false, (d, e) => (d as TreeGrid).OnObserveChildItemsChanged(e))
            );

        /// <summary>
        /// If set to true, the control observes not only the directly bound
        /// <see cref="ItemsSource"/> collection, but also all rendered child
        /// collections for changes in order to reflect updates of the data
        /// source.<br/>
        /// If this property is set to false, you can always update the
        /// tree by invoking <see cref="Render()"/> or one of its overloads.
        /// This property also does not affect the behaviour of the main
        /// <see cref="ItemsSource"/> property. Changing <see cref="ItemsSource"/> always
        /// recreates and fully updates the tree.
        /// </summary>
        public bool ObserveChildItems
        {
            get { return (bool)GetValue(ObserveChildItemsProperty); }
            set { SetValue(ObserveChildItemsProperty, value); }
        }

        private void OnObserveChildItemsChanged(DependencyPropertyChangedEventArgs e)
        {
            if (this.IsInitialized) this.Render();
        }

        #endregion

        #region NodeContextMenu DependencyProperty

        public static readonly DependencyProperty NodeContextMenuProperty = DependencyProperty.Register(
            "NodeContextMenu", typeof(ContextMenu), typeof(TreeGrid),
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
        public ContextMenu NodeContextMenu
        {
            get { return (ContextMenu)GetValue(NodeContextMenuProperty); }
            set { SetValue(NodeContextMenuProperty, value); }
        }

        #endregion

        #region AutoCollapse DependencyProperty

        public static readonly DependencyProperty AutoCollapseProperty = DependencyProperty.Register(
            "AutoCollapse", typeof(bool), typeof(TreeGrid),
            new FrameworkPropertyMetadata(false, (d, e) => (d as TreeGrid).OnAutoCollapseChanged(e))
            );

        /// <summary>
        /// 如果设置为 true，则不是 <see cref="SelectedItem"/> 的父结点都会被折叠起来。
        /// </summary>
        public bool AutoCollapse
        {
            get { return (bool)GetValue(AutoCollapseProperty); }
            set { SetValue(AutoCollapseProperty, value); }
        }

        private void OnAutoCollapseChanged(DependencyPropertyChangedEventArgs e)
        {
            if (this.IsInitialized) { this.ApplyAutoCollapse(); }
        }

        #endregion

        #endregion
    }
}