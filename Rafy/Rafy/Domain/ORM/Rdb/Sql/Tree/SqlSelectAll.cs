/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20131210
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20131210 20:08
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.Domain.ORM.SqlTree
{
    /// <summary>
    /// 用于表示选择所有列、或者表示选择某个表的所有列。
    /// </summary>
    class SqlSelectAll : SqlNode
    {
        public static readonly SqlSelectAll Default = new SqlSelectAll();

        public override SqlNodeType NodeType
        {
            get { return SqlNodeType.SqlSelectAll; }
        }

        /// <summary>
        /// 如果本属性为空，表示选择所有数据源的所有列；否则表示选择指定表的所有列。
        /// </summary>
        public SqlNamedSource Table { get; set; }
    }
}
