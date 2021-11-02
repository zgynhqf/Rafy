/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20150314
 * 运行环境：.NET 4.5
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20150314 14:54
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rafy.DbMigration;
using Rafy.MetaModel;

namespace Rafy.Domain.ORM
{
    internal static class RdbTableInfoFactory
    {
        internal static IRdbTableInfo CreateTableInfo(EntityMeta em, string dbProvider)
        {
            if (em.TableMeta == null)
            {
                throw new ORMException(string.Format("类型 {0} 没有映射数据库，无法为其创造 ORM 运行时对象。", em.EntityType.FullName));
            }

            var identifierProvider = DbMigrationProviderFactory.GetIdentifierProvider(dbProvider);
            var dbTypeConverter = DbMigrationProviderFactory.GetDbTypeConverter(dbProvider);

            var name = identifierProvider.Prepare(em.TableMeta.TableName);
            var res = new RdbTableInfo(name, em.EntityType);

            ProcessManagedProperties(res, em, identifierProvider, dbTypeConverter);

            return res;
        }

        private static void ProcessManagedProperties(
            RdbTableInfo table,
            EntityMeta em,
            IDbIdentifierQuoter identifierProvider,
            DbTypeConverter dbTypeConverter
            )
        {
            foreach (var property in em.EntityProperties)
            {
                var columnMeta = property.ColumnMeta;
                if (columnMeta == null) continue;

                var propertyName = property.Name;

                var epm = em.Property(propertyName);
                if (epm == null) { throw new ArgumentNullException(string.Format("{0}.{1} 属性需要使用托管属性进行编写。", em.EntityType.FullName, propertyName)); }

                var columnName = identifierProvider.Prepare(columnMeta.ColumnName);

                var dbType = columnMeta.DbType ?? dbTypeConverter.FromClrType(epm.PropertyType);
                var column = new RdbColumnInfo(columnName, epm, columnMeta, table, dbType);

                if (columnMeta.IsPrimaryKey)
                {
                    table.PKColumn = column;
                }

                table.Columns.Add(column);
            }
        }
    }
}
