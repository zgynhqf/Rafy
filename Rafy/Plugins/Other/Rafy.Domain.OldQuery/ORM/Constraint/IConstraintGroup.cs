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
    /// 查询的一组约束条件。
    /// <remarks>
    /// 一个约束条件对象，可以表示一组连续的约束的列表或组。
    /// 每一次调用 Add 方法就是把一个属性对比条件或者另一个约束组加入到本组约束中。
    /// 每一个不同的约束组，在查询时会用括号包含起来。
    /// 条件经过组合后，会在查询时生成相应的 Where 条件语句。
    /// </remarks>
    /// </summary>
    internal interface IConstraintGroup
    {
        /// <summary>
        /// 判断是否本条件是空的。
        /// 空条件表示没有进行任何约束。
        /// </summary>
        bool IsEmpty { get; }

        /// <summary>
        /// 在当前约束条件的基础上再添加指定的属性对比约束。
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        IConstraintGroup And(PropertyComparisonExpression exp);

        /// <summary>
        /// 在当前约束条件的基础上再添加指定的属性对比约束。
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        IConstraintGroup Or(PropertyComparisonExpression exp);

        /// <summary>
        /// 在当前约束条件的基础上再添加指定的约束。
        /// </summary>
        /// <param name="constraint"></param>
        /// <returns></returns>
        IConstraintGroup And(IConstraintGroup constraint);

        /// <summary>
        /// 在当前约束条件的基础上再添加指定的约束。
        /// </summary>
        /// <param name="constraint"></param>
        /// <returns></returns>
        IConstraintGroup Or(IConstraintGroup constraint);
    }
}