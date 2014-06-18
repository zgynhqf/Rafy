/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130412 10:53
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130412 10:53
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DesignerEngine
{
    /// <summary>
    /// 画布中可容纳的元素。
    /// </summary>
    public interface IDesignerCanvasItem
    {
        /// <summary>
        /// 是否已经被选择。
        /// </summary>
        bool IsSelected { get; }

        /// <summary>
        /// 外层的 Canvas
        /// </summary>
        DesignerCanvas OwnerCanvas { get; }
    }

    /// <summary>
    /// 一个可以被选择的项。
    /// 
    /// 设计器元素、连接线，都是可以被选择的。
    /// </summary>
    internal interface IDesignerCanvasItemInternal : IDesignerCanvasItem
    {
        /// <summary>
        /// 是否已经被选择。
        /// </summary>
        new bool IsSelected { get; set; }
    }
}
