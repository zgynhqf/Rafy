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

namespace Rafy.Domain.ORM
{
    internal static class DbTableFactory
    {
        /// <summary>
        /// 为某个指定的仓库对象构造一个 DbTable
        /// </summary>
        /// <param name="repo"></param>
        /// <returns></returns>
        internal static DbTable CreateORMTable(IRepositoryInternal repo)
        {
            var em = repo.EntityMeta;
            if (em.TableMeta == null)
            {
                throw new ORMException(string.Format("类型 {0} 没有映射数据库，无法为其创造 ORM 运行时对象。", em.EntityType.FullName));
            }

            DbTable res = null;

            switch (repo.RdbDataProvider.DbSetting.ProviderName)
            {
                case DbSetting.Provider_SqlClient:
                    res = new SqlTable(repo);
                    break;
                case DbSetting.Provider_SqlCe:
                    res = new SqlCeTable(repo);
                    break;
                case DbSetting.Provider_Oracle:
                    res = new OracleTable(repo);
                    break;
                default:
                    throw new NotSupportedException();
            }

            ProcessManagedProperties(em.EntityType, res, em);

            return res;
        }

        private static void ProcessManagedProperties(Type type, DbTable table, EntityMeta em)
        {
            foreach (var property in em.EntityProperties)
            {
                var meta = property.ColumnMeta;
                if (meta == null) continue;

                var propertyName = property.Name;

                //生成 ManagedPropertyBridge
                var epm = em.Property(propertyName);
                if (epm == null) { throw new ArgumentNullException(string.Format("{0}.{1} 属性需要使用托管属性进行编写。", type.FullName, propertyName)); }
                var mp = epm.ManagedProperty as IProperty;

                //列名
                var columnName = meta.ColumnName;
                if (string.IsNullOrWhiteSpace(columnName)) columnName = propertyName;

                var column = table.CreateColumn(columnName, epm);
                column.IsIdentity = meta.IsIdentity;
                column.IsPrimaryKey = meta.IsPrimaryKey;
                table.Add(column);
            }
        }
    }
}