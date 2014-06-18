/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130402 20:29
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130402 20:29
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace DesignerEngine
{
    internal static class RectAlgorithm
    {
        /// <summary>
        /// 获取 0,0 到 1,1 的一个值，表示边上的一个点。
        /// 
        /// 把 DesignerItem 看成一个 (0,0) 到 (1,1) 的矩形，这个属性描述连接点在这个矩形的位置。
        /// 这个位置应该只在四条边上。
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="rectPoint">这个点必须在矩形边上，否则会尝试使用矩形边上最靠近这个点的点来进行计算。</param>
        /// <returns></returns>
        public static Point GetRectRelative(Rect rect, Point rectPoint)
        {
            Rect2 rect2 = rect;
            var direction = rect2.LineContains(rectPoint);
            if (direction == null)
            {
                //为防止异常，直接计算为使用点所在的区域对应的边。
                direction = GetRectArea(rect, rectPoint);

                ////如果不是边上的点，则使用最靠近的那个点来进行计算。
                //var rectPoint2 = NearestPointOnLine(rect, rectPoint);
                //direction = rect2.LineContains(rectPoint2);
                //if (direction == null) throw new InvalidOperationException();
            }

            var res = new Point(0.5, 0.5);
            switch (direction.Value)
            {
                case RectLineDirection.Left:
                    res.X = 0;
                    res.Y = (rectPoint.Y - rect.Top) / rect.Height;
                    break;
                case RectLineDirection.Right:
                    res.X = 1;
                    res.Y = (rectPoint.Y - rect.Top) / rect.Height;
                    break;
                case RectLineDirection.Top:
                    res.X = (rectPoint.X - rect.Left) / rect.Width;
                    res.Y = 0;
                    break;
                case RectLineDirection.Bottom:
                    res.X = (rectPoint.X - rect.Left) / rect.Width;
                    res.Y = 1;
                    break;
                default:
                    break;
            }

            return res;
        }

        /// <summary>
        /// 通知矩形边上某点的相对位置，计算该点的绝对位置
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="relative"></param>
        /// <returns></returns>
        public static Point GetRectAbsolute(Rect rect, Point relative)
        {
            var x = rect.Left + rect.Width * relative.X;
            var y = rect.Top + rect.Height * relative.Y;
            return new Point(x, y);
        }

        /// <summary>
        /// 返回矩形中心点与指定点的向量，与矩形相交的点。
        /// 简单地认为，这个点即是矩形边上最靠近指定点的点。
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Point NearestPointOnLine(Rect rect, Point point)
        {
            Point res = point;

            if (rect.Contains(res))
            {
                var center = rect.GetCenterPoint();

                //没有斜率
                if (res.Y == center.Y)
                {
                    if (res.X < center.X)
                    {
                        res = RectAlgorithm.GetRectAbsolute(rect, new Point(0, 0.5));
                    }
                    else
                    {
                        res = RectAlgorithm.GetRectAbsolute(rect, new Point(1, 0.5));
                    }
                    return res;
                }
                else
                {
                    //使用向量继续向该方向延长。
                    Vector v = new Vector(res.X - center.X, res.Y - center.Y);
                    v *= 10000;
                    while (true)
                    {
                        res += v;
                        if (!rect.Contains(res)) break;
                    }
                }
            }

            res = CalcCrossPoint(rect, point);

            return res;
        }

        /// <summary>
        /// 计算矩形外某个点与矩形中点的连线，与矩形的相交点。
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="outerPoint"></param>
        /// <returns></returns>
        public static Point CalcCrossPoint(Rect rect, Point outerPoint)
        {
            //单元测试:
            //var res = CalcCrossPoint(new Point(300, 150), new Rect(0, 0, 200, 100));
            //if (res != new Point(200, 100)) throw new NotSupportedException();

            //res = CalcCrossPoint(new Point(100, 200), new Rect(0, 0, 200, 100));
            //if (res != new Point(100, 100)) throw new NotSupportedException();

            //res = CalcCrossPoint(new Point(100, -100), new Rect(0, 0, 200, 100));
            //if (res != new Point(100, 0)) throw new NotSupportedException();

            var area = GetRectArea(rect, outerPoint);

            Line line1 = new Line { A = rect.GetCenterPoint(), B = outerPoint };
            Line line2 = new Line();

            switch (area)
            {
                case RectLineDirection.Left:
                    line2.A = rect.TopLeft;
                    line2.B = rect.BottomLeft;
                    break;
                case RectLineDirection.Top:
                    line2.A = rect.TopLeft;
                    line2.B = rect.TopRight;
                    break;
                case RectLineDirection.Right:
                    line2.A = rect.TopRight;
                    line2.B = rect.BottomRight;
                    break;
                case RectLineDirection.Bottom:
                    line2.A = rect.BottomRight;
                    line2.B = rect.BottomLeft;
                    break;
                default:
                    break;
            }

            Point point = CalcCrossPoint(line1, line2);
            point.X = Round(point.X);
            point.Y = Round(point.Y);
            return point;
        }

        /// <summary>
        /// 按照中心点与四个角的连线，把空间分为四个区域。
        /// 返回点所在的区域。
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static RectLineDirection GetRectArea(Rect rect, Point point)
        {
            var c = rect.GetCenterPoint();
            var fromAngle = AngleFromCenter(c, point);

            var tl = AngleFromCenter(c, rect.TopLeft);
            var tr = AngleFromCenter(c, rect.TopRight);
            var br = AngleFromCenter(c, rect.BottomRight);
            var bl = AngleFromCenter(c, rect.BottomLeft);

            if (fromAngle > tl && fromAngle <= tr)
            {
                return RectLineDirection.Top;
            }
            else if (fromAngle > tr && fromAngle <= br)
            {
                return RectLineDirection.Right;
            }
            else if (fromAngle > br && fromAngle <= bl)
            {
                return RectLineDirection.Bottom;
            }
            else
            {
                return RectLineDirection.Left;
            }
        }

        /// <summary>
        /// 计算某个点到矩形中点的角度。
        /// </summary>
        /// <param name="center"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        private static double AngleFromCenter(Point center, Point point)
        {
            return ModelingHelper.ToAngle(point.Y - center.Y, point.X - center.X);
        }

        /// <summary>
        /// http://hi.baidu.com/gososoft/item/257aff956c79c830326eeb7c
        /// </summary>
        /// <param name="line1"></param>
        /// <param name="line2"></param>
        /// <returns></returns>
        public static Point CalcCrossPoint(Line line1, Line line2)
        {
            /*
            * L1，L2都存在斜率的情况：
            * 直线方程L1: ( y - y1 ) / ( y2 - y1 ) = ( x - x1 ) / ( x2 - x1 ) 
            * => y = [ ( y2 - y1 ) / ( x2 - x1 ) ]( x - x1 ) + y1
            * 令 a = ( y2 - y1 ) / ( x2 - x1 )
            * 有 y = a * x - a * x1 + y1   .........1
            * 直线方程L2: ( y - y3 ) / ( y4 - y3 ) = ( x - x3 ) / ( x4 - x3 )
            * 令 b = ( y4 - y3 ) / ( x4 - x3 )
            * 有 y = b * x - b * x3 + y3 ..........2
            * 
            * 如果 a = b，则两直线平等，否则， 联解方程 1,2，得:
            * x = ( a * x1 - b * x3 - y1 + y3 ) / ( a - b )
            * y = a * x - a * x1 + y1
            * 
            * L1存在斜率, L2平行Y轴的情况：
            * x = x3
            * y = a * x3 - a * x1 + y1
            * 
            * L1 平行Y轴，L2存在斜率的情况：
            * x = x1
            * y = b * x - b * x3 + y3
            * 
            * L1与L2都平行Y轴的情况：
            * 如果 x1 = x3，那么L1与L2重合，否则平等
            * 
           */

            var p1 = line1.A;
            var p2 = line1.B;
            var p3 = line2.A;
            var p4 = line2.B;

            double a = 0, b = 0;
            int state = 0;
            if (p1.X != p2.X)
            {
                a = (p2.Y - p1.Y) / (p2.X - p1.X);
                state |= 1;
            }
            if (p3.X != p4.X)
            {
                b = (p4.Y - p3.Y) / (p4.X - p3.X);
                state |= 2;
            }
            switch (state)
            {
                case 0: //L1与L2都平行Y轴
                    {
                        if (p1.X == p3.X)
                        {
                            throw new InvalidProgramException("两条直线互相重合，且平行于Y轴，无法计算交点。");
                        }
                        else
                        {
                            throw new InvalidProgramException("两条直线互相平行，且平行于Y轴，无法计算交点。");
                        }
                    }
                case 1: //L1存在斜率, L2平行Y轴
                    {
                        var x = p3.X;
                        var y = a * x - a * p1.X + p1.Y;
                        return new Point(x, y);
                    }
                case 2: //L1 平行Y轴，L2存在斜率
                    {
                        var x = p1.X;
                        var y = b * x + b * p3.X + p3.Y;
                        return new Point(x, y);
                    }
                case 3: //L1，L2都存在斜率
                    {
                        if (a == b)
                        {
                            throw new InvalidProgramException("两条直线平行或重合，无法计算交点。");
                        }
                        var x = (a * p1.X - b * p3.X - p1.Y + p3.Y) / (a - b);
                        var y = a * x - a * p1.X + p1.Y;
                        return new Point(x, y);
                    }
            }

            throw new InvalidProgramException("不可能发生的情况");
        }

        private static double Round(double y)
        {
            return ModelingHelper.Round(y);
        }

        /// <summary>
        /// 获取矩形的中心点。
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        public static Point GetCenterPoint(this Rect rect)
        {
            return new Point(
                rect.Left + rect.Width / 2,
                rect.Top + rect.Height / 2
                );
        }
    }
}