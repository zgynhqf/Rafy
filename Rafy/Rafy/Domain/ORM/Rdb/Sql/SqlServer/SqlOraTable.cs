/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130122 17:15
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130122 17:15
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Rafy;
using Rafy.Data;
using Rafy.Domain;
using Rafy.Domain.ORM.Query;
using Rafy.Domain.ORM.Query.Impl;
using Rafy.Domain.ORM.SqlTree;
using Rafy.MetaModel;

namespace Rafy.Domain.ORM
{
    /// <summary>
    /// SqlServer 、 Oracle 两个数据库都支持的一些功能，封装在这个类中。
    /// </summary>
    internal abstract class SqlOraTable : RdbTable
    {
        public SqlOraTable(IRepositoryInternal repository) : base(repository) { }

        /// <summary>
        /// SqlServer 、 Oracle 都支持在数据库层面进行分页。
        /// </summary>
        protected override PagingLocation GetPagingLocation(PagingInfo pagingInfo)
        {
            //虽然本类默认使用数据库分页，但是它的子类可以重写本方法来使用内存分页。
            //所以本类中的所有方法，在重新实现时，都会分辨这两种情况。
            return PagingLocation.Database;
        }

        #region 以 Sql 查询的方式

        public override void QueryList(IDbAccesser dba, ISqlSelectArgs args)
        {
            var pagingInfo = args.PagingInfo;
            if (PagingInfo.IsNullOrEmpty(pagingInfo) || this.GetPagingLocation(pagingInfo) == PagingLocation.Memory)
            {
                base.QueryList(dba, args);
            }
            else
            {
                //转换为分页查询 SQL
                var parts = ParsePagingSqlParts(args.FormattedSql);
                CreatePagingSql(ref parts, pagingInfo);

                //读取分页的实体
                using (var reader = dba.QueryDataReader(parts.PagingSql, args.Parameters))
                {
                    this.FillDataIntoList(
                        reader, ReadDataType.ByName,
                        args.List, args.FetchingFirst, PagingInfo.Empty, args.MarkTreeFullLoaded
                        );
                }

                QueryTotalCountIf(dba, pagingInfo, parts, args.Parameters);
            }
        }

        public override void QueryTable(IDbAccesser dba, ITableQueryArgs args)
        {
            var pagingInfo = args.PagingInfo;
            if (PagingInfo.IsNullOrEmpty(pagingInfo) || this.GetPagingLocation(pagingInfo) == PagingLocation.Memory)
            {
                base.QueryTable(dba, args);
            }
            else
            {
                //转换为分页查询 SQL
                var parts = ParsePagingSqlParts(args.FormattedSql);
                CreatePagingSql(ref parts, pagingInfo);

                //读取分页的数据
                var table = args.ResultTable;
                using (var reader = dba.QueryDataReader(parts.PagingSql, args.Parameters))
                {
                    LiteDataTableAdapter.Fill(table, reader);
                }

                QueryTotalCountIf(dba, pagingInfo, parts, args.Parameters);
            }
        }

        /// <summary>
        /// 如果需要统计，则生成统计语句进行查询。
        /// </summary>
        /// <param name="dba"></param>
        /// <param name="pagingInfo"></param>
        /// <param name="parts"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private static void QueryTotalCountIf(IDbAccesser dba, PagingInfo pagingInfo, PagingSqlParts parts, object[] parameters)
        {
            if (pagingInfo.IsNeedCount)
            {
                var pagingCountSql = "SELECT COUNT(0) " + parts.FromWhere;

                //查询值。（由于所有参数都不会在 OrderBy、Select 语句中，所以把所有参数都传入。
                var value = dba.QueryValue(pagingCountSql, parameters);
                pagingInfo.TotalCount = Convert.ToInt64(value);
            }
        }

        #endregion

        #region 以 IQuery 查询的方式

