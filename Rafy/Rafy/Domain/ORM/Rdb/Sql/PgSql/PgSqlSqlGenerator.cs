/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20250815
 * 运行环境：.Net Standard 2.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20250815 20:39
 * 
*******************************************************/

using Rafy.Data;
using Rafy.DbMigration;
using Rafy.DbMigration.MySql;
using Rafy.DbMigration.Oracle;
using Rafy.DbMigration.PgSql;
using Rafy.Domain.ORM.SqlTree;
using Rafy.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rafy.Domain.ORM.PgSql
{
    /// <summary>
    /// PgSql 的Sql语句生成器
    /// </summary>
    internal class PgSqlSqlGenerator : SqlGenerator
    {
        public PgSqlSqlGenerator()
        {
            this.IdentifierProvider = PgSqlIdentifierQuoter.Instance;
            this.ValueConverter = PgSqlDbTypeConverter.Instance;
        }

        /// <summary>
        /// Sql Server 中没有限制 In 语句中的项的个数。（但是如果使用参数的话，则最多只能使用 2000 个参数。）
        /// 
        /// In 语句中可以承受的最大的个数。
        /// 如果超出这个个数，则会抛出 TooManyItemsInInClauseException。
        /// </summary>
        protected override int MaxItemsInInClause => int.MaxValue;

        protected override object PrepareSqlEmbedParameter(object value)
        {
            if (value is bool) return value;
            return base.PrepareSqlEmbedParameter(value);
        }

        /// <summary>
        /// 名称别名设置
        /// </summary>
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