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
        NotLike = SqlColumnConstraintOperator.NotLike,
        Contains = SqlColumnConstraintOperator.Contains,
        NotContains = SqlColumnConstraintOperator.NotContains,
        StartsWith = SqlColumnConstraintOperator.StartsWith,
        NotStartsWith = SqlColumnConstraintOperator.NotStartsWith,
        EndsWith = SqlColumnConstraintOperator.EndsWith,
        NotEndsWith = SqlColumnConstraintOperator.NotEndsWith,

        In = SqlColumnConstraintOperator.In,
        NotIn = SqlColumnConstraintOperator.NotIn,
    }

    internal class PropertyOperatorHelper
    {
        public static PropertyOperator Reverse(PropertyOperator op)
        {
            switch (op)
            {
                case PropertyOperator.Equal:
                    return PropertyOperator.NotEqual;
                case PropertyOperator.NotEqual:
                    return PropertyOperator.Equal;
                case PropertyOperator.Greater:
                    return PropertyOperator.LessEqual;
                case PropertyOperator.GreaterEqual:
                    return PropertyOperator.Less;
                case PropertyOperator.Less:
                    return PropertyOperator.GreaterEqual;
                case PropertyOperator.LessEqual:
                    return PropertyOperator.Greater;
                case PropertyOperator.Like:
                    return PropertyOperator.NotLike;
                case PropertyOperator.NotLike:
                    return PropertyOperator.Like;
                case PropertyOperator.Contains:
                    return PropertyOperator.NotContains;
                case PropertyOperator.NotContains:
                    return PropertyOperator.Contains;
                case PropertyOperator.StartsWith:
                    return PropertyOperator.NotStartsWith;
                case PropertyOperator.NotStartsWith:
                    return PropertyOperator.StartsWith;
                case PropertyOperator.EndsWith:
                    return PropertyOperator.NotEndsWith;
                case PropertyOperator.NotEndsWith:
                    return PropertyOperator.EndsWith;
                case PropertyOperator.In:
                    return PropertyOperator.NotIn;
                case PropertyOperator.NotIn:
                    return PropertyOperator.In;
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
