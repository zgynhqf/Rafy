/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20131214
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20131214 21:48
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.Domain.ORM
{
    /// <summary>
    /// IDb Select 方法的参数。
    /// </summary>
    internal interface IPropertySelectArgs : ISelectArgs
    {
        /// <summary>
        /// 对应的查询条件定义。
        /// </summary>
        IPropertyQuery PropertyQuery { get; }
    }
}
