/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130401
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130401
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace DesignerEngine
{
    /// <summary>
    /// 一个新的矩形类型。
    /// 其中计算出了矩形的四条边。
    /// </summary>
    internal struct Rect2
    {
        public Rect Rect;

        public Line Left, Top, Right, Bottom;

        public Rect2(Rect rect)
        {
            Rect = rect;
            Left = new Line(rect.BottomLeft, rect.TopLeft);
            Top = new Line(rect.TopLeft, rect.TopRight);
            Right = new Line(rect.TopRight, rect.BottomRight);
            Bottom = new Line(rect.BottomRight, rect.BottomLeft);
        }

        /// <summary>
        /// 如果某条边包含这个点，则返回这条边。
        /// 否则返回 null。
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public RectLineDirection? LineContains(Point point)
        {
            if (Left.Contains(point)) { return RectLineDirection.Left; }
            if (Top.Contains(point)) { return RectLineDirection.Top; }
            if (Right.Contains(point)) { return RectLineDirection.Right; }
            if (Bottom.Contains(point)) { return RectLineDirection.Bottom; }
            return null;
        }

        public static implicit operator Rect2(Rect value)
        {
            return new Rect2(value);
        }

        public static implicit operator Rect(Rect2 value)
        {
            return value.Rect;
        }
    }

    /// <summary>
    /// 描述两个点连接成的线段
    /// </summary>
    internal struct Line
    {
        public Line(Point a, Point b)
        {
            A = a;
            B = b;
        }

        public Point A;

        public Point B;

        public bool Contains(Point point)
        {
            //斜率近似一致。
            if (point.Y == A.Y && B.Y == point.Y) return true;

            var g1 = (point.X - A.X) / (point.Y - A.Y);
            var g2 = (B.X - point.X) / (B.Y - point.Y);
            var distance = g1 - g2;
            var distance2 = Math.Round(distance, 6);
            return distance2 == 0;
        }
    }

    /// <summary>
    /// 矩形边的四个方向
    /// </summary>
    internal enum RectLineDirection { Left, Top, Right, Bottom }
}