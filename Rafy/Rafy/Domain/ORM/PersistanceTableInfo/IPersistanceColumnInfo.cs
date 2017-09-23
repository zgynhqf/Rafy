/*******************************************************
 * 
 * 作者：CodeProject
 * 创建时间：2009
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 CodeProject 2009
 * 
*******************************************************/

using System;
using System.Data;
using Rafy.MetaModel;

namespace Rafy.Domain.ORM
{
    /// <summary>
    /// 数据表列
    /// </summary>
    internal interface IPersistanceColumnInfo
    {
        /// <summary>
        /// 对应的列的元数据。
        /// </summary>
        ColumnMeta Meta { get; }

        /// <summary>
        /// 对应的表
        /// </summary>
        IPersistanceTableInfo Table { get; }

        /// <summary>
        /// 列名
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 属性的类型。
        /// 如果属性是可空类型。这里会去除可空类型，返回内部的真实属性类型。
        /// </summary>
        Type PropertyType { get; }

        /// <summary>
        /// 该列的列类型。
        /// </summary>
        DbType DbType { get; }

        /// <summary>
        /// 是否为自增长主键列。
        /// </summary>
        bool IsIdentity { get; }

        /// <summary>
        /// 是否主键列
        /// </summary>
        bool IsPrimaryKey { get; }

        /// <summary>
        /// 对应的托管属性
        /// </summary>
        IProperty Property { get; }
    }
}