        public override void QueryList(IDbAccesser dba, IEntitySelectArgs args)
        {
            /*********************** 代码块解释 *********************************
             * 
             * 以下代码用于支持数据库分页
             * 
            **********************************************************************/

            //检查分页条件。（如果是树状实体，也不支持在数据库中进行分页。）
            var pagingInfo = args.PagingInfo;
            bool isPaging = !PagingInfo.IsNullOrEmpty(pagingInfo) &&
                this.GetPagingLocation(pagingInfo) == PagingLocation.Database &&
                !Repository.SupportTree;
            if (isPaging)
            {
                var query = args.Query;

                var autoSelection = AutoSelectionForLOB(query);

                //生成分页 Sql
                var generator = this.CreateSqlGenerator();
                generator.Generate(query as SqlSelect, pagingInfo);
                var pagingSql = generator.Sql;

                //查询数据库
                using (var reader = dba.QueryDataReader(pagingSql, pagingSql.Parameters))
                {
                    //填充到列表中。
                    this.FillDataIntoList(
                        reader, autoSelection ? ReadDataType.ByIndex : ReadDataType.ByName,
                        args.List, false, pagingInfo, args.MarkTreeFullLoaded
                        );
                }

                //最后，如果需要，则统计一下总行数。
                if (pagingInfo.IsNeedCount)
                {
                    pagingInfo.TotalCount = this.Count(dba, query);
                }
            }
            else
            {
                base.QueryList(dba, args);
            }
        }

        #endregion

        #region ToPagingSql

        protected virtual void CreatePagingSql(ref PagingSqlParts parts, PagingInfo pagingInfo)
        {
            /*********************** 代码块解释 *********************************
             * 
             * 注意，这个方法只支持不太复杂 SQL 的转换。
             *
             * 源格式：
             * select ...... from ...... order by xxxx asc, yyyy desc
             * 不限于以上格式，只要满足没有复杂的嵌套查询，最外层是一个 Select 和 From 语句即可。
             * 
             * 目标格式：
             * select * from (select ......, row_number() over(order by xxxx asc, yyyy desc) _rowNumber from ......) x where x._rowNumber<10 and x._rowNumber>5;
            **********************************************************************/

            var startRow = pagingInfo.PageSize * (pagingInfo.PageNumber - 1) + 1;
            var endRow = startRow + pagingInfo.PageSize - 1;

            var sql = new StringBuilder("SELECT * FROM (");

            //在 Select 和 From 之间插入：
            //,row_number() over(order by UPDDATETIME desc) rn 
            sql.AppendLine().Append(parts.Select)
                .Append(", row_number() over(")
                .Append(parts.OrderBy);
            //query.AppendSqlOrder(res, this);
            sql.Append(") dataRowNumber ").Append(parts.FromWhere)
                .Append(") x").AppendLine()
                .Append("WHERE x.dataRowNumber >= ").Append(startRow)
                .Append(" AND x.dataRowNumber <= ").Append(endRow);

            parts.PagingSql = sql.ToString();
        }

        private static Regex FromRegex = new Regex(@"[^\w]FROM[^\w]", RegexOptions.IgnoreCase);

        private static PagingSqlParts ParsePagingSqlParts(string sql)
        {
            var fromIndex = FromRegex.Match(sql).Index;
            var orderByIndex = sql.LastIndexOf("ORDER BY", StringComparison.OrdinalIgnoreCase);
            if (orderByIndex < 0) { throw new InvalidProgramException("使用数据库分页时，Sql 语句中必须指定 OrderBy 语句。"); }

            var parts = new PagingSqlParts();
            parts.RawSql = sql;
            parts.Select = sql.Substring(0, fromIndex).Trim();
            parts.FromWhere = sql.Substring(fromIndex, orderByIndex - fromIndex).Trim();
            parts.OrderBy = sql.Substring(orderByIndex).Trim();
            return parts;
        }

        protected struct PagingSqlParts
        {
            /// <summary>
            /// 原始 SQL
            /// </summary>
            public string RawSql;

            /// <summary>
            /// Select 语句
            /// </summary>
            public string Select;

            /// <summary>
            /// From 以及 Where 语句
            /// </summary>
            public string FromWhere;

            /// <summary>
            /// OrderBy 语句
            /// </summary>
            public string OrderBy;

            /// <summary>
            /// 转换后的分页 SQL
            /// </summary>
            public string PagingSql;
        }

        #endregion
    }
}
