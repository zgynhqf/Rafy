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
using Rafy.WPF.Controls;
using System.Windows.Controls;
using System.Windows;

namespace Rafy.WPF.Controls
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
    [DefaultProperty("DataItemsSource")]
    partial class TreeGrid
    {
        #region RootPId DependencyProperty

        public static readonly DependencyProperty RootPIdProperty = DependencyProperty.Register(
            "RootPId", typeof(object), typeof(TreeGrid)
            );

        /// <summary>
        /// 当前显示的第一级列表的每一个项，都应该是这个 PId
        /// </summary>
        public object RootPId
        {
            get { return (object)this.GetValue(RootPIdProperty); }
            set { this.SetValue(RootPIdProperty, value); }
        }

        #endregion

        #region DataItemsSource DependencyProperty

        public static readonly DependencyProperty RootItemsProperty = DependencyProperty.Register(
            "RootItems", typeof(IList), typeof(TreeGrid),
            new PropertyMetadata((d, e) => (d as TreeGrid).OnRootItemsChanged(e))
            );

        public IList RootItems
        {
            get { return (IList)this.GetValue(RootItemsProperty); }
            set { this.SetValue(RootItemsProperty, value); }
        }

        private void OnRootItemsChanged(DependencyPropertyChangedEventArgs e)
        {
            this.RootPId = null;

            this.Refresh();

            if (this.OnlyGridMode)
            {
                if (this.AutoCalcRowHeaderWidth)
                {
                    this.AutoCalculateRowHeaderWidth(e.NewValue);
                }
            }
            else
            {
                //树形控件的默认状态：数据默认展开第一级。
                var items = this.RootItemsControl.Items;
                if (items.Count == 1)
                {
                    var row = this.FindRow(items[0]);
                    if (row != null) row.IsExpanded = true;
                }
            }
        }

        #endregion

        /// <summary>
        /// 完整的全刷新。
        /// 
        /// 重新绑定全新的数据列表到根对象列表上。
        /// </summary>
        public void Refresh()
        {
            this.ClearSelectionOnRefreshing();

            this.SetUIV();

            this.Render();

            this.InvalidateSummary();

            this.ListenItemsPropertyChanged();
        }

        /// <summary>
        /// 获取对应对象的节点
        /// 
        /// 如果找不到（虚拟化中，目前没有生成该行），则返回 null。
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public TreeGridRow FindRow(object item)
        {
            if (item == null) return null;

            //找到 ItemsControl 容器
            ItemsControl itemsControl = null;

            var parent = this.GetParentItem(item);
            if (parent != null)
            {
                var parentRow = this.FindRow(parent);
                if (parentRow == null) return null;
                itemsControl = parentRow;
            }
            else
            {
                itemsControl = this.RootItemsControl;
            }

            var row = itemsControl.ItemContainerGenerator.ContainerFromItem(item) as TreeGridRow;

            return row;
        }

        /// <summary>
        /// 在指定的行中查找子行。
        /// </summary>
        /// <param name="item"></param>
        /// <param name="container">可以是 TreeGrid，也可以是 TreeGridRow。</param>
        /// <returns></returns>
        public TreeGridRow FindChildRow(object item, ItemsControl container)
        {
            if (item == null) return null;

            var row = container.ItemContainerGenerator.ContainerFromItem(item) as TreeGridRow;

            return row;
        }

        /// <summary>
        /// 尝试找到指定对象的指定单元格。
        /// 如果该行该列正处于虚拟化中，则返回 null。
        /// </summary>
        /// <param name="item"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public TreeGridCell FindCell(object item, TreeGridColumn column)
        {
            TreeGridCell cell = null;

            var row = this.FindRow(item);
            if (row != null) { cell = row.FindCell(column); }

            return cell;
        }

        /// <summary>
        /// 树型控件中的 Id 获取方法不能直接使用 Id，而是应该：
        /// 尽量使用 TreeId，不支持时，再使用实体自己的 Id。
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private object GetId(TreeGridRow item)
        {
            var entity = item.DataContext;
            if (entity == null) throw new InvalidOperationException("这个 TreeGridRow 还没有绑定实体，获取 Id 失败。");

            return GetId(entity);
        }

        internal object FindItemById(object key)
        {
            return EachNode(i => object.Equals(GetId(i), key));
        }

        #region 监听每一项的属性变化

        private void ListenItemsPropertyChanged()
        {
            EachNode(item =>
            {
                var propertyChanged = item as INotifyPropertyChanged;
                if (propertyChanged == null) return true;

                PropertyChangedEventManager.AddListener(propertyChanged, this);
                return false;
            });
        }

        private void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //任意项的任意属性变化时，都重新计算合计值。
            this.InvalidateSummary();
        }

        #endregion
    }
}
