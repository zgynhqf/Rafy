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
using OEA.Library;
using OEA.ManagedProperty;
using OEA.MetaModel;
using OEA.MetaModel.Attributes;
using OEA.ORM;
using OEA.ORM.sqlserver;
using OEA.Utils;


namespace OEA.ORM
{
    public static class ORMMetaExtension
    {
        public static SqlTable GetORMTable(this EntityMeta em)
        {
            if (em == null || em.TableMeta == null)
            {
                throw new LightException("no TableAttribute or found on " + em.EntityType.FullName);
            }

            var res = em.TableMeta.ORMRuntime as SqlTable;
            if (res == null)
            {
                res = new SqlTable(em, em.TableMeta.TableName);
                ProcessManagedProperties(em.EntityType, res, em);

                em.TableMeta.ORMRuntime = res;
            }

            return res;
        }

        private static void ProcessManagedProperties(Type type, SqlTable table, EntityMeta em)
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
                var bridge = new ManagedPropertyBridge(mp);

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

                table.Add(new SqlColumn(table, columnName, bridge)
                {
                    PropertyName = mainProperty,
                    RefPropertyName = refProperty,
                    IsPK = meta.IsPK,
                    IsID = meta.IsPK && property.Runtime.PropertyType == typeof(int)
                });
            }
        }
    }
}