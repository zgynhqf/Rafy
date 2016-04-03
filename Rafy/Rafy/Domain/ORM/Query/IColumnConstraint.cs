/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20131212
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20131212 11:00
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.Domain.ORM.SqlTree;

namespace Rafy.Domain.ORM.Query
{
    /// <summary>
    /// 列的约束条件节点
    /// </summary>
    public interface IColumnConstraint : IConstraint
    {
        /// <summary>
        /// 要对比的列。
        /// </summary>
        IColumnNode Column { get; set; }

        /// <summary>
        /// 对比操作符
        /// </summary>
        PropertyOperator Operator { get; set; }

        /// <summary>
        /// 要对比的值。
        /// </summary>
        object Value { get; set; }
    }
}
