using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using OEA.Module.WPF.Editors;
using System.Collections.ObjectModel;

using OEA.MetaModel;
using OEA.MetaModel.View;

namespace OEA.Module.WPF.Controls
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

        #region LastParent

        public static readonly DependencyProperty LastParentProperty =
            DependencyProperty.RegisterAttached("LastParent", typeof(FrameworkElement), typeof(WPFMeta));

        public static void SetLastParent(FrameworkElement d, FrameworkElement value)
        {
            d.SetValue(LastParentProperty, value);
        }

        public static FrameworkElement GetLastParent(FrameworkElement d)
        {
            return d.GetValue(LastParentProperty) as FrameworkElement;
        }

        #endregion

        #region LastIndexInParent

        public static readonly DependencyProperty LastIndexInParentProperty =
            DependencyProperty.RegisterAttached("LastIndexInParent", typeof(int), typeof(WPFMeta));

        public static void SetLastIndexInParent(FrameworkElement d, int value)
        {
            d.SetValue(LastIndexInParentProperty, value);
        }

        public static int GetLastIndexInParent(FrameworkElement d)
        {
            return (int)d.GetValue(LastIndexInParentProperty);
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