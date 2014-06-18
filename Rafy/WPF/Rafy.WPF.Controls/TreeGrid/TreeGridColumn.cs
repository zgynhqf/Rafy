/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110217
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20110217
 * 
*******************************************************/

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Rafy.WPF.Controls
{
    [DebuggerDisplay("HeaderLabel: {HeaderLabel}")]
    public abstract class TreeGridColumn : DependencyObject, INotifyPropertyChanged
    {
        #region 字段及构造函数

        private TreeGridColumnHeader _header;

        /// <summary>
        /// 本列在 TreeGridColumnCollection 集合对象中内部 _columns 列表中的索引。
        /// </summary>
        internal int StableIndex;

        public TreeGridColumn()
        {
            this.ResetPrivateData();
        }

        /// <summary>
        /// 重置所有字段到起始状态。
        /// </summary>
        internal void ResetPrivateData()
        {
            this.StableIndex = -1;
            this._desiredDataWidth = 0.0;
            this._state = double.IsNaN(this.Width) ? ColumnMeasureState.Init : ColumnMeasureState.SpecificWidth;
        }

        #endregion

        /// <summary>
        /// 对应的 TreeGrid 控件
        /// </summary>
        public TreeGrid TreeGrid { get; internal set; }

        #region 依赖属性 及 公有属性

        #region SortDirection DependencyProperty

        private static readonly DependencyPropertyKey SortDirectionPropertyKey = DependencyProperty.RegisterReadOnly(
            "SortDirection", typeof(TreeGridColumnSortDirection), typeof(TreeGridColumn),
            new PropertyMetadata(TreeGridColumnSortDirection.None, (d, e) => (d as TreeGridColumn).OnSortDirectionChanged(e))
            );

        public static readonly DependencyProperty SortDirectionProperty = SortDirectionPropertyKey.DependencyProperty;

        /// <summary>
        /// 本列的排序方向。
        /// </summary>
        /// 主要用于控制界面中上下箭头的显示。
        public TreeGridColumnSortDirection SortDirection
        {
            get { return (TreeGridColumnSortDirection)this.GetValue(SortDirectionProperty); }
            internal set { this.SetValue(SortDirectionPropertyKey, value); }
        }

        private void OnSortDirectionChanged(DependencyPropertyChangedEventArgs e)
        {
            var value = (TreeGridColumnSortDirection)e.NewValue;
            if (_header != null)
            {
                _header.SortDirection = value;
            }
        }

        #endregion

        #region HeaderLabel DependencyProperty

        public static readonly DependencyProperty HeaderLabelProperty = DependencyProperty.Register(
            "HeaderLabel", typeof(string), typeof(TreeGridColumn),
            new PropertyMetadata((d, e) => (d as TreeGridColumn).OnHeaderLabelChanged(e))
            );

        /// <summary>
        /// 列头用于显示的名称
        /// </summary>
        public string HeaderLabel
        {
            get { return (string)this.GetValue(HeaderLabelProperty); }
            set { this.SetValue(HeaderLabelProperty, value); }
        }

        private void OnHeaderLabelChanged(DependencyPropertyChangedEventArgs e)
        {
            var value = (string)e.NewValue;
            _header = new TreeGridColumnHeader
            {
                Content = value
            };
            this.Header = _header;
        }

        #endregion

        #region PropertyName DependencyProperty

        public static readonly DependencyProperty PropertyNameProperty = DependencyProperty.Register(
            "PropertyName", typeof(string), typeof(TreeGridColumn),
            new PropertyMetadata((d, e) => (d as TreeGridColumn).OnPropertyNameChanged(e))
            );

        /// <summary>
        /// 列对应的属性名。
        /// 设置此属性可以简单地设置相应的 Binding。
        /// 同时影响排序功能：此属性即是用于排序的属性的名称。
        /// </summary>
        public string PropertyName
        {
            get { return (string)this.GetValue(PropertyNameProperty); }
            set { this.SetValue(PropertyNameProperty, value); }
        }

        private void OnPropertyNameChanged(DependencyPropertyChangedEventArgs e)
        {
            var value = (string)e.NewValue;
            if (!string.IsNullOrEmpty(value) && this.Binding == null)
            {
                this.Binding = new Binding(value);
            }
        }

        #endregion

        #region SortingProperty DependencyProperty

        public static readonly DependencyProperty SortingPropertyProperty = DependencyProperty.Register(
            "SortingProperty", typeof(string), typeof(TreeGridColumn)
            );

        /// <summary>
        /// 排序功能起作用时，按照此属性进行排序。
        /// </summary>
        public string SortingProperty
        {
            get { return (string)this.GetValue(SortingPropertyProperty); }
            set { this.SetValue(SortingPropertyProperty, value); }
        }

        #endregion

        #region Binding DependencyProperty

        public static readonly DependencyProperty BindingProperty = DependencyProperty.Register(
            "Binding", typeof(BindingBase), typeof(TreeGridColumn),
            new PropertyMetadata((d, e) => (d as TreeGridColumn).OnBindingChanged(e))
            );

        /// <summary>
        /// 显示、编辑控件都使用的绑定。
        /// </summary>
        public BindingBase Binding
        {
            get { return (BindingBase)this.GetValue(BindingProperty); }
            set { this.SetValue(BindingProperty, value); }
        }

        protected virtual void OnBindingChanged(DependencyPropertyChangedEventArgs e)
        {
            this.OnPropertyChanged(BindingProperty.Name);
        }

        #endregion

        #region DisplayTextBlockStyle DependencyProperty

        public static readonly DependencyProperty DisplayTextBlockStyleProperty = DependencyProperty.Register(
            "DisplayTextBlockStyle", typeof(Style), typeof(TreeGridColumn)
            );

        /// <summary>
        /// 显示模式下的 TextBlock 样式。
        /// </summary>
        public Style DisplayTextBlockStyle
        {
            get { return (Style)this.GetValue(DisplayTextBlockStyleProperty); }
            set { this.SetValue(DisplayTextBlockStyleProperty, value); }
        }

        #endregion

        #region Header DependencyProperty

        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
            "Header", typeof(object), typeof(TreeGridColumn),
            new FrameworkPropertyMetadata((d, e) => (d as TreeGridColumn).OnHeaderChanged(e))
            );

        public object Header
        {
            get { return (object)this.GetValue(HeaderProperty); }
            set { this.SetValue(HeaderProperty, value); }
        }

        private void OnHeaderChanged(DependencyPropertyChangedEventArgs e)
        {
            this.OnPropertyChanged(HeaderProperty.Name);
        }

        #endregion

        #region HeaderContainerStyle DependencyProperty

        public static readonly DependencyProperty HeaderContainerStyleProperty = DependencyProperty.Register(
            "HeaderContainerStyle", typeof(Style), typeof(TreeGridColumn),
            new FrameworkPropertyMetadata((d, e) => (d as TreeGridColumn).OnHeaderContainerStyleChanged(e))
            );

        public Style HeaderContainerStyle
        {
            get { return (Style)this.GetValue(HeaderContainerStyleProperty); }
            set { this.SetValue(HeaderContainerStyleProperty, value); }
        }

        private void OnHeaderContainerStyleChanged(DependencyPropertyChangedEventArgs e)
        {
            this.OnPropertyChanged(HeaderContainerStyleProperty.Name);
        }

        #endregion

        #region HeaderTemplate DependencyProperty

        public static readonly DependencyProperty HeaderTemplateProperty = DependencyProperty.Register(
            "HeaderTemplate", typeof(DataTemplate), typeof(TreeGridColumn),
            new FrameworkPropertyMetadata((d, e) => (d as TreeGridColumn).OnHeaderTemplateChanged(e))
            );

        public DataTemplate HeaderTemplate
        {
            get { return (DataTemplate)this.GetValue(HeaderTemplateProperty); }
            set { this.SetValue(HeaderTemplateProperty, value); }
        }

        private void OnHeaderTemplateChanged(DependencyPropertyChangedEventArgs e)
        {
            TreeGridHelper.CheckTemplateAndTemplateSelector("Header", HeaderTemplateProperty, HeaderTemplateSelectorProperty, this);
            this.OnPropertyChanged(HeaderTemplateProperty.Name);
        }

        #endregion

        #region HeaderTemplateSelector DependencyProperty

        public static readonly DependencyProperty HeaderTemplateSelectorProperty = DependencyProperty.Register(
            "HeaderTemplateSelector", typeof(DataTemplateSelector), typeof(TreeGridColumn),
            new FrameworkPropertyMetadata((d, e) => (d as TreeGridColumn).OnHeaderTemplateSelectorChanged(e))
            );

        public DataTemplateSelector HeaderTemplateSelector
        {
            get { return (DataTemplateSelector)this.GetValue(HeaderTemplateSelectorProperty); }
            set { this.SetValue(HeaderTemplateSelectorProperty, value); }
        }

        private void OnHeaderTemplateSelectorChanged(DependencyPropertyChangedEventArgs e)
        {
            TreeGridHelper.CheckTemplateAndTemplateSelector("Header", HeaderTemplateProperty, HeaderTemplateSelectorProperty, this);
            this.OnPropertyChanged(TreeGridColumn.HeaderTemplateSelectorProperty.Name);
        }

        #endregion

        #region HeaderStringFormat DependencyProperty

        public static readonly DependencyProperty HeaderStringFormatProperty = DependencyProperty.Register(
            "HeaderStringFormat", typeof(string), typeof(TreeGridColumn)
            );

        public string HeaderStringFormat
        {
            get { return (string)this.GetValue(HeaderStringFormatProperty); }
            set { this.SetValue(HeaderStringFormatProperty, value); }
        }

        #endregion

        #region CellStyle DependencyProperty

        public static readonly DependencyProperty CellStyleProperty = DependencyProperty.Register(
            "CellStyle", typeof(Style), typeof(TreeGridColumn),
            new PropertyMetadata((d, e) => (d as TreeGridColumn).OnCellStyleChanged(e))
            );

        /// <summary>
        /// 单元格样式
        /// </summary>
        public Style CellStyle
        {
            get { return (Style)this.GetValue(CellStyleProperty); }
            set { this.SetValue(CellStyleProperty, value); }
        }

        private void OnCellStyleChanged(DependencyPropertyChangedEventArgs e)
        {
            this.OnPropertyChanged(CellStyleProperty.Name);
        }

        #endregion

        #region CellContentTemplate DependencyProperty

        public static readonly DependencyProperty CellContentTemplateProperty = DependencyProperty.Register(
            "CellContentTemplate", typeof(DataTemplate), typeof(TreeGridColumn),
            new PropertyMetadata((d, e) => (d as TreeGridColumn).OnCellContentTemplateChanged(e))
            );

        /// <summary>
        /// TreeGridCell 中的内容模板。
        /// </summary>
        public DataTemplate CellContentTemplate
        {
            get { return (DataTemplate)this.GetValue(CellContentTemplateProperty); }
            set { this.SetValue(CellContentTemplateProperty, value); }
        }

        private void OnCellContentTemplateChanged(DependencyPropertyChangedEventArgs e)
        {
            this.OnPropertyChanged(CellContentTemplateProperty.Name);
        }

        #endregion

        #region CellContentTemplateSelector DependencyProperty

        public static readonly DependencyProperty CellContentTemplateSelectorProperty = DependencyProperty.Register(
            "CellContentTemplateSelector", typeof(DataTemplateSelector), typeof(TreeGridColumn),
            new PropertyMetadata((d, e) => (d as TreeGridColumn).OnCellContentTemplateSelectorChanged(e))
            );

        public DataTemplateSelector CellContentTemplateSelector
        {
            get { return (DataTemplateSelector)this.GetValue(CellContentTemplateSelectorProperty); }
            set { this.SetValue(CellContentTemplateSelectorProperty, value); }
        }

        private void OnCellContentTemplateSelectorChanged(DependencyPropertyChangedEventArgs e)
        {
            this.OnPropertyChanged(CellContentTemplateSelectorProperty.Name);
        }

        #endregion

        #endregion

        #region 宽度计算

        /***********************GridView 自动列宽的原理及实现*************************
         * 
         * GridView 自动列宽的原理：
         * 调用 RequestDataWidth 方法之后，列的状态被设置为 Init，表明需要自动列宽功能。
         * 接下来到达界面测量过程时，先是 TreeGridHeaderRowPresenter 发生它的 MeasureOverride 方法，其中主要完成对最小动态列宽的限制；
         * 然后是每一行的 TreeGridRowPresenter 进行测量，其会根据每一行的对应列的宽度来调用本类的 EnsureWidth 方法，来保证所有行的列都使用同一个列宽。
         * 
         * 代码实现：
         * 相关的代码可以参见：TreeGridHeaderRowPresenter.MeasureOverride 以及 TreeGridRowPresenter.MeasureOverrideReflected 方法
         * 
         * 该部分代码主要是为了重新实现基类 MeasureOverride 方法中 EnsureWidth 方法的逻辑：
         * 由于我们需要 EnsureWidth 方法中考虑 TreeGridColumn.MinDataWidth/MaxDataWidth 两个属性，
         * 所以我们需要重写 EnsureWidth 方法，但是基类并没有提供这个扩展点，所以无奈之下，只能使用复制代码 + 反射的方案：
         * 也就是说：把整个 base.MeasureOverride 的方法复制出来，私有属性使用反射（尽量缓存），同时，
         * 重写新的 TreeGridColumn.EnsureWidth 方法，使其最后的值处于 TreeGridColumn.MinDataWidth/MaxDataWidth 之间。
         * 
        **********************************************************************/

        /// <summary>
        /// 当前需要的动态列宽（由 EnsureWidth 方法来动态变大）
        /// </summary>
        private double _desiredDataWidth;

        private double _actualWidth;

        private ColumnMeasureState _state;

        private bool _maxDataWidthEnabled = true;

        #region Width DependencyProperty

        public static readonly DependencyProperty WidthProperty = FrameworkElement.WidthProperty.AddOwner(typeof(TreeGridColumn),
            new PropertyMetadata(double.NaN, (d, e) => (d as TreeGridColumn).OnWidthChanged(e), (d, baseValue) => (d as TreeGridColumn).CoerceWidthChanged(baseValue)));

        [TypeConverter(typeof(LengthConverter))]
        public double Width
        {
            get { return (double)this.GetValue(WidthProperty); }
            set { this.SetValue(WidthProperty, value); }
        }

        private void OnWidthChanged(DependencyPropertyChangedEventArgs e)
        {
            var value = (double)e.NewValue;
            this.State = double.IsNaN(value) ? ColumnMeasureState.Init : ColumnMeasureState.SpecificWidth;

            this.OnPropertyChanged(WidthProperty.Name);
        }

        private object CoerceWidthChanged(object baseValue)
        {
            var value = (double)baseValue;

            var max = this.MaxWidth;
            if (!double.IsNaN(max) && max < value)
            {
                value = max;
            }
            else
            {
                var min = this.MinWidth;
                if (!double.IsNaN(min) && min > value)
                {
                    value = min;
                }
            }

            return value;
        }

        #endregion

        #region MinDataWidthProperty

        public static readonly DependencyProperty MinDataWidthProperty = DependencyProperty.Register(
            "MinDataWidth", typeof(double), typeof(TreeGridColumn),
            new PropertyMetadata(100d)
            );

        /// <summary>
        /// 动态计算列宽时，不应该低于这个值。默认为 100。
        /// </summary>
        public double MinDataWidth
        {
            get { return (double)this.GetValue(MinDataWidthProperty); }
            set { this.SetValue(MinDataWidthProperty, value); }
        }

        #endregion

        #region MaxDataWidthProperty

        public static readonly DependencyProperty MaxDataWidthProperty = DependencyProperty.Register(
            "MaxDataWidth", typeof(double), typeof(TreeGridColumn),
            new PropertyMetadata(400d)
            );

        /// <summary>
        /// 动态计算列宽时，不应该超过这个值。默认为 400。
        /// </summary>
        public double MaxDataWidth
        {
            get { return (double)this.GetValue(MaxDataWidthProperty); }
            set { this.SetValue(MaxDataWidthProperty, value); }
        }

        #endregion

        #region MinWidthProperty

        public static readonly DependencyProperty MinWidthProperty = DependencyProperty.Register(
            "MinWidth", typeof(double), typeof(TreeGridColumn),
            new PropertyMetadata(40d)
            );

        /// <summary>
        /// 最小宽度。
        /// 如果被设置为 double.NaN 则表示不限制。默认为 40。
        /// </summary>
        public double MinWidth
        {
            get { return (double)this.GetValue(MinWidthProperty); }
            set { this.SetValue(MinWidthProperty, value); }
        }

        #endregion

        #region MaxWidthProperty

        public static readonly DependencyProperty MaxWidthProperty = DependencyProperty.Register(
            "MaxWidth", typeof(double), typeof(TreeGridColumn),
            new PropertyMetadata(double.NaN)
            );

        /// <summary>
        /// 最大宽度。
        /// 如果被设置为 double.NaN 则表示不限制。默认为 double.NaN。
        /// </summary>
        public double MaxWidth
        {
            get { return (double)this.GetValue(MaxWidthProperty); }
            set { this.SetValue(MaxWidthProperty, value); }
        }

        #endregion

        /// <summary>
        /// 最终显示的实际宽度
        /// </summary>
        public double ActualWidth
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get { return this._actualWidth; }
            private set
            {
                if (!double.IsNaN(value) && !double.IsInfinity(value))
                {
                    if (value < 0.0) { return; }

                    if (this._actualWidth != value)
                    {
                        this._actualWidth = value;
                        this.OnPropertyChanged(c_ActualWidthName);
                    }
                }
            }
        }

        /// <summary>
        /// 当前的列的列宽的计算状态。
        /// 
        /// 基类的 State 属性
        /// </summary>
        internal ColumnMeasureState State
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get { return this._state; }
            set
            {
                if (this._state == value)
                {
                    if (value == ColumnMeasureState.SpecificWidth)
                    {
                        this.UpdateActualWidth();
                    }
                }
                else
                {
                    this._state = value;
                    if (value != ColumnMeasureState.Init)
                    {
                        this.UpdateActualWidth();
                    }
                    else
                    {
                        this._desiredDataWidth = 0.0;
                    }
                }
            }
        }

        /// <summary>
        /// 需要的动态宽度
        /// </summary>
        internal double DesiredDataWidth
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get { return this._desiredDataWidth; }
        }

        private void UpdateActualWidth()
        {
            this.ActualWidth = this.CalculateActualWidth();
        }

        /// <summary>
        /// 实时计算当前需要的实际宽度
        /// </summary>
        /// <returns></returns>
        internal double CalculateActualWidth()
        {
            return this._state == ColumnMeasureState.SpecificWidth ? this.Width : this._desiredDataWidth;
        }

        /// <summary>
        /// 重新计算该列的自动列宽
        /// </summary>
        /// <param name="maxDataWidthEnabled">
        /// 是否启用最大自动列宽限制（当双击列时，不需要限制）
        /// </param>
        public void RequestDataWidth(bool maxDataWidthEnabled = true)
        {
            /**********************************************************************
             * 
             * 本方法的代码来自于 GridViewColumnHeader.OnGripperDoubleClicked
             * 
             * Width 为 double.NaN 时表示需要动态列宽，但是这时可能已经计算完成了，
             * 所以需要先把 Width 设置为一个无关的值，再设置为 NaN，这样，它的 State 就会变为 Init 了。
             * 
            **********************************************************************/

            if (double.IsNaN(this.Width))
            {
                this.Width = this.ActualWidth;
            }
            this.Width = double.NaN;

            this._maxDataWidthEnabled = maxDataWidthEnabled;
        }

        /// <summary>
        /// 尽量保证动态宽度到指定的值。
        /// </summary>
        /// <param name="width">需要的动态宽度</param>
        /// <returns>最后可用的动态宽度</returns>
        internal double EnsureDataWidth(double width)
        {
            var changed = false;
            var value = this._desiredDataWidth;

            if (value < width)
            {
                changed = true;
                value = width;
            }

            //限制最大自动宽度
            if (this._maxDataWidthEnabled)
            {
                var minMaxWidth = double.MaxValue;
                var maxWidth = this.MaxWidth;
                if (!double.IsNaN(maxWidth)) minMaxWidth = maxWidth;
                var maxDataWidth = this.MaxDataWidth;
                if (!double.IsNaN(maxDataWidth)) minMaxWidth = Math.Min(minMaxWidth, maxDataWidth);

                if (value > minMaxWidth)
                {
                    value = minMaxWidth;
                    changed = true;
                }
            }

            //限制最小自动宽度
            var maxMinWidth = 0d;
            var minWidth = this.MinWidth;
            if (!double.IsNaN(minWidth)) maxMinWidth = minWidth;
            var minDataWidth = this.MinDataWidth;
            if (!double.IsNaN(minDataWidth)) maxMinWidth = Math.Max(maxMinWidth, minDataWidth);
            if (value < maxMinWidth)
            {
                value = maxMinWidth;
                changed = true;
            }

            if (changed)
            {
                this._desiredDataWidth = value;
            }

            return value;
        }

        #endregion

        #region 列的可见性

        internal TreeGridColumnCollection _ownerCollection;

        internal int _oldIndexAsUnvisible;

        #region IsVisible DependencyProperty

        public static readonly DependencyProperty IsVisibleProperty = DependencyProperty.Register(
            "IsVisible", typeof(bool), typeof(TreeGridColumn),
            new PropertyMetadata(true, (d, e) => (d as TreeGridColumn).OnIsVisibleChanged(e))
            );

        /// <summary>
        /// 该列是否可见。
        /// </summary>
        public bool IsVisible
        {
            get { return (bool)this.GetValue(IsVisibleProperty); }
            set { this.SetValue(IsVisibleProperty, value); }
        }

        private void OnIsVisibleChanged(DependencyPropertyChangedEventArgs e)
        {
            if (this._ownerCollection == null) return;
            var value = (bool)e.NewValue;
            this._ownerCollection.SetVisibility(this, value);
        }

        #endregion

        #endregion

        #region 编辑状态

        /// <summary>
        /// 在编辑控件按 Tab 切换到下一列
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="moveNext">是向后编辑还是向前编辑。</param>
        /// <param name="e"></param>
        /// <param name="firstStarted"></param>
        internal void EditNextColumnOnTabKey(TreeGridCell cell, bool moveNext, KeyEventArgs e, TreeGridColumn firstStarted)
        {
            var grid = this.TreeGrid;
            var columns = grid.Columns;

            var index = columns.IndexOf(this);
            index += moveNext ? 1 : -1;

            //循环：如果是最后一个，则使用第一个；如果是第一个，则使用最后一个。
            if (index >= columns.Count)
            {
                index = 0;
            }
            else if (index < 0)
            {
                index = columns.Count - 1;
            }

            //尝试编辑
            var row = cell.Row;
            if (row != null)
            {
                var nextColumn = columns[index] as TreeGridColumn;

                //如果要编辑的单元格和起始单元格是同一个格子，则已经循环了一次，不需要继续了。
                if (nextColumn == firstStarted) return;

                grid.BringColumnIntoView(index);
                var nextCell = row.FindCell(nextColumn);
                if (nextCell != null)
                {
                    bool success = false;
                    if (grid.EditingMode == TreeGridEditingMode.Cell)
                    {
                        success = grid.TryEditCell(nextCell, e);
                    }
                    else
                    {
                        //行模式下，只是把焦点移到下一个单元格即可。
                        if (nextCell.IsEditing)
                        {
                            nextCell.FocusOnEditing(e);

                            success = true;
                        }
                    }

                    if (success)
                    {
                        e.Handled = true;
                        return;
                    }

                    //如果 nextCell 编辑失败，并且不是循环第二次，则继续编辑下一个。
                    nextColumn.EditNextColumnOnTabKey(nextCell, moveNext, e, firstStarted);
                }
            }
        }

        internal bool CanEdit(object dataItem)
        {
            return this.CanEnterEditing(dataItem);
        }

        /// <summary>
        /// 子类实现此方法来动态判断当前列是否可以进入编辑状态。
        /// 默认返回 true。
        /// </summary>
        /// <param name="dataItem"></param>
        /// <returns></returns>
        protected virtual bool CanEnterEditing(object dataItem)
        {
            return true;
        }

        #endregion

        #region 生成控件

        private DataTemplate _defaultDisplayTemplate;

        /// <summary>
        /// 返回当前用于显示的控件模板。
        /// 
        /// 如果没有指定 CellTemplate，则会调用 GenerateDefaultDisplayTemplate 生成默认的控件模板。
        /// </summary>
        /// <returns></returns>
        internal DataTemplate GetDisplayCellTemplate()
        {
            var cellTemplate = this.CellContentTemplate;
            if (cellTemplate == null)
            {
                if (this._defaultDisplayTemplate == null)
                {
                    this._defaultDisplayTemplate = new DataTemplate
                    {
                        VisualTree = this.GenerateDefaultDisplayTemplate(this.Binding)
                    };
                }

                cellTemplate = this._defaultDisplayTemplate;
            }

            return cellTemplate;
        }

        /// <summary>
        /// 生成一个默认的控件模板，用于显示。
        /// 
        /// 这个模板的绑定是动态的，所以无法放到 xaml 中去定义，只能用代码动态生成。
        /// </summary>
        /// <returns></returns>
        protected virtual FrameworkElementFactory GenerateDefaultDisplayTemplate(BindingBase binding)
        {
            var textBlock = new FrameworkElementFactory(typeof(TextBlock));

            textBlock.SetBinding(TextBlock.TextProperty, binding);

            var style = this.DisplayTextBlockStyle;
            if (style != null) textBlock.SetValue(TextBlock.StyleProperty, style);

            return textBlock;
        }

        /// <summary>
        /// 生成这个列使用的编辑控件
        /// </summary>
        /// <returns></returns>
        internal FrameworkElement GenerateEditingElement()
        {
            return this.GenerateEditingElementCore();
        }

        /// <summary>
        /// 生成这个 Column 使用的编辑控件
        /// </summary>
        /// <returns></returns>
        protected abstract FrameworkElement GenerateEditingElementCore();

        /// <summary>
        /// 准备这个编辑控件，使其进入准备状态。
        /// 例如，文本框应该在此操作中实现：根据鼠标位置选中对应的文本。
        /// </summary>
        /// <param name="editingElement"></param>
        /// <param name="editingEventArgs"></param>
        internal protected abstract void PrepareElementForEdit(FrameworkElement editingElement, RoutedEventArgs editingEventArgs);

        #endregion

        #region 合计行

        internal const string SummaryProperty = "Summary";

        private string _summary;

        /// <summary>
        /// 合计值。
        /// </summary>
        public string Summary
        {
            get
            {
                return this._summary;
            }
            internal set
            {
                if (this._summary != value)
                {
                    this._summary = value;

                    this.OnPropertyChanged(SummaryProperty);
                }
            }
        }

        /// <summary>
        /// 该列是否需要统计数值到统计行中。
        /// </summary>
        internal protected virtual bool NeedSummary { get { return false; } }

        /// <summary>
        /// 如果需要合计描述，则此函数返回所有对象应该返回的描述。
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        internal protected virtual string GetSummary(ItemCollection items) { return string.Empty; }

        #endregion

        #region event PropertyChanged

        internal const string c_ActualWidthName = "ActualWidth";

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }

    /// <summary>
    /// 表格排序的方向
    /// </summary>
    public enum TreeGridColumnSortDirection
    {
        /// <summary>
        /// 未排序
        /// </summary>
        None,
        /// <summary>
        /// 正序排列
        /// </summary>
        Ascending,
        /// <summary>
        /// 反序排列
        /// </summary>
        Descending
    }
}