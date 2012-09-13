using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using OEA.MetaModel;
using OEA.MetaModel.View;
using OEA.Module.WPF.Editors;

namespace OEA.Module.WPF
{
    /// <summary>
    /// 用于给某些已有的控件添加额外的属性
    /// </summary>
    public class WPFMeta : DependencyObject
    {
        #region ObjectView

        public static readonly DependencyProperty ObjectViewProperty =
            DependencyProperty.RegisterAttached("ObjectView", typeof(ObjectView), typeof(WPFMeta));

        public static void SetObjectView(DependencyObject d, ObjectView value)
        {
            d.SetValue(ObjectViewProperty, value);
        }

        public static ObjectView GetObjectView(DependencyObject d)
        {
            return d.GetValue(ObjectViewProperty) as ObjectView;
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