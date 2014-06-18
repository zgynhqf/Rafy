/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20111206
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20111206
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.WPF.Controls
{
    /// <summary>
    /// 列宽的测量/计算的状态
    /// 
    /// 前三种状态一同合作来实现动态列宽，最后一种状态则是手工指定的静态列宽。
    /// </summary>
    internal enum ColumnMeasureState
    {
        /// <summary>
        /// 正在初始化。
        /// 
        /// 此状态表明列宽还没有计算出来，需要初始化。
        /// </summary>
        Init,
        /// <summary>
        /// 列宽已经使用列头被计算出来。
        /// </summary>
        Headered,
        /// <summary>
        /// 列宽已经使用该列所有行中最长的一行计算出来。
        /// </summary>
        Data,
        /// <summary>
        /// 被指定了宽度，不需要自动列宽。
        /// </summary>
        SpecificWidth
    }
}
