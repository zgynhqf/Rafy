/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120330
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120330
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OEA.Library.Validation
{
    /// <summary>
    /// 类型规则的存储器。
    /// 内部使用。
    /// </summary>
    internal interface ITypeValidationsHost
    {
        /// <summary>
        /// 类型规则集合
        /// </summary>
        ValidationRulesManager Rules { get; }
    }
}