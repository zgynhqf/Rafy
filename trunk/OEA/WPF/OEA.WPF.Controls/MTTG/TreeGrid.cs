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
 * MultiObjectTreeView 重命名为 MultiTypesTreeGrid。 胡庆访 20111122
 * 2.1.0 支持 UIV。 胡庆访 20111213
 * 2.2.0 抽取为独立的模块，与 OEA 元数据解耦。 胡庆访 20120820
 * 
*******************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using OEA.Module.WPF.Controls;

namespace OEA.Module.WPF.Controls
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
    public abstract partial class TreeGrid : CompositeTreeView
    {
        public TreeGrid()
        {
            this.OnSelectionConstruct();

            this.OnDataBindConstruct();

            //此行会调用 Refresh 刷新整个控件，此时其它的属性需要被初始化完成。
            base.Tree = this.CreateTreeView();
        }

        #region IsTree DependencyProperty

        public static readonly DependencyProperty IsTreeProperty = DependencyProperty.Register(
            "IsTree", typeof(bool), typeof(TreeGrid),
            new PropertyMetadata(true, (d, e) => (d as TreeGrid).OnIsTreeChanged(e))
            );

        /// <summary>
        /// 是否为树型表格。
        /// </summary>
        public bool IsTree
        {
            get { return (bool)this.GetValue(IsTreeProperty); }
            set { this.SetValue(IsTreeProperty, value); }
        }

        private void OnIsTreeChanged(DependencyPropertyChangedEventArgs e)
        {
            var value = (bool)e.NewValue;

            if (!value)
            {
                //这个控件不需要支持多类型对象了，如果不是树型，直接是 Grid 模式
                var tree = this.Tree;
                if (tree != null)
                {
                    tree.OnlyGridMode = true;
                    //this.Tree.OnlyGridMode = this.RootEntityViewMeta.TreeChildEntity == null;
                }
            }
        }

        #endregion

        /// <summary>
        /// 所有的列。
        /// </summary>
        public GridTreeViewColumnCollection Columns
        {
            get { return this.Tree.Columns; }
        }

        #region API - 展开节点

        /// <summary>
        /// 展开某个节点
        /// </summary>
        /// <param name="node"></param>
        /// <param name="recur">是否递归展开node的子节点</param>
        /// <returns></returns>
        public void Expand(object node, bool recur = true)
        {
            this.EnsureNodeIsVisible(node);

            //如果对象不在Items里面，则return
            var curItem = this.FindRow(node);
            if (curItem != null)
            {
                curItem.IsExpanded = true;

                if (recur)
                {
                    foreach (GridTreeViewRow item in curItem.Items)
                    {
                        var n = GetEntity(item);
                        if (this.HasChildItems(n)) { this.Expand(n, true); }
                    }
                }
            }
        }

        /// <summary>
        /// 折叠指定的节点
        /// </summary>
        /// <param name="node"></param>
        /// <param name="recur"></param>
        public void Collapse(object node, bool recur = true)
        {
            this.EnsureNodeIsVisible(node);

            //如果对象不在Items里面，则return
            var curItem = this.FindRow(node);
            if (curItem != null)
            {
                if (recur)
                {
                    for (int i = curItem.Items.Count - 1; i > 0; i--)
                    {
                        var item = curItem.Items[i] as GridTreeViewRow;

                        var n = GetEntity(item);
                        if (this.HasChildItems(n)) { this.Collapse(n, recur); }
                    }
                }
                curItem.IsExpanded = false;
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
                var curItem = this.FindRow(node);
                if (curItem != null)
                {
                    foreach (GridTreeViewRow item in curItem.Items)
                    {
                        var child = GetEntity(item);
                        ExpandToDepth(child, depth - 1);
                    }
                }
            }
            else
            {
                this.Collapse(node);
            }
        }

        #endregion

        #region 重写父类方法

        /// <summary>
        /// 在结点被收缩起来时，应该它的子结点的所有数据在_objectItems中对应的项都移除。
        /// </summary>
        /// <param name="treeNode"></param>
        protected override void OnNodeCollapsed(TreeViewItem treeNode)
        {
            //在结点被收缩起来时，应该它的子结点的所有数据在 _objectItems 中对应的项都移除。
            var children = this.GetChildItems(GetEntity(treeNode));
            foreach (var child in children)
            {
                this._entityRows.Remove(GetId(child));
            }

            //调用基类来处理 LazyLoading
            base.OnNodeCollapsed(treeNode);

            this.RefreshRowNo_OnCollapsedOrExpanded();
        }

        protected override void OnNodeExpanded(TreeViewItem treeNode)
        {
            //调用基类来处理 LazyLoading
            base.OnNodeExpanded(treeNode);

            this.RefreshRowNo_OnCollapsedOrExpanded();
        }

        protected override TreeView CreateTreeView()
        {
            return new GridTreeView()
            {
                TreeGrid = this,
                Columns = this.CreateColumns()
            };
        }

        protected virtual GridTreeViewColumnCollection CreateColumns()
        {
            return new GridTreeViewColumnCollection();
        }

        #endregion

        #region CreateTreeViewItem

        protected override TreeViewItem CreateItem(object item)
        {
            var row = new GridTreeViewRow(this);
            if (item == null) { return row; }

            this.SelectAsCreated(row, item);

            this._entityRows[GetId(item)] = row;

            this.OnItemCreated(row);

            return row;
        }

        #region ItemCreated 事件

        public event EventHandler<RowCreatedEventArgs> ItemCreated;

        private void OnItemCreated(GridTreeViewRow item)
        {
            var handler = this.ItemCreated;
            if (handler != null) handler(this, new RowCreatedEventArgs(item));
        }

        #endregion

        #endregion
    }

    #region public class TreeViewItemCreatedEventArgs

    public class RowCreatedEventArgs : EventArgs
    {
        public RowCreatedEventArgs(GridTreeViewRow row)
        {
            this.Row = row;
        }

        public GridTreeViewRow Row { get; private set; }
    }

    #endregion
}