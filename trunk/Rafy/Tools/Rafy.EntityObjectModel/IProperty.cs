/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130329 10:26
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130329 10:26
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.EntityObjectModel
{
    /// <summary>
    /// 属性
    /// </summary>
    public interface IProperty
    {
        /// <summary>
        /// 属性名
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 属性类型
        /// </summary>
        string PropertyType { get; }
    }
}
