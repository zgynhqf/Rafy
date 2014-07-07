/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20140703
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20140703 18:55
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rafy.Domain.ORM.SqlTree;

namespace Rafy.Domain
{
    /// <summary>
    /// 属性的对比操作符
    /// </summary>
    public enum PropertyOperator
    {
        Equal = SqlColumnConstraintOperator.Equal,
        NotEqual = SqlColumnConstraintOperator.NotEqual,
        Greater = SqlColumnConstraintOperator.Greater,
        GreaterEqual = SqlColumnConstraintOperator.GreaterEqual,
        Less = SqlColumnConstraintOperator.Less,
        LessEqual = SqlColumnConstraintOperator.LessEqual,

        Like = SqlColumnConstraintOperator.Like,
        Contains = SqlColumnConstraintOperator.Contains,
        StartWith = SqlColumnConstraintOperator.StartWith,
        EndWith = SqlColumnConstraintOperator.EndWith,

        In = SqlColumnConstraintOperator.In,
        NotIn = SqlColumnConstraintOperator.NotIn,
    }
}
