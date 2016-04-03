/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130526
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130526 14:52
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.ManagedProperty
{
    /// <summary>
    /// 大对象属性
    /// </summary>
    public interface ILOBProperty : IManagedProperty
    {
        /// <summary>
        /// LOB属性的类型
        /// </summary>
        LOBType LOBType { get; }
    }
}