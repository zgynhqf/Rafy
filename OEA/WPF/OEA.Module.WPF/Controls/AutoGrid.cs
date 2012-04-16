using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;

namespace OEA.Module.WPF.Controls
{
    /// <summary>
    /// Defines a flexible grid area that consists of columns and rows.
    /// Depending on the orientation, either the rows or the columns are auto-generated,
    /// and the children's position is set according to their index.
    /// </summary>
    public class AutoGrid : Grid
    {
        #region Fields

        /// <summary>
        /// A value of <c>true</c> forces children to be re-indexed at the next oportunity.
        /// </summary>
        private bool _indexDirty = true;

        private int _rowOrColumnCount;

        #endregion

        #region Dependency Properties

        /// <summary>
        /// Gets or sets a value indicating whether the children are automatically indexed.
        /// <remarks>
        /// The default is <c>true</c>.
        /// Note that if children are already indexed, setting this property to <c>false</c> will not remove their indices.
        /// </remarks>
        /// </summary>
        public bool IsAutoIndexing
        {
            get { return (bool)GetValue(IsAutoIndexingProperty); }
            set { SetValue(IsAutoIndexingProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="IsAutoIndexing"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsAutoIndexingProperty =
            DependencyProperty.Register("IsAutoIndexing", typeof(bool), typeof(AutoGrid), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsMeasure, OnPropertyChanged));

        /// <summary>
        /// Gets or sets the orientation.
        /// <remarks>The default is Vertical.</remarks>
        /// </summary>
        /// <value>The orientation.</value>
        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="Orientation"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation", typeof(Orientation), typeof(AutoGrid), new FrameworkPropertyMetadata(Orientation.Vertical, FrameworkPropertyMetadataOptions.AffectsMeasure, OnPropertyChanged));


        /// <summary>
        /// Gets or sets the child margin.
        /// </summary>
        /// <value>The child margin.</value>
        public Thickness? ChildMargin
        {
            get { return (Thickness?)GetValue(ChildMarginProperty); }
            set { SetValue(ChildMarginProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="ChildMargin"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ChildMarginProperty =
            DependencyProperty.Register("ChildMargin", typeof(Thickness?), typeof(AutoGrid), new FrameworkPropertyMetadata((Thickness?)null, FrameworkPropertyMetadataOptions.AffectsMeasure, OnPropertyChanged));


        /// <summary>
        /// Gets or sets the child horizontal alignment.
        /// </summary>
        /// <value>The child horizontal alignment.</value>
        public HorizontalAlignment? ChildHorizontalAlignment
        {
            get { return (HorizontalAlignment?)GetValue(ChildHorizontalAlignmentProperty); }
            set { SetValue(ChildHorizontalAlignmentProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="ChildHorizontalAlignment"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ChildHorizontalAlignmentProperty =
            DependencyProperty.Register("ChildHorizontalAlignment", typeof(HorizontalAlignment?), typeof(AutoGrid), new FrameworkPropertyMetadata((HorizontalAlignment?)null, FrameworkPropertyMetadataOptions.AffectsMeasure, OnPropertyChanged));


        /// <summary>
        /// Gets or sets the child vertical alignment.
        /// </summary>
        /// <value>The child vertical alignment.</value>
        public VerticalAlignment? ChildVerticalAlignment
        {
            get { return (VerticalAlignment?)GetValue(ChildVerticalAlignmentProperty); }
            set { SetValue(ChildVerticalAlignmentProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="ChildVerticalAlignment"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ChildVerticalAlignmentProperty =
            DependencyProperty.Register("ChildVerticalAlignment", typeof(VerticalAlignment?), typeof(AutoGrid), new FrameworkPropertyMetadata((VerticalAlignment?)null, FrameworkPropertyMetadataOptions.AffectsMeasure, OnPropertyChanged));


        private static void OnPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ((AutoGrid)o).InvalidateIndex();
        }

        #endregion

        #region Overrides

        protected override Size MeasureOverride(Size constraint)
        {
            bool isVertical = this.Orientation == Orientation.Vertical;

            //如果需要重新计算所有孩子的索引，则执行计算逻辑。
            if (this._indexDirty || (this.IsAutoIndexing &&
                ((isVertical && this._rowOrColumnCount != ColumnDefinitions.Count) ||
                (!isVertical && this._rowOrColumnCount != RowDefinitions.Count))))
            {
                if (IsAutoIndexing)
                {
                    if (isVertical)
                    {
                        this._rowOrColumnCount = this.ColumnDefinitions.Count;
                        if (this._rowOrColumnCount == 0) this._rowOrColumnCount = 1;

                        int curRow = 0, cellMax = this._rowOrColumnCount, cellLeft = cellMax;
                        foreach (UIElement child in this.Children)
                        {
                            var span = Grid.GetColumnSpan(child);
                            //如果该子对象需要的格子数据超过限制，则自动修复该子对象的索引，并停止当前计算，等待下次计算。
                            if (span > cellMax)
                            {
                                Grid.SetColumnSpan(child, cellMax);
                                return base.MeasureOverride(constraint);
                            }

                            //如果当前格子不够用了，则需要另起一行
                            if (span > cellLeft)
                            {
                                curRow++;
                                cellLeft = cellMax;

                                Grid.SetColumn(child, 0);
                            }
                            else
                            {
                                Grid.SetColumn(child, cellMax - cellLeft);
                            }

                            Grid.SetRow(child, curRow);
                            cellLeft -= span;
                        }

                        var rowsCount = curRow + 1;

                        //最后再同步行数
                        var rows = this.RowDefinitions;
                        while (rows.Count < rowsCount)
                        {
                            rows.Add(new RowDefinition());
                        }

                        if (rows.Count > rowsCount) { rows.RemoveRange(rowsCount, rows.Count - rowsCount); }
                    }
                    else
                    {
                        #region 水平方向（算法同上）

                        this._rowOrColumnCount = this.RowDefinitions.Count;
                        if (this._rowOrColumnCount == 0) this._rowOrColumnCount = 1;

                        int curColumn = 0, cellMax = this._rowOrColumnCount, cellLeft = cellMax;
                        foreach (UIElement child in this.Children)
                        {
                            var span = Grid.GetRowSpan(child);
                            if (span > cellMax)
                            {
                                Grid.SetRowSpan(child, cellMax);
                                return base.MeasureOverride(constraint);
                            }

                            if (span > cellLeft)
                            {
                                curColumn++;
                                cellLeft = cellMax;

                                Grid.SetRow(child, 0);
                            }
                            else
                            {
                                Grid.SetRow(child, cellMax - cellLeft);
                            }

                            Grid.SetColumn(child, curColumn);
                            cellLeft -= span;
                        }

                        var columnsCount = curColumn + 1;

                        var columns = this.ColumnDefinitions;
                        while (columns.Count < columnsCount)
                        {
                            columns.Add(new ColumnDefinition());
                        }
                        if (columns.Count > columnsCount)
                        {
                            columns.RemoveRange(columnsCount, columns.Count - columnsCount);
                        }

                        #endregion
                    }
                }

                this.SetChildrenDefault();

                this._indexDirty = false;
            }

            return base.MeasureOverride(constraint);
        }

        private void SetChildrenDefault()
        {
            foreach (UIElement child in this.Children)
            {
                // Set margin and alignment
                if (ChildMargin != null)
                {
                    child.SetIfDefault(FrameworkElement.MarginProperty, ChildMargin.Value);
                }
                if (ChildHorizontalAlignment != null)
                {
                    child.SetIfDefault(FrameworkElement.HorizontalAlignmentProperty, ChildHorizontalAlignment.Value);
                }
                if (ChildVerticalAlignment != null)
                {
                    child.SetIfDefault(FrameworkElement.VerticalAlignmentProperty, ChildVerticalAlignment.Value);
                }
            }
        }

        /// <summary>
        /// Called when the visual children of a <see cref="Grid"/> element change.
        /// <remarks>Used to mark that the grid children have changed.</remarks>
        /// </summary>
        /// <param name="visualAdded">Identifies the visual child that's added.</param>
        /// <param name="visualRemoved">Identifies the visual child that's removed.</param>
        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            this.InvalidateIndex();

            base.OnVisualChildrenChanged(visualAdded, visualRemoved);

            var propertyToListen = this.Orientation == Orientation.Vertical ?
                Grid.ColumnSpanProperty : Grid.RowSpanProperty;
            var descriptor = DependencyPropertyDescriptor.FromProperty(propertyToListen, typeof(Grid));

            if (visualAdded != null) descriptor.AddValueChanged(visualAdded, OnChildIndexChanged);
            if (visualRemoved != null) descriptor.RemoveValueChanged(visualRemoved, OnChildIndexChanged);
        }

        private void OnChildIndexChanged(object sender, EventArgs e)
        {
            this.InvalidateIndex();
        }

        private void InvalidateIndex()
        {
            this._indexDirty = true;
            this.InvalidateMeasure();
        }

        #endregion
    }
}
