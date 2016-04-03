/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20131212
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20131212 11:02
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.Domain.ORM.Query
{
    /// <summary>
    /// 是否存在查询结果的约束条件节点
    /// </summary>
    public interface IExistsConstraint : IConstraint
    {
        /// <summary>
        /// 要检查的查询。
        /// </summary>
        IQuery Query { get; set; }
    }
}