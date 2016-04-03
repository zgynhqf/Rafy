/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20131211
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20131211 11:13
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.Domain.ORM.SqlTree
{
    /// <summary>
    /// 子查询。
    /// 对一个子查询分配别名后，可以作为一个新的源。
    /// </summary>
    class SqlSubSelect : SqlNamedSource
    {
        public override SqlNodeType NodeType
        {
            get { return SqlNodeType.SqlSubSelect; }
        }

        /// <summary>
        /// 子查询
        /// </summary>
        public SqlSelect Select { get; set; }

        /// <summary>
        /// 别名，必须填写
        /// </summary>
        public string Alias { get; set; }

        internal override string GetName()
        {
            return this.Alias;
        }
    }
}
