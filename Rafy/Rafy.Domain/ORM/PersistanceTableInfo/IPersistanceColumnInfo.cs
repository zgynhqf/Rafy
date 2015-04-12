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

namespace Rafy.Domain.ORM
{
    /// <summary>
    /// 数据表列
    /// </summary>
    public interface IPersistanceColumnInfo
    {
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