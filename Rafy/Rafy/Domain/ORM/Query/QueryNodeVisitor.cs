/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20131212
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20131212 11:09
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.Domain.ORM.Query.Impl;

namespace Rafy.Domain.ORM.Query
{
    /// <summary>
    /// 如果需要遍历 IQuery 对象，可以使用本类来进行访问。
    /// 
    /// Rafy.Domain.ORM.Query 中的所有接口，构成了新的面向 Entity、IManagedProperty 的查询语法树。
    /// </summary>
    public abstract class QueryNodeVisitor
    {
        protected virtual IQueryNode Visit(IQueryNode node)
        {
            switch (node.NodeType)
            {
                case QueryNodeType.Query:
                    return this.VisitQuery(node as IQuery);
                case QueryNodeType.SubQuery:
                    return this.VisitSubQueryRef(node as ISubQuery);
                case QueryNodeType.Array:
                    return this.VisitArray(node as IArray);
                case QueryNodeType.TableSource:
                    return this.VisitEntitySource(node as ITableSource);
                case QueryNodeType.SelectAll:
                    return this.VisitSelectAll(node as ISelectAll);
                case QueryNodeType.Join:
                    return this.VisitJoin(node as IJoin);
                case QueryNodeType.OrderBy:
                    return this.VisitOrderBy(node as IOrderBy);
                case QueryNodeType.Column:
                    return this.VisitProperty(node as IColumnNode);
                case QueryNodeType.ColumnConstraint:
                    return this.VisitPropertyConstraint(node as IColumnConstraint);
                case QueryNodeType.ColumnsComparisonConstraint:
                    return this.VisitTwoPropertiesConstraint(node as IColumnsComparison);
                case QueryNodeType.BinaryConstraint:
                    return this.VisitBinaryConstraint(node as IBinaryConstraint);
                case QueryNodeType.ExistsConstraint:
                    return this.VisitExistsConstraint(node as IExistsConstraint);
                case QueryNodeType.NotConstraint:
                    return this.VisitNotConstraint(node as INotConstraint);
                case QueryNodeType.Literal:
                    return this.VisitLiteral(node as ILiteral);
                default:
                    throw new NotSupportedException();
            }
        }

        protected virtual IJoin VisitJoin(IJoin node)
        {
            this.Visit(node.Left);
            this.Visit(node.Right);
            this.Visit(node.Condition);
            return node;
        }

        protected virtual IBinaryConstraint VisitBinaryConstraint(IBinaryConstraint node)
        {
            this.Visit(node.Left);
            this.Visit(node.Right);
            return node;
        }

        protected virtual IQuery VisitQuery(IQuery node)
        {
            if (node.Selection != null)
            {
                this.Visit(node.Selection);
            }
            this.Visit(node.From);
            if (node.Where != null)
            {
                this.Visit(node.Where);
            }
            var entityQuery = node as TableQuery;
            if (entityQuery.HasOrdered())
            {
                for (int i = 0, c = node.OrderBy.Count; i < c; i++)
                {
                    var item = node.OrderBy[i];
                    this.Visit(item);
                }
            }
            return node;
        }

        protected virtual ITableSource VisitEntitySource(ITableSource node)
        {
            return node;
        }

        protected virtual IColumnNode VisitProperty(IColumnNode node)
        {
            return node;
        }

        protected virtual IColumnConstraint VisitPropertyConstraint(IColumnConstraint node)
        {
            this.Visit(node.Column);
            return node;
        }

        protected virtual ILiteral VisitLiteral(ILiteral node)
        {
            return node;
        }

        protected virtual IArray VisitArray(IArray node)
        {
            for (int i = 0, c = node.Items.Count; i < c; i++)
            {
                var item = node.Items[i];
                this.Visit(item);
            }
            return node;
        }

        protected virtual ISelectAll VisitSelectAll(ISelectAll node)
        {
            return node;
        }

        protected virtual IColumnsComparison VisitTwoPropertiesConstraint(IColumnsComparison node)
        {
            this.Visit(node.LeftColumn);
            this.Visit(node.RightColumn);
            return node;
        }

        protected virtual IExistsConstraint VisitExistsConstraint(IExistsConstraint node)
        {
            this.Visit(node.Query);
            return node;
        }

        protected virtual INotConstraint VisitNotConstraint(INotConstraint node)
        {
            this.Visit(node.Constraint);
            return node;
        }

        protected virtual ISubQuery VisitSubQueryRef(ISubQuery node)
        {
            this.Visit(node.Query);
            return node;
        }

        protected virtual IQueryNode VisitOrderBy(IOrderBy node)
        {
            this.Visit(node.Column);
            return node;
        }
    }
}