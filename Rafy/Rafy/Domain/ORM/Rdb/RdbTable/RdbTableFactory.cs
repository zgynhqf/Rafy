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

namespace Rafy.Domain.ORM
{
    /// <summary>
    /// 获取指定类型的数据表对象的工厂类
    /// </summary>
    internal static class RdbTableFactory
    {
        /// <summary>
        /// 为某个指定的仓库对象构造一个 DbTable
        /// </summary>
        /// <param name="repo"></param>
        /// <returns></returns>
        internal static RdbTable CreateORMTable(IRepositoryInternal repo)
        {
            RdbTable table = null;

            var provider = RdbDataProvider.Get(repo).DbSetting.ProviderName;
            switch (provider)
            {
                case DbSetting.Provider_SqlClient:
                    table = new SqlServerTable(repo);
                    break;
                case DbSetting.Provider_SqlCe:
                    table = new SqlCeTable(repo);
                    break;
                case DbSetting.Provider_MySql:
                    table = new MySqlTable(repo);
                    break;
                default:
                    if (DbConnectionSchema.IsOracleProvider(provider))
                    {
                        table = new OracleTable(repo);
                        break;
                    }
                    throw new NotSupportedException();
            }

            var em = repo.EntityMeta;
            foreach (var columnInfo in table.Info.Columns)
            {
                //生成 ManagedPropertyBridge
                var epm = em.Property(columnInfo.Property);
                if (epm == null) { throw new ArgumentNullException(string.Format("{0}.{1} 属性需要使用托管属性进行编写。", table.Info.Class.FullName, columnInfo.Property.Name)); }

                var column = table.CreateColumn(columnInfo);
                table.Add(column);
            }

            return table;
        }
    }
}