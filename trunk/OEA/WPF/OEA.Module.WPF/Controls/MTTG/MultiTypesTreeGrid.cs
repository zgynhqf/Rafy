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


using Hardcodet.Wpf.GenericTreeView;
using OEA.Library;
using OEA.MetaModel;
using OEA.MetaModel.View;
using OEA.Module.WPF.Editors;

namespace OEA.Module.WPF.Controls
{
    /// <summary>
    /// 树型的列表编辑控件。
    /// 
    /// 本控件整合了两个开源控件：
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
    public partial class MultiTypesTreeGrid : TreeViewBase<Entity>
    {
        /// <summary>
        /// 主对象是否为树
        /// </summary>
        private bool _rootIsTree;

        private EntityViewMeta _rootMeta;

        static MultiTypesTreeGrid()
        {
            //此属性如果为真，GetChildItems 方法返回值必须实现 INotifyCollectionChanged 接口。
            //目前，Entity.ChildrenNodes 无法进行监听，所以我们选择不实现自动模式，而是直接调用 Refresh 接口。
            //相关 API 定义，请查看 ObserveChildItems 属性文档。
            ObserveChildItemsProperty.OverrideMetadata(typeof(MultiTypesTreeGrid), new FrameworkPropertyMetadata(false));
        }

        public MultiTypesTreeGrid(EntityViewMeta rootEntityViewMeta)
        {
            if (rootEntityViewMeta == null) throw new ArgumentNullException("rootEntityViewMeta");

            this._rootIsTree = rootEntityViewMeta.EntityMeta.IsTreeEntity;
            this._rootMeta = rootEntityViewMeta;

            this.OnDataBindConstruct();

            //此行会调用 Refresh 刷新整个控件，此时其它的属性需要被初始化完成。
            base.Tree = this.CreateTreeView();

            //在 TreeView 生成完毕后，再初始化一些视图属性。这些代码需要放在最后。
            this.OnViewConstruct();
        }

        public EntityViewMeta RootEntityViewMeta
        {
            get { return this._rootMeta; }
        }

        /// <summary>
        /// 所有的列。
        /// </summary>
        public TreeColumnCollection Columns
        {
            get { return this.Tree.Columns as TreeColumnCollection; }
        }

        #region API - 展开节点

