/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20131212
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20131212 11:20
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.Domain.ORM.Query
{
    /// <summary>
    /// 两个列进行对比的约束条件节点
    /// </summary>
    public interface IColumnsComparison : IConstraint
    {
        /// <summary>
        /// 第一个需要对比的列。
        /// </summary>
        IColumnNode LeftColumn { get; set; }

        /// <summary>
        /// 第二个需要对比的列。
        /// </summary>
        IColumnNode RightColumn { get; set; }

        /// <summary>
        /// 对比条件。
        /// </summary>
        PropertyOperator Operator { get; set; }
    }
}
