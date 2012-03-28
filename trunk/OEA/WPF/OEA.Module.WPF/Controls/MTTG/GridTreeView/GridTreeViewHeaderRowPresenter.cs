/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20111206
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20111206
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace OEA.Module.WPF.Controls
{
    /// <summary>
    /// 列头行的显示器
    /// </summary>
    public class GridTreeViewHeaderRowPresenter : GridViewHeaderRowPresenter
    {
        #region 复制基类 MeasureOverride 方法的代码，并调用新的 GridTreeViewColumn.EnsureWidth 方法

        #region 基类的私有字段

        private UIElement __paddingHeader;

        private List<double> _desiredWidthList;

        private UIElement _indicator
        {
            get { return GridViewInternal._indicatorField.GetValue(this) as UIElement; }
        }

        private UIElement _floatingHeader
        {
            get { return GridViewInternal._floatingHeaderField.GetValue(this) as UIElement; }
        }

        private bool _isHeaderDragging
        {
            get { return (bool)GridViewInternal._isHeaderDraggingField.GetValue(this); }
        }

        private UIElement _paddingHeader
        {
            get
            {
                if (this.__paddingHeader == null)
                {
                    this.__paddingHeader = GridViewInternal._paddingHeaderField.GetValue(this) as UIElement;
                }

                return this.__paddingHeader;
            }
        }

        private List<double> DesiredWidthList
        {
            get
            {
                if (this._desiredWidthList == null)
                {
                    this._desiredWidthList = GridViewInternal.DesiredWidthListProperty.GetValue(this, null) as List<double>;
                }

                return this._desiredWidthList;
            }
        }

        #endregion

        protected override Size MeasureOverride(Size constraint)
        {
            var columns = base.Columns;
            double num = 0.0;
            double num2 = 0.0;
            double height = constraint.Height;
            bool flag = false;
            if (columns != null)
            {
                for (int i = 0; i < columns.Count; i++)
                {
                    UIElement uIElement = base.GetVisualChild(this.GetVisualIndex(i)) as UIElement;
                    if (uIElement != null)
                    {
                        double num3 = Math.Max(0.0, constraint.Width - num2);
                        var gridViewColumn = columns[i] as GridTreeViewColumn;
                        var state = gridViewColumn.StateReflected;
                        if (state == ColumnMeasureState.Init)
                        {
                            if (!flag)
                            {
                                GridViewInternal.EnsureDesiredWidthListMethod.Invoke(this, null);
                                base.LayoutUpdated += new EventHandler(this.OnLayoutUpdated);
                                flag = true;
                            }
                            uIElement.Measure(new Size(num3, height));
                            var desiredWidth = gridViewColumn.EnsureWidth(uIElement.DesiredSize.Width);
                            this.DesiredWidthList[gridViewColumn.ActualIndexReflected] = desiredWidth;
                            num2 += desiredWidth;
                        }
                        else
                        {
                            if (state == ColumnMeasureState.Headered || state == ColumnMeasureState.Data)
                            {
                                num3 = Math.Min(num3, gridViewColumn.DesiredWidthReflected);
                                uIElement.Measure(new Size(num3, height));
                                num2 += gridViewColumn.DesiredWidthReflected;
                            }
                            else
                            {
                                num3 = Math.Min(num3, gridViewColumn.Width);
                                uIElement.Measure(new Size(num3, height));
                                num2 += gridViewColumn.Width;
                            }
                        }
                        num = Math.Max(num, uIElement.DesiredSize.Height);
                    }
                }
            }
            this._paddingHeader.Measure(new Size(0.0, height));
            num = Math.Max(num, this._paddingHeader.DesiredSize.Height);
            num2 += 2.0;
            if (this._isHeaderDragging)
            {
                this._indicator.Measure(constraint);
                this._floatingHeader.Measure(constraint);
            }
            return new Size(num2, num);
        }

        private void OnLayoutUpdated(object sender, EventArgs e)
        {
            bool flag = false;
            var columns = base.Columns;
            for (int i = 0, c = columns.Count; i < c; i++)
            {
                var current = columns[i] as GridTreeViewColumn;

                var state = current.StateReflected;
                if (state != ColumnMeasureState.SpecificWidth)
                {
                    if (state == ColumnMeasureState.Init)
                    {
                        current.StateReflected = ColumnMeasureState.Headered;
                    }
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
            base.LayoutUpdated -= new EventHandler(this.OnLayoutUpdated);
        }

        private int GetVisualIndex(int columnIndex)
        {
            return base.VisualChildrenCount - 3 - columnIndex;
        }

        #endregion
    }
}