        /// <summary>
        /// 展开某个节点
        /// </summary>
        /// <param name="node"></param>
        /// <param name="recur">是否递归展开node的子节点</param>
        /// <returns></returns>
        public void Expand(Entity node, bool recur = true)
        {
            this.EnsureNodeIsVisible(node);

            //如果对象不在Items里面，则return
            var curItem = this.GetRow(node);
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
        public void Collapse(Entity node, bool recur = true)
        {
            this.EnsureNodeIsVisible(node);

            //如果对象不在Items里面，则return
            var curItem = this.GetRow(node);
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
        public void ExpandToDepth(Entity node, int depth)
        {
            if (depth > 0)
            {
                this.Expand(node, false);

                //如果对象不在Items里面，则return
                var curItem = this.GetRow(node);
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

        #region 查找父子树型结点

        /// <summary>
        /// for LazyTreeNodeRelation
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        protected override bool HasChildItems(Entity parent)
        {
            if (parent.SupportTree)
            {
                if (parent.TreeChildren.Any(this.PassFilter)) return true;
                //var anyChildCanShow = this.FindChildrenNodes(parent).Any(c => this.PassFilter(c));
                //if (anyChildCanShow) { return true; }
            }

            ////如果没有返回True，再检查子对象
            //var treeChildPropertyInfo = this.SearchMeta(parent).TreeChildPropertyInfo;
            //if (treeChildPropertyInfo != null)
            //{
            //    var children = parent.GetPropertyValue<IList<Entity>>(treeChildPropertyInfo);
            //    return children.Any(c => this.PassFilter(c));
            //}

            return false;
        }

        public override ICollection<Entity> GetChildItems(Entity parent)
        {
            var list = new List<Entity>();

            //检查主对象
            if (parent.SupportTree)
            {
                foreach (var child in parent.TreeChildren)
                {
                    if (this.PassFilter(child)) { list.Add(child); }
                }

                //foreach (var child in this.FindChildrenNodes(parent))
                //{
                //    if (this.PassFilter(child)) { list.Add(child); }
                //}
            }

            ////判断子对象
            //var treeChildPropertyInfo = this.SearchMeta(parent).TreeChildPropertyInfo;
            //if (treeChildPropertyInfo != null)
            //{
            //    var childList = parent.GetPropertyValue<IList>(treeChildPropertyInfo);
            //    if (childList.Count > 0)
            //    {
            //        foreach (Entity child in childList)
            //        {
            //            //如果孩子对象也是树型对象，则只显示第一层孩子对象。
            //            var isFirstLayerChild = !child.SupportTree || child.TreePId == null;
            //            if (isFirstLayerChild && this.PassFilter(child)) { list.Add(child); }
            //        }
            //    }
            //}

            return list;
        }

        public override Entity GetParentItem(Entity item)
        {
            if (item == null) throw new ArgumentNullException("item");

            //先尝试使用树型对象的父来查找
            if (item.SupportTree)
            {
                return item.TreeParent;

                //if (item.TreePId != this._rootPId)
                //{
                //    return this.FindParentNode(item);
                //}
            }

            ////如果上面没有找到，而且 item 并不是控件中显示的聚合根对象，则返回它的聚合父对象
            //var meta = this.SearchMeta(item);
            //if (meta != this.RootEntityViewMeta)
            //{
            //    return item.GetParentEntity();
            //}

            return null;
        }

        ///// <summary>
        ///// 以不依赖树型属性的方式查找父元素
        ///// </summary>
        ///// <param name="node"></param>
        ///// <returns></returns>
        //private Entity FindParentNode(Entity node)
        //{
        //    if (!node.SupportTree) return null;

        //    var parentList = node.ParentList;
        //    if (parentList == null) return null;

        //    var id = node.TreePId;
        //    return parentList.FirstOrDefault(n => GetId(n) == id);
        //}

        ///// <summary>
        ///// 以不依赖树型属性的方式查找树型子元素
        ///// </summary>
        ///// <param name="node"></param>
        ///// <returns></returns>
        //private IEnumerable<Entity> FindChildrenNodes(Entity node)
        //{
        //    if (!node.SupportTree) return new Entity[0];

        //    var parentList = node.ParentList;
        //    if (parentList == null) return new Entity[0];

        //    var id = GetId(node);

        //    var result = parentList.Where(e => e.TreePId == id);

        //    //if (parentList.SupportOrder) { result = result.OrderBy(e => e.OrderNo); }

        //    return result;
        //}

        #endregion

        #region 重写父类方法

        public override string GetItemKey(Entity item)
        {
            return GetId(item).ToString();
        }

        /// <summary>
        /// 在结点被收缩起来时，应该它的子结点的所有数据在_objectItems中对应的项都移除。
        /// </summary>
        /// <param name="treeNode"></param>
        protected internal override void OnNodeCollapsed(TreeViewItem treeNode)
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

        protected internal override void OnNodeExpanded(TreeViewItem treeNode)
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
                Columns = new TreeColumnCollection()
            };
        }

        #endregion

        #region CreateTreeViewItem

        protected override TreeViewItem CreateItem(Entity item)
        {
            if (item == null)
            {
                return new GridTreeViewRow(this, this.RootEntityViewMeta);
            }

            //赋值属性
            var result = new GridTreeViewRow(this, this.RootEntityViewMeta);
            result.DataContext = item;//赋值DataContext，否则TreeColumn生成的控件绑定有问题

            this.SetBackground(result, item);

            this.SelectAsCreated(result, item);

            this._entityRows[GetId(item)] = result;

            this.OnItemCreated(result);

            return result;
        }

        #region ItemCreated 事件

        public event EventHandler<TreeViewItemCreatedEventArgs> ItemCreated;

        private void OnItemCreated(GridTreeViewRow item)
        {
            var handler = this.ItemCreated;
            if (handler != null) handler(this, new TreeViewItemCreatedEventArgs(item));
        }

        #endregion

        #endregion

        //private EntityViewMeta SearchMeta(Entity item)
        //{
        //    return this.SearchMeta(item.GetType());
        //}

        //private EntityViewMeta SearchMeta(Type entityType)
        //{
        //    var evm = this.RootEntityViewMeta;
        //    while (evm.EntityType != entityType)
        //    {
        //        evm = evm.TreeChildEntity;
        //        if (evm == null) { throw new InvalidOperationException("没有在 RootEntityViewMeta 对象树中找到对应的元数据。"); }
        //    }
        //    return evm;
        //}
    }

    #region public class TreeViewItemCreatedEventArgs

    public class TreeViewItemCreatedEventArgs : EventArgs
    {
        public TreeViewItemCreatedEventArgs(GridTreeViewRow item)
        {
            this.Item = item;
        }

        public GridTreeViewRow Item { get; private set; }
    }

    #endregion
}