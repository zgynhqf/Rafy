/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20131212
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20131212 11:06
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
    /// 数据源与实体数据源连接后的结果节点
    /// </summary>
    public interface IJoin : ISource
    {
        /// <summary>
        /// 左边需要连接的数据源。
        /// </summary>
        ISource Left { get; set; }

        /// <summary>
        /// 连接方式
        /// </summary>
        JoinType JoinType { get; set; }

        /// <summary>
        /// 右边需要连接的数据源。
        /// </summary>
        ITableSource Right { get; set; }

        /// <summary>
        /// 连接所使用的约束条件。
        /// </summary>
        IConstraint Condition { get; set; }
    }

    /// <summary>
    /// 支持的连接方式。
    /// </summary>
    public enum JoinType
    {
        /// <summary>
        /// 内连接
        /// </summary>
        Inner = SqlJoinType.Inner,
        /// <summary>
        /// 左外连接
        /// </summary>
        LeftOuter = SqlJoinType.LeftOuter
    }
}