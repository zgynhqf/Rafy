/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20131210
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20131210 10:53
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.Domain.ORM.SqlTree
{
    /// <summary>
    /// 表示某个表、或者查询结果中的某一列。
    /// </summary>
    class SqlColumn : SqlNode
    {
        public override SqlNodeType NodeType
        {
            get { return SqlNodeType.SqlColumn; }
        }

        /// <summary>
        /// 只能是 <see cref="SqlTable"/>、<see cref="SqlSubSelect"/>
        /// </summary>
        public SqlNamedSource Table { get; set; }

        /// <summary>
        /// 列名
        /// </summary>
        public string ColumnName { get; set; }

        /// <summary>
        /// 别名。
        /// 列的别名只用在 Select 语句之后。
        /// </summary>
        public string Alias { get; set; }
    }
}