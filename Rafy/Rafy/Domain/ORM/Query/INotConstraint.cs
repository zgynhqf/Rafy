/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20131212
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20131212 11:11
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.Domain.ORM.Query
{
    /// <summary>
    /// 对指定约束条件节点执行取反规则的约束条件节点
    /// </summary>
    public interface INotConstraint : IConstraint
    {
        /// <summary>
        /// 需要被取反的条件。
        /// </summary>
        IConstraint Constraint { get; set; }
    }
}
