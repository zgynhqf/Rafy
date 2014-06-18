/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20111123
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20111123
 * 
*******************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime;
using System.Runtime.CompilerServices;
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
using System.Windows.Threading;
using Rafy.WPF.Controls;

namespace Rafy.WPF.Controls
{
    /// <summary>
    /// TreeGrid 控件中的一行
    /// </summary>
    public partial class TreeGridRow : HeaderedItemsControl
    {
        static TreeGridRow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TreeGridRow), new FrameworkPropertyMetadata(typeof(TreeGridRow)));
        }

        public TreeGridRow()
        {
            this.PrepareToAdjustFirstColumnWidth();
        }

        #region 字段与属性

        /// <summary>
        /// 内部使用的单元格容器控件
        /// </summary>
        internal TreeGridCellsPresenter CellsPresenter;

        /// <summary>
        /// 内部使用的单元格容器 Panel
        /// </summary>
        internal TreeGridCellsPanel CellsPanel;

        /// <summary>
        /// 当前行所在的 TreeGrid 容器。
        /// 如果 TreeGridRow 没有放在 TreeGrid 容器中时，这个属性返回 null.
        /// </summary>
        public TreeGrid TreeGrid
        {
            get
            {
                //找到对应的 TreeGrid 控件
                for (var parent = ItemsControl.ItemsControlFromItemContainer(this); parent != null; parent = ItemsControl.ItemsControlFromItemContainer(parent))
                {
                    var treeGrid = parent as TreeGrid;
                    if (treeGrid != null) { return treeGrid; }
                }

                return null;
            }
        }

        #endregion

        #region AutomationProperty AttachedDependencyProperty

        /// <summary>
        /// 每一行的 AutomationName 需要根据当前行绑定的实体来获取，而这个属性表示被绑定实体用于自动化的属性。
        /// 
        /// 该属性已被标记为 Inherits，这样，当显示为一棵树时，子行直接从父行获取值。
        /// </summary>
        public static readonly DependencyProperty AutomationPropertyProperty = DependencyProperty.RegisterAttached(
            "AutomationProperty", typeof(string), typeof(TreeGridRow),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits)
            );

        public static string GetAutomationProperty(FrameworkElement element)
        {
            return (string)element.GetValue(AutomationPropertyProperty);
        }

        public static void SetAutomationProperty(FrameworkElement element, string value)
        {
            element.SetValue(AutomationPropertyProperty, value);
        }

        #endregion

        #region HasChild DependencyProperty

        public static readonly DependencyProperty HasChildProperty = DependencyProperty.Register(
            "HasChild", typeof(bool), typeof(TreeGridRow)
            );

        /// <summary>
        /// 表示当前行是否有子行。
        /// 
        /// 如果有子行，应该显示一个用于展开子行的“加号”按钮。
        /// </summary>
        public bool HasChild
        {
            get { return (bool)this.GetValue(HasChildProperty); }
            set { this.SetValue(HasChildProperty, value); }
        }

        #endregion

        #region IsChecked DependencyProperty

        public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register(
            "IsChecked", typeof(bool), typeof(TreeGridRow),
            new PropertyMetadata((d, e) => (d as TreeGridRow).OnIsCheckedChanged(e))
            );
        /// <summary>
        /// 是否被勾选中
        /// </summary>
        public bool IsChecked
        {
            get { return (bool)this.GetValue(IsCheckedProperty); }
            set { this.SetValue(IsCheckedProperty, value); }
        }

        internal void SetIsCheckedField(bool value)
        {
            try
            {
                this._isUsingFields = true;
                this.IsChecked = value;
            }
            finally
            {
                this._isUsingFields = false;
            }
        }

        private void OnIsCheckedChanged(DependencyPropertyChangedEventArgs e)
        {
            if (!this._isUsingFields)
            {
                //值改变的来源是：勾选视图时，勾选或者反勾选某一行。
                var grid = this.TreeGrid;
                if (grid != null)
                {
                    grid.CheckRowWithCascade(this, (bool)e.NewValue);
                }
            }
        }

        #endregion

        #region RowNoProperty

        internal static readonly DependencyProperty RowNoProperty = DependencyProperty.Register(
            "RowNo", typeof(int), typeof(TreeGridRow),
            new PropertyMetadata(0, (d, e) => (d as TreeGridRow).OnRowNoChanged(e))
            );

        /// <summary>
        /// 此行应该显示行号
        /// </summary>
        public int RowNo
        {
            get { return (int)this.GetValue(RowNoProperty); }
            internal set { this.SetValue(RowNoProperty, value); }
        }

        private void OnRowNoChanged(DependencyPropertyChangedEventArgs e)
        {
            //如果行号是偶数，则设置本行的背景色为 TreeGrid 中定义的偶数行背景色。
            this.IsAlternatingRow = (int)e.NewValue % 2 == 0;
        }

        #endregion

        #region IsAlternatingRow DependencyProperty

        public static readonly DependencyProperty IsAlternatingRowProperty = DependencyProperty.Register(
            "IsAlternatingRow", typeof(bool), typeof(TreeGridRow)
            );

        /// <summary>
        /// 是否为交替行
        /// </summary>
        public bool IsAlternatingRow
        {
            get { return (bool)this.GetValue(IsAlternatingRowProperty); }
            set { this.SetValue(IsAlternatingRowProperty, value); }
        }

        #endregion

        #region Level

        private int _level = -1;

        /// <summary>
        /// 树中的级别
        /// 根是 0，其下每一级加 1。
        /// </summary>
        public int Level
        {
            get
            {
                if (this._level == -1)
                {
                    var parent = ItemsControl.ItemsControlFromItemContainer(this) as TreeGridRow;
                    this._level = parent != null ? parent.Level + 1 : 0;
                }

                return this._level;
            }
        }

        #endregion

        #region Cells

        /// <summary>
        /// 找到该行中的某一个单元格
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public TreeGridCell FindCell(TreeGridColumn column)
        {
            TreeGridCell cell = null;

            if (this.CellsPresenter != null)
            {
                var grid = this.TreeGrid;
                if (grid != null)
                {
                    var columnIndex = grid.Columns.IndexOf(column);
                    if (columnIndex >= 0)
                    {
                        cell = this.CellsPresenter
                            .ItemContainerGenerator.ContainerFromIndex(columnIndex) as TreeGridCell;
                    }
                }
            }

            return cell;
        }

        /// <summary>
        /// 滚动到指定列，等待其生成完毕后，返回该行中的某一个单元格。如果还没有找到，则抛出异常。
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public TreeGridCell ScrollToCell(TreeGridColumn column)
        {
            var grid = this.TreeGrid;
            if (grid != null && this.CellsPresenter != null)
            {
                var columnIndex = grid.Columns.IndexOf(column);
                if (columnIndex >= 0)
                {
                    int i = 0;
                    TreeGridCell cell = null;
                    while (cell == null && i++ < 1000)//1000 进入防止死循环
                    {
                        grid.BringColumnIntoView(columnIndex);
                        Dispatcher.Invoke(DispatcherPriority.Background, (DispatcherOperationCallback)delegate(object unused)
                        {
                            cell = this.CellsPresenter
                                .ItemContainerGenerator.ContainerFromIndex(columnIndex) as TreeGridCell;
                            return null;
                        }, null);
                    }

                    return cell;
                }
            }

            throw new NotSupportedException("无法滚动到指定的列：" + column.HeaderLabel + "，可能当前行还没有加载完成。");
        }

        /// <summary>
        /// 遍历行中所有已经生成的单元格。
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<TreeGridCell> TraverseCells()
        {
            if (this.CellsPresenter != null)
            {
                var grid = this.TreeGrid;
                if (grid != null)
                {
                    for (int i = 0, c = grid.Columns.Count; i < c; i++)
                    {
                        var cell = this.CellsPresenter
                            .ItemContainerGenerator.ContainerFromIndex(i) as TreeGridCell;

                        if (cell != null) yield return cell;
                    }
                }
            }
        }

        #endregion

        #region 重写父类方法 并实现 ItemsControl

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            //左键点击时，通知树型控件需要进入编辑状态。
            var grid = this.TreeGrid;
            if (grid != null)
            {
                //未进入编辑状态；或者已经进入编辑状态，并使用双击切换时。本行进入编辑状态。
                if (!grid.IsEditing ||
                    grid.EditingItem != this.DataContext &&
                    grid.RowEditingChangeMode == RowEditingChangeMode.DoubleClick)
                {
                    grid.TryEditRow(this, e, null);
                }
            }

            base.OnMouseDoubleClick(e);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.SetAutomationName();
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new TreeGridRow();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is TreeGridRow;
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            var treeGrid = this.TreeGrid;
            if (treeGrid != null)
            {
                treeGrid.PrepareRowForItem(element, item);
            }
            else
            {
                base.PrepareContainerForItemOverride(element, item);
            }
        }

        #endregion

        #region 自动扩张第一列的宽度
        //解决问题：http://ipm.grandsoft.com.cn/issues/166987

        /// <summary>
        /// 监听适当的事件，以触发调整宽度的代码。
        /// 
        /// 构造函数调用此方法。
        /// </summary>
        private void PrepareToAdjustFirstColumnWidth()
        {
            /*********************** 代码块解释 *********************************
             * 
             * 目前所选择的触发条件是：
             * 当一个节点被 Expand 时，该节点下的子节点会生成，
             * ItemContainerGenerator.StatusChanged 事件会发生，且 Status 是 GeneratorStatus.ContainersGenerated。
             * 但是此时各子 TreeGridRow 并没有生成它们各自的 VisualChild，所以需要再次监听最后一个子节点的的 Loaded 事件，
             * 此时所有节点及其 VisualChild 都已经加载完成，可以开始调整第一列的宽度。
             * 这里没有选择 Expanded 事件的原因，主要是那个事件发生时，所有子节点 TreeGridRow 都没有生成。
             * 
            **********************************************************************/

            this.ItemContainerGenerator.StatusChanged += (o, e) =>
            {
                if (this.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
                {
                    var children = this.Items;
                    if (children.Count > 0)
                    {
                        var lastChild = children[children.Count - 1];
                        var row = this.ItemContainerGenerator.ContainerFromItem(lastChild) as TreeGridRow;
                        row.Loaded += (oo, ee) =>
                        {
                            var treeGrid = this.TreeGrid;
                            if (treeGrid != null)
                            {
                                var columns = treeGrid.Columns;
                                if (columns.Count > 0)
                                {
                                    var firstColumn = columns[0] as TreeGridColumn;
                                    firstColumn.RequestDataWidth();
                                }
                            }
                        };
                    }
                }
            };
        }

        #endregion

        #region 自动化

        private void SetAutomationName()
        {
            //获取当前行可用的自动化属性。
            var value = GetAutomationProperty(this);
            if (!string.IsNullOrEmpty(value))
            {
                this.SetBinding(AutomationProperties.NameProperty, value);
            }
        }

        internal bool _focusByAutomation = false;

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new TreeGridRowAutomationPeer(this);
        }

        #endregion
    }

    public class TreeGridRowAutomationPeer : ItemsControlAutomationPeer, ISelectionItemProvider
    {
        private TreeGridRow _owner;

        public TreeGridRowAutomationPeer(TreeGridRow owner)
            : base(owner)
        {
            this._owner = owner;
        }

        //protected override string GetNameCore()
        //{
        //    var data = this._owner.DataContext;
        //    if (data != null)
        //    {
        //        var autoProperty = this._owner.EntityViewMeta.TryGetPrimayDisplayProperty();
        //        if (!string.IsNullOrWhiteSpace(autoProperty))
        //        {
        //            var value = data.GetPropertyValue(autoProperty);
        //            if (value != null) { return value.ToString(); }
        //        }
        //    }

        //    return base.GetNameCore();
        //}

        protected override void SetFocusCore()
        {
            try
            {
                this._owner._focusByAutomation = true;

                base.SetFocusCore();
            }
            finally
            {
                this._owner._focusByAutomation = false;
            }
        }

        #region ISelectionItemProvider

        bool ISelectionItemProvider.IsSelected
        {
            get { return (bool)this._owner.GetValue(this.GetSelectedProperty()); }
        }

        void ISelectionItemProvider.AddToSelection()
        {
            this._owner.SetValue(this.GetSelectedProperty(), true);
        }

        void ISelectionItemProvider.RemoveFromSelection()
        {
            this._owner.SetValue(this.GetSelectedProperty(), false);
        }

        void ISelectionItemProvider.Select()
        {
            this._owner.SetValue(this.GetSelectedProperty(), true);
        }

        /// <summary>
        /// 在勾选模式下，直接使用 IsCheckedProperty 属性来实现 ISelectionItemProvider，
        /// 如果不是勾选模式，则应该使用 IsSelected 属性。
        /// </summary>
        /// <returns></returns>
        private DependencyProperty GetSelectedProperty()
        {
            var treeGrid = this._owner.TreeGrid;
            if (treeGrid != null && treeGrid.IsCheckingEnabled)
            {
                return TreeGridRow.IsCheckedProperty;
            }
            else
            {
                return TreeGridRow.IsSelectedProperty;
            }
        }

        #endregion

        #region 暂未实现

        protected override ItemAutomationPeer CreateItemAutomationPeer(object item)
        {
            return new TreeGridRowDataItemAutomationPeer(item, this);
        }

        public IRawElementProviderSimple SelectionContainer
        {
            get
            {
                var parentItemsControl = this._owner.ParentItemsControl;
                if (parentItemsControl != null)
                {
                    var automationPeer = UIElementAutomationPeer.FromElement(parentItemsControl);
                    if (automationPeer != null)
                    {
                        return base.ProviderFromPeer(automationPeer);
                    }
                }
                return null;
            }
        }

        #endregion
    }

    public class TreeGridRowDataItemAutomationPeer : ItemAutomationPeer
    {
        public TreeGridRowDataItemAutomationPeer(object item, ItemsControlAutomationPeer itemsControlAutomationPeer)
            : base(item, itemsControlAutomationPeer) { }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.Custom;
        }

        protected override string GetClassNameCore()
        {
            return typeof(TreeGridRow).Name;
        }
    }

}