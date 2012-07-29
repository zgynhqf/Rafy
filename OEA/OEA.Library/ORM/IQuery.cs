/*******************************************************
 * 
 * 作者：LiteORM
 * 创建时间：2009
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.2.0
 * 
 * 历史记录：
 * 创建文件 LiteORM 2009
 * 1.2 添加关联查询接口。 胡庆访 20120605 18:43
 * 
*******************************************************/

using System.Collections;
using OEA.ManagedProperty;
using System;
using OEA.Library;

namespace OEA.ORM
{
    public interface IQuery
    {
        /// <summary>
        /// 当前正在查询的实体类型
        /// </summary>
        Type EntityType { get; }

        /// <summary>
        /// 是否还没有任何语句
        /// </summary>
        bool IsEmpty { get; }

        /// <summary>
        /// 是否已经有 Inner Join 条件语句
        /// </summary>
        bool HasInnerJoin { get; }

        /// <summary>
        /// 是否已经有 Where 条件语句
        /// </summary>
        bool HasWhere { get; }

        /// <summary>
        /// 是否已经有了 OrderBy 语句
        /// </summary>
        bool HasOrdered { get; }

        /// <summary>
        /// 条件是否已经可以使用。
        /// 
        /// 当使用了 And、Or 并且还没有添加其它条件时，此属性会返回 false。
        /// </summary>
        bool IsCompleted { get; }

        /// <summary>
        /// “并且”，两个不同的 Constrain 之间的连接符。
        /// </summary>
        /// <returns></returns>
        IQuery And();

        /// <summary>
        /// “或者”，两个不同的 Constrain 之间的连接符。
        /// </summary>
        /// <returns></returns>
        IQuery Or();

        /// <summary>
        /// 查询某个属性
        /// </summary>
        /// <param name="property">指定的托管属性</param>
        /// <returns></returns>
        IQueryValue Constrain(IManagedProperty property);

        /// <summary>
        /// 直接使用 SQL 作为约束条件
        /// </summary>
        /// <param name="formatSql">sql</param>
        /// <returns></returns>
        IQuery ConstrainSql(string formatSql, params object[] parameters);

        /// <summary>
        /// 对引用属性指定的表使用关联查询
        /// 
        /// 调用此语句会生成相应的 INNER JOIN 语句，并把所有关联的数据在 SELECT 中加上。
        /// 
        /// 注意！！！
        /// 目前不支持同时 Join 两个不同的引用属性，它们都引用同一个实体/表。
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        IQuery JoinRef(IRefProperty property);

        /// <summary>
        /// 对引用属性指定的表使用关联查询
        /// 
        /// 调用此语句会生成相应的 INNER JOIN 语句，并把所有关联的数据在 SELECT 中加上。
        /// 
        /// 注意！！！
        /// 目前不支持同时 Join 两个不同的引用属性，它们都引用同一个实体/表。
        /// </summary>
        /// <param name="property"></param>
        /// <param name="propertyOwnerType">
        /// 显式指定该引用属性对应的拥有类型。
        /// 一般使用在以下情况中：当引用属性定义在基类中，而当前正在对子类进行查询时。
        /// </param>
        /// <returns></returns>
        IQuery JoinRef(IRefProperty property, Type propertyOwnerType);

        /// <summary>
        /// 按照某个属性排序。
        /// </summary>
        /// <param name="property">按照此属性排序</param>
        /// <param name="ascending">正序还是反序</param>
        /// <returns></returns>
        IQuery Order(IManagedProperty property, bool ascending);
    }

    /// <summary>
    /// 某个查询属性可用的值判断方法
    /// </summary>
    public interface IQueryValue
    {
        IQuery Equal(object val);
        IQuery NotEqual(object val);
        IQuery Greater(object val);
        IQuery GreaterEqual(object val);
        IQuery Less(object val);
        IQuery LessEqual(object val);
        IQuery Like(string val);
        IQuery Contains(string val);
        IQuery StartWith(string val);

        IQuery In(IList values);
        IQuery NotIn(IList values);
    }

    public static class QueryExtension
    {
        /// <summary>
        /// 如果提供的值是不可空的，则为查询条件添加相等的查询判断。
        /// </summary>
        /// <param name="query"></param>
        /// <param name="property">查询某个属性。</param>
        /// <param name="value">当 value 不可空时，才添加查询判断。</param>
        /// <returns></returns>
        public static IQuery AddConstrainEqualIf(this IQuery query, IManagedProperty property, object value)
        {
            if (SqlWriter.IsNotNull(value)) { query.AddConstrain(property).Equal(value); }
            return query;
        }

        public static IQuery AddConstrainGreaterEqualIf(this IQuery query, IManagedProperty property, object value)
        {
            if (SqlWriter.IsNotNull(value)) { query.AddConstrain(property).GreaterEqual(value); }
            return query;
        }

        public static IQuery AddConstrainLessEqualIf(this IQuery query, IManagedProperty property, object value)
        {
            if (SqlWriter.IsNotNull(value)) { query.AddConstrain(property).LessEqual(value); }
            return query;
        }

        public static IQuery AddConstrainContainsIf(this IQuery query, IManagedProperty property, string value)
        {
            if (SqlWriter.IsNotNull(value)) { query.AddConstrain(property).Contains(value); }
            return query;
        }

        /// <summary>
        /// 直接添加某个查询条件。
        /// 
        /// 如果没有相应的 And 连接符，则自动添加。
        /// </summary>
        /// <param name="query"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public static IQueryValue AddConstrain(this IQuery query, IManagedProperty property)
        {
            if (query.HasWhere && query.IsCompleted) query.And();
            return query.Constrain(property);
        }
    }
}