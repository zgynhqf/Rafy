/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110621
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20110621
 * 
*******************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Hardcodet.Wpf.GenericTreeView;
using OEA.Library;
using System.Windows.Controls;

namespace OEA.Module.WPF.Controls
{
    /// <summary>
    /// 此文件包含 数据绑定、移除节点 部分的代码
    /// 
    /// 界面显示原理：
    /// 1. 基类的 Items 属性表示根对象列表，当 ObservableRootItems 属性为真时，这个集合必须是一个 INotifyCollectionChanged 的集合。
    /// 2. 但是由于一般我们会把整个数据的列表都传给控件，而不单单只是根对象的列表，所以我们在这里设计了一个 ItemsSource 属性，
    ///     用于表示整个可用的实体的列表，而在其中找到所有根对象并组装为一个全新的可监听变化的根对象集合并赋值给 Items 属性（RebindRoots方法中）。
    /// 3. 关于刷新：
    ///     由于本控件并不会监听 ItemsSource 的变化，所以当其中的项有所变化时，需要调用 RefreshControl 方法来通知 TreeGrid 进行重新绑定根对象列表。
    ///     基类的 Refresh 操作是基于 Items 及指定的 TreeLayout 来进行刷新的。
    ///     如果 ItemsSource 中的根对象发生变化后，基类不会知道，所以这个类使用 new 来定义了几个新的 Refresh 方法，里面加入了数据的重新生成。
    ///     这样，保证外界使用时，可以完全不考虑数据的变化通知；同时也不会影响基类的 Refresh 方法。
    /// 4. 缺点：
    ///     每次 Refresh 都是整个界面重新刷新，性能消耗比较大。但是不刷新，直接同步方案的过为复杂，暂时不考虑。
    /// </summary>
    public partial class MultiTypesTreeGrid
    {
        #region 字段

        /// <summary>
        /// 已经生成完毕的每个对象对应的节点列表
        /// Key: Key of Entity
        /// Value: TreeViewItem.
        /// </summary>
        private Dictionary<int, GridTreeViewRow> _entityRows;

        /// <summary>
        /// 当前绑定的所有数据。（EntityList）
        /// </summary>
        private IList _itemsSource;

        /// <summary>
        /// 当前显示的第一级列表的每一个项，都应该是这个 PId
        /// </summary>
        private int? _rootPId;

        #endregion

        private void OnDataBindConstruct()
        {
            this._entityRows = new Dictionary<int, GridTreeViewRow>();
        }

        public IList ItemsSource
        {
            get { return this._itemsSource; }
            set
            {
                this._itemsSource = value;

                this.BindData(null);
                this.Tree.Data = value;

                //树形控件的默认状态：数据默认展开第一级。
                if (!this.Tree.OnlyGridMode && this.Items.Count() == 1)
                {
                    var row = this.GetRow(this.Items.First());
                    row.IsExpanded = true;
                }
            }
        }

        public new void Refresh()
        {
            this.RebindRoots();
        }

        /// <summary>
        /// 根据 rootPid 绑定 this._roots 和 this.Items
        /// </summary>
        /// <param name="rootPid">
        /// rootPid不指定时,自动构造树;如果这个值不是null，则这个值表示绑定的所有根节点的父id。
        /// </param>
        public void BindData(int? rootPid)
        {
            this._rootPId = rootPid;

            this.RebindRoots();
        }

        /// <summary>
        /// 重新绑定全新的数据列表到根对象列表上。
        /// </summary>
        private void RebindRoots()
        {
            this._selectedItems.Clear();

            var roots = new List<Entity>();

            if (this._itemsSource != null)
            {
                //所有的树型结点的 Id 的缓存。
                var idsCache = new Lazy<List<int>>(
                    () => this._itemsSource.Cast<Entity>().Select(e => GetId(e)).ToList()
                    );

                //如果某个 item 是个 rootItem，则把它加入到 roots 中
                foreach (Entity item in this._itemsSource)
                {
                    if (this.IsRootItem(item, idsCache)) { roots.Add(item); }
                }
            }

            //表格模式下，重新绑定数据时，如果没有分组，则打开 UIV。
            var tree = this.Tree;
            if (tree != null && tree.OnlyGridMode)
            {
                if (this.RootGroupDescriptions == null || !this.RootGroupDescriptions.Any())
                {
                    tree.IsGridVirtualizing = true;
                }
            }

            //设置 Items 时根据当前的 Layout 来刷新界面
            this.Items = roots;
        }

        protected override sealed TreeViewItem EnsureNodeIsVisible(Entity item)
        {
            var row = this.FindOrGenerateNode(item);

            if (row == null) throw new NotSupportedException(string.Format("没有展开到 {0} 对应的行。", item));

            return row;
        }

        /// <summary>
        /// 尝试生成 item 所对应的行，并返回。
        /// 
        /// 要生成它所对应的行，应该把它的所有父元素全部展开。
        /// </summary>
        /// <param name="item"></param>
        /// <returns>
        /// 返回找到的行，如果无法展开它的所有父结点，则返回 null。
        /// </returns>
        protected internal virtual GridTreeViewRow FindOrGenerateNode(Entity item)
        {
            var row = this.GetRow(item);
            if (row == null)
            {
                var parent = this.GetParentItem(item);
                if (parent != null)
                {
                    var parentRow = this.FindOrGenerateNode(parent);
                    if (parentRow != null)
                    {
                        parentRow.IsExpanded = true;
                    }

                    row = this.GetRow(item);
                }
            }

            return row;
        }

        private bool IsRootItem(Entity item, Lazy<List<int>> allIdsCache)
        {
            if (item.GetType() == this.RootRuntimeType)
            {
                //如果主对象是树，则根据RootPid装载，否则全部装载
                if (!this._rootIsTree) { return true; }

                //如果某个 item 是个 rootItem，则把它加入到 roots 中
                if (this._rootPId != null)
                {
                    //进行过滤时，则需要 PId 等于指定的 PId
                    if (this._rootPId == item.TreePId) { return true; }
                }
                else
                {
                    //未进行过滤时，如果是 根对象 或者 列表中不包含该对象的父对象，都可以直接加入根对象集合中。
                    if (item.TreePId == null || !allIdsCache.Value.Contains(item.TreePId.Value)) { return true; }
                }
            }

            return false;
        }

        /// <summary>
        /// 获取对应对象的节点
        /// 
        /// 如果找不到（目前没有生成），则返回 null。
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public GridTreeViewRow GetRow(Entity item)
        {
            GridTreeViewRow treeListItem = null;

            if (item != null)
            {
                this._entityRows.TryGetValue(GetId(item), out treeListItem);
            }

            return treeListItem;
        }

        /// <summary>
        /// 树型控件中的 Id 获取方法不能直接使用 Id，而是应该：
        /// 尽量使用 TreeId，不支持时，再使用实体自己的 Id。
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private static int GetId(Entity item)
        {
            return item.SupportTree ? item.TreeId : item.Id;
        }

        /// <summary>
        /// 树型控件中的 Id 获取方法不能直接使用 Id，而是应该：
        /// 尽量使用 TreeId，不支持时，再使用实体自己的 Id。
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private static int GetId(GridTreeViewRow item)
        {
            var entity = GetEntity(item);
            if (entity == null) throw new InvalidOperationException("这个 GridTreeViewRow 还没有绑定实体，获取 Id 失败。");

            return GetId(entity);
        }
    }
}
