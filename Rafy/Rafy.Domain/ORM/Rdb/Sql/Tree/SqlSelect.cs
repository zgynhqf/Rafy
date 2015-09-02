/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20131210
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20131210 09:45
 * 
*******************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.Data;

namespace Rafy.Domain.ORM.SqlTree
{
    /// <summary>
    /// 表示一个 Sql 查询语句。
    /// </summary>
    class SqlSelect : SqlNode, ISqlSelect
    {
        public override SqlNodeType NodeType
        {
            get { return SqlNodeType.SqlSelect; }
        }

        /// <summary>
        /// 是否只查询数据的条数。
        /// 
        /// 如果这个属性为真，那么不再需要使用 Selection。
        /// </summary>
        public bool IsCounting { get; set; }

        /// <summary>
        /// 是否需要查询不同的结果。
        /// </summary>
        public bool IsDistinct { get; set; }

        /// <summary>
        /// 要查询的内容。
        /// 如果本属性为空，表示要查询所有列。
        /// </summary>
        public ISqlNode Selection { get; set; }

        /// <summary>
        /// 要查询的数据源。
        /// </summary>
        public ISqlSource From { get; set; }

        /// <summary>
        /// 查询的过滤条件。
        /// </summary>
        public ISqlConstraint Where { get; set; }

        /// <summary>
        /// 查询的排序规则。
        /// 可以指定多个排序条件，其中每一项都必须是一个 SqlOrderBy 对象。
        /// </summary>
        public SqlOrderByList OrderBy { get; set; }

        internal bool HasOrdered()
        {
            return OrderBy != null && OrderBy.Count > 0;
        }
    }
}