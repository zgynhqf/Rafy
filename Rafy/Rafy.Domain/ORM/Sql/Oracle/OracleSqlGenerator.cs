/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20131210
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20131210 15:18
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.Domain.ORM.SqlTree;

namespace Rafy.Domain.ORM.Oracle
{
    class OracleSqlGenerator : SqlGenerator
    {
        protected override void QuoteAppend(string identifier)
        {
            if (this.AutoQuota)
            {
                identifier = this.PrepareIdentifier(identifier);
                Sql.Append("\"").Append(identifier).Append("\"");
            }
            else
            {
                base.QuoteAppend(identifier);
            }
        }

        protected override string PrepareIdentifier(string identifier)
        {
            return identifier.ToUpper();
        }

        protected override SqlColumnConstraint VisitSqlColumnConstraint(SqlColumnConstraint node)
        {
            switch (node.Operator)
            {
                case SqlColumnConstraintOperator.Equal:
                case SqlColumnConstraintOperator.NotEqual:
                    //在 Oracle 中，空字符串的对比，需要转换为对 Null 值的对比。
                    var strValue = node.Value as string;
                    if (strValue != null && strValue.Length == 0)
                    {
                        node.Value = null;
                    }
                    break;
                default:
                    break;
            }

            return base.VisitSqlColumnConstraint(node);
        }
    }
}
