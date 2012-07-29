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
using hxy.Common.Data;
using OEA.Library;
using OEA.ManagedProperty;
using OEA.MetaModel;
using OEA.MetaModel.Attributes;
using OEA.ORM;
using OEA.ORM.Oracle;
using OEA.ORM.SqlServer;
using OEA.Utils;

namespace OEA.ORM
{
    internal static class DbTableFactory
    {
        /// <summary>
        /// 为某个指定的仓库对象构造一个 DbTable
        /// </summary>
        /// <param name="repo"></param>
        /// <returns></returns>
        internal static DbTable CreateORMTable(IRepository repo)
        {
            var em = repo.EntityMeta;
            if (em == null || em.TableMeta == null)
            {
                throw new ORMException("该类型没有映射数据库： " + em.EntityType.FullName);
            }

            DbTable res = null;

            switch (repo.DbSetting.ProviderName)
            {
                case DbSetting.Provider_SqlClient:
                case DbSetting.Provider_SqlCe:
                    res = new SqlTable(em);
                    break;
                case DbSetting.Provider_Oracle:
                    res = new OracleTable(em);
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

                //ManagedPropertyBridge
                var epm = em.Property(propertyName);
                if (epm == null) { throw new ArgumentNullException(string.Format("{0}.{1} 属性需要使用托管属性进行编写。", type.FullName, propertyName)); }
                var mp = epm.ManagedProperty;
                var bridge = table.CreateBridge(mp);

                //列名
                var columnName = meta.ColumnName;
                if (string.IsNullOrWhiteSpace(columnName)) columnName = propertyName;

                //属性名
                var mainProperty = propertyName;
                var refProperty = string.Empty;
                var refMeta = mp.GetMeta(type) as IRefPropertyMetadata;
                if (refMeta != null)
                {
                    mainProperty = refMeta.IdProperty;
                    refProperty = mp.Name;
                }

                table.Add(new DbColumn(table, columnName, bridge)
                {
                    PropertyName = mainProperty,
                    RefPropertyName = refProperty,
                    IsPKID = meta.IsPKID
                });
            }
        }
    }
}