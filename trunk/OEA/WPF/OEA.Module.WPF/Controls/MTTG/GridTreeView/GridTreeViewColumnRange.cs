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

using System.Windows;
using System.Windows.Controls;
using System;

namespace OEA.Module.WPF.Controls
{
    /// <summary>
    /// 用于设置 GridColumn 列的宽度范围。
    /// 
    /// 代码来自：[WPF疑难] 如何限定ListView列宽度
    /// http://www.cnblogs.com/zhouyinhui/archive/2008/06/03/1213030.html
    /// </summary>
    public static class GridTreeViewColumnRange
    {
        public static void SetDefault(GridViewColumn column)
        {
            SetMinWidth(column, 40);
        }

        #region MinWidthProperty

        public static readonly DependencyProperty MinWidthProperty = DependencyProperty.RegisterAttached(
                "MinWidth", typeof(double), typeof(GridTreeViewColumnRange));

        public static double GetMinWidth(DependencyObject obj)
        {
            return (double)obj.GetValue(MinWidthProperty);
        }

        public static void SetMinWidth(DependencyObject obj, double minWidth)
        {
            obj.SetValue(MinWidthProperty, minWidth);
        }

        #endregion

        #region MaxWidthProperty

        public static readonly DependencyProperty MaxWidthProperty = DependencyProperty.RegisterAttached(
                "MaxWidth", typeof(double), typeof(GridTreeViewColumnRange));

        public static double GetMaxWidth(DependencyObject obj)
        {
            return (double)obj.GetValue(MaxWidthProperty);
        }

        public static void SetMaxWidth(DependencyObject obj, double maxWidth)
        {
            obj.SetValue(MaxWidthProperty, maxWidth);
        }

        #endregion

        public static bool IsRangeColumn(GridViewColumn column)
        {
            if (column == null) { return false; }

            return GetRangeMinWidth(column).HasValue || GetRangeMaxWidth(column).HasValue;
        }

        public static double? GetRangeMinWidth(GridViewColumn column)
        {
            return GetLocalDoubleValue(column, MinWidthProperty);
        }

        public static double? GetRangeMaxWidth(GridViewColumn column)
        {
            return GetLocalDoubleValue(column, MaxWidthProperty);
        }

        public static GridViewColumn ApplyWidth(GridViewColumn gridViewColumn, double minWidth, double width, double maxWidth)
        {
            SetMinWidth(gridViewColumn, minWidth);
            gridViewColumn.Width = width;
            SetMaxWidth(gridViewColumn, maxWidth);
            return gridViewColumn;
        }

        private static double? GetLocalDoubleValue(GridViewColumn column, DependencyProperty dp)
        {
            if (column == null) { throw new ArgumentNullException("column"); }

            object value = column.ReadLocalValue(dp);
            if (value != null && value.GetType() == typeof(double))
            {
                return (double)value;
            }

            return null;
        }
    }
}