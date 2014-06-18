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
using Rafy.ManagedProperty;
using System;
using Rafy.Domain;
using Rafy.MetaModel;
using Rafy;
using System.Linq;

namespace Rafy.Domain.ORM
{
    /// <summary>
    /// 使用托管属性进行查询的条件封装。
    /// </summary>
    public interface IPropertyQuery : IDirectlyConstrain
    {
        /// <summary>
        /// 是否还没有任何语句
        /// </summary>
        bool IsEmpty { get; }

        /// <summary>
        /// 是否已经有 Inner Join 条件语句
        /// </summary>
        bool HasInnerJoin { get; }

        /// <summary>
        /// 返回 !Where.IsEmpty。
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
        /// 当前的查询是一个分页查询，并使用这个对象来描述分页的信息。
        /// </summary>
        PagingInfo PagingInfo { get; }

        ///// <summary>
        ///// 用于查询的 Where 条件。
        ///// </summary>
        //IConstraintGroup Where { get; set; }

        ///// <summary>
        ///// 约束条件工厂。
        ///// </summary>
        //IConstraintFactory ConstraintFactory { get; }

        /// <summary>
        /// 对引用属性指定的表使用关联查询
        /// 
        /// 调用此语句会生成相应的 INNER JOIN 语句，并把所有关联的数据在 SELECT 中加上。
        /// 
        /// 注意！！！
        /// 目前不支持同时 Join 两个不同的引用属性，它们都引用同一个实体/表。
        /// </summary>
        /// <param name="property"></param>
        /// <param name="propertyOwner">
        /// 显式指定该引用属性对应的拥有类型。
        /// 一般使用在以下情况中：当引用属性定义在基类中，而当前正在对子类进行查询时。
        /// </param>
        /// <returns></returns>
        IPropertyQuery JoinRef(IRefProperty property, Type propertyOwner = null);

        /// <summary>
        /// 按照某个属性排序。
        /// 
        /// 可以调用此方法多次来指定排序的优先级。
        /// </summary>
        /// <param name="property">按照此属性排序</param>
        /// <param name="direction">排序方向。</param>
        /// <returns></returns>
        IPropertyQuery OrderBy(IManagedProperty property, OrderDirection direction);

        /// <summary>
        /// 声明最终的查询应该只显示指定的页码。
        /// 
        /// 如果 pagingInfo 中需要总行数时，还会把总行数存储到其中。
        /// </summary>
        /// <param name="pagingInfo">该参数支持传入 null 或者是 <see cref="Rafy.PagingInfo.Empty"/>，表示不需要分页查询。</param>
        void Paging(PagingInfo pagingInfo);

        /// <summary>
        /// 贪婪加载某个属性
        /// </summary>
        /// <param name="property">需要贪婪加载的托管属性。可以是一个引用属性，也可以是一个组合子属性。</param>
        /// <param name="propertyOwner">这个属性的拥有者类型。</param>
        void EagerLoad(IProperty property, Type propertyOwner = null);

        /// <summary>
        /// 可以使用一个 Linq 语句来对 PropertyQuery 进行配置。
        /// </summary>
        /// <param name="queryable"></param>
        /// <returns></returns>
        IPropertyQuery CombineLinq(IQueryable queryable);
    }

    /// <summary>
    /// 直接进行查询的属性对比接口。
    /// 主要是兼容旧的代码。
    /// </summary>
    public interface IDirectlyConstrain
    {
        /// <summary>
        /// “并且”，两个不同的 Constrain 之间的连接符。
        /// </summary>
        /// <returns></returns>
        IPropertyQuery And();

        /// <summary>
        /// “或者”，两个不同的 Constrain 之间的连接符。
        /// </summary>
        /// <returns></returns>
        IPropertyQuery Or();

        /// <summary>
        /// 查询某个属性
        /// </summary>
        /// <param name="property">指定的托管属性</param>
        /// <param name="propertyOwner">如果托管属性是声明在基类中，则需要指定托管属性对应的具体类型</param>
        /// <returns></returns>
        IPropertyQueryValue Constrain(IManagedProperty property, Type propertyOwner = null);

        /// <summary>
        /// 直接使用 SQL 作为约束条件
        /// </summary>
        /// <param name="formattedSql">sql</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        IPropertyQuery ConstrainSql(string formattedSql, params object[] parameters);
    }

    /// <summary>
    /// 某个查询属性可用的值判断方法
    /// </summary>
    public interface IPropertyQueryValue
    {
        IPropertyQuery Equal(object val);
        IPropertyQuery NotEqual(object val);
        IPropertyQuery Greater(object val);
        IPropertyQuery GreaterEqual(object val);
        IPropertyQuery Less(object val);
        IPropertyQuery LessEqual(object val);

        IPropertyQuery Equal(IManagedProperty property, Type propertyOwner = null);
        IPropertyQuery NotEqual(IManagedProperty property, Type propertyOwner = null);
        IPropertyQuery Greater(IManagedProperty property, Type propertyOwner = null);
        IPropertyQuery GreaterEqual(IManagedProperty property, Type propertyOwner = null);
        IPropertyQuery Less(IManagedProperty property, Type propertyOwner = null);
        IPropertyQuery LessEqual(IManagedProperty property, Type propertyOwner = null);

        IPropertyQuery Like(string val);
        IPropertyQuery Contains(string val);
        IPropertyQuery StartWith(string val);
        IPropertyQuery EndWith(string val);

        IPropertyQuery In(IEnumerable values);
        IPropertyQuery NotIn(IEnumerable values);
    }

    /// <summary>
    /// JoinRef 方法的类型。
    /// </summary>
    internal enum JoinRefType
    {
        /// <summary>
        /// 只为了使用关系查询
        /// </summary>
        JoinOnly,
        /// <summary>
        /// 可以使用关联查询，并同时查询出相关的实体。
        /// </summary>
        QueryData
    }
}