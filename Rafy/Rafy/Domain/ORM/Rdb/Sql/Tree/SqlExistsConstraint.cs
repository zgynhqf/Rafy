/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20131211
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20131211 10:29
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.Domain.ORM.SqlTree
{
    /// <summary>
    /// 表示对指定的查询进行是否存在查询行的逻辑的判断。
    /// </summary>
    class SqlExistsConstraint : SqlConstraint
    {
        public override SqlNodeType NodeType
        {
            get { return SqlNodeType.SqlExistsConstraint; }
        }

        /// <summary>
        /// 要检查的查询。
        /// </summary>
        public SqlSelect Select { get; set; }
    }
}
