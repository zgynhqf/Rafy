/*******************************************************
 * 
 * 作者：刘雷
 * 创建日期：20170104
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 刘雷 20170104 11:08
 * 
*******************************************************/

using Rafy.DbMigration.MySql;
using Rafy.Domain.ORM.SqlTree;
using Rafy.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rafy.Domain.ORM.MySql
{
    /// <summary>
    /// MySql的Sql语句生成器
    /// </summary>
    internal sealed class MySqlGenerator : SqlGenerator
    {
        /// <summary>
        /// Sql Server 中没有限制 In 语句中的项的个数。（但是如果使用参数的话，则最多只能使用 2000 个参数。）
        /// 
        /// In 语句中可以承受的最大的个数。
        /// 如果超出这个个数，则会抛出 TooManyItemsInInClauseException。
        /// </summary>
        protected override int MaxItemsInInClause => int.MaxValue;

        /// <summary>
        /// 名称别名设置
        /// </summary>
        protected override void AppendNameCast()
        {
            Sql.Append(" AS ");
        }

        /// <summary>
        /// 为指定标识符增加引用符号
        /// </summary>
        /// <param name="identifier">标识符</param>
        protected override void QuoteAppend(string identifier)
        {
            if (this.AutoQuota)
            {
                identifier = this.PrepareIdentifier(identifier);
                Sql.Append("`").Append(identifier).Append("`");
            }
            else
            {
                base.QuoteAppend(identifier);
            }
        }

        /// <summary>
        /// 转换空值比较
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override SqlColumnConstraint VisitSqlColumnConstraint(SqlColumnConstraint node)
        {
            switch (node.Operator)
            {
                case SqlColumnConstraintOperator.Equal:
                case SqlColumnConstraintOperator.NotEqual:
                    //在 MySql 中，空字符串的对比，需要转换为对 Null 值的对比。
                    if (node.Value is string)
                    {
                        if (node.Value != null)
                        {
                            if (node.Value.ToString().Length == 0)
                            {
                                node.Value = "";
                            }
                        }
                    }
                    break;
                //case SqlColumnConstraintOperator.Contains:
                //    if (node.Value is string && node.Value.ToString().IndexOf('%')>=0)
                //    {
                //        node.Value = "locate('" + node.Value + "'," + node.Column.ColumnName + ")!=0";
                //    }
                //    break;
                //case SqlColumnConstraintOperator.NotContains:
                //    if (node.Value is string&& node.Value.ToString().IndexOf('%') >= 0)
                //    {
                //        node.Value = "locate('" + node.Value + "'," + node.Column.ColumnName + ") = 0";
                //    }
                //    break;
                default:
                    break;
            }

            return base.VisitSqlColumnConstraint(node);
        }

        /// <summary>
        /// 进行数据类型的转换
        /// </summary>
        /// <param name="value">待转化的CLR数据类型</param>
        /// <returns>返回MySql兼容的数据类型</returns>
        public override object PrepareConstraintValue(object value)
        {
            value = base.PrepareConstraintValue(value);

            value = PrepareConstraintValueInternal(value);

            return value;
        }

        /// <summary>
        /// 针对布尔类型和枚举类型进行特殊处理
        /// </summary>
        /// <param name="value">待转换的CLR数据类型</param>
        /// <returns>返回MySql兼容的具体数据类型</returns>
        internal static object PrepareConstraintValueInternal(object value)
        {
            if (value != DBNull.Value)
            {
                if (value is bool)
                {
                    value = MySqlDbTypeHelper.ToDbBoolean((bool)value);
                }
                else if (value.GetType().IsEnum)
                {
                    value = TypeHelper.CoerceValue(typeof(int), value);
                }
            }

            return value;
        }

        /// <summary>
        /// 为指定的原始查询生成指定分页效果的新查询。
        /// </summary>
        /// <param name="raw">原始查询</param>
        /// <param name="pagingInfo">分页信息。</param>
        /// <returns></returns>
        protected override ISqlSelect ModifyToPagingTree(SqlSelect raw, PagingInfo pagingInfo)
        {
            var pageNumber = pagingInfo.PageNumber;
            var pageSize = pagingInfo.PageSize;

            return new SqlNodeList
            {
                raw,
                new SqlLiteral(@" LIMIT " + (pageNumber - 1) * pageSize + "," + pageSize)
            };
        }
    }
}