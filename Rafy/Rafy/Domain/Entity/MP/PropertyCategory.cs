/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20111110
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20111110
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.Domain
{
    /// <summary>
    /// Rafy 中可用的属性类型
    /// </summary>
    public enum PropertyCategory
    {
        /// <summary>
        /// 一般属性
        /// </summary>
        Normal,
        /// <summary>
        /// 引用属性
        /// </summary>
        ReferenceId,
        /// <summary>
        /// 引用属性
        /// </summary>
        ReferenceEntity,
        /// <summary>
        /// 列表属性
        /// </summary>
        List,
        /// <summary>
        /// 只读属性
        /// </summary>
        Readonly,
        /// <summary>
        /// 冗余属性
        /// </summary>
        Redundancy,
        /// <summary>
        /// LOB 属性
        /// </summary>
        LOB
    }
}