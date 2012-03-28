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
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using OEA.WPF;

namespace OEA.Module.WPF.Controls
{
    /// <summary>
    /// 用于控制 GridTreeView 列的宽度。
    /// 
    /// 代码来自：[WPF疑难] 如何限定ListView列宽度
    /// http://www.cnblogs.com/zhouyinhui/archive/2008/06/03/1213030.html
    /// </summary>
    public class GridTreeViewColumnRangeManager
    {
        #region EnableProperty（外部 Xaml 接口）

        public static readonly DependencyProperty EnabledProperty = DependencyProperty.RegisterAttached(
            "Enabled", typeof(bool), typeof(GridTreeViewColumnRangeManager),
            new FrameworkPropertyMetadata(OnLayoutManagerEnabledChanged)
            );

        public static void SetEnabled(GridTreeView grid, bool enabled)
        {
            grid.SetValue(EnabledProperty, enabled);
        }

        private static void OnLayoutManagerEnabledChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var treeListView = dependencyObject as GridTreeView;
            if (treeListView != null)
            {
                bool enabled = (bool)e.NewValue;
                if (enabled)
                {
                    new GridTreeViewColumnRangeManager(treeListView);
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
        }

        #endregion

        #region 字段

        private readonly GridTreeView _gridTreeView;

        private ScrollViewer _scrollViewer;

        private bool _loaded;

        private Cursor _resizeCursor;

        private ScrollBarVisibility _verticalScrollBarVisibility = ScrollBarVisibility.Auto;

        #endregion

        public GridTreeViewColumnRangeManager(GridTreeView gridTreeView)
        {
            if (gridTreeView == null)
            {
                throw new ArgumentNullException("listView");
            }

            this._gridTreeView = gridTreeView;
            this._gridTreeView.Loaded += this.OnGridLoaded;
        }

        private void OnGridLoaded(object sender, RoutedEventArgs e)
        {
            this.RegisterEvents(this._gridTreeView);
            this.ResizeColumns();
            this._loaded = true;
        }

        /// <summary>
        /// 迭归找到对应元素，然后注册需要的事件。
        /// </summary>
        /// <param name="start"></param>
        private void RegisterEvents(DependencyObject start)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(start); i++)
            {
                var childVisual = VisualTreeHelper.GetChild(start, i) as Visual;
                if (childVisual is Thumb)
                {
                    GridViewColumn gridViewColumn = FindColumn(childVisual);
                    if (gridViewColumn == null) { continue; }

                    Thumb thumb = childVisual as Thumb;
                    thumb.PreviewMouseMove += new MouseEventHandler(OnThumbPreviewMouseMove);

                    var dp = DependencyPropertyDescriptor.FromProperty(GridViewColumn.WidthProperty, typeof(GridViewColumn));
                    dp.AddValueChanged(gridViewColumn, OnGridColumnWidthChanged);
                    this._gridTreeView.Unloaded += (s, e) => { dp.RemoveValueChanged(gridViewColumn, OnGridColumnWidthChanged); };
                    //notifier = new PropertyChangeNotifier(gridViewColumn, "Width");
                    //notifier.ValueChanged += new EventHandler(GridColumnWidthChanged);
                }
                else if (this._scrollViewer == null && childVisual is ScrollViewer)
                {
                    this._scrollViewer = childVisual as ScrollViewer;
                    this._scrollViewer.ScrollChanged += new ScrollChangedEventHandler(OnScrollViewerScrollChanged);
                    // assume we do the regulation of the horizontal scrollbar
                    this._scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
                    this._scrollViewer.VerticalScrollBarVisibility = this._verticalScrollBarVisibility;
                }

                this.RegisterEvents(childVisual);
            }
        }

        private void ResizeColumns()
        {
            //GridView view = this.listView.View as GridView;
            //if (view == null)
            //{
            //    return;
            //}

            double actualWidth = this._scrollViewer != null ? this._scrollViewer.ViewportWidth : this._gridTreeView.ActualWidth;
            if (actualWidth <= 0) return;

            double resizeableRegionCount = 0;
            double otherColumnsWidth = 0;

            // determine column sizes
            foreach (GridViewColumn gridViewColumn in _gridTreeView.Columns)
            {
                otherColumnsWidth += gridViewColumn.ActualWidth;
            }

            if (resizeableRegionCount <= 0)
            {
                this._scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                return;
            }

            double resizeableColumnsWidth = actualWidth - otherColumnsWidth;
            if (resizeableColumnsWidth <= 0) return;
        }

        private void OnThumbPreviewMouseMove(object sender, MouseEventArgs e)
        {
            Thumb thumb = sender as Thumb;
            GridViewColumn gridViewColumn = FindColumn(thumb);
            if (gridViewColumn == null) return;

            // check range column bounds
            if (thumb.IsMouseCaptured && GridTreeViewColumnRange.IsRangeColumn(gridViewColumn))
            {
                double? minWidth = GridTreeViewColumnRange.GetRangeMinWidth(gridViewColumn);
                double? maxWidth = GridTreeViewColumnRange.GetRangeMaxWidth(gridViewColumn);

                if (minWidth.HasValue && maxWidth.HasValue && minWidth > maxWidth) return; // invalid case

                if (this._resizeCursor == null)
                {
                    this._resizeCursor = thumb.Cursor; // save the resize cursor
                }

                if (minWidth.HasValue && gridViewColumn.Width <= minWidth.Value)
                {
                    thumb.Cursor = Cursors.No;
                }
                else if (maxWidth.HasValue && gridViewColumn.Width >= maxWidth.Value)
                {
                    thumb.Cursor = Cursors.No;
                }
                else
                {
                    thumb.Cursor = this._resizeCursor; // between valid min/max
                }
            }
        }

        private void OnGridColumnWidthChanged(object sender, EventArgs e)
        {
            if (this._loaded)
            {
                GridViewColumn gridViewColumn = sender as GridViewColumn;

                // ensure range column within the bounds
                if (GridTreeViewColumnRange.IsRangeColumn(gridViewColumn))
                {
                    if (SetRangeColumnToBounds(gridViewColumn) != 0) return;
                }

                this.ResizeColumns();
            }
        }

        private void OnScrollViewerScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (this._loaded && e.ViewportWidthChange != 0)
            {
                this.ResizeColumns();
            }
        }

        private static double SetRangeColumnToBounds(GridViewColumn gridViewColumn)
        {
            double startWidth = gridViewColumn.Width;

            double? minWidth = GridTreeViewColumnRange.GetRangeMinWidth(gridViewColumn);
            double? maxWidth = GridTreeViewColumnRange.GetRangeMaxWidth(gridViewColumn);

            if ((minWidth.HasValue && maxWidth.HasValue) && (minWidth > maxWidth)) return 0; // invalid case

            if (minWidth.HasValue && gridViewColumn.Width < minWidth.Value)
            {
                gridViewColumn.Width = minWidth.Value;
            }
            else if (maxWidth.HasValue && gridViewColumn.Width > maxWidth.Value)
            {
                gridViewColumn.Width = maxWidth.Value;
            }

            return gridViewColumn.Width - startWidth;
        }

        private static GridViewColumn FindColumn(DependencyObject element)
        {
            if (element == null) { return null; }

            while (element != null)
            {
                var header = element as GridViewColumnHeader;
                if (header != null) return header.Column;

                element = VisualTreeHelper.GetParent(element);
            }

            return null;
        }
    }
}
