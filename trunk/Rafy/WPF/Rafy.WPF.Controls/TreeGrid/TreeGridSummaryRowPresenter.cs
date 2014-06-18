/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20121030 17:34
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20121030 17:34
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Rafy.WPF.Controls
{
    /// <summary>
    /// TreeGrid 合计行
    /// </summary>
    public class TreeGridSummaryRowPresenter : TreeGridRowPresenterBase
    {
        private TreeGridColumnSummary _paddingSummary;

        private TextBlock _summaryRowTitle;

        internal override void OnColumnCollectionChanged(TreeGridColumnCollectionChangedEventArgs e)
        {
            base.OnColumnCollectionChanged(e);

            this.NeedUpdateVisualTree = true;
            this.InvalidateMeasure();
        }

        protected override Size MeasureOverride(Size constraint)
        {
            this.ConnectToGrid();
            this.UpdateVisualTree();

            //不论外界是否虚拟化，这里都不虚拟化，按照无限宽度来进行测量。
            //否则会造成可用的宽度被使用完，视区外的列会被测量出过高的高度。
            constraint = new Size(double.PositiveInfinity, constraint.Height);

            var internalChildren = base.UIChildren;
            double heightDesired = 0.0;
            double widthDesired = 0.0;
            double heightConstraint = constraint.Height;

            var columns = base.Columns;
            for (int i = 0; i < columns.Count; i++)
            {
                var uiElement = internalChildren[i];
                if (uiElement != null)
                {
                    double widthConstraint = Math.Max(0.0, constraint.Width - widthDesired);
                    var column = columns[i];
                    if (column.State == ColumnMeasureState.Init)
                    {
                        uiElement.Measure(new Size(widthConstraint, heightConstraint));
                        column.EnsureDataWidth(uiElement.DesiredSize.Width);
                        widthDesired += column.DesiredDataWidth;
                    }
                    else
                    {
                        var columnDesiredWidth = column.State == ColumnMeasureState.Headered || column.State == ColumnMeasureState.Data
                            ? column.DesiredDataWidth : column.Width;
                        widthConstraint = Math.Min(widthConstraint, columnDesiredWidth);
                        uiElement.Measure(new Size(widthConstraint, heightConstraint));
                        widthDesired += columnDesiredWidth;
                    }

                    heightDesired = Math.Max(heightDesired, uiElement.DesiredSize.Height);
                }
            }

            this._paddingSummary.Measure(new Size(0.0, heightConstraint));
            heightDesired = Math.Max(heightDesired, this._paddingSummary.DesiredSize.Height);
            widthDesired += c_EndPadding;

            this._summaryRowTitle.Measure(constraint);

            return new Size(widthDesired, heightDesired);
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            var internalChildren = base.UIChildren;

            //x 从负方向的 HorizontalOffset 开始
            double zero = -this.GetHorizontalOffsetForArrange();
            double x = zero;
            double widthAvailable = arrangeSize.Width;
            var columns = base.Columns;
            for (int i = 0; i < columns.Count; i++)
            {
                var column = columns[i];
                var uiElement = internalChildren[i];

                var columnDesiredWidth = column.State == ColumnMeasureState.SpecificWidth ? column.Width : column.DesiredDataWidth;
                double width = Math.Min(widthAvailable, columnDesiredWidth);

                uiElement.Arrange(new Rect(x, 0.0, width, arrangeSize.Height));

                widthAvailable -= width;
                x += width;
            }

            var headerRect = new Rect(x, 0.0, Math.Max(widthAvailable, 0.0), arrangeSize.Height);
            this._paddingSummary.Arrange(headerRect);

            //_summaryRowTitle 直接放在最左边。
            this._summaryRowTitle.Arrange(new Rect(zero, 0, this._summaryRowTitle.DesiredSize.Width, arrangeSize.Height));

            return arrangeSize;
        }

        /// <summary>
        /// 如果需要，创建初始的 UI 元素。
        /// </summary>
        private void UpdateVisualTree()
        {
            if (this.NeedUpdateVisualTree)
            {
                var internalChildren = base.UIChildren;
                internalChildren.Clear();

                foreach (var column in base.Columns)
                {
                    var summary = new TreeGridColumnSummary();
                    summary.Column = column;
                    summary.DataContext = column;
                    summary.SetBinding(TreeGridColumnSummary.SummaryTextProperty, TreeGridColumn.SummaryProperty);
                    summary.IsSummaryVisible = column.NeedSummary;

                    internalChildren.Add(summary);
                }

                //在所有列后添加一个 _paddingSummary。
                this._paddingSummary = new TreeGridColumnSummary();
                internalChildren.Add(this._paddingSummary);

                //把 rowTitle 添加为最后一个元素。
                var grid = this.TreeGrid;
                this._summaryRowTitle = new TextBlock
                {
                    Text = grid.SummaryRowTitle,
                    Style = grid.SummaryRowTitleStyle
                };
                internalChildren.Add(this._summaryRowTitle);

                this.NeedUpdateVisualTree = false;
            }
        }
    }
}