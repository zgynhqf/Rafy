/*******************************************************
 * 
 * 作者：CodeProject
 * 创建时间：2009
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 2009
 * 
*******************************************************/

using System.Collections.Generic;
using OEA.ManagedProperty;

namespace OEA.ORM
{
    internal interface IConstraint : IWhereConstraint
    {
        /// <summary>
        /// alias
        /// </summary>
        IManagedProperty Property { get; }

        /// <summary>
        /// =, <>, ...
        /// </summary>
        string Operator { get; }

        /// <summary>
        /// 确定的值
        /// </summary>
        object Value { get; set; }

        /// <summary>
        /// 此条件应用的表。
        /// </summary>
        DbTable PropertyTable { get; }
    }
}