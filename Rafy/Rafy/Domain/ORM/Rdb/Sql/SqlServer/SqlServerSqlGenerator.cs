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
             *      SELECT * FROM
             *      (
             *          SELECT A.*, ROW_NUMBER() OVER (order by  Id)_rowNumber
             *          FROM  A
             *      ) T
             *      WHERE _rowNumber between 1 and 10
            **********************************************************************/

            var newRaw = new SqlSelect
            {
                Selection = new SqlNodeList()
                    {
                        new SqlLiteral {FormattedSql = "ROW_NUMBER() OVER ("},
                        raw.OrderBy,
                        new SqlLiteral {FormattedSql = ")_rowNumber,"},
                        raw.Selection ?? SqlSelectAll.Default
                    },
                From = raw.From,
                Where = raw.Where,
                IsDistinct = raw.IsDistinct,
                IsCounting = raw.IsCounting,
               
            };
            var startRow = pagingInfo.PageSize * (pagingInfo.PageNumber - 1) + 1;
            var endRow = startRow + pagingInfo.PageSize - 1;

            var res = MakePagingTree(newRaw, startRow, endRow);

            return res;
        }

        private static ISqlSelect MakePagingTree(SqlSelect raw, long startRow, long endRow)
        {
            /*********************** 代码块解释 *********************************
             * 以下转换使用 row_number 行号字段来实现分页。只需要简单地在查询的 WHERE 语句中加入等号的判断即可。
             *
             * 源格式：
             *     SELECT *
             *     FROM A
             *     WHERE A.Id > 0
             *     ORDER BY A.NAME ASC
             * 
             * 目标格式：
             *      SELECT * FROM
             *      (
             *          SELECT A.*, row_number() over (order by XXXXXX)_rowNumber
             *          FROM  A
             *      ) T
             *      WHERE _rowNumber between 1 and 10
            **********************************************************************/

            return new SqlNodeList
            {
                                new SqlLiteral(
                @"SELECT * FROM
                ("),
                                raw,
                                new SqlLiteral(
                @")T WHERE _rowNumber BETWEEN " + startRow + @" AND " +endRow )
            };
        }

    }
}
