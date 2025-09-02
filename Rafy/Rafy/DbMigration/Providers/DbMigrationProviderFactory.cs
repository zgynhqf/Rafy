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

using Rafy.Data;
using Rafy.Data.Providers;
using Rafy.DbMigration.Model;
using Rafy.DbMigration.MongoDb;
using Rafy.DbMigration.MySql;
using Rafy.DbMigration.Oracle;
using Rafy.DbMigration.PgSql;
using Rafy.DbMigration.SQLite;
using Rafy.DbMigration.SqlServer;
using Rafy.DbMigration.SqlServerCe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.DbMigration
{
    /// <summary>
    /// <see cref="DbMigrationProvider"/>、<see cref="DbIdentifierQuoter"/> 的工厂类型。
    /// </summary>
    public static class DbMigrationProviderFactory
    {
        public static DbMigrationProvider GetProvider(DbSetting dbSetting)
        {
            DbMigrationProvider provider = null;

            switch (dbSetting.ProviderName)
            {
                case DbConnectionSchema.Provider_SqlClient:
                    provider = new SqlServerMigrationProvider();
                    break;
                case DbConnectionSchema.Provider_SqlCe:
                    provider = new SqlServerCeMigrationProvider();
                    break;
                case DbConnectionSchema.Provider_MySql:
                    provider = new MySqlMigrationProvider();
                    break;
                case DbConnectionSchema.Provider_SQLite:
                    provider = new SQLiteMigrationProvider();
                    break;
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

        public static DbIdentifierQuoter GetIdentifierProvider(string providerName)
        {
            switch (providerName)
            {
                case DbConnectionSchema.Provider_SqlClient:
                case DbConnectionSchema.Provider_SqlCe:
                    return SqlServerIdentifierQuoter.Instance;
                case DbConnectionSchema.Provider_MySql:
                case DbConnectionSchema.Provider_SQLite:
                    return MySqlIdentifierQuoter.Instance;
                case DbConnectionSchema.Provider_Pgsql:
                    return PgSqlIdentifierQuoter.Instance;
                case DbConnectionSchema.Provider_MongoDb:
                    return EmptyIdentifierQuoter.Instance;
                default:
                    if (DbConnectionSchema.IsOracleProvider(providerName))
                    {
                        return OracleIdentifierQuoter.Instance;
                    }
                    throw new NotSupportedException("This type of database is not supportted now:" + providerName);
            }
        }

        public static DbTypeConverter GetDbTypeConverter(string providerName)
        {
            switch (providerName)
            {
                case DbConnectionSchema.Provider_SqlClient:
                    return SqlServerDbTypeConverter.Instance;
                case DbConnectionSchema.Provider_Pgsql:
                    return PgSqlDbTypeConverter.Instance;
                case DbConnectionSchema.Provider_SqlCe:
                    return SqlServerCeDbTypeConverter.Instance;
                case DbConnectionSchema.Provider_MySql:
                    return MySqlDbTypeConverter.Instance;
                case DbConnectionSchema.Provider_SQLite:
                    return SQLiteDbTypeConverter.Instance;
                case DbConnectionSchema.Provider_MongoDb:
                    return MongoDbDbTypeConverter.Instance;
                default:
                    if (DbConnectionSchema.IsOracleProvider(providerName))
                    {
                        return OracleDbTypeConverter.Instance;
                    }
                    throw new NotSupportedException("This type of database is not supportted now:" + providerName);
            }
        }
    }
}