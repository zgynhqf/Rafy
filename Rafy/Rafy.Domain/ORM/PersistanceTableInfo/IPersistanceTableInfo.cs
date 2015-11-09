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
using Rafy.Domain;
using System.Collections.Generic;

namespace Rafy.Domain.ORM
{
    /// <summary>
    /// 某个实体类型对应的‘逻辑表’的元数据。
    /// 这个表并不一定就是数据库中的表，而可能是 MongoDb 中的表、一个 XML 文档等。
    /// </summary>
    internal interface IPersistanceTableInfo
    {
        /// <summary>
        /// 对应的实体类型
        /// </summary>
        Type Class { get; }

        /// <summary>
        /// 表名
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 主键列（每个表肯定有一个主键列）
        /// </summary>
        IPersistanceColumnInfo PKColumn { get; }

        /// <summary>
        /// 所有的列
        /// </summary>
        IReadOnlyList<IPersistanceColumnInfo> Columns { get; }
    }
}