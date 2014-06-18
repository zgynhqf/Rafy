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

namespace Rafy.WPF.Controls
{
    /// <summary>
    /// 界面渲染、界面元素构造相关方法
    /// </summary>
    partial class TreeGrid
    {
        /// <summary>
        /// 是否树型控件已经被渲染
        /// </summary>
        private bool _isTreeRendered = false;

        /// <summary>
        /// will be used by automation. from MS.GridView
        /// </summary>
        internal TreeGridHeaderRowPresenter HeaderRowPresenter;

        /// <summary>
        /// 保证树已经呈现。
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            //如果需要，创建一个默认的树控件，并第一次渲染它。
            if (!_isTreeRendered)
            {
                this.ApplySorting();

                //everything has been created, but the tree has not
                //been rendered yet, because IsInitialized was false
                //-> render now
                this.Render();
            }
        }

        #region 重写 ItemsControl 相关方法

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is TreeGridRow;
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new TreeGridRow();
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            this.PrepareRowForItem(element, item);
        }

        #endregion

        /// <summary>
        /// 渲染
        /// </summary>
        /// <param name="currentLayout">是否保持当前的布局</param>
        internal void Render(bool currentLayout = true)
        {
            //保存或者创建一个新的目标布局
            _renderExpansion = currentLayout ? this.GetCurrentExpandedItems() : new TreeGridExpandedItems();
            object selectedId = null;
            if (currentLayout)
            {
                var selected = this.SelectedItem;
                if (selected != null) selectedId = GetId(selected);
            }

            _monitor.Clear();

            #region 如果有自定义根结点，则设置根结点并展开它。

            var root = this.RootNode;
            if (root != null)
            {
                root.IsExpanded = true;

                //在有根节点的情况下，this.Items 中只有一个元素，这个元素被直接设置为 RootNode。
                if (!this.Items.Contains(root))
                {
                    this.Items.Add(root);
                }
            }

            #endregion

            #region 把根结点列表直接绑定到 ItemsSource 属性上。

            var rootControl = this.RootItemsControl;

            //rootItem 的数据使用 ItemsSource 来进行生成，这样，就可以使用 GroupDescriptions 来实现分组功能。
            var oldRoots = rootControl.ItemsSource;
            var newRoots = this.RootItems;
            if (newRoots != oldRoots)
            {
                if (oldRoots != null) _monitor.UnregisterRootItems(oldRoots);
                if (newRoots != null) _monitor.RegisterRootItems(newRoots);
                rootControl.ItemsSource = newRoots;
            }
            else
            {
                rootControl.Items.Refresh();
            }

            #endregion

            #region 设置选择项

            object selectedItem = null;
            if (selectedId != null && this.RootItems != null)
            {
                selectedItem = this.RootItems.OfType<object>().FirstOrDefault(e => object.Equals(GetId(e), selectedId));
            }
            if (this.SelectedItem != selectedItem)
            {
                this.SelectedItem = selectedItem;
            }
            else
            {
                //如果 selectedItem 已经相同的情况下，需要调用以下方法把选中行显示在界面上。
                this.ExpandToView(selectedItem);
            }

            #endregion

            this.RequestDataWidthes();

            _isTreeRendered = true;
        }

        /// <summary>
        /// 在 TreeGridRow 被父层 ItemsControl 创建时，会调用此方法为后期生成的行设置初始属性。
        /// </summary>
        /// <param name="element"></param>
        /// <param name="item"></param>
        internal void PrepareRowForItem(DependencyObject element, object item)
        {
            var row = element as TreeGridRow;

            row.HasChild = this.HasChildItems(item);

            var itemId = GetId(item);
            if (_renderExpansion.IsExpanded(itemId))
            {
                row.IsExpanded = true;

                //由于直接设置属性值时，该行还没有加入到逻辑树中，所以没有发生路由事件处理逻辑，这里需要手工调用此方法。
                this.OnNodeExpanded(row);
            }

            //当重新生成某一行时，如果是已经选中的实体，需要初始化它们的选中状态
            //下行代码前必须使用 SetEntity 把 row 的 Entity 设置好，否则会把 null 加入到 SelectionModel.InnerItems 集合中。
            var sm = this.SelectionModel;
            if (sm.InnerItems.Contains(item))
            {
                sm.ModifyRowProperty(row, true);
            }

            var cm = this.CheckingModel;
            if (cm.InnerItems.Contains(item))
            {
                cm.ModifyRowProperty(row, true);
            }

            this.RefreshRowNo(row);

            this.ApplyNodeStyle(row);
            this.ApplyFilter(row);
            this.ApplySorting(row);

            if (this.ObserveChildItems)
            {
                var children = this.GetChildItems(item);
                _monitor.RegisterItem(itemId, children);
            }
        }

        /// <summary>
        /// 为指定的结点应用 <see cref="RowStyle"/> 样式。
        /// 
        /// 子类重写此方法用于应用自定义样式。
        /// </summary>
        /// <param name="treeNode">The node to be styled.</param>
        private void ApplyNodeStyle(TreeGridRow treeNode)
        {
            var style = this.RowStyle;
            if (style != null) treeNode.Style = style;
        }
    }
}