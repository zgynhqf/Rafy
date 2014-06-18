/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：2011
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 2011
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Threading;
using System.Windows.Input;
using System.Windows.Automation;

namespace Rafy.WPF.Controls
{
    /// <summary>
    /// MTTG 控件中对应的每一个单元格对象
    /// 
    /// 实现：
    /// 行对象的查找直接使用可视树即可，而列对象则需要在构造的时候赋值（模板中完成）。
    /// </summary>
    public class TreeGridCell : ContentControl
    {
        static TreeGridCell()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TreeGridCell), new FrameworkPropertyMetadata(typeof(TreeGridCell)));
        }

        private TreeGridRow _row;

        /// <summary>
        /// 获取包含该单元格的行。
        /// 
        /// 如果当前 TreeGridCell 没有放到 TreeGridRow 中，则返回 null。
        /// </summary>
        public TreeGridRow Row
        {
            get { return this._row; }
        }

        /// <summary>
        /// 获取包含该单元格的表格控件。
        /// </summary>
        private TreeGrid TreeGrid
        {
            get
            {
                if (this._row != null) { return this._row.TreeGrid; }

                return null;
            }
        }

        /// <summary>
        /// 对应的列
        /// </summary>
        public TreeGridColumn Column { get; internal set; }

        /// <summary>
        /// 在虚拟化回收过程中，是否本单元格正在被显示。而不是被回收了。
        /// </summary>
        internal bool VisibleOnVirtualizing = true;

        #region IsEditing DependencyProperty

        private bool _isUsingFields = false;

        internal void SetIsEditingField(bool value)
        {
            try
            {
                this._isUsingFields = true;

                this.IsEditing = value;
            }
            finally
            {
                this._isUsingFields = false;
            }
        }

        public static readonly DependencyProperty IsEditingProperty = DependencyProperty.Register(
            "IsEditing", typeof(bool), typeof(TreeGridCell),
            new PropertyMetadata(false, (d, e) => (d as TreeGridCell).OnIsEditingChanged(e))
            );

        /// <summary>
        /// 当前单元格是否正处于编辑状态下。
        /// </summary>
        public bool IsEditing
        {
            get { return (bool)this.GetValue(IsEditingProperty); }
            set { this.SetValue(IsEditingProperty, value); }
        }

        private void OnIsEditingChanged(DependencyPropertyChangedEventArgs e)
        {
            //如果是外部设置值，则应该把更新编辑状态的事件递交到 TreeGrid 去执行。
            if (!this._isUsingFields)
            {
                throw new NotSupportedException("暂时不支持直接设置此属性以改变编辑状态，请调用 TreeGrid 相关方法。");
            }

            var value = (bool)e.NewValue;
            this.UpdateContent(value);
            this.UpdateContentVisual();
        }

        /// <summary>
        /// 更新指定的 TreeGridCell 内容为编辑状态/非编辑状态。
        /// </summary>
        /// <param name="lvi"></param>
        /// <param name="isEditing"></param>
        internal void UpdateContent(bool isEditing)
        {
            if (isEditing)
            {
                //设置编辑元素为 Content，清空 ContentTemplate 后才可显示。
                this.Content = this.Column.GenerateEditingElement();
                this.ContentTemplate = null;
                this.ContentTemplateSelector = null;
            }
            else
            {
                //使用指定的模板来显示内容。
                this.Content = this.DataContext;
                this.ContentTemplate = this.Column.GetDisplayCellTemplate();
                this.ContentTemplateSelector = this.Column.CellContentTemplateSelector;
            }
        }

        private void UpdateContentVisual()
        {
            //只有调用 contentPresenter.ApplyTemplate 后，content 与 Cell 的可视树关系才被建立。
            this.ApplyTemplate();
            var contentPresenter = this.GetVisualChild<ContentPresenter>();
            if (contentPresenter != null)
            {
                contentPresenter.ApplyTemplate();
            }
        }

        #endregion

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var rowPresenter = ItemsControl.ItemsControlFromItemContainer(this) as TreeGridCellsPresenter;
            if (rowPresenter != null) { this._row = rowPresenter.Row; }
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            //左键点击时，通知树型控件需要进入编辑状态。
            var grid = this.TreeGrid;
            if (grid != null)
            {
                grid.TryEditCell(this, e);
            }
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            //在编辑控件按 Tab 切换到下一列
            if (e.Key == Key.Tab)
            {
                bool moveNext = !TreeGridHelper.IsShiftPressed();
                this.Column.EditNextColumnOnTabKey(this, moveNext, e, this.Column);
            }
        }

        /// <summary>
        /// 如果当前单元格正处于编辑状态中，则把焦点定位到编辑器中。
        /// </summary>
        /// <param name="editingEventArgs">引发此操作的事件参数。可为 null。</param>
        internal void FocusOnEditing(RoutedEventArgs editingEventArgs)
        {
            if (this.IsEditing)
            {
                if (editingEventArgs == null)
                {
                    editingEventArgs = new RoutedEventArgs(UIElement.MouseDownEvent, this);
                }

                //模仿 DataGrid.BeginEdit(FrameworkElement editingElement, RoutedEventArgs e) 方法
                var editingElement = this.Content as FrameworkElement;
                if (editingElement != null)
                {
                    editingElement.UpdateLayout();
                    editingElement.Focus();

                    this.Column.PrepareElementForEdit(editingElement, editingEventArgs);
                }
            }
        }

        #region UI Automation

        //*****************************
        //MTTGCellAutomationPeer 类被 UIAutomation 调用 Invoke 时，实现以下功能：
        //当前单元格进入编辑状态
        //*****************************

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new MTTGCellAutomationPeer(this);
        }

        private class MTTGCellAutomationPeer : FrameworkElementAutomationPeer, IInvokeProvider
        {
            public MTTGCellAutomationPeer(TreeGridCell owner) : base(owner) { }

            public override object GetPattern(PatternInterface patternInterface)
            {
                //支持 Invoke
                if (patternInterface == PatternInterface.Invoke) { return this; }

                return base.GetPattern(patternInterface);
            }

            void IInvokeProvider.Invoke()
            {
                Action a = () =>
                {
                    var cell = this.Owner as TreeGridCell;
                    var grid = cell.TreeGrid;
                    if (grid != null)
                    {
                        grid.TryEditCell(cell);
                    }
                };
                this.Dispatcher.BeginInvoke(DispatcherPriority.Input, a);
            }
        }

        #endregion
    }
}