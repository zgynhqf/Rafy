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
using Rafy.MetaModel;

namespace Rafy.Domain.ORM
{
    internal static class PersistanceTableInfoFactory
    {
        /// <summary>
        /// 为某个指定的仓库对象构造一个 DbTable
        /// </summary>
        /// <param name="repo"></param>
        /// <returns></returns>
        internal static IPersistanceTableInfo CreateTableInfo(IRepositoryInternal repo)
        {
            var em = repo.EntityMeta;
            if (em.TableMeta == null)
            {
                throw new ORMException(string.Format("类型 {0} 没有映射数据库，无法为其创造 ORM 运行时对象。", em.EntityType.FullName));
            }

            var res = new PersistanceTableInfo(repo);

            ProcessManagedProperties(em.EntityType, res, em);

            return res;
        }

        private static void ProcessManagedProperties(Type type, PersistanceTableInfo table, EntityMeta em)
        {
            foreach (var property in em.EntityProperties)
            {
                var meta = property.ColumnMeta;
                if (meta == null) continue;

                var propertyName = property.Name;

                //生成 ManagedPropertyBridge
                var epm = em.Property(propertyName);
                if (epm == null) { throw new ArgumentNullException(string.Format("{0}.{1} 属性需要使用托管属性进行编写。", type.FullName, propertyName)); }


                var column = new PersistanceColumnInfo(epm, meta, table);

                if (meta.IsPrimaryKey)
                {
                    table.PKColumn = column;
                }

                table.Columns.Add(column);
            }
        }
    }
}
