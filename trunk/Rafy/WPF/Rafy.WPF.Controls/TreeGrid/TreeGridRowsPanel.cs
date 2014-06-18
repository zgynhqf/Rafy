/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20121017 18:09
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20121017 18:09
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Rafy.WPF.Controls
{
    /// <summary>
    /// TreeGrid 中的行的虚拟化容器
    /// 
    /// 部分代码来自于 DataGridRowsPresenter。
    /// </summary>
    public class TreeGridRowsPanel : VirtualizingStackPanel, IScrollInfo
    {
        private TreeGrid _owner;

        internal TreeGrid TreeGrid
        {
            get
            {
                if (this._owner == null)
                {
                    this._owner = ItemsControl.GetItemsOwner(this) as TreeGrid;
                }
                return this._owner;
            }
        }

        internal void InternalBringRowIntoView(int index)
        {
            this.BringIndexIntoView(index);
        }

        /// <summary>
        /// 根据 ColumnIndex 计算并设置新的 HorizontalOffset。
        /// </summary>
        /// <param name="index"></param>
        internal void InternalBringColumnIntoView(int index)
        {
            var grid = this.TreeGrid;
            if (grid == null) return;

            var columns = grid.Columns;

            if (index < 0 || index >= columns.Count)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            var left = this.HorizontalOffset;
            var right = this.HorizontalOffset + this.ViewportWidth;
            double columnLeft = 0.0, columnRight = 0.0;
            for (int i = 0; i < index; i++)
            {
                var column = columns[i];
                columnLeft += column.ActualWidth;
            }
            columnRight = columnLeft + columns[index].ActualWidth;

            if (columnLeft < left)
            {
                //目标列在视区的左边，需要向左滚动
                this.SetHorizontalOffset(columnLeft);
            }
            else if (columnRight > right)
            {
                //目标列在视区的右边，需要向右滚动
                left += columnRight - right;
                this.SetHorizontalOffset(left);
            }

            //IScrollInfo scrollInfo = null;
            //if (this._isVirtualizing)
            //{
            //    scrollInfo = this;
            //}
            //else
            //{
            //    scrollInfo = this.GetVisualParent<ScrollContentPresenter>();
            //    if (scrollInfo == null)
            //    {
            //        base.BringIndexIntoView(index);
            //        return;
            //    }
            //}

            //double num = 0.0;
            //double value = scrollInfo.HorizontalOffset;
            //while (!this.IsChildInView(index, out num) && !DoubleUtil.AreClose(value, num))
            //{
            //    scrollInfo.SetHorizontalOffset(num);
            //    base.UpdateLayout();
            //    value = num;
            //}
        }

        protected override void OnIsItemsHostChanged(bool oldIsItemsHost, bool newIsItemsHost)
        {
            base.OnIsItemsHostChanged(oldIsItemsHost, newIsItemsHost);

            if (newIsItemsHost)
            {
                var owner = this.TreeGrid;
                if (owner != null)
                {
                    IItemContainerGenerator itemContainerGenerator = owner.ItemContainerGenerator;
                    if (itemContainerGenerator != null && itemContainerGenerator == itemContainerGenerator.GetItemContainerGeneratorForPanel(this))
                    {
                        owner.RowsPanel = this;
                        return;
                    }
                }
            }
            else
            {
                if (this._owner != null && this._owner.RowsPanel == this)
                {
                    this._owner.RowsPanel = null;
                }
                this._owner = null;
            }
        }

        protected override void OnViewportOffsetChanged(Vector oldViewportOffset, Vector newViewportOffset)
        {
            if (oldViewportOffset.X != newViewportOffset.X)
            {
                this.InvalidateCells();
            }
        }

        protected override void OnViewportSizeChanged(Size oldViewportSize, Size newViewportSize)
        {
            this.InvalidateMeasure();

            if (oldViewportSize.Width != newViewportSize.Width)
            {
                this.InvalidateCells();
            }

            //TreeGrid owner = this.Owner;
            //if (owner != null)
            //{
            //    var internalScrollContentPresenter = owner.InternalScrollContentPresenter;
            //    if (internalScrollContentPresenter == null || internalScrollContentPresenter.CanContentScroll)
            //    {
            //        owner.OnViewportSizeChanged(oldViewportSize, newViewportSize);
            //    }
            //}
        }

        protected override void OnCleanUpVirtualizedItem(CleanUpVirtualizedItemEventArgs e)
        {
            base.OnCleanUpVirtualizedItem(e);
            if (e.UIElement != null && Validation.GetHasError(e.UIElement))
            {
                e.Cancel = true;
            }
        }

        /// <summary>
        /// 让当前所有行的单元格重新测量。
        /// </summary>
        private void InvalidateCells()
        {
            var grid = this.TreeGrid;
            if (grid != null && grid.IsColumnsVirtualizingEnabled)
            {
                var rows = TreeGridHelper.TraverseRows(grid);
                foreach (var item in rows)
                {
                    if (item.CellsPanel != null)
                    {
                        item.CellsPanel.InvalidateMeasure();
                    }
                }
            }
        }

        #region 适配列头到滚动条

        /*********************** 代码块解释 *********************************
         * 当没有任何行时，基类 VirtualizingStackPanel 会认为没有内容，而不需要进行任何滚动。
         * 但是，其实列头已经超过了可视区范围，所以，需要把 Header 的宽度适配到滚动信息上。
        **********************************************************************/

        /// <summary>
        /// 是否需要适配列头宽度到滚动信息上。
        /// </summary>
        private bool AdaptHeaderToScrollInfo
        {
            get { return this.Children.Count == 0; }
        }

        private double _adaptedHorizontalOffset;

        double IScrollInfo.HorizontalOffset
        {
            get
            {
                if (this.AdaptHeaderToScrollInfo)
                {
                    return _adaptedHorizontalOffset;
                }
                else
                {
                    return base.HorizontalOffset;
                }
            }
        }

        double IScrollInfo.ExtentWidth
        {
            get
            {
                var width = base.ExtentWidth;

                //如果是适配到列头上，则使用列头的整个宽度作为滚动条的最大宽度。
                if (this.AdaptHeaderToScrollInfo)
                {
                    var grid = this.TreeGrid;
                    if (grid != null && grid.HeaderRowPresenter != null)
                    {
                        width = Math.Max(width, grid.HeaderRowPresenter.RealDesiredWith);
                    }
                }

                return width;
            }
        }

        void IScrollInfo.SetHorizontalOffset(double offset)
        {
            if (this.AdaptHeaderToScrollInfo)
            {
                _adaptedHorizontalOffset = offset;

                if (this.ScrollOwner != null)
                {
                    this.ScrollOwner.InvalidateScrollInfo();
                }
            }
            else
            {
                base.SetHorizontalOffset(offset);
            }
        }

        #endregion
    }
}