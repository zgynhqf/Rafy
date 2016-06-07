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
        ColumnMeta ColumnMeta { get; }

        /// <summary>
        /// 对应的表
        /// </summary>
        IPersistanceTableInfo Table { get; }

        /// <summary>
        /// 列名
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 数据类型
        /// </summary>
        Type DataType { get; }

        /// <summary>
        /// 返回本列是否为一个 Boolean 类型的属性（专为 Oracle 特制）
        /// </summary>
        bool IsBooleanType { get; }

        /// <summary>
        /// 返回本列是否为一个 String 类型的属性（专为 Oracle 特制）
        /// </summary>
        bool IsStringType { get; }

        /// <summary>
        /// 是否为自增长主键列。
        /// </summary>
        bool IsIdentity { get; }

        /// <summary>
        /// 是否可空列。
        /// </summary>
        bool IsNullable { get; }

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