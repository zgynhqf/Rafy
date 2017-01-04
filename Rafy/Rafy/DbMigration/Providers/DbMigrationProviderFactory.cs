/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120424
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120424
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.DbMigration.Model;
using Rafy.Data;
using Rafy.DbMigration.SqlServer;
using Rafy.Data.Providers;
using Rafy.DbMigration.SqlServerCe;
using Rafy.DbMigration.Oracle;
using Rafy.DbMigration.MySql;

namespace Rafy.DbMigration
{
    internal static class DbMigrationProviderFactory
    {
        public static DbMigrationProvider GetProvider(DbSetting dbSetting)
        {
            DbMigrationProvider provider = null;

            //ISqlConverter Factory
            switch (dbSetting.ProviderName)
            {
                case DbConnectionSchema.Provider_SqlClient:
                    provider = new SqlServerMigrationProvider();
                    break;
                case DbConnectionSchema.Provider_SqlCe:
                    provider = new SqlServerCeMigrationProvider();
                    break;
                //Patrickliu增加的代码块
                case DbConnectionSchema.Provider_MySql:
                    provider = new MySqlMigrationProvider();
                    break;
                //case "System.Data.Odbc":
                //    return new ODBCProvider();
                default:
                    if (DbConnectionSchema.IsOracleProvider(dbSetting))
                    {
                        provider = new OracleMigrationProvider();
                        break;
                    }
                    throw new NotSupportedException("This type of database is not supportted now:" + dbSetting.ProviderName);
            }

            provider.DbSetting = dbSetting;

            return provider;
        }
    }
}