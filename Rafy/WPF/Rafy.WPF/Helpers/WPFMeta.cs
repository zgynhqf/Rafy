using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Rafy.MetaModel;
using Rafy.MetaModel.View;
using Rafy.WPF.Editors;

namespace Rafy.WPF
{
    /// <summary>
    /// 用于给某些已有的控件添加额外的属性
    /// </summary>
    public class WPFMeta : DependencyObject
    {
        #region LogicalView

        public static readonly DependencyProperty LogicalViewProperty =
            DependencyProperty.RegisterAttached("LogicalView", typeof(LogicalView), typeof(WPFMeta));

        public static void SetLogicalView(DependencyObject d, LogicalView value)
        {
            d.SetValue(LogicalViewProperty, value);
        }

        public static LogicalView GetLogicalView(DependencyObject d)
        {
            return d.GetValue(LogicalViewProperty) as LogicalView;
        }

        #endregion

        //#region LastResizeSize

        //public static readonly DependencyProperty LastResizeSizeProperty =
        //    DependencyProperty.RegisterAttached("LastResizeSize", typeof(ResizeSize), typeof(WPFMeta));

        //public static void SetLastResizeSize(FrameworkElement d, ResizeSize value)
        //{
        //    d.SetValue(LastResizeSizeProperty, value);
        //}

        //public static ResizeSize GetLastResizeSize(FrameworkElement d)
        //{
        //    return d.GetValue(LastResizeSizeProperty) as ResizeSize;
        //}

        //#endregion

        //public class ResizeSize
        //{
        //    public GridLength Width;
        //    public GridLength Height;
        //}
    }
}