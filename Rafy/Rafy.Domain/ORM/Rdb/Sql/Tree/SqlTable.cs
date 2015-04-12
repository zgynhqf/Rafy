/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20131210
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20131210 10:55
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.Domain.ORM.SqlTree
{
    /// <summary>
    /// 表示一个具体的表。
    /// </summary>
    class SqlTable : SqlNamedSource
    {
        public override SqlNodeType NodeType
        {
            get { return SqlNodeType.SqlTable; }
        }

        /// <summary>
        /// 表名
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// 查询中使用的别名
        /// </summary>
        public string Alias { get; set; }

        internal override string GetName()
        {
            return this.Alias ?? this.TableName;
        }
    }
}
