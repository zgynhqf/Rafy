/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20131212
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20131212 11:10
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.Domain.ORM.Query
{
    /// <summary>
    /// 查询文本。
    /// 查询文本可以表示一个条件。
    /// </summary>
    public interface ILiteral : IQueryNode, IConstraint
    {
        /// <summary>
        /// 查询文本。
        /// </summary>
        string FormattedSql { get; set; }

        /// <summary>
        /// 对应的参数值列表
        /// </summary>
        object[] Parameters { get; set; }
    }
}