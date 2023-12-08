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
using System.Collections;
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
                    return this.VisitTableSource(node as ITableSource);
                case QueryNodeType.SelectAll:
                    return this.VisitSelectAll(node as ISelectAll);
                case QueryNodeType.Join:
                    return this.VisitJoin(node as IJoin);
                case QueryNodeType.OrderBy:
                    return this.VisitOrderBy(node as IOrderBy);
                case QueryNodeType.Column:
                    return this.VisitColumn(node as IColumnNode);
                case QueryNodeType.ColumnConstraint:
                    return this.VisitColumnConstraint(node as IColumnConstraint);
                case QueryNodeType.ColumnsComparisonConstraint:
                    return this.VisitTwoColumnsConstraint(node as IColumnsComparison);
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

        protected virtual IQueryNode VisitJoin(IJoin node)
        {
            var c = node.Left;
            var r = this.Visit(c) as ISource;
            if (r != c) node.Left = r;

            var rc = node.Right;
            var rr = this.Visit(rc) as ITableSource;
            if (rr != rc) node.Right = rr;

            var cc = node.Condition;
            var cr = this.Visit(cc) as IConstraint;
            if (cr != cc) node.Condition = cr;
            return node;
        }

        protected virtual IQueryNode VisitBinaryConstraint(IBinaryConstraint node)
        {
            var c = node.Left;
            var r = this.Visit(c) as IConstraint;
            if (r != c) node.Left = r;

            c = node.Right;
            r = this.Visit(c) as IConstraint;
            if (r != c) node.Right = r;

            return node;
        }

        protected virtual IQueryNode VisitQuery(IQuery node)
        {
            if (node.Selection != null)
            {
                var s = node.Selection;
                var sr = this.Visit(s);
                if (sr != s) node.Selection = sr;
            }

            var fc = node.From;
            var fr = this.Visit(fc) as ISource;
            if (fr != fc) node.From = fr;

            if (node.Where != null)
            {
                var wc = node.Where;
                var wr = this.Visit(wc) as IConstraint;
                if (wr != wc) node.Where = wr;
            }

            if (TableQuery.HasOrdered(node))
            {
                var orderBy = node.OrderBy;
                for (int i = 0, c = orderBy.Count; i < c; i++)
                {
                    var item = orderBy[i];
                    var or = this.Visit(item) as IOrderBy;
                    if (or != item) orderBy[i] = or;
                }
            }

            return node;
        }

        protected virtual IQueryNode VisitTableSource(ITableSource node)
        {
            return node;
        }

        protected virtual IQueryNode VisitLiteral(ILiteral node)
        {
            return node;
        }

        protected virtual IQueryNode VisitArray(IArray node)
        {
            var items = node.Items;
            for (int i = 0, c = items.Count; i < c; i++)
            {
                var item = items[i];
                var newItem = this.Visit(item);
                if (newItem != item) items[i] = newItem;
            }
            return node;
        }

        protected virtual IQueryNode VisitSelectAll(ISelectAll node)
        {
            return node;
        }

        protected virtual IQueryNode VisitColumn(IColumnNode node)
        {
            return node;
        }

        protected virtual IQueryNode VisitColumnConstraint(IColumnConstraint node)
        {
            var c = node.Column;
            var r = this.Visit(c) as IColumnNode;
            if (r != c) node.Column = r;
            return node;
        }

        protected virtual IQueryNode VisitTwoColumnsConstraint(IColumnsComparison node)
        {
            var c = node.LeftColumn;
            var r = this.Visit(c) as IColumnNode;
            if (r != c) node.LeftColumn = r;

            c = node.RightColumn;
            r = this.Visit(c) as IColumnNode;
            if (r != c) node.RightColumn = r;

            return node;
        }

        protected virtual IQueryNode VisitExistsConstraint(IExistsConstraint node)
        {
            var c = node.Query;
            var r = this.Visit(c) as IQuery;
            if (r != c) node.Query = r;

            return node;
        }

        protected virtual IQueryNode VisitNotConstraint(INotConstraint node)
        {
            var c = node.Constraint;
            var r = this.Visit(c) as IConstraint;
            if (r != c) node.Constraint = r;

            return node;
        }

        protected virtual IQueryNode VisitSubQueryRef(ISubQuery node)
        {
            var c = node.Query;
            var r = this.Visit(c) as IQuery;
            if (r != c) node.Query = r;

            return node;
        }

        protected virtual IQueryNode VisitOrderBy(IOrderBy node)
        {
            var c = node.Column;
            var r = this.Visit(c) as IColumnNode;
            if (r != c) node.Column = r;
            return node;
        }
    }
}