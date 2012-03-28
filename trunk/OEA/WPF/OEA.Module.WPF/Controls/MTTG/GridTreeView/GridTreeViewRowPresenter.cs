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
using System.Diagnostics;
using System.Reflection;
using System.ComponentModel;
using System.Windows.Input;
using OEA.Module.WPF;
using System.Windows.Media;
using System.Windows.Data;
using System.Collections.Specialized;
using System.Windows.Controls.Primitives;

namespace OEA.Module.WPF.Controls
{
    /// <summary>
    /// GridTreeViewRow 的行显示器。
    /// （使用 GridView 来进行显示。）
    /// 
    /// 每一行的第一列应该是：RowHeader + Expander + ColumnUI
    /// </summary>
    public class GridTreeViewRowPresenter : GridViewRowPresenter
    {
        #region 字段

        /// <summary>
        /// 由于 RowHeader 可能不是 UIElement，所以这里面需要申明一个容器
        /// </summary>
        private ContentControl _rowHeaderContainer = new ContentControl();

        /// <summary>
        /// 需要定义一个 UIElementCollection 元素，并调用它的 Add 方法，来间接实现设置子控件的 LogicalParent 和 VisualParent。
        /// </summary>
        private UIElementCollection _childs;

        #endregion

        public GridTreeViewRowPresenter()
        {
            this._childs = new UIElementCollection(this, this);
            this.SetAncestorBinding(ColumnsProperty, GridTreeView.ColumnsProperty, typeof(GridTreeView));
            this.SetAncestorBinding(HasRowHeaderProperty, GridTreeView.HasRowNoProperty, typeof(GridTreeView));
            this.SetAncestorBinding(HideExpanderProperty, GridTreeView.OnlyGridModeProperty, typeof(GridTreeView));

            //不能使用 Loaded 事件，而是使用 LayoutUpdated 事件，
            //否则有时在二级窗体（GIX4 必填属性）造成无法找到 Columns 对象。原因未查。
            this.LayoutUpdated += new EventHandler(OnLayoutUpdated);
        }

        #region RowHeaderWidthProperty

        public static readonly DependencyProperty RowHeaderWidthProperty = DependencyProperty.Register(
            "RowHeaderWidth", typeof(int), typeof(GridTreeViewRowPresenter), new PropertyMetadata(21)
            );

        /// <summary>
        /// 行号的左缩进量
        /// </summary>
        public int RowHeaderWidth
        {
            get { return (int)this.GetValue(RowHeaderWidthProperty); }
            set { this.SetValue(RowHeaderWidthProperty, value); }
        }

        #endregion

        #region RowHeaderProperty

        public static DependencyProperty RowHeaderProperty = DependencyProperty.Register(
            "RowHeader", typeof(object), typeof(GridTreeViewRowPresenter),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, (d, e) => (d as GridTreeViewRowPresenter).OnRowHeaderChanged(e))
            );

        /// <summary>
        /// 这个属性主要用于绑定 GridTreeViewRow 中的 RowHeader
        /// </summary>
        public object RowHeader
        {
            get { return this.GetValue(RowHeaderProperty); }
            set { this.SetValue(RowHeaderProperty, value); }
        }

        private void OnRowHeaderChanged(DependencyPropertyChangedEventArgs e)
        {
            this.OnVisualChildChanged(e);

            this._rowHeaderContainer.Content = e.NewValue;
        }

        #endregion

        #region FirstColumnIndentProperty

        public static DependencyProperty FirstColumnIndentProperty = DependencyProperty.Register(
            "FirstColumnIndent", typeof(double), typeof(GridTreeViewRowPresenter),
            new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender)
            );

        /// <summary>
        /// 当前行的第一列的左缩进量
        /// 
        /// 这个值需要绑定到 GridTreeViewRow 的 Level 上。
        /// </summary>
        public double FirstColumnIndent
        {
            get { return (double)this.GetValue(FirstColumnIndentProperty); }
            set { this.SetValue(FirstColumnIndentProperty, value); }
        }

        #endregion

        #region ExpanderProperty

        public static DependencyProperty ExpanderProperty = DependencyProperty.Register(
            "Expander", typeof(UIElement), typeof(GridTreeViewRowPresenter),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, (d, e) => (d as GridTreeViewRowPresenter).OnVisualChildChanged(e))
            );

        public UIElement Expander
        {
            get { return (UIElement)this.GetValue(ExpanderProperty); }
            set { this.SetValue(ExpanderProperty, value); }
        }

        private void OnVisualChildChanged(DependencyPropertyChangedEventArgs e)
        {
            var oldElement = e.OldValue as UIElement;
            if (oldElement != null) { this._childs.Remove(oldElement); }

            var newElement = e.NewValue as UIElement;
            if (newElement != null) { this._childs.Add(newElement); }
        }

        #endregion

        #region HasRowHeaderProperty

        private static readonly DependencyProperty HasRowHeaderProperty = DependencyProperty.Register(
            "HasRowHeader", typeof(bool), typeof(GridTreeViewRowPresenter),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender)
            );

        /// <summary>
        /// 是否绘制 RowHeader 控件。
        /// 
        /// 这个值是直接绑定到 <see cref="GridTreeView.HasRowNoProperty"/> 属性上的。
        /// </summary>
        private bool HasRowHeader
        {
            get { return (bool)this.GetValue(HasRowHeaderProperty); }
        }

        #endregion

        #region HideExpanderProperty

        private static readonly DependencyProperty HideExpanderProperty = DependencyProperty.Register(
            "HideExpander", typeof(bool), typeof(GridTreeViewRowPresenter),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender)
            );

        /// <summary>
        /// 是否需要不绘制 Expander。
        /// 
        /// 注意：
        /// 由于需要显示层级的缩进，就算当前行没有子行，它的 Expander 也需要占位，即 Hidden 而不是 Collapsed。
        /// 这里，只有当完全不显示层级缩进时，这个值才会是 true。
        /// 
        /// 这个值是直接绑定到 <see cref="GridTreeView.OnlyGridModeProperty"/> 属性上的。
        /// </summary>
        private bool HideExpander
        {
            get { return (bool)this.GetValue(HideExpanderProperty); }
        }

        #endregion

        #region ========= 核心 == 绘图 ===========

        /**********************************************************************
         * 
         * 说明：
         * 每一行的第一列应该是：RowHeader + Expander + ColumnUI
         * 位置都安排在第一列，
         * 把这两个多余的元素作为子 Visual 列表的最后两个，Index 则是 RowHeader 在前，Expander 在后。
         * （GridTreeViewRow 中也会使用到这个排序。）
         * 
        **********************************************************************/

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            //第一列中的布局：RowHeaderWidth + FirstColumnIndent(Level) + ExpanderWidth + ColumnUI

            var size = base.ArrangeOverride(arrangeSize);

            var columns = this.Columns;
            if (columns == null || columns.Count == 0) return size;

            double columnX = 0;
            double rowWidth = arrangeSize.Width;
            for (int i = 0; i < columns.Count; i++)
            {
                var column = columns[i] as GridTreeViewColumn;

                var actualIndex = column.ActualIndexReflected;
                var uiColumn = (UIElement)base.GetVisualChild(actualIndex);

                var columnAvailableWidth = column.StateReflected != ColumnMeasureState.SpecificWidth ?
                    column.DesiredWidthReflected : column.Width;
                columnAvailableWidth = Math.Min(columnAvailableWidth, rowWidth);

                if (i == 0)
                {
                    //计算列所需要的宽度：
                    var columnWidth = columnAvailableWidth;

                    //画上 RowHeader
                    if (this.HasRowHeader)
                    {
                        var rowHeaderWidth = Math.Min(columnWidth, this.RowHeaderWidth);

                        this._rowHeaderContainer.Arrange(new Rect(0, 0, rowHeaderWidth, double.MaxValue));

                        columnX += rowHeaderWidth;
                        columnWidth -= rowHeaderWidth;
                    }

                    //留下 FirstColumnIndent
                    var firstColumnIndent = Math.Min(columnWidth, this.FirstColumnIndent);
                    columnX += firstColumnIndent;
                    columnWidth -= firstColumnIndent;

                    //画上 Expander
                    if (!this.HideExpander)
                    {
                        var expander = this.Expander;
                        double expanderWidth = Math.Min(columnWidth, expander.DesiredSize.Width);

                        expander.Arrange(new Rect(columnX, 0, expanderWidth, expander.DesiredSize.Height));

                        columnX += expanderWidth;
                        columnWidth -= expanderWidth;
                    }

                    //this._firstColumnDesiredWidth = columnX + uiColumn.DesiredSize.Width;

                    //画上 ColumnUI
                    uiColumn.Arrange(new Rect(columnX, 0, columnWidth, arrangeSize.Height));

                    columnX += columnWidth;
                }
                else
                {
                    uiColumn.Arrange(new Rect(columnX, 0, columnAvailableWidth, arrangeSize.Height));

                    columnX += columnAvailableWidth;
                }

                rowWidth -= columnAvailableWidth;
                if (rowWidth == 0) break;
            }

            return size;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            var columns = base.Columns;
            if (columns == null) { return default(Size); }

            double desiredHeight = 0.0;
            double x = 0.0;
            double maxHeight = constraint.Height;
            bool flag = false;
            for (int i = 0, c = columns.Count; i < c; i++)
            {
                var current = columns[i] as GridTreeViewColumn;
                var actualIndex = current.ActualIndexReflected;
                UIElement uIElement = base.GetVisualChild(actualIndex) as UIElement;
                if (uIElement != null)
                {
                    double availableWidth = Math.Max(0.0, constraint.Width - x);
                    double columnWidth = availableWidth;

                    var state = current.StateReflected;
                    if (state == ColumnMeasureState.Init || state == ColumnMeasureState.Headered)
                    {
                        if (!flag)
                        {
                            GridViewInternal.EnsureDesiredWidthListMethod.Invoke(this, null);
                            base.LayoutUpdated += new EventHandler(this.OnMeasureLayoutUpdated);
                            flag = true;
                        }

                        if (i == 0)
                        {
                            if (this.HasRowHeader)
                            {
                                x += this.RowHeaderWidth;
                                columnWidth -= this.RowHeaderWidth;
                                //this._rowHeaderContainer.Measure(new Size(columnWidth, maxHeight));
                                //x += this._rowHeaderContainer.DesiredSize.Width;
                                //columnWidth -= this._rowHeaderContainer.DesiredSize.Width;
                            }

                            var firstColumnIndent = Math.Min(this.FirstColumnIndent, columnWidth);
                            x += firstColumnIndent;
                            columnWidth -= firstColumnIndent;

                            if (!this.HideExpander)
                            {
                                var expander = this.Expander;
                                expander.Measure(new Size(columnWidth, maxHeight));
                                x += expander.DesiredSize.Width;
                                columnWidth -= expander.DesiredSize.Width;
                            }

                            uIElement.Measure(new Size(columnWidth, maxHeight));
                            var desiredWidth = current.EnsureWidth(uIElement.DesiredSize.Width + x);
                            this.DesiredWidthList[actualIndex] = desiredWidth;
                            x = desiredWidth;
                        }
                        else
                        {

                            uIElement.Measure(new Size(columnWidth, maxHeight));
                            var desiredWidth = current.EnsureWidth(uIElement.DesiredSize.Width);
                            this.DesiredWidthList[actualIndex] = desiredWidth;
                            x += desiredWidth;
                        }
                    }
                    else
                    {
                        if (state == ColumnMeasureState.Data)
                        {
                            columnWidth = Math.Min(columnWidth, current.DesiredWidthReflected);
                            uIElement.Measure(new Size(columnWidth, maxHeight));
                            x += current.DesiredWidthReflected;
                        }
                        else
                        {
                            columnWidth = Math.Min(columnWidth, current.Width);
                            uIElement.Measure(new Size(columnWidth, maxHeight));
                            x += current.Width;
                        }
                    }
                    desiredHeight = Math.Max(desiredHeight, uIElement.DesiredSize.Height);
                }
            }
            x += 2.0;

            return new Size(x, desiredHeight);
        }

        protected override Visual GetVisualChild(int index)
        {
            var rawCount = base.VisualChildrenCount;

            if (index < rawCount) return base.GetVisualChild(index);

            var rowHeader = this.RowHeader;

            //多了一个元素，则优先使用 RowHeader
            if (index == rawCount)
            {
                if (this.HasRowHeader) { return this._rowHeaderContainer; }
                return this.Expander;
            }

            //多了两个元素，则第二个元素应该是 Expander
            if (index == rawCount + 1) { return this.Expander; }

            throw new NotSupportedException();
        }

        protected override int VisualChildrenCount
        {
            get
            {
                var count = base.VisualChildrenCount;

                if (this.HasRowHeader) count++;

                if (!this.HideExpander) count++;

                return count;
            }
        }

        #region 复制基类 MeasureOverride 方法的代码，并调用新的 GridTreeViewColumn.EnsureWidth 方法

        private List<double> _desiredWidthList;

        public List<double> DesiredWidthList
        {
            get
            {
                if (this._desiredWidthList == null)
                {
                    this._desiredWidthList = GridViewInternal.DesiredWidthListProperty
                            .GetValue(this, null) as List<double>;
                }

                return this._desiredWidthList;
            }
        }

        private void OnMeasureLayoutUpdated(object sender, EventArgs e)
        {
            bool flag = false;
            foreach (GridTreeViewColumn current in base.Columns)
            {
                if (current.StateReflected != ColumnMeasureState.SpecificWidth)
                {
                    current.StateReflected = ColumnMeasureState.Data;

                    var actualIndex = current.ActualIndexReflected;

                    if (this.DesiredWidthList == null || actualIndex >= this.DesiredWidthList.Count)
                    {
                        flag = true;
                        break;
                    }

                    var desiredWidth = current.DesiredWidthReflected;
                    if (!GridViewInternal.AreClose(desiredWidth, this.DesiredWidthList[actualIndex]))
                    {
                        this.DesiredWidthList[actualIndex] = desiredWidth;
                        flag = true;
                    }
                }
            }
            if (flag)
            {
                base.InvalidateMeasure();
            }
            base.LayoutUpdated -= new EventHandler(this.OnMeasureLayoutUpdated);
        }

        #endregion

        #endregion

        #region 帮助方法

        private void SetAncestorBinding(
            DependencyProperty property, DependencyProperty ancestorProperty, Type ancestorType
            )
        {
            this.SetBinding(property, new Binding
            {
                Path = new PropertyPath(ancestorProperty),
                Mode = BindingMode.OneWay,
                RelativeSource = new RelativeSource
                {
                    Mode = RelativeSourceMode.FindAncestor,
                    AncestorType = ancestorType
                }
            });
        }

        #endregion

        #region ClearCellContainerMargin

        private bool _marginInitialized;

        private void OnLayoutUpdated(object sender, EventArgs e)
        {
            this.InitMargin();
        }

        private void InitMargin()
        {
            if (this._marginInitialized) return;

            var columns = this.Columns;
            if (columns != null)
            {
                this._marginInitialized = true;

                this.ClearCellContainerMargin();

                columns.CollectionChanged += (o, ee) =>
                {
                    switch (ee.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                        case NotifyCollectionChangedAction.Replace:
                        case NotifyCollectionChangedAction.Reset:
                            this.ClearCellContainerMargin();
                            break;
                        default:
                            break;
                    }
                };
            }
        }

        /// <summary>
        /// 由于在 GridViewRowPresenter.CreateCell 方法中直接设置了每一个 Cell 的 Margin 是 6,0，
        /// 所以这里需要把这些 Margin 都去掉。
        /// </summary>
        private void ClearCellContainerMargin()
        {
            var e = this.LogicalChildren;
            while (e.MoveNext())
            {
                var cellContainer = e.Current as FrameworkElement;
                if (cellContainer == null) break;

                cellContainer.ClearValue(FrameworkElement.MarginProperty);
            }
        }

        #endregion

        //protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        //{
        //    base.OnPropertyChanged(e);

        //    if (e.Property == WidthProperty)
        //    {
        //        this.InvalidateArrange();
        //    }
        //}
    }
}