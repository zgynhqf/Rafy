/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130401 11:55
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130401 11:55
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using DesignerEngine;

namespace DesignerEngine
{
    /// <summary>
    /// 直线算法。
    /// </summary>
    internal class StraightRouter
    {
        /// <summary>
        /// 为某个指定的目标连接器进行导航
        /// </summary>
        /// <param name="source">起始连接器</param>
        /// <param name="sink">终止位置</param>
        /// <returns></returns>
        internal List<Point> Route(ConnectorInfo source, Point sinkPoint)
        {
            return new List<Point>
            {
                source.Position,
                sinkPoint
            };
        }

        /// <summary>
        /// 为某个指定的目标位置导航
        /// </summary>
        /// <param name="source">起始连接器</param>
        /// <param name="sinkPoint">目标位置</param>
        /// <param name="preferredOrientation">目标连接器的方向</param>
        /// <returns></returns>
        internal List<Point> Route(ConnectorInfo source, ConnectorInfo sink)
        {
            var linePoints = new List<Point>();

            var start = RectAlgorithm.CalcCrossPoint(source.DesignerItemRect, sink.Position);

            var end = RectAlgorithm.CalcCrossPoint(sink.DesignerItemRect, source.Position);

            linePoints.Insert(0, start);
            linePoints.Add(end);

            return linePoints;
        }
    }
}