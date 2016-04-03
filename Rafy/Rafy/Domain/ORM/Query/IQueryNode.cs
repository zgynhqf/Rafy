/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20131212
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20131212 10:14
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.Domain.ORM.Query
{
    /// <summary>
    /// 表示查询语法树中的一个节点。
    /// Rafy.Domain.ORM.Query 中的接口，构成了面向 IManagedProperty 的查询语法树。
    /// </summary>
    public interface IQueryNode
    {
        /// <summary>
        /// 节点的类型。
        /// </summary>
        QueryNodeType NodeType { get; }
    }

    /// <summary>
    /// 查询树节点的类型。
    /// </summary>
    public enum QueryNodeType
    {
        /// <summary>
        /// 查询结果
        /// </summary>
        Query,
        /// <summary>
        /// 可嵌套的子查询
        /// </summary>
        SubQuery,
        /// <summary>
        /// 节点的数组
        /// </summary>
        Array,
        /// <summary>
        /// 查询的表数据源
        /// </summary>
        TableSource,
        /// <summary>
        /// 表示查询数据源中的所有属性的节点
        /// </summary>
        SelectAll,
        /// <summary>
        /// 数据源与实体数据源连接后的结果节点
        /// </summary>
        Join,
        /// <summary>
        /// 排序节点
        /// </summary>
        OrderBy,
        /// <summary>
        /// 列节点
        /// </summary>
        Column,
        /// <summary>
        /// 列的约束条件节点
        /// </summary>
        ColumnConstraint,
        /// <summary>
        /// 两个列进行对比的约束条件节点
        /// </summary>
        ColumnsComparisonConstraint,
        /// <summary>
        /// 二位操作符连接的节点
        /// </summary>
        BinaryConstraint,
        /// <summary>
        /// 是否存在查询结果的约束条件节点
        /// </summary>
        ExistsConstraint,
        /// <summary>
        /// 对指定约束条件节点执行取反规则的约束条件节点
        /// </summary>
        NotConstraint,
        /// <summary>
        /// 查询文本
        /// </summary>
        Literal,
    }
}