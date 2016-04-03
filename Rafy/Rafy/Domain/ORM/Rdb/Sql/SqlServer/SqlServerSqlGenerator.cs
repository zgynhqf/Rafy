/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20131210
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20131210 15:51
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.Domain.ORM.SqlTree;

namespace Rafy.Domain.ORM
{
    class SqlServerSqlGenerator : SqlGenerator
    {
        protected override void QuoteAppend(string identifier)
        {
            if (this.AutoQuota)
            {
                identifier = this.PrepareIdentifier(identifier);
                Sql.Append("[").Append(identifier).Append("]");
            }
            else
            {
                base.QuoteAppend(identifier);
            }
        }

        protected override void AppendNameCast()
        {
            Sql.Append(" AS ");
        }

        /// <summary>
        /// 为指定的原始查询生成指定分页效果的新查询。
        /// </summary>
        /// <param name="raw">原始查询</param>
        /// <param name="pagingInfo">分页信息。</param>
        /// <returns></returns>
        protected override ISqlSelect ModifyToPagingTree(SqlSelect raw, PagingInfo pagingInfo)
        {
            if (PagingInfo.IsNullOrEmpty(pagingInfo)) { throw new ArgumentNullException("pagingInfo"); }
            if (!raw.HasOrdered()) { throw new InvalidProgramException("必须排序后才能使用分页功能。"); }

            //如果是第一页，则只需要使用 TOP 语句即可。
            if (pagingInfo.PageNumber == 1)
            {
                return new SqlSelect
                {
                    Selection = new SqlNodeList
                    {
                        new SqlLiteral { FormattedSql = "TOP " + pagingInfo.PageSize + " " },
                        raw.Selection ?? SqlSelectAll.Default
                    },
                    From = raw.From,
                    Where = raw.Where,
                    OrderBy = raw.OrderBy
                };
            }

            /*********************** 代码块解释 *********************************
             * 
             * 转换方案：
             * 
             * SELECT * 
             * FROM ASN
             * WHERE ASN.Id > 0
             * ORDER BY ASN.AsnCode ASC
             * 
             * 转换分页后：
             * 
             * SELECT TOP 10 * 
             * FROM ASN
             * WHERE ASN.Id > 0 AND ASN.Id NOT IN(
             *     SELECT TOP 20 Id
             *     FROM ASN
             *     WHERE ASN.Id > 0 
             *     ORDER BY ASN.AsnCode ASC
             * )
             * ORDER BY ASN.AsnCode ASC
             * 
            **********************************************************************/

            //先要找到主表的 PK，分页时需要使用此主键列来生成分页 Sql。
            //这里约定 Id 为主键列名。
            var finder = new FirstTableFinder();
            var pkTable = finder.Find(raw.From);
            var pkColumn = new SqlColumn { Table = pkTable, ColumnName = EntityConvention.IdColumnName };

            //先生成内部的 Select
            var excludeSelect = new SqlSelect
            {
                Selection = new SqlNodeList
                {
                    new SqlLiteral { FormattedSql = "TOP " + (pagingInfo.PageNumber - 1) * pagingInfo.PageSize + " " },
                    pkColumn
                },
                From = raw.From,
                Where = raw.Where,
                OrderBy = raw.OrderBy,
            };

            var res = new SqlSelect
            {
                Selection = new SqlNodeList
                {
                    new SqlLiteral { FormattedSql = "TOP " + pagingInfo.PageSize + " " },
                    raw.Selection ?? SqlSelectAll.Default
                },
                From = raw.From,
                OrderBy = raw.OrderBy,
            };

            var newWhere = new SqlColumnConstraint
            {
                Column = pkColumn,
                Operator = SqlColumnConstraintOperator.NotIn,
                Value = excludeSelect
            };
            if (raw.Where != null)
            {
                res.Where = new SqlBinaryConstraint
                {
                    Left = raw.Where,
                    Opeartor = SqlBinaryConstraintType.And,
                    Right = newWhere
                };
            }
            else
            {
                res.Where = newWhere;
            }

            return res;
        }
    }
}
