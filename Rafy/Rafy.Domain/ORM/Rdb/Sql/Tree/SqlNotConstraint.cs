/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20131211
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20131211 10:52
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.Domain.ORM.SqlTree
{
    /// <summary>
    /// 表示一个取反规则的条件。
    /// </summary>
    class SqlNotConstraint : SqlConstraint
    {
        public override SqlNodeType NodeType
        {
            get { return SqlNodeType.SqlNotConstraint; }
        }

        /// <summary>
        /// 需要被取反的条件。
        /// </summary>
        public SqlConstraint Constraint { get; set; }
    }
}
