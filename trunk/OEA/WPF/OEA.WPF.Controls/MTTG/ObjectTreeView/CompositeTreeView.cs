/*******************************************************
 * 
 * 作者：hardcodet
 * 创建时间：2008
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：2.0.0
 * 
 * 历史记录：
 * 创建文件 hardcodet 2008
 * 2.0 胡庆访 20120911 14:42
 * 
*******************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;

namespace OEA.Module.WPF.Controls
{
    /// <summary>
    /// 一个封装了系统 TreeView 控件，并提供更强大 API 的控件类型。
    /// </summary>
    public abstract partial class CompositeTreeView : ContentControl
    {
        #region 构造函数 & 初始化

        /// <summary>
        /// 用于跟踪子结点集合变更的对象。见 <see cref="ObserveChildItems"/> 属性。
        /// </summary>
        private ItemMonitor _monitor;

        static CompositeTreeView()
        {
            EventManager.RegisterClassHandler(typeof(CompositeTreeView), TreeView.SelectedItemChangedEvent, new RoutedPropertyChangedEventHandler<object>(TreeView_SelectedItemChanged));
            EventManager.RegisterClassHandler(typeof(CompositeTreeView), TreeViewItem.ExpandedEvent, new RoutedEventHandler(TreeViewItem_Expanded));
            EventManager.RegisterClassHandler(typeof(CompositeTreeView), TreeViewItem.CollapsedEvent, new RoutedEventHandler(TreeViewItem_Collapsed));
        }

        public CompositeTreeView()
        {
            this._monitor = new ItemMonitor(this);
        }

        #endregion

        #region Tree DependencyProperty

        public static readonly DependencyProperty TreeProperty = DependencyProperty.Register(
            "Tree", typeof(TreeView), typeof(CompositeTreeView),
            new FrameworkPropertyMetadata(null, (d, e) => (d as CompositeTreeView).OnTreeChanged(e))
            );

        /// <summary>
        /// 获取或设置底层的 <see cref="TreeView"/> 控件。
        /// </summary>
        [Category("TreeGrid")]
        [Description("The underlying WPF TreeView that renders the assigned data.")]
        public TreeView Tree
        {
            get { return (TreeView)GetValue(TreeProperty); }
            set { SetValue(TreeProperty, value); }
        }

        protected virtual void OnTreeChanged(DependencyPropertyChangedEventArgs e)
        {
            var oldTreeView = (TreeView)e.OldValue;
            var treeView = (TreeView)e.NewValue;

            this.Content = treeView;

            //同步 TreeStyle 属性
            if (treeView != null && this.TreeStyle != null) treeView.Style = this.TreeStyle;

            if (this.IsInitialized)
            {
                this.ApplySorting();

                this.Refresh(null);
            }
        }

        #endregion

        #region Items DependencyProperty

        public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register(
            "Items", typeof(IEnumerable<object>), typeof(CompositeTreeView),
            new FrameworkPropertyMetadata(null, (d, e) => (d as CompositeTreeView).OnItemsChanged(e))
            );

        /// <summary>
        /// 绑定到树上的对象列表
        /// </summary>
        [Category("TreeGrid")]
        [Description("The root items that provide the top level of the tree.")]
        public IEnumerable<object> Items
        {
            get { return (IEnumerable<object>)GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }

        /// <summary>
        /// 在 Items 变更时，刷新整棵树。
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnItemsChanged(DependencyPropertyChangedEventArgs e)
        {
            var oldValue = (IEnumerable)e.OldValue;
            var newValue = (IEnumerable)e.NewValue;

            if (oldValue != null) this._monitor.UnregisterRootItems(oldValue);
            if (newValue != null) this._monitor.RegisterRootItems(newValue);

            //尝试刷新控件
            if (this.IsInitialized) { this.Refresh(); }
        }

        #endregion

        #region 父子关系

        /// <summary>
        /// 实现时应该返回指定实体对应的整形 Id
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public abstract int GetId(object item);

        /// <summary>
        /// Gets all child items of a given parent item. The
        /// tree needs this method to properly traverse the
        /// logic tree of a given item.<br/>
        /// Important: If you plan to have the tree automatically
        /// update itself if nested content is being changed, you
        /// the <see cref="ObserveChildItems"/> property must be
        /// true, and the collection that is being returned
        /// needs to implement the <see cref="INotifyCollectionChanged"/>
        /// interface (e.g. by returning an collection of type
        /// <see cref="ObservableCollection{object}"/>.
        /// </summary>
        /// <param name="parent">A currently processed item that
        /// is being represented as a node of the tree.</param>
        /// <returns>All child items to be represented by the
        /// tree. The returned collection needs to implement
        /// <see cref="INotifyCollectionChanged"/> if the
        /// <see cref="ObserveChildItems"/> feature is supposed
        /// to work.</returns>
        /// <remarks>If this is an expensive operation, you should
        /// override <see cref="HasChildItems"/> which
        /// invokes this method by default.</remarks>
        public abstract ICollection<object> GetChildItems(object parent);

        /// <summary>
        /// 获取指定实体对应的父实体，如果没有则返回 null。
        /// </summary>
        /// <param name="dataItem">The currently processed item.</param>
        /// <returns>The parent of the item, if available.</returns>
        public abstract object GetParentItem(object dataItem);

        /// <summary>
        /// 检测指定的结点是否有子结点。
        /// 本方法会被调用来检测是否某结点需要显示一个 Expander。
        /// 
        /// 默认实现是直接使用 <see cref="GetChildItems"/> 方法来检测子结点的个数是否大于 0。
        /// </summary>
        /// <remarks>
        /// You should override this method if invoking
        /// <see cref="GetChildItems"/> is an expensive operation
        /// (e.g. because data needs to be retrieved from a web
        /// service). In case there is no possibility for a cheaper solution,
        /// you may just return true: In that case, an expander will
        /// be rendered and removed as soon as the user attempts to
        /// expand the node, if there are no child items available.<br />
        /// However: Overriding this method is pointless if
        /// <see cref="ObserveChildItems"/> is set to true. In that
        /// case, this method will not be used as
        /// <see cref="GetChildItems"/> is being invoked anyway to get
        /// the observed collection.
        /// </remarks>
        protected virtual bool HasChildItems(object parent)
        {
            return GetChildItems(parent).Count > 0;
        }

        /// <summary>
        /// Gets a list of all ancestors of a given item up to the
        /// root element, excluding the item itself. The root element
        /// is supposed to be contained at index 0, while the immediate
        /// parent is being placed at the end of the list.
        /// </summary>
        /// <param name="child">The processed item that marks the
        /// starting point.</param>
        /// <returns>A list of all the item's parents.</returns>
        protected List<object> GetParentItemList(object child)
        {
            var parents = new List<object>();
            object parentItem = GetParentItem(child);
            while (parentItem != null)
            {
                parents.Insert(0, parentItem);
                parentItem = GetParentItem(parentItem);
            }
            return parents;
        }

        /// <summary>
        /// 检测是否指定的两个实体是父子关系。
        /// </summary>
        /// <param name="child"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public bool IsDescendantOf(object child, object parent)
        {
            object directParent = GetParentItem(child);

            if (directParent == null) return false;
            if (directParent == parent) return true;

            return this.IsDescendantOf(directParent, parent);
        }

        #endregion

        #region 查找结点

        /// <summary>
        /// Gets a given node of the tree. Note that with lazy loading
        /// enabled, the tree returns null, if the corresponding tree
        /// node has not been created yet.
        /// </summary>
        /// <param name="item">The item that is being represented
        /// by the node to be looked up.</param>
        /// <returns>The node that corresponds to the item, if any.
        /// Otherwise null.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="item"/>
        /// is a null reference.</exception>
        public TreeViewItem TryFindNode(object item)
        {
            if (item == null) throw new ArgumentNullException("item");

            //get item key and delegate to overload
            var itemKey = GetId(item);
            return this.TryFindNodeByKey(itemKey);
        }

        /// <summary>
        /// Returns a node of the tree which represents a given item.
        /// Note that if lazy loading is enabled, the tree returns null,
        /// if the corresponding tree node has not been created yet.
        /// </summary>
        /// <param name="itemKey">The item identifier, as created by
        /// the <see cref="GetId"/> method.</param>
        /// <returns>The node that matches the submitted key, if any.
        /// Otherwise null.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="itemKey"/>
        /// is a null reference.</exception>
        public TreeViewItem TryFindNodeByKey(int itemKey)
        {
            if (itemKey == 0) throw new ArgumentNullException("itemKey");
            return this.TryFindItemNode(this.Tree.Items, itemKey, true);
        }

        /// <summary>
        /// Recursively searches the tree for a node that represents
        /// a given item starting at any given level of the tree. Note that
        /// with lazy loading enabled, this method returns null if the
        /// matching node has not been created yet.
        /// </summary>
        /// <param name="treeNodes">The items to be browsed recursively.</param>
        /// <param name="itemKey">The unique node ID of the item.</param>
        /// <param name="recurse">Whether to limit the search to the <paramref name="treeNodes"/>
        /// collection or not. If true, the descendants of all items will be searched
        /// recursively.</param>
        /// <returns>The matching node, if any. Otherwise null.</returns>
        protected internal TreeViewItem TryFindItemNode(ItemCollection treeNodes, int itemKey, bool recurse)
        {
            foreach (TreeViewItem treeNode in treeNodes)
            {
                object nodeItem = GetEntity(treeNode);
                if (nodeItem != null)
                {
                    //the root item does not provide a matching header...
                    var id = GetId(nodeItem);
                    if (itemKey == id) return treeNode;
                }

                //browse child items
                if (recurse)
                {
                    var match = TryFindItemNode(treeNode.Items, itemKey, true);
                    if (match != null) return match;
                }
            }

            return null;
        }

        #endregion
    }
}