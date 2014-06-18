/*******************************************************
 * 
 * 作者：CodeProject
 * 创建时间：20130330
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130330
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
    /// 连接点的信息。
    /// 
    /// 提取出这些纯粹的数据，方便计算。
    /// </summary>
    internal struct ConnectorInfo
    {
        /// <summary>
        /// 连接点所属的设计元素的大小
        /// </summary>
        public Rect DesignerItemRect { get; set; }

        /// <summary>
        /// 连接点的位置
        /// </summary>
        public Point Position { get; set; }
    }
}
