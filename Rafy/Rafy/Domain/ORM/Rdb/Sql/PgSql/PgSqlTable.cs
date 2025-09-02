/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20250815
 * 运行环境：.Net Standard 2.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20250815 20:21
 * 
*******************************************************/

using Rafy.Domain.ORM.MySql;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rafy.Domain.ORM.PgSql
{
    internal class PgSqlTable : MySqlTable
    {
        public PgSqlTable(IRepositoryInternal repository, string dbProvider) : base(repository, dbProvider)
        {
            _insertSql = new Lazy<string>(() =>
            {
                var generatedSql = this.GenerateInsertSQL(false);
                return $@"{generatedSql}
RETURNING {this.PKColumn.Name};";
            });
        }

        public override SqlGenerator CreateSqlGenerator()
        {
            return new PgSqlSqlGenerator();
        }
    }
}
