/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20121115 20:45
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20121115 20:45
 * 
*******************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.Domain.ORM.Oracle;
using Rafy.ManagedProperty;

namespace Rafy.Domain.ORM.Query
{
    /// <summary>
    /// 链式查询条件拼装接口。
    /// 
    /// 简单封装了 QueryFactory 类型。
    /// </summary>
    public static partial class QueryExtensions
    {

        /// <summary>
        /// 如果提供的值是不可空的，则为查询添加一个对应的约束条件，并以 And 与原条件进行连接。
        /// </summary>
        /// <param name="query">查询.</param>
        /// <param name="property">要约束的属性.</param>
        /// <param name="value">当 value 不可空时，才添加这个对比约束条件。</param>
        /// <returns></returns>
        public static IQuery AddConstraintIf(this IQuery query, IManagedProperty property, object value)
        {
            return AddConstraintIf(query, property, PropertyOperator.Equal, value);
        }

        /// <summary>
        /// 如果提供的值是不可空的，则为查询添加一个对应的约束条件，并以 And 与原条件进行连接。
        /// </summary>
        /// <param name="query">查询.</param>
        /// <param name="property">要约束的属性.</param>
        /// <param name="op">约束条件操作符.</param>
        /// <param name="value">当 value 不可空时，才添加这个对比约束条件。</param>
        /// <returns></returns>
        public static IQuery AddConstraintIf(this IQuery query, IManagedProperty property, PropertyOperator op, object value)
        {
            if (DomainHelper.IsNotEmpty(value))
            {
                return AddConstraint(query, property, op, value);
            }
            return query;
        }

        /// <summary>
        /// 如果提供的值是不可空的，则为查询添加一个对应的约束条件，并以 And 与原条件进行连接。
        /// </summary>
        /// <param name="query">查询.</param>
        /// <param name="property">要约束的属性.</param>
        /// <param name="op">约束条件操作符.</param>
        /// <param name="value">当 value 不可空时，才添加这个对比约束条件。</param>
        /// <param name="propertySource">指定该属性所属的实体数据源。</param>
        /// <returns></returns>
        public static IQuery AddConstraintIf(this IQuery query, IManagedProperty property, PropertyOperator op, object value, ITableSource propertySource)
        {
            if (DomainHelper.IsNotEmpty(value))
            {
                return AddConstraint(query, property, op, value, propertySource);
            }

            return query;
        }

        /// <summary>
        /// 为查询添加一个对应的约束条件，并以 And 与原条件进行连接。
        /// </summary>
        /// <param name="query">查询.</param>
        /// <param name="property">要约束的属性.</param>
        /// <param name="value">对比的值。</param>
        /// <returns></returns>
        public static IQuery AddConstraint(this IQuery query, IManagedProperty property, object value)
        {
            return AddConstraint(query, property, PropertyOperator.Equal, value);
        }

        /// <summary>
        /// 为查询添加一个对应的约束条件，并以 And 与原条件进行连接。
        /// </summary>
        /// <param name="query">查询.</param>
        /// <param name="property">要约束的属性.</param>
        /// <param name="op">约束条件操作符.</param>
        /// <param name="value">对比的值。</param>
        /// <returns></returns>
        public static IQuery AddConstraint(this IQuery query, IManagedProperty property, PropertyOperator op, object value)
        {
            var source = query.MainTable;
            if (!property.OwnerType.IsAssignableFrom(source.EntityRepository.EntityType))
            {
                source = query.From.FindTable(property.OwnerType);
            }
            return AddConstraint(query, property, op, value, source);
        }

        /// <summary>
        /// 为查询添加一个对应的约束条件，并以 And 与原条件进行连接。
        /// </summary>
        /// <param name="query">查询.</param>
        /// <param name="property">要约束的属性.</param>
        /// <param name="op">约束条件操作符.</param>
        /// <param name="value">对比的值。</param>
        /// <param name="propertySource">指定该属性所属的实体数据源。</param>
        /// <returns></returns>
        public static IQuery AddConstraint(this IQuery query, IManagedProperty property, PropertyOperator op, object value, ITableSource propertySource)
        {
            var f = QueryFactory.Instance;

            var propertyNode = propertySource.Column(property);

            var where = f.Constraint(propertyNode, op, value);
            if (query.Where == null)
            {
                query.Where = where;
            }
            else
            {
                query.Where = f.And(query.Where, where);
            }

            return query;
        }

        public static ISelectAll Star(this ITableSource table)
        {
            return QueryFactory.Instance.SelectAll(table);
        }

        public static IJoin Join(this ITableSource left, ITableSource right)
        {
            return QueryFactory.Instance.Join(left, right);
        }
        public static IJoin Join(this ISource left, ITableSource right, IRefProperty leftToRight)
        {
            return QueryFactory.Instance.Join(left, right, leftToRight);
        }
        public static IJoin Join(this ISource left, ITableSource right, IConstraint condition, JoinType joinType = JoinType.Inner)
        {
            return QueryFactory.Instance.Join(left, right, condition, joinType);
        }

        public static IConstraint Equal(this IColumnNode column, object value)
        {
            return QueryFactory.Instance.Constraint(column, PropertyOperator.Equal, value);
        }
        public static IConstraint NotEqual(this IColumnNode column, object value)
        {
            return QueryFactory.Instance.Constraint(column, PropertyOperator.NotEqual, value);
        }
        public static IConstraint Greater(this IColumnNode column, object value)
        {
            return QueryFactory.Instance.Constraint(column, PropertyOperator.Greater, value);
        }
        public static IConstraint GreaterEqual(this IColumnNode column, object value)
        {
            return QueryFactory.Instance.Constraint(column, PropertyOperator.GreaterEqual, value);
        }
        public static IConstraint Less(this IColumnNode column, object value)
        {
            return QueryFactory.Instance.Constraint(column, PropertyOperator.Less, value);
        }
        public static IConstraint LessEqual(this IColumnNode column, object value)
        {
            return QueryFactory.Instance.Constraint(column, PropertyOperator.LessEqual, value);
        }
        public static IConstraint Like(this IColumnNode column, object value)
        {
            return QueryFactory.Instance.Constraint(column, PropertyOperator.Like, value);
        }
        public static IConstraint NotLike(this IColumnNode column, object value)
        {
            return QueryFactory.Instance.Constraint(column, PropertyOperator.NotLike, value);
        }
        public static IConstraint Contains(this IColumnNode column, object value)
        {
            return QueryFactory.Instance.Constraint(column, PropertyOperator.Contains, value);
        }
        public static IConstraint NotContains(this IColumnNode column, object value)
        {
            return QueryFactory.Instance.Constraint(column, PropertyOperator.NotContains, value);
        }
        public static IConstraint StartWith(this IColumnNode column, object value)
        {
            return QueryFactory.Instance.Constraint(column, PropertyOperator.StartsWith, value);
        }
        public static IConstraint NotStartWith(this IColumnNode column, object value)
        {
            return QueryFactory.Instance.Constraint(column, PropertyOperator.NotStartsWith, value);
        }
        public static IConstraint EndWith(this IColumnNode column, object value)
        {
            return QueryFactory.Instance.Constraint(column, PropertyOperator.EndsWith, value);
        }
        public static IConstraint NotEndWith(this IColumnNode column, object value)
        {
            return QueryFactory.Instance.Constraint(column, PropertyOperator.NotEndsWith, value);
        }
        public static IConstraint In(this IColumnNode column, object value)
        {
            var parameters = value as IList;

            IConstraint res = null;
            var maxItemsCount = SqlGenerator.CampatibleMaxItemsInInClause;
            //处理大数目In 条件
            if (parameters != null && parameters.Count > maxItemsCount)
            {
                var start = 0;
                while (start < parameters.Count)
                {
                    var paramSection = new List<object>(maxItemsCount);

                    var end = Math.Min(start + maxItemsCount - 1, parameters.Count - 1);
                    for (int i = start; i <= end; i++)
                    {
                        paramSection.Add(parameters[i]);
                    }

                    var sectionConstraint = QueryFactory.Instance.Constraint(column, PropertyOperator.In, paramSection);

                    res = QueryFactory.Instance.Or(res, sectionConstraint);

                    start += paramSection.Count;
                }
            }
            else
            {
                res = QueryFactory.Instance.Constraint(column, PropertyOperator.In, value);
            }

            return res;
        }
        public static IConstraint NotIn(this IColumnNode column, object value)
        {
            var parameters = value as IList;

            IConstraint res = null;
            //处理大数目NotIn 条件
            var maxItemsCount = SqlGenerator.CampatibleMaxItemsInInClause;
            if (parameters != null && parameters.Count > maxItemsCount)
            {
                var start = 0;
                while (start < parameters.Count)
                {
                    var paramSection = new List<object>(maxItemsCount);

                    var end = Math.Min(start + maxItemsCount - 1, parameters.Count - 1);
                    for (int i = start; i <= end; i++)
                    {
                        paramSection.Add(parameters[i]);
                    }

                    var sectionConstraint = QueryFactory.Instance.Constraint(column, PropertyOperator.NotIn, paramSection);

                    res = QueryFactory.Instance.And(res, sectionConstraint);

                    start += paramSection.Count;
                }
            }
            else
            {
                res = QueryFactory.Instance.Constraint(column, PropertyOperator.NotIn, value);
            }

            return res;
        }

        public static IConstraint Equal(this IColumnNode column, IColumnNode rightColumn)
        {
            return QueryFactory.Instance.Constraint(column, PropertyOperator.Equal, rightColumn);
        }
        public static IConstraint NotEqual(this IColumnNode column, IColumnNode rightColumn)
        {
            return QueryFactory.Instance.Constraint(column, PropertyOperator.NotEqual, rightColumn);
        }
        public static IConstraint Greater(this IColumnNode column, IColumnNode rightColumn)
        {
            return QueryFactory.Instance.Constraint(column, PropertyOperator.Greater, rightColumn);
        }
        public static IConstraint GreaterEqual(this IColumnNode column, IColumnNode rightColumn)
        {
            return QueryFactory.Instance.Constraint(column, PropertyOperator.GreaterEqual, rightColumn);
        }
        public static IConstraint Less(this IColumnNode column, IColumnNode rightColumn)
        {
            return QueryFactory.Instance.Constraint(column, PropertyOperator.Less, rightColumn);
        }
        public static IConstraint LessEqual(this IColumnNode column, IColumnNode rightColumn)
        {
            return QueryFactory.Instance.Constraint(column, PropertyOperator.LessEqual, rightColumn);
        }
        public static IConstraint Like(this IColumnNode column, IColumnNode rightColumn)
        {
            return QueryFactory.Instance.Constraint(column, PropertyOperator.Like, rightColumn);
        }
        public static IConstraint NotLike(this IColumnNode column, IColumnNode rightColumn)
        {
            return QueryFactory.Instance.Constraint(column, PropertyOperator.NotLike, rightColumn);
        }
        public static IConstraint Contains(this IColumnNode column, IColumnNode rightColumn)
        {
            return QueryFactory.Instance.Constraint(column, PropertyOperator.Contains, rightColumn);
        }
        public static IConstraint NotContains(this IColumnNode column, IColumnNode rightColumn)
        {
            return QueryFactory.Instance.Constraint(column, PropertyOperator.NotContains, rightColumn);
        }
        public static IConstraint StartWith(this IColumnNode column, IColumnNode rightColumn)
        {
            return QueryFactory.Instance.Constraint(column, PropertyOperator.StartsWith, rightColumn);
        }
        public static IConstraint NotStartWith(this IColumnNode column, IColumnNode rightColumn)
        {
            return QueryFactory.Instance.Constraint(column, PropertyOperator.NotStartsWith, rightColumn);
        }
        public static IConstraint EndWith(this IColumnNode column, IColumnNode rightColumn)
        {
            return QueryFactory.Instance.Constraint(column, PropertyOperator.EndsWith, rightColumn);
        }
        public static IConstraint NotEndWith(this IColumnNode column, IColumnNode rightColumn)
        {
            return QueryFactory.Instance.Constraint(column, PropertyOperator.NotEndsWith, rightColumn);
        }
        public static IConstraint In(this IColumnNode column, IColumnNode rightColumn)
        {
            return QueryFactory.Instance.Constraint(column, PropertyOperator.In, rightColumn);
        }
        public static IConstraint NotIn(this IColumnNode column, IColumnNode rightColumn)
        {
            return QueryFactory.Instance.Constraint(column, PropertyOperator.NotIn, rightColumn);
        }
    }
}