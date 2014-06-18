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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.Domain.ORM.Query;
using Rafy.ManagedProperty;

namespace Rafy.Domain.ORM
{
    /// <summary>
    /// IQuery 接口的扩展方法集。
    /// </summary>
    public static class QueryExtension
    {
        /// <summary>
        /// 直接添加某个查询条件。
        /// 如果没有相应的 And 连接符，则自动添加。
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="property">The property.</param>
        /// <param name="propertyOwner">The property owner.</param>
        /// <returns></returns>
        public static IPropertyQueryValue AddConstrain(this IPropertyQuery query, IManagedProperty property, Type propertyOwner = null)
        {
            if (query.HasWhere && query.IsCompleted) query.And();
            return query.Constrain(property, propertyOwner);
        }

        /// <summary>
        /// 直接添加某个 Sql 查询条件。
        /// 如果没有相应的 And 连接符，则自动添加。
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="formatSql">The format SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public static IPropertyQuery AddConstrainSql(this IPropertyQuery query, string formatSql, params object[] parameters)
        {
            if (query.HasWhere && query.IsCompleted) query.And();
            return query.ConstrainSql(formatSql, parameters);
        }

        /// <summary>
        /// 如果提供的值是不可空的，则为查询条件添加相等的查询判断。
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="property">查询某个属性。</param>
        /// <param name="value">当 value 不可空时，才添加查询判断。</param>
        /// <param name="propertyOwner">The property owner.</param>
        /// <returns></returns>
        public static IPropertyQuery AddConstrainEqualIf(this IPropertyQuery query, IManagedProperty property, object value, Type propertyOwner = null)
        {
            if (ConditionalSql.IsNotEmpty(value)) { query.AddConstrain(property, propertyOwner).Equal(value); }
            return query;
        }

        /// <summary>
        /// 如果提供的值是不可空的，则为查询条件添加不相等的查询判断。
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="property">查询某个属性。</param>
        /// <param name="value">当 value 不可空时，才添加查询判断。</param>
        /// <param name="propertyOwner">The property owner.</param>
        /// <returns></returns>
        public static IPropertyQuery AddConstrainNotEqualIf(this IPropertyQuery query, IManagedProperty property, object value, Type propertyOwner = null)
        {
            if (ConditionalSql.IsNotEmpty(value)) { query.AddConstrain(property, propertyOwner).NotEqual(value); }
            return query;
        }

        /// <summary>
        /// 如果提供的值是不可空的，则为查询条件添加大于的查询判断。
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="property">查询某个属性。</param>
        /// <param name="value">当 value 不可空时，才添加查询判断。</param>
        /// <param name="propertyOwner">The property owner.</param>
        /// <returns></returns>
        public static IPropertyQuery AddConstrainGreaterIf(this IPropertyQuery query, IManagedProperty property, object value, Type propertyOwner = null)
        {
            if (ConditionalSql.IsNotEmpty(value)) { query.AddConstrain(property, propertyOwner).Greater(value); }
            return query;
        }

        /// <summary>
        /// 如果提供的值是不可空的，则为查询条件添加大于等于的查询判断。
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="property">查询某个属性。</param>
        /// <param name="value">当 value 不可空时，才添加查询判断。</param>
        /// <param name="propertyOwner">The property owner.</param>
        /// <returns></returns>
        public static IPropertyQuery AddConstrainGreaterEqualIf(this IPropertyQuery query, IManagedProperty property, object value, Type propertyOwner = null)
        {
            if (ConditionalSql.IsNotEmpty(value)) { query.AddConstrain(property, propertyOwner).GreaterEqual(value); }
            return query;
        }

        /// <summary>
        /// 如果提供的值是不可空的，则为查询条件添加小于的查询判断。
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="property">查询某个属性。</param>
        /// <param name="value">当 value 不可空时，才添加查询判断。</param>
        /// <param name="propertyOwner">The property owner.</param>
        /// <returns></returns>
        public static IPropertyQuery AddConstrainLessIf(this IPropertyQuery query, IManagedProperty property, object value, Type propertyOwner = null)
        {
            if (ConditionalSql.IsNotEmpty(value)) { query.AddConstrain(property, propertyOwner).Less(value); }
            return query;
        }

        /// <summary>
        /// 如果提供的值是不可空的，则为查询条件添加小于等于的查询判断。
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="property">查询某个属性。</param>
        /// <param name="value">当 value 不可空时，才添加查询判断。</param>
        /// <param name="propertyOwner">The property owner.</param>
        /// <returns></returns>
        public static IPropertyQuery AddConstrainLessEqualIf(this IPropertyQuery query, IManagedProperty property, object value, Type propertyOwner = null)
        {
            if (ConditionalSql.IsNotEmpty(value)) { query.AddConstrain(property, propertyOwner).LessEqual(value); }
            return query;
        }

        /// <summary>
        /// 如果提供的值是不可空的，则为查询条件添加 Like 的查询判断。
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="property">查询某个属性。</param>
        /// <param name="value">当 value 不可空时，才添加查询判断。</param>
        /// <param name="propertyOwner">The property owner.</param>
        /// <returns></returns>
        public static IPropertyQuery AddConstrainLikeIf(this IPropertyQuery query, IManagedProperty property, string value, Type propertyOwner = null)
        {
            if (ConditionalSql.IsNotEmpty(value)) { query.AddConstrain(property, propertyOwner).Like(value); }
            return query;
        }

        /// <summary>
        /// 如果提供的值是不可空的，则为查询条件添加包含的查询判断。
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="property">查询某个属性。</param>
        /// <param name="value">当 value 不可空时，才添加查询判断。</param>
        /// <param name="propertyOwner">The property owner.</param>
        /// <returns></returns>
        public static IPropertyQuery AddConstrainContainsIf(this IPropertyQuery query, IManagedProperty property, string value, Type propertyOwner = null)
        {
            if (ConditionalSql.IsNotEmpty(value)) { query.AddConstrain(property, propertyOwner).Contains(value); }
            return query;
        }

        /// <summary>
        /// 如果提供的值是不可空的，则为查询条件添加是否为起始字符串的查询判断。
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="property">查询某个属性。</param>
        /// <param name="value">当 value 不可空时，才添加查询判断。</param>
        /// <param name="propertyOwner">The property owner.</param>
        /// <returns></returns>
        public static IPropertyQuery AddConstrainStartWithIf(this IPropertyQuery query, IManagedProperty property, string value, Type propertyOwner = null)
        {
            if (ConditionalSql.IsNotEmpty(value)) { query.AddConstrain(property, propertyOwner).StartWith(value); }
            return query;
        }
    }
}