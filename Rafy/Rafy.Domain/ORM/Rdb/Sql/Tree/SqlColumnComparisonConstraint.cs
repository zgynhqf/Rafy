/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20131210
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20131210 11:30
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.Domain.ORM.SqlTree
{
    /// <summary>
    /// 表示某个列与某个值进行对比的约束条件。
    /// </summary>
    class SqlColumnConstraint : SqlConstraint
    {
        public override SqlNodeType NodeType
        {
            get { return SqlNodeType.SqlColumnConstraint; }
        }

        /// <summary>
        /// 要对比的列。
        /// </summary>
        public SqlColumn Column { get; set; }

        /// <summary>
        /// 对比操作符
        /// </summary>
        public SqlColumnConstraintOperator Operator { get; set; }

        /// <summary>
        /// 要对比的值。
        /// </summary>
        public object Value { get; set; }
    }

    /// <summary>
    /// 对比操作符
    /// </summary>
    enum SqlColumnConstraintOperator
    {
        Equal,
        NotEqual,
        Greater,
        GreaterEqual,
        Less,
        LessEqual,

        Like,
        NotLike,
        Contains,
        NotContains,
        StartsWith,
        NotStartsWith,
        EndsWith,
        NotEndsWith,

        In,
        NotIn,
    }
}