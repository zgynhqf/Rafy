/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130529
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130529 16:41
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Rafy.Data;

namespace Rafy.Domain.ORM
{
    internal class AndOrConstraint : Constraint
    {
        internal Constraint Left, Right;

        /// <summary>
        /// true: And, false: Or
        /// </summary>
        internal bool IsAnd;

        public override ConstraintType Type
        {
            get { return ConstraintType.AndOr; }
        }

        //public override void GetSql(TextWriter sql, FormattedSqlParameters parameters)
        //{
        //    //先计算是否需要输出括号。
        //    bool leftBracket = false, rightBracket = false;
        //    switch (Left.Type)
        //    {
        //        case ConstraintType.Empty:
        //            Right.GetSql(sql, parameters);
        //            return;
        //        case ConstraintType.AndOr:
        //            if (!(Left as AndOrConstraint).IsAnd && this.IsAnd)
        //            {
        //                leftBracket = true;
        //            }
        //            break;
        //        case ConstraintType.Group:
        //            leftBracket = true;
        //            break;
        //        default:
        //            break;
        //    }

        //    switch (Right.Type)
        //    {
        //        case ConstraintType.Empty:
        //            Left.GetSql(sql, parameters);
        //            return;
        //        case ConstraintType.AndOr:
        //            if (!(Right as AndOrConstraint).IsAnd && this.IsAnd)
        //            {
        //                rightBracket = true;
        //            }
        //            break;
        //        case ConstraintType.Group:
        //            rightBracket = true;
        //            break;
        //        default:
        //            break;
        //    }

        //    //开始输出
        //    if (leftBracket) { sql.Write('('); }
        //    Left.GetSql(sql, parameters);
        //    if (leftBracket) { sql.Write(')'); }

        //    sql.Write(' ');
        //    if (IsAnd)
        //    {
        //        sql.Write(AndOperator);
        //    }
        //    else
        //    {
        //        sql.Write(OrOperator);
        //    }
        //    sql.Write(' ');

        //    if (rightBracket) { sql.Write('('); }
        //    Right.GetSql(sql, parameters);
        //    if (rightBracket) { sql.Write(')'); }
        //}
    }
}