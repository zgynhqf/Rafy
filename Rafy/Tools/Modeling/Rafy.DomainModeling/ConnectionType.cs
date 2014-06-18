/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130403 19:31
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130403 19:31
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.DomainModeling
{
    /// <summary>
    /// 连接的类型。
    /// </summary>
    public enum ConnectionType
    {
        /// <summary>
        /// 非空引用
        /// </summary>
        Reference,
        /// <summary>
        /// 可空引用
        /// </summary>
        NullableReference,
        /// <summary>
        /// 组合
        /// </summary>
        Composition,
        /// <summary>
        /// 聚合
        /// </summary>
        Aggregation,
        /// <summary>
        /// 继承
        /// </summary>
        Inheritance
    }
}