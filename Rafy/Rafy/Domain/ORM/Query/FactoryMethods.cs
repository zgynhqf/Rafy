/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20211214
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20211214 23:30
 * 
*******************************************************/

using Rafy.ManagedProperty;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rafy.Domain.ORM.Query
{
    /// <summary>
    /// 本类是 QueryFactory 的门面 API。
    /// 使用时，请在文件中使用 using static <see cref="FactoryMethods"/> 来进行引用。
    /// </summary>
    public static class FactoryMethods
    {
        private static QueryFactory f  => QueryFactory.Instance;

        /// <summary>
        /// 为指定的仓库构造一个查询。
        /// </summary>
        /// <param name="mainTableRepository">主表对应的实体的仓库。</param>
        /// <returns></returns>
        public static IQuery Query(IRepository mainTableRepository)
        {
            return f.Query(mainTableRepository);
        }

        /// <summary>
        /// 为指定的仓库构造一个查询。
        /// </summary>
        /// <typeparam name="TEntity">主表对应的实体。</typeparam>
        /// <returns></returns>
        public static IQuery Query<TEntity>()
            where TEntity : Entity
        {
            return f.Query<TEntity>();
        }

        /// <summary>
        /// 构造一个查询对象。
        /// </summary>
        /// <param name="from">要查询的数据源。</param>
        /// <param name="selection">
        /// 要查询的内容。
        /// 如果本属性为空，表示要查询所有数据源的所有属性。
        /// </param>
        /// <param name="where">查询的过滤条件。</param>
        /// <param name="orderBy">
        /// 查询的排序规则。
        /// 可以指定多个排序条件。
        /// </param>
        /// <param name="isCounting">
        /// 是否只查询数据的条数。
        /// 
        /// 如果这个属性为真，那么不再需要使用 Selection。
        /// </param>
        /// <param name="isDistinct">是否需要查询不同的结果。</param>
        /// <returns></returns>
        public static IQuery Query(
            ISource from,
            IQueryNode selection = null,
            IConstraint where = null,
            List<IOrderBy> orderBy = null,
            bool isCounting = false,
            bool isDistinct = false
            )
        {
            return f.Query(from, selection, where, orderBy, isCounting, isDistinct);
        }

        /// <summary>
        /// 构造一个数组节点。
        /// </summary>
        /// <param name="nodes">所有数组中的项。</param>
        /// <returns></returns>
        public static IArray Array(params IQueryNode[] nodes)
        {
            return f.Array(nodes);
        }

        /// <summary>
        /// 构造一个数组节点。
        /// </summary>
        /// <param name="nodes">所有数组中的项。</param>
        /// <returns></returns>
        public static IArray Array(IEnumerable<IQueryNode> nodes)
        {
            return f.Array(nodes);
        }

        /// <summary>
        /// 构造一个 SelectAll 节点。
        /// </summary>
        /// <param name="table">如果本属性为空，表示选择所有数据源的所有属性；否则表示选择指定数据源的所有属性。</param>
        /// <returns></returns>
        public static ISelectAll SelectAll(ITableSource table = null)
        {
            return f.SelectAll(table);
        }

        /// <summary>
        /// 构造一个实体的数据源节点。
        /// </summary>
        /// <param name="repository">本实体数据源来自于这个实体仓库。</param>
        /// <param name="alias">同一个实体仓库可以表示多个不同的数据源。这时，需要这些不同的数据源指定不同的别名。</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">entityRepository</exception>
        public static ITableSource Table(IRepository repository, string alias = null)
        {
            return f.Table(repository, alias);
        }

        /// <summary>
        /// 构造一个实体的数据源节点。
        /// </summary>
        /// <typeparam name="TEntity">本实体数据源来自于这个实体对应的仓库。</typeparam>
        /// <param name="alias">同一个实体仓库可以表示多个不同的数据源。这时，需要这些不同的数据源指定不同的别名。</param>
        /// <returns></returns>
        public static ITableSource Table<TEntity>(string alias = null)
            where TEntity : Entity
        {
            return f.Table<TEntity>(alias);
        }

        /// <summary>
        /// 构造一个属性与指定值"相等"的约束条件节点。
        /// </summary>
        /// <param name="column">要对比的属性。</param>
        /// <param name="value">要对比的值。</param>
        /// <returns></returns>
        public static IConstraint Constraint(
            IColumnNode column,
            object value
            )
        {
            return f.Constraint(column, value);
        }

        /// <summary>
        /// 构造一个属性的约束条件节点。
        /// </summary>
        /// <param name="column">要对比的属性。</param>
        /// <param name="op">对比操作符</param>
        /// <param name="value">要对比的值。</param>
        /// <returns></returns>
        public static IConstraint Constraint(
            IColumnNode column,
            PropertyOperator op,
            object value
            )
        {
            return f.Constraint(column, op, value);
        }

        /// <summary>
        /// 构造一个两个属性"相等"的约束条件节点。
        /// </summary>
        /// <param name="leftColumn">第一个需要对比的列。</param>
        /// <param name="rightColumn">第二个需要对比的列。</param>
        /// <returns></returns>
        public static IConstraint Constraint(
            IColumnNode leftColumn,
            IColumnNode rightColumn
            )
        {
            return f.Constraint(leftColumn, rightColumn);
        }

        /// <summary>
        /// 构造一个两个属性进行对比的约束条件节点。
        /// </summary>
        /// <param name="leftColumn">第一个需要对比的列。</param>
        /// <param name="op">对比条件。</param>
        /// <param name="rightColumn">第二个需要对比的列。</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">
        /// leftProperty
        /// or
        /// rightProperty
        /// </exception>
        public static IConstraint Constraint(
            IColumnNode leftColumn,
            PropertyOperator op,
            IColumnNode rightColumn
            )
        {
            return f.Constraint(leftColumn, op, rightColumn);
        }

        /// <summary>
        /// 构造一个 是否存在查询结果的约束条件节点
        /// </summary>
        /// <param name="query">要检查的查询。</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">query</exception>
        public static IConstraint Exists(IQuery query)
        {
            return f.Exists(query);
        }

        /// <summary>
        /// 构造一个对指定约束条件节点执行取反规则的约束条件节点。
        /// </summary>
        /// <param name="constraint">需要被取反的条件。</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">constraint</exception>
        public static IConstraint Not(IConstraint constraint)
        {
            return f.Not(constraint);
        }

        /// <summary>
        /// 构造一个查询文本。
        /// </summary>
        /// <param name="formattedSql">查询文本。</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">text</exception>
        public static ILiteral Literal(string formattedSql, params object[] parameters)
        {
            return f.Literal(formattedSql, parameters);
        }

        /// <summary>
        /// 对指定的所有约束构造一个 And 连接串节点。
        /// </summary>
        /// <param name="constraints">需要使用 And 进行连接的所有约束。</param>
        /// <returns></returns>
        public static IConstraint And(params IConstraint[] constraints)
        {
            return f.And(constraints);
        }

        /// <summary>
        /// 构造一个 And 连接的节点。
        /// </summary>
        /// <param name="left">二位运算的左操作结点。</param>
        /// <param name="right">二位运算的右操作节点。</param>
        public static IConstraint And(IConstraint left, IConstraint right)
        {
            return f.And(left, right);
        }

        /// <summary>
        /// 对指定的所有约束构造一个 Or 连接串节点。
        /// </summary>
        /// <param name="constraints">需要使用 Or 进行连接的所有约束。</param>
        /// <returns></returns>
        public static IConstraint Or(params IConstraint[] constraints)
        {
            return f.Or(constraints);
        }

        /// <summary>
        /// 构造一个 Or 连接的节点。
        /// </summary>
        /// <param name="left">二位运算的左操作结点。</param>
        /// <param name="right">二位运算的右操作节点。</param>
        public static IConstraint Or(IConstraint left, IConstraint right)
        {
            return f.Or(left, right);
        }

        /// <summary>
        /// 构造一个二位操作符连接的节点。
        /// </summary>
        /// <param name="left">二位运算的左操作结点。</param>
        /// <param name="op">二位运算类型。</param>
        /// <param name="right">二位运算的右操作节点。</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">
        /// left
        /// or
        /// right
        /// </exception>
        public static IConstraint Binary(
            IConstraint left,
            BinaryOperator op,
            IConstraint right
            )
        {
            return f.Binary(left, op, right);
        }

        /// <summary>
        /// 通过左实体数据源和右实体数据源，来找到它们之间的第一个的引用属性，用以构造一个连接。
        /// </summary>
        /// <param name="left">拥有引用关系的左数据源。</param>
        /// <param name="right">右实体数据源。</param>
        /// <returns></returns>
        public static IJoin Join(
            ITableSource left,
            ITableSource right
            )
        {
            return f.Join(left, right);
        }

        /// <summary>
        /// 通过左数据源和右实体数据源，以及从左到右的引用属性，来构造一个连接。
        /// </summary>
        /// <param name="left">左实体数据源。</param>
        /// <param name="right">右实体数据源。</param>
        /// <param name="leftToRight">从左到右的引用属性。</param>
        /// <returns></returns>
        public static IJoin Join(
            ITableSource left,
            ITableSource right,
            IRefProperty leftToRight
            )
        {
            return f.Join(left, right, leftToRight);
        }

        /// <summary>
        /// 通过左数据源和右实体数据源，以及从左到右的引用属性，来构造一个连接。
        /// </summary>
        /// <param name="left">左数据源。</param>
        /// <param name="right">右实体数据源。</param>
        /// <param name="leftToRight">从左到右的引用属性。</param>
        /// <returns></returns>
        public static IJoin Join(
            ISource left,
            ITableSource right,
            IRefProperty leftToRight
            )
        {
            return f.Join(left, right, leftToRight);
        }

        /// <summary>
        /// 构造一个数据源与实体数据源连接后的结果节点
        /// </summary>
        /// <param name="left">左边需要连接的数据源。</param>
        /// <param name="right">右边需要连接的数据源。</param>
        /// <param name="condition">连接所使用的约束条件。</param>
        /// <param name="joinType">连接方式</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">
        /// left
        /// or
        /// right
        /// or
        /// condition
        /// </exception>
        public static IJoin Join(
            ISource left,
            ITableSource right,
            IConstraint condition,
            JoinType joinType = JoinType.Inner
            )
        {
            return f.Join(left, right, condition, joinType);
        }

        /// <summary>
        /// 构造一个子查询。
        /// </summary>
        /// <param name="query">内部的查询对象。</param>
        /// <param name="alias">必须对这个子查询指定别名。</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">
        /// query
        /// or
        /// alias
        /// </exception>
        public static ISubQuery SubQuery(IQuery query, string alias)
        {
            return f.SubQuery(query, alias);
        }

        /// <summary>
        /// 构造一个排序节点。
        /// </summary>
        /// <param name="property">使用这个属性进行排序。</param>
        /// <param name="direction">使用这个方向进行排序。</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">property</exception>
        public static IOrderBy OrderBy(IColumnNode property, OrderDirection direction = OrderDirection.Ascending)
        {
            return f.OrderBy(property, direction);
        }
    }
}