/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20131210
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20131210 17:15
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.Domain.ORM.SqlTree
{
    /// <summary>
    /// 排序结点。
    /// </summary>
    class SqlOrderBy : SqlNode
    {
        public override SqlNodeType NodeType
        {
            get { return SqlNodeType.SqlOrderBy; }
        }

        /// <summary>
        /// 使用这个列进行排序。
        /// </summary>
        public SqlColumn Column { get; set; }

        /// <summary>
        /// 使用这个方向进行排序。
        /// </summary>
        public OrderDirection Direction { get; set; }
    }
}
