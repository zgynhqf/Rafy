/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20210819
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20210819 00:26
 * 
*******************************************************/

using Rafy.ManagedProperty;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rafy.Domain.ORM
{
    /// <summary>
    /// Rafy ORM 中可以进行的全局性配置。
    /// </summary>
    public abstract class ORMSettings
    {
        /// <summary>
        /// 默认为 false，表示如果在查询时，Sql 中没有给出实体需要映射的列时，默认会禁用这些属性。
        /// 可以通过配置此属性为 true，来关闭此功能。
        /// 注意：
        /// 一旦关闭此功能，所有未查询出来的列所对应的属性，都被处于可用状态。这也意味着，这些属性的值虽然不可用，但是下次保存实体时，这些属性的值可能会被保存到数据库中。
        /// 您也可以通过调用 <see cref="ManagedPropertyObject.Disable(ManagedProperty.IManagedProperty, bool)"/> 方法，来手动启用实体的指定属性。
        /// </summary>
        public static bool EnablePropertiesIfNotFoundInSqlQuery { get; set; }

        /// <summary>
        /// 为阻止数据丢失，如果在查询时，Sql 中没有给出实体需要映射的列，
        /// 且已经启用 <see cref="EnablePropertiesIfNotFoundInSqlQuery"/> 的情况时，是否抛出异常？
        /// 默认为 true。
        /// </summary>
        public static bool ErrorIfColumnNotFoundInSql { get; set; } = true;
    }
}