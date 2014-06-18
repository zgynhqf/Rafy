/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130402 13:55
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130402 13:55
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace DesignerEngine
{
    internal static class ModelingHelper
    {
        /// <summary>
        /// Get angle from tangent vector
        /// </summary>
        /// <param name="y"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        public static double ToAngle(double y, double x)
        {
            return Math.Atan2(y, x) * (180 / Math.PI);
        }

        public static double Round(double y)
        {
            return Math.Round(y, 4);
        }

        public static bool RoundEqual(Point a, Point b)
        {
            var x1 = Round(a.X);
            var y1 = Round(a.Y);
            var x2 = Round(b.X);
            var y2 = Round(b.Y);

            return x1 == x2 && y1 == y2;
        }

        public static T GetVisualParent<T>(DependencyObject child) where T : Visual
        {
            var parent = VisualTreeHelper.GetParent(child);
            if (parent == null) return null;
            if (parent is T) return parent as T;

            return GetVisualParent<T>(parent);
        }
    }
}