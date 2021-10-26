/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130528
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130528 17:27
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.Domain.ORM
{
    /// <summary>
    /// 属性约束条件工厂
    /// </summary>
    internal interface IConstraintFactory
    {
        /// <summary>
        /// 创建一个新的空的条件。
        /// </summary>
        /// <returns></returns>
        IConstraintGroup New();

        /// <summary>
        /// 通过属性表达式来构造一个约束条件。
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        IConstraintGroup New(PropertyComparisonExpression exp);

        /// <summary>
        /// 通过 Sql 语句来构造一个约束条件。
        /// </summary>
        /// <param name="formatSql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        IConstraintGroup New(string formatSql, params object[] parameters);

        //IConstraint CreateAnd(IConstraint left, IConstraint right);
        //IConstraint CreateOr(IConstraint left, IConstraint right);
    }
}