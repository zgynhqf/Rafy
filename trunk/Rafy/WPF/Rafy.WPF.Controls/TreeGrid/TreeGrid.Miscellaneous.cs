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
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Rafy.WPF.Controls
{
    partial class TreeGrid
    {
        /// <summary>
        /// 用于跟踪子结点集合变更的对象。见 <see cref="ObserveChildItems"/> 属性。
        /// 
        /// 暂时不使用。
        /// </summary>
        private ItemMonitor _monitor;

        #region 右键选择功能

        #region SelectNodesOnRightClick DependencyProperty

        public static readonly DependencyProperty SelectNodesOnRightClickProperty = DependencyProperty.Register(
            "SelectNodesOnRightClick", typeof(bool), typeof(TreeGrid),
            new FrameworkPropertyMetadata(false)
            );

        /// <summary>
        /// If set to true, treeview items are automatically selected on right clicks,
        /// which simplifies context menu handling.
        /// </summary>
        public bool SelectNodesOnRightClick
        {
            get { return (bool)GetValue(SelectNodesOnRightClickProperty); }
            set { SetValue(SelectNodesOnRightClickProperty, value); }
        }

        #endregion

        /// <summary>
        /// Intercepts right mouse button clicks an checks whether a tree
        /// node was clicked. If this is the case, the node will be selected
        /// in case it's not selected an the <see cref="SelectNodesOnRightClick"/>
        /// dependency property is set.<br/>
        /// If the <see cref="NodeContextMenu"/> property is set and no custom
        /// context menu was assigned to the item, the <see cref="NodeContextMenu"/>
        /// will be opened with its <see cref="ContextMenu.PlacementTarget"/> property
        /// set to the clicked tree node. Right clicks on a <see cref="RootNode"/>
        /// will be ignored.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
        {
            //return if no node was clicked
            var os = e.OriginalSource as DependencyObject;
            var item = os.GetVisualParent<TreeGridRow>();
            if (item == null) return;

            //activate item if necessary
            if (this.SelectNodesOnRightClick && !item.IsSelected)
            {
                item.IsSelected = true;
            }

            //context menu handling: don't do anything if no context menu
            //was defined or one was assigned by custom code
            if (this.NodeContextMenu == null || item.ContextMenu != null) return;

            //also don't show a context menu if the root node was clicked
            if (ReferenceEquals(item, RootNode)) return;

            //temporarily assign the menu to the item - this ensures that
            //a the PlacementTarget property of the context menu points to
            //the item (can be evaluated in a click event or command handler)
            item.ContextMenu = NodeContextMenu;

            //open the context menu for the clicked item
            this.NodeContextMenu.PlacementTarget = item;
            this.NodeContextMenu.IsOpen = true;

            //mark as handled - let the event bubble on...
            e.Handled = true;

            //reset the context menu assignment
            item.ContextMenu = null;
        }

        #endregion

        #region 其它方法

        /// <summary>
        /// 获取当前所有已经生成的行的迭代器。
        /// </summary>
        public IEnumerable<TreeGridRow> RecursiveRows
        {
            get { return TreeGridHelper.TraverseRows(this.RootItemsControl, true); }
        }

        /// <summary>
        /// 最上层的 ItemsControl，可能是 TreeView 或者 RootNode。
        /// </summary>
        public ItemsControl RootItemsControl
        {
            get
            {
                var rootNode = this.RootNode;
                return rootNode == null ? this : rootNode as ItemsControl;
            }
        }

        bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (managerType == typeof(PropertyChangedEventManager))
            {
                this.OnItemPropertyChanged(sender, e as PropertyChangedEventArgs);
                return true;
            }

            return false;
        }

        #endregion
    }
}
