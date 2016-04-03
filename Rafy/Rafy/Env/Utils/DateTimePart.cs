/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120602 19:56
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120602 19:56
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.Utils
{
    /// <summary>
    /// 系统的 DateTime 类型正在被使用的部分
    /// </summary>
    public enum DateTimePart
    {
        /// <summary>
        /// 日期及时间都被使用
        /// Rafy 中属性编辑器的默认值。
        /// </summary>
        DateTime,
        /// <summary>
        /// 使用日期部分
        /// </summary>
        Date,
        /// <summary>
        /// 使用时间部分
        /// </summary>
        Time
    }
}