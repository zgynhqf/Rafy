﻿/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20210830
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20210830 04:39
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Rafy.Domain.ORM.SqlTree
{
    /// <summary>
    /// 表示查询结果中的一个列。
    /// </summary>
    public interface ISqlSelectionColumn //: ISqlNode
    {
        /// <summary>
        /// 只能是 <see cref="SqlTable"/>、<see cref="SqlSubSelect"/>
        /// </summary>
        IHasName Table { get; }

        /// <summary>
        /// 列名
        /// </summary>
        string ColumnName { get; }

        /// <summary>
        /// 别名。
        /// 列的别名只用在 Select 语句之后。
        /// </summary>
        string Alias { get; }

        /// <summary>
        /// 该列的列类型。
        /// </summary>
        DbType DbType { get; }

        /// <summary>
        /// 该列是否拥有索引。
        /// </summary>
        bool HasIndex { get; }
    }
}