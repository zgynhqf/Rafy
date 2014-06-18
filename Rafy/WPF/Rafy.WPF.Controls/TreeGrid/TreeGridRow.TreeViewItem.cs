/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120928 11:46
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件，主要来自于 MS 的 TreeViewItem 类。 胡庆访 20120928 11:46
 * 
*******************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime;
using System.Text;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Rafy.WPF.Controls;

namespace Rafy.WPF.Controls
{
    /// <summary>
    /// 本文件中的代码一开始来自于 MS 的 TreeViewItem 类，然后改变了一些逻辑。
    /// </summary>
    partial class TreeGridRow
    {
        #region 路由事件

        public static readonly RoutedEvent SelectedEvent = EventManager.RegisterRoutedEvent("Selected", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TreeGridRow));
        public event RoutedEventHandler Selected
        {
            add { this.AddHandler(SelectedEvent, value); }
            remove { this.RemoveHandler(SelectedEvent, value); }
        }
        protected virtual void OnSelected(RoutedEventArgs e)
        {
            this.RaiseEvent(e);
        }

        public static readonly RoutedEvent UnselectedEvent = EventManager.RegisterRoutedEvent("Unselected", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TreeGridRow));
        public event RoutedEventHandler Unselected
        {
            add { this.AddHandler(UnselectedEvent, value); }
            remove { this.RemoveHandler(UnselectedEvent, value); }
        }
        protected virtual void OnUnselected(RoutedEventArgs e)
        {
            this.RaiseEvent(e);
        }

        public static readonly RoutedEvent CollapsedEvent = EventManager.RegisterRoutedEvent("Collapsed", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TreeGridRow));
        public event RoutedEventHandler Collapsed
        {
            add { this.AddHandler(CollapsedEvent, value); }
            remove { this.RemoveHandler(CollapsedEvent, value); }
        }
        protected virtual void OnCollapsed(RoutedEventArgs e)
        {
            this.RaiseEvent(e);
        }

        public static readonly RoutedEvent ExpandedEvent = EventManager.RegisterRoutedEvent("Expanded", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TreeGridRow));
        public event RoutedEventHandler Expanded
        {
            add { this.AddHandler(ExpandedEvent, value); }
            remove { this.RemoveHandler(ExpandedEvent, value); }
        }
        protected virtual void OnExpanded(RoutedEventArgs e)
        {
            this.RaiseEvent(e);
        }

        #endregion

        #region IsExpanded DependencyProperty

        public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register(
            "IsExpanded", typeof(bool), typeof(TreeGridRow),
            new PropertyMetadata(BooleanBoxes.False, (d, e) => (d as TreeGridRow).OnIsExpandedChanged(e))
            );

        public bool IsExpanded
        {
            get { return (bool)this.GetValue(IsExpandedProperty); }
            set { this.SetValue(IsExpandedProperty, value); }
        }

        #endregion

        #region IsSelected DependencyProperty

        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register("IsSelected", typeof(bool), typeof(TreeGridRow),
            new FrameworkPropertyMetadata(BooleanBoxes.False, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (d, e) => (d as TreeGridRow).OnIsSelectedChanged(e)));

        public bool IsSelected
        {
            get { return (bool)this.GetValue(IsSelectedProperty); }
            set { this.SetValue(IsSelectedProperty, BooleanBoxes.Box(value)); }
        }

        #endregion

        #region 方便使用的内部属性

        internal TreeGridRow ParentRow
        {
            get { return this.ParentItemsControl as TreeGridRow; }
        }

        internal ItemsControl ParentItemsControl
        {
            get { return ItemsControl.ItemsControlFromItemContainer(this); }
        }

        private ItemsPresenter ItemsHostPresenter
        {
            get
            {
                return base.GetTemplateChild("ItemsHost") as ItemsPresenter;
            }
        }

        #endregion

        #region 选择相关代码

        /// <summary>
        /// 表明是否本对象正在被设置字段、而不是依赖属性值。
        /// 
        /// 设置字段时，只会简单地设置底层的字段值，该依赖属性对应的变更事件处理函数将不会执行。
        /// 
        /// 设置核心的选中状态属性，由于属性是最底层的数据，所以需要保证设置属性的过程中不会再发生上层的选中响应事件。
        /// </summary>
        private bool _isUsingFields;

        internal void SetIsSelectedField(bool value)
        {
            try
            {
                this._isUsingFields = true;
                this.IsSelected = value;
            }
            finally
            {
                this._isUsingFields = false;
            }
        }

        private void OnIsSelectedChanged(DependencyPropertyChangedEventArgs e)
        {
            bool value = (bool)e.NewValue;

            //如果不作为单纯的属性被使用时，例如正在被程序直接设置时，需要通知 TreeGrid 更新选中状态。
            if (!this._isUsingFields)
            {
                var grid = this.TreeGrid;
                if (grid != null)
                {
                    using (grid.SelectionProcess.TryEnterProcess(TGSelectionProcess.Row_IsSelected))
                    {
                        this.Select(value);
                    }
                }
            }

            if (value)
            {
                this.OnSelected(new RoutedEventArgs(TreeGridRow.SelectedEvent, this));
            }
            else
            {
                this.OnUnselected(new RoutedEventArgs(TreeGridRow.UnselectedEvent, this));
            }

            this.UpdateVisualState();
        }

        /// <summary>
        /// 设置当前行的选中状态。
        /// </summary>
        /// <param name="selected"></param>
        private void Select(bool selected)
        {
            var parentTreeGrid = this.TreeGrid;
            var parentItemsControl = this.ParentItemsControl;
            if (parentTreeGrid != null && parentItemsControl != null)
            {
                object itemOrContainerFromContainer = GetItemOrContainerFromContainer(parentItemsControl, this);
                parentTreeGrid.ChangeSelection(itemOrContainerFromContainer, this, selected);
                if (selected && parentTreeGrid.IsKeyboardFocusWithin && !base.IsKeyboardFocusWithin)
                {
                    base.Focus();
                }
            }
        }

        internal static object GetItemOrContainerFromContainer(ItemsControl itemsControl, DependencyObject container)
        {
            object obj = itemsControl.ItemContainerGenerator.ItemFromContainer(container);
            if (obj == DependencyProperty.UnsetValue &&
                ItemsControl.ItemsControlFromItemContainer(container) == itemsControl &&
                IsItemItsOwnContainer(itemsControl, container))
            {
                obj = container;
            }

            return obj;
        }

        private static bool IsItemItsOwnContainer(ItemsControl itemsControl, DependencyObject container)
        {
            if (itemsControl is TreeGridRow || itemsControl is TreeGrid) return container is TreeGridRow;

            return container is UIElement;
        }

        #endregion

        private void OnIsExpandedChanged(DependencyPropertyChangedEventArgs e)
        {
            bool flag = (bool)e.NewValue;
            var grid = this.TreeGrid;
            if (grid != null && !flag)
            {
                grid.HandleSelectionOnCollapsed(this);
            }
            var itemsHostPresenter = this.ItemsHostPresenter;
            if (itemsHostPresenter != null)
            {
                this.InvalidateMeasure();

                //MS Code，暂未实现
                //Helper.InvalidateMeasureOnPath(itemsHostPresenter, this, false);
            }

            if (flag)
            {
                this.OnExpanded(new RoutedEventArgs(TreeGridRow.ExpandedEvent, this));
            }
            else
            {
                this.OnCollapsed(new RoutedEventArgs(TreeGridRow.CollapsedEvent, this));
            }

            this.UpdateVisualState();
        }

        /// <summary>
        /// 左键点击时，选中该行。
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (!e.Handled && base.IsEnabled)
            {
                //在变更选择项前，需要保存 IsEdting 属性，后面会用到。
                var isEditing = false;
                var grid = this.TreeGrid;
                if (grid != null) { isEditing = grid.IsEditing; }

                //如果本行还没有被选中，则应该设置本行为选中行。
                bool isFocused = base.IsFocused;
                if (base.Focus())
                {
                    if (isFocused && !this.IsSelected)
                    {
                        this.Select(true);
                    }
                    //e.Handled = true;
                }

                //左键点击时，如果使用单击切换编辑行的模式，则切换编辑行到本行。
                if (isEditing &&
                    grid.EditingItem != this.DataContext &&
                    grid.RowEditingChangeMode == RowEditingChangeMode.SingleClick)
                {
                    grid.TryEditRow(this, e, null);
                }

                //不管是否成功获取焦点，都标记为已处理，否则父级结点会继续发生此事件而造成其被选中。
                e.Handled = true;

                //不需要 双击展开/折叠 的行为
                //if (e.ClickCount % 2 == 0)
                //{
                //    this.IsExpanded = this.IsExpanded;
                //    e.Handled = true;
                //}
            }

            base.OnMouseLeftButtonDown(e);
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            /**********************************************************************
             * 
             * 如果是自动化触发的焦点事件，则不需要调用 Select 方法，
             * 否则会造成选中该行，而导致下拉界面的关闭。
             * 
            **********************************************************************/

            if (!this._focusByAutomation)
            {
                //在获取焦点的时候选中当前行
                this.Select(true);
            }

            base.OnGotFocus(e);
        }

        private void UpdateVisualState()
        {
            //暂时不实现
        }
    }
}