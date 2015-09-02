/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20131210
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20131210 09:47
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.Data;
using Rafy.Domain.ORM.SqlTree;

namespace Rafy.Domain.ORM.SqlTree
{
    /// <summary>
    /// SqlNode 语法树的访问器
    /// </summary>
    abstract class SqlNodeVisitor
    {
        protected virtual ISqlNode Visit(ISqlNode node)
        {
            switch (node.NodeType)
            {
                case SqlNodeType.SqlLiteral:
                    return this.VisitSqlLiteral(node as SqlLiteral);
                case SqlNodeType.SqlNodeList:
                    return this.VisitSqlNodeList(node as SqlNodeList);
                case SqlNodeType.SqlSelect:
                    return this.VisitSqlSelect(node as SqlSelect);
                case SqlNodeType.SqlColumn:
                    return this.VisitSqlColumn(node as SqlColumn);
                case SqlNodeType.SqlTable:
                    return this.VisitSqlTable(node as SqlTable);
                case SqlNodeType.SqlColumnConstraint:
                    return this.VisitSqlColumnConstraint(node as SqlColumnConstraint);
                case SqlNodeType.SqlBinaryConstraint:
                    return this.VisitSqlBinaryConstraint(node as SqlBinaryConstraint);
                case SqlNodeType.SqlJoin:
                    return this.VisitSqlJoin(node as SqlJoin);
                case SqlNodeType.SqlArray:
                    return this.VisitSqlArray(node as SqlArray);
                case SqlNodeType.SqlSelectAll:
                    return this.VisitSqlSelectAll(node as SqlSelectAll);
                case SqlNodeType.SqlColumnsComparisonConstraint:
                    return this.VisitSqlColumnsComparisonConstraint(node as SqlColumnsComparisonConstraint);
                case SqlNodeType.SqlExistsConstraint:
                    return this.VisitSqlExistsConstraint(node as SqlExistsConstraint);
                case SqlNodeType.SqlNotConstraint:
                    return this.VisitSqlNotConstraint(node as SqlNotConstraint);
                case SqlNodeType.SqlSubSelect:
                    return this.VisitSqlSubSelect(node as SqlSubSelect);
                case SqlNodeType.SqlOrderBy:
                    return this.VisitSqlOrderBy(node as SqlOrderBy);
                case SqlNodeType.SqlOrderByList:
                    return this.VisitSqlOrderByList(node as SqlOrderByList);
                default:
                    break;
            }
            throw new NotImplementedException();
        }

        protected virtual SqlNode VisitSqlNodeList(SqlNodeList sqlNodeList)
        {
            for (int i = 0, c = sqlNodeList.Items.Count; i < c; i++)
            {
                var item = sqlNodeList.Items[i];
                if (item != null)
                {
                    this.Visit(item);
                }
            }
            return sqlNodeList;
        }

        protected virtual SqlJoin VisitSqlJoin(SqlJoin sqlJoin)
        {
            this.Visit(sqlJoin.Left);
            this.Visit(sqlJoin.Right);
            this.Visit(sqlJoin.Condition);
            return sqlJoin;
        }

        protected virtual SqlBinaryConstraint VisitSqlBinaryConstraint(SqlBinaryConstraint node)
        {
            this.Visit(node.Left);
            this.Visit(node.Right);
            return node;
        }

        protected virtual SqlSelect VisitSqlSelect(SqlSelect sqlSelect)
        {
            if (sqlSelect.Selection != null)
            {
                this.Visit(sqlSelect.Selection);
            }
            this.Visit(sqlSelect.From);
            if (sqlSelect.Where != null)
            {
                this.Visit(sqlSelect.Where);
            }
            if (sqlSelect.HasOrdered())
            {
                for (int i = 0, c = sqlSelect.OrderBy.Count; i < c; i++)
                {
                    var item = sqlSelect.OrderBy.Items[i] as SqlNode;
                    this.Visit(item);
                }
            }
            return sqlSelect;
        }

        protected virtual SqlTable VisitSqlTable(SqlTable sqlTable)
        {
            return sqlTable;
        }

        protected virtual SqlColumn VisitSqlColumn(SqlColumn sqlColumn)
        {
            return sqlColumn;
        }

        protected virtual SqlColumnConstraint VisitSqlColumnConstraint(SqlColumnConstraint node)
        {
            this.Visit(node.Column);
            return node;
        }

        protected virtual SqlLiteral VisitSqlLiteral(SqlLiteral sqlLiteral)
        {
            return sqlLiteral;
        }

        protected virtual SqlArray VisitSqlArray(SqlArray sqlArray)
        {
            for (int i = 0, c = sqlArray.Items.Count; i < c; i++)
            {
                var item = sqlArray.Items[i] as SqlNode;
                this.Visit(item);
            }
            return sqlArray;
        }

        protected virtual SqlSelectAll VisitSqlSelectAll(SqlSelectAll sqlSelectStar)
        {
            return sqlSelectStar;
        }

        protected virtual SqlColumnsComparisonConstraint VisitSqlColumnsComparisonConstraint(SqlColumnsComparisonConstraint sqlColumnsConstraint)
        {
            this.Visit(sqlColumnsConstraint.LeftColumn);
            this.Visit(sqlColumnsConstraint.RightColumn);
            return sqlColumnsConstraint;
        }

        protected virtual SqlExistsConstraint VisitSqlExistsConstraint(SqlExistsConstraint sqlExistsConstraint)
        {
            this.Visit(sqlExistsConstraint.Select);
            return sqlExistsConstraint;
        }

        protected virtual SqlNotConstraint VisitSqlNotConstraint(SqlNotConstraint sqlNotConstraint)
        {
            this.Visit(sqlNotConstraint.Constraint);
            return sqlNotConstraint;
        }

        protected virtual SqlSubSelect VisitSqlSubSelect(SqlSubSelect subSelect)
        {
            this.Visit(subSelect.Select);
            return subSelect;
        }

        protected virtual SqlOrderBy VisitSqlOrderBy(SqlOrderBy sqlOrderBy)
        {
            return sqlOrderBy;
        }

        protected virtual SqlOrderByList VisitSqlOrderByList(SqlOrderByList sqlOrderByList)
        {
            foreach (SqlOrderBy item in sqlOrderByList.Items)
            {
                this.Visit(item);
            }
            return sqlOrderByList;
        }
    }
}