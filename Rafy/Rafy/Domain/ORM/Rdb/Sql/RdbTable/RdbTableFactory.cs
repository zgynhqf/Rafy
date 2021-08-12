/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20111110
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20111110
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rafy.Data;
using Rafy.Domain;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.Domain.ORM;
using Rafy.Domain.ORM.Oracle;
using Rafy.Domain.ORM.SqlCe;
using Rafy.Domain.ORM.SqlServer;
using Rafy.Utils;
using Rafy.Domain.ORM.MySql;
using Rafy.DbMigration;
using Rafy.Domain.ORM.SQLite;

namespace Rafy.Domain.ORM
{
    /// <summary>
    /// 获取指定类型的数据表对象的工厂类
    /// </summary>
    internal class RdbTableFactory : IRdbTableFactory
    {
        public static IRdbTableFactory Current = new RdbTableFactory();

        /// <summary>
        /// 为某个指定的仓库对象构造一个 DbTable
        /// </summary>
        /// <param name="repo"></param>
        /// <returns></returns>
        internal static RdbTable CreateORMTable(IRepositoryInternal repo)
        {
            return Current.CreateRdbTable(repo);
        }

        public RdbTable CreateRdbTable(IRepositoryInternal repo)
        {
            RdbTable table = null;

            var provider = RdbDataProvider.Get(repo).DbSetting.ProviderName;
            table = this.CreateRdbTableCore(repo, provider);

            table.IdentifierProvider = DbMigrationProviderFactory.GetIdentifierProvider(provider);
            table.DbTypeConverter = DbMigrationProviderFactory.GetDbTypeConverter(provider);

            var em = repo.EntityMeta;
            foreach (var columnInfo in table.Info.Columns)
            {
                var epm = em.Property(columnInfo.Property);
                if (epm == null) { throw new ArgumentNullException(string.Format("{0}.{1} 属性需要使用托管属性进行编写。", table.Info.Class.FullName, columnInfo.Property.Name)); }

                var column = table.CreateColumn(columnInfo);

                table.Add(column);
            }

            return table;
        }

        protected virtual RdbTable CreateRdbTableCore(IRepositoryInternal repo, string provider)
        {
            switch (provider)
            {
                case DbSetting.Provider_SqlClient:
                    return new SqlServerTable(repo);
                case DbSetting.Provider_SqlCe:
                    return new SqlCeTable(repo);
                case DbSetting.Provider_SQLite:
                    return new SQLiteTable(repo);
                case DbSetting.Provider_MySql:
                    return new MySqlTable(repo);
                default:
                    if (DbConnectionSchema.IsOracleProvider(provider))
                    {
                        return new OracleTable(repo);
                    }
                    break;
            }

            throw new NotSupportedException();
        }
    }

    internal interface IRdbTableFactory
    {
        RdbTable CreateRdbTable(IRepositoryInternal repo);
    }
}