/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20131210
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20131210 09:41
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Rafy.Data;

namespace Rafy.Domain.ORM.SqlTree
{
    /// <summary>
    /// 表示 Sql 语法树中的一个节点。
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay}")]
    abstract class SqlNode : ISqlNode
    {
        /// <summary>
        /// 返回当前树节点的类型。
        /// </summary>
        /// <value>
        /// The type of the node.
        /// </value>
        public abstract SqlNodeType NodeType { get; }

        private string DebuggerDisplay
        {
            get
            {
                var generator = new SqlServerSqlGenerator();
                generator.Generate(this);
                return string.Format(generator.Sql, generator.Sql.Parameters);
            }
        }
    }

    /// <summary>
    /// 语法树节点类型。
    /// </summary>
    enum SqlNodeType
    {
        SqlNodeList,
        SqlLiteral,
        SqlArray,
        SqlSelect,
        SqlTable,
        SqlColumn,
        SqlJoin,
        SqlOrderBy,
        SqlOrderByList,
        SqlSelectAll,
        SqlSubSelect,
        SqlColumnConstraint,
        SqlBinaryConstraint,
        SqlColumnsComparisonConstraint,
        SqlExistsConstraint,
        SqlNotConstraint,
        //XXXXXXXXXXXXXXXXXXX,
        //XXXXXXXXXXXXXXXXXXX,
        //XXXXXXXXXXXXXXXXXXX,
        //XXXXXXXXXXXXXXXXXXX,
        //XXXXXXXXXXXXXXXXXXX,
        //XXXXXXXXXXXXXXXXXXX,
        //XXXXXXXXXXXXXXXXXXX,
        //XXXXXXXXXXXXXXXXXXX,
        //XXXXXXXXXXXXXXXXXXX,
        //XXXXXXXXXXXXXXXXXXX,
        //XXXXXXXXXXXXXXXXXXX,
        //XXXXXXXXXXXXXXXXXXX,
    }
}