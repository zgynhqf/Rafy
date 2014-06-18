/*******************************************************
 * 
 * 作者：周金根
 * 创建时间：20100330
 * 说明：支持控件放大功能
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 周金根 20100330
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows;

namespace Rafy.WPF
{
    public static class Zoom
    {
        private const double MIN_SCALE = 0.8;

        public static void DisableZoom(FrameworkElement fe)
        {
            fe.PreviewMouseWheel -= new System.Windows.Input.MouseWheelEventHandler(ctrl_PreviewMouseWheel);
            fe.PreviewMouseDown -= new MouseButtonEventHandler(ctrl_PreviewMouseDown);
        }

        public static void EnableZoom(FrameworkElement fe)
        {
            DisableZoom(fe);
            fe.PreviewMouseWheel += new System.Windows.Input.MouseWheelEventHandler(ctrl_PreviewMouseWheel);
            fe.PreviewMouseDown += new MouseButtonEventHandler(ctrl_PreviewMouseDown);
        }

        public static void EnableZoom(FrameworkElement fe, double zoomScale)
        {
            EnableZoom(fe);
            if (zoomScale != 1)
            {
                if (!(fe.LayoutTransform is ScaleTransform)) fe.LayoutTransform = new ScaleTransform();
                ScaleTransform st = fe.LayoutTransform as ScaleTransform;
                st.ScaleX = zoomScale;
                st.ScaleY = zoomScale;
            }
        }

        /// <summary>
        /// 滚轮中键恢复默认
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void ctrl_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement ctrl = sender as FrameworkElement;
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if (e.MiddleButton == MouseButtonState.Pressed)
                {
                    ScaleTransform st = ctrl.LayoutTransform as ScaleTransform;
                    if (null == st) return;
                    st.ScaleX = 1;
                    st.ScaleY = 1;
                }
            }
        }

        /// <summary>
        /// 限制缩小值
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void ctrl_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            FrameworkElement ctrl = sender as FrameworkElement;
            if (!(ctrl.LayoutTransform is ScaleTransform)) ctrl.LayoutTransform = new ScaleTransform();

            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                ScaleTransform stf = ctrl.LayoutTransform as ScaleTransform;
                stf.ScaleX += (e.Delta > 0) ? 0.1 : -0.1;
                stf.ScaleY += (e.Delta > 0) ? 0.1 : -0.1;
                if (stf.ScaleX < MIN_SCALE) stf.ScaleX = MIN_SCALE;
                if (stf.ScaleY < MIN_SCALE) stf.ScaleY = MIN_SCALE;
            }
        }
    }
}
