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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DesignerEngine
{
    /// <summary>
    /// 正交路径提供器。
    /// </summary>
    internal class PathHelper
    {
        private static StraightRouter _router = new StraightRouter();

        /// <summary>
        /// 为某个指定的目标连接器进行导航
        /// </summary>
        /// <param name="source">起始连接器</param>
        /// <param name="sink">终止位置</param>
        /// <param name="showLastLine"></param>
        /// <returns></returns>
        internal static PathGeometry DarwGeometry(ConnectorInfo source, ConnectorInfo sink)
        {
            var points = _router.Route(source, sink);
            var res = GenerateGeometry(points);
            return res;
        }

        /// <summary>
        /// 为某个指定的目标位置导航
        /// </summary>
        /// <param name="source">起始连接器</param>
        /// <param name="sinkPoint">目标位置</param>
        /// <param name="preferredOrientation">目标连接器的方向</param>
        /// <returns></returns>
        internal static PathGeometry DarwGeometry(ConnectorInfo source, Point sinkPoint)
        {
            var points = _router.Route(source, sinkPoint);
            var res = GenerateGeometry(points);
            return res;
        }

        /// <summary>
        /// 绘制两个点间的连线。
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        internal static PathGeometry DarwGeometry(Point from, Point to)
        {
            return GenerateGeometry(new List<Point> { from, to });
        }

        private static PathGeometry GenerateGeometry(List<Point> points)
        {
            bool pointsCreated = false;

            if (points.Count > 0)
            {
                pointsCreated = true;

                //当两个块的边叠在一起时，距离为 0，这时会造成外层的逻辑出错。所以应该做些临时的处理。
                if (points.Count == 2)
                {
                    var start = points[0];
                    var end = points[1];
                    if (ModelingHelper.RoundEqual(start, end))
                    {
                        pointsCreated = false;
                        //points[1] = new Point(end.X + 1, end.Y + 1);
                    }
                }
            }

            if (pointsCreated)
            {
                var geometry = new PathGeometry();

                PathFigure figure = new PathFigure();
                figure.StartPoint = points[0];
                points.Remove(points[0]);
                figure.Segments.Add(new PolyLineSegment(points, true));
                geometry.Figures.Add(figure);

                return geometry;
            }

            return null;
        }
    }
}