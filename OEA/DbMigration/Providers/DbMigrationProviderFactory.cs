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
using DbMigration.Model;
using hxy.Common.Data;
using DbMigration.SqlServer;
using hxy.Common.Data.Providers;
using DbMigration.SqlServerCe;
using DbMigration.Oracle;

namespace DbMigration
{
    internal static class DbMigrationProviderFactory
    {
        public static DbMigrationProvider GetProvider(DbSetting dbSetting)
        {
            DbMigrationProvider provider = null;

            //ISqlConverter Factory
            switch (dbSetting.ProviderName)
            {
                case DbSetting.Provider_SqlClient:
                    provider = new SqlServerMigrationProvider();
                    break;
                case DbSetting.Provider_SqlCe:
                    provider = new SqlServerCeMigrationProvider();
                    break;
                case DbSetting.Provider_Oracle:
                    provider = new OracleMigrationProvider();
                    break;
                //case "System.Data.Odbc":
                //    return new ODBCProvider();
                default:
                    throw new NotSupportedException("This type of database is not supportted now:" + dbSetting.ProviderName);
            }

            provider.DbSetting = dbSetting;

            return provider;
        }
    }
}