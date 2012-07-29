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
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using OEA.Module.WPF.Controls;
using System.Diagnostics;

using System.Collections;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using OEA.MetaModel;
using OEA.MetaModel.View;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using OEA.Module.WPF.Automation;
using Hardcodet.Wpf.GenericTreeView;

namespace OEA.Module.WPF.Controls
{
    /// <summary>
    /// GridTreeView 控件中的一行
    /// </summary>
    [TemplatePart(Name = "PART_RowPresenter", Type = typeof(GridTreeViewRowPresenter))]
    public class GridTreeViewRow : TreeViewItem
    {
        static GridTreeViewRow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GridTreeViewRow), new FrameworkPropertyMetadata(typeof(GridTreeViewRow)));
        }

        public GridTreeViewRow(MultiTypesTreeGrid treeGrid, EntityViewMeta entityViewMeta)
        {
            this._treeGrid = treeGrid;
            this._entityViewMeta = entityViewMeta;

            this.PrepareToAdjustFirstColumnWidth();
        }

        #region 字段与属性

        private EntityViewMeta _entityViewMeta;

        private MultiTypesTreeGrid _treeGrid;

        public MultiTypesTreeGrid TreeGrid
        {
            get { return this._treeGrid; }
        }

        public EntityViewMeta EntityViewMeta
        {
            get { return this._entityViewMeta; }
        }

        #endregion

        #region Cells

        private List<MTTGCell> _cells = new List<MTTGCell>();

        internal void AddCell(MTTGCell cell)
        {
            if (!this._cells.Contains(cell))
            {
                var index = this._cells.Count;
                cell.Column = this._treeGrid.Columns[index] as TreeColumn;

                this._cells.Add(cell);
            }
        }

        /// <summary>
        /// 找到该行中的某一个单元格
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public MTTGCell FindCell(TreeColumn column)
        {
            return this._cells.Find(c => c.Column == column);
        }

        /// <summary>
        /// 获取该行中的某一个单元格，没有找到，则抛出异常。
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public MTTGCell GetCell(TreeColumn column)
        {
            var cell = this._cells.Find(c => c.Column == column);
            if (cell == null) throw new InvalidOperationException("当前行还没有加载完成，目前无法找到对应的单元格。");
            return cell;
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
                    var parent = ItemsControl.ItemsControlFromItemContainer(this) as GridTreeViewRow;
                    this._level = parent != null ? parent.Level + 1 : 0;
                }

                return this._level;
            }
        }

        #endregion

        #region RowNoProperty

        internal static readonly DependencyProperty RowNoProperty = DependencyProperty.Register(
            "RowNo", typeof(int), typeof(GridTreeViewRow),
            new PropertyMetadata(0, (d, e) => (d as GridTreeViewRow).OnRowNoChanged(e))
            );

        /// <summary>
        /// 此行应该显示行号
        /// </summary>
        internal int RowNo
        {
            get { return (int)this.GetValue(RowNoProperty); }
            set { this.SetValue(RowNoProperty, value); }
        }

        private void OnRowNoChanged(DependencyPropertyChangedEventArgs e)
        {
            //当前只是把值简单地同步到 RowHeader 上，这样，
            //由于 GridTreeViewRowPresenter 绑定了这个值，就可以把这个数字显示出来。
            this.RowHeader = e.NewValue;
        }

        #endregion

        #region RowHeaderProperty

        public static readonly DependencyProperty RowHeaderProperty = DependencyProperty.Register(
            "RowHeader", typeof(object), typeof(GridTreeViewRow)
            );

        /// <summary>
        /// RowHeader 是行头控件。
        /// </summary>
        public object RowHeader
        {
            get { return this.GetValue(RowHeaderProperty); }
            set { this.SetValue(RowHeaderProperty, value); }
        }

        #endregion

        #region 实现 ItemsControl 及重写父类的一些方法

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new GridTreeViewRow(this._treeGrid, this._entityViewMeta);
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is GridTreeViewRow;
        }

        protected override void OnCollapsed(RoutedEventArgs e)
        {
            base.OnCollapsed(e);

            this._treeGrid.OnNodeCollapsed(this);
        }

        protected override void OnExpanded(RoutedEventArgs e)
        {
            base.OnExpanded(e);

            this._treeGrid.OnNodeExpanded(this);
        }

        /// <summary>
        /// 防止 TreeViewItem 更改时导致容器 TreeViewItem 也触发 SelectedItemChanged
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Source != this) { e.Handled = true; }
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
             * 但是此时各子 GridTreeViewRow 并没有生成它们各自的 VisualChild，所以需要再次监听最后一个子节点的的 Loaded 事件，
             * 此时所有节点及其 VisualChild 都已经加载完成，可以开始调整第一列的宽度。
             * 这里没有选择 Expanded 事件的原因，主要是那个事件发生时，所有子节点 GridTreeViewRow 都没有生成。
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
                        var row = this.ItemContainerGenerator.ContainerFromItem(lastChild) as GridTreeViewRow;
                        row.Loaded += (oo, ee) =>
                        {
                            var columns = this.TreeGrid.Columns;
                            if (columns.Count > 0)
                            {
                                var firstColumn = columns[0] as GridTreeViewColumn;
                                firstColumn.RequestDataWidth();
                            }
                        };
                    }
                }
            };
        }

        #endregion

        #region IsMultiSelectedProperty

        public static readonly DependencyProperty IsMultiSelectedProperty = DependencyProperty.Register(
            "IsMultiSelected", typeof(bool), typeof(GridTreeViewRow),
            new UIPropertyMetadata(false, (d, e) => (d as GridTreeViewRow).OnIsMultiSelectedChanged(e))
            );

        /// <summary>
        /// 这个属性用于表示当前项是否已经被选中。
        /// 目前主要用于通知 Style 做出相应的颜色变换。
        /// </summary>
        public bool IsMultiSelected
        {
            get { return (bool)this.GetValue(IsMultiSelectedProperty); }
            set { this.SetValue(IsMultiSelectedProperty, value); }
        }

        private void OnIsMultiSelectedChanged(DependencyPropertyChangedEventArgs e)
        {
            var value = (bool)e.NewValue;

            this.IsUISelected = value;
        }

        #endregion

        #region IsChecked DependencyProperty

        public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register(
            "IsChecked", typeof(bool), typeof(GridTreeViewRow),
            new PropertyMetadata((d, e) => (d as GridTreeViewRow).OnIsCheckedChanged(e))
            );
        /// <summary>
        /// 是否被勾选中
        /// </summary>
        public bool IsChecked
        {
            get { return (bool)this.GetValue(IsCheckedProperty); }
            set { this.SetValue(IsCheckedProperty, value); }
        }

        private void OnIsCheckedChanged(DependencyPropertyChangedEventArgs e)
        {
            var value = (bool)e.NewValue;
            if (!this.IsSettingInternal)
            {
                //值改变的来源是：勾选视图时，勾选或者反勾选某一行。
                this.TreeGrid.CheckRowWithCascade(this, (bool)e.NewValue);
            }
        }

        /// <summary>
        /// 表明 IsChecked 的值是否正在被内部方法改变。
        /// </summary>
        internal bool IsSettingInternal;

        #endregion

        #region IsUISelected DependencyProperty

        public static readonly DependencyProperty IsUISelectedProperty = DependencyProperty.Register(
            "IsUISelected", typeof(bool), typeof(GridTreeViewRow)
            );

        /// <summary>
        /// 这个属性只用于界面绑定当前行的选中状态。
        /// </summary>
        public bool IsUISelected
        {
            get { return (bool)this.GetValue(IsUISelectedProperty); }
            set { this.SetValue(IsUISelectedProperty, value); }
        }

        #endregion

        #region 自动化

        internal bool _isAutomationFired = false;

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            /**********************************************************************
             * 
             * 如果是自动化触发的焦点事件，则需要屏蔽基类 OnGotFocus 方法中的 Select 方法，
             * 否则会造成该行的选中，导致下拉界面的关闭。
             * 
            **********************************************************************/

            if (!this._isAutomationFired)
            {
                //基类在获取焦点的时候会选中当前行
                base.OnGotFocus(e);
            }
            else
            {
                if (base.IsKeyboardFocused)
                {
                    this.BringIntoView();
                }
                this.RaiseEvent(e);
            }
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new GridTreeViewRowAutomationPeer(this);
        }

        #endregion
    }

    public class GridTreeViewRowAutomationPeer : TreeViewItemAutomationPeer, ISelectionItemProvider
    {
        private GridTreeViewRow _owner;

        public GridTreeViewRowAutomationPeer(GridTreeViewRow owner)
            : base(owner)
        {
            this._owner = owner;
        }

        protected override string GetNameCore()
        {
            var data = this._owner.DataContext;
            if (data != null)
            {
                var autoProperty = this._owner.EntityViewMeta.TryGetPrimayDisplayProperty();
                if (!string.IsNullOrWhiteSpace(autoProperty))
                {
                    var value = data.GetPropertyValue(autoProperty);
                    if (value != null) { return value.ToString(); }
                }
            }

            return base.GetNameCore();
        }

        protected override void SetFocusCore()
        {
            try
            {
                this._owner._isAutomationFired = true;

                base.SetFocusCore();
            }
            finally
            {
                this._owner._isAutomationFired = false;
            }
        }

        #region ISelectionItemProvider

        /**************************************************
         * 
         * 在勾选模式下，直接使用 IsMultiSelected 属性来实现 ISelectionItemProvider，
         * 如果不是勾选模式，则应该使用 IsSelected 属性。
         * 
         ***************************************************/

        bool ISelectionItemProvider.IsSelected
        {
            get { return this._owner.IsMultiSelected; }
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

        private DependencyProperty GetSelectedProperty()
        {
            if (this._owner.TreeGrid.CheckingMode == CheckingMode.CheckingRow)
            {
                return GridTreeViewRow.IsCheckedProperty;
                //return GridTreeViewRow.IsMultiSelectedProperty;
            }
            else
            {
                return GridTreeViewRow.IsSelectedProperty;
            }
        }

        #endregion
    }
}
