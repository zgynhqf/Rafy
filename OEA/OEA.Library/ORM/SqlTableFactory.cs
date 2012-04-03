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
using OEA.Core;

namespace OEA.ORM
{
    public class SqlTableFactory
    {
        #region SingleTon

        public static readonly SqlTableFactory Instance = new SqlTableFactory();

        private SqlTableFactory() { }

        #endregion

        #region 外部接口

        private Dictionary<Type, SqlTable> _cache = new Dictionary<Type, SqlTable>(50);

        public SqlTable Build(Type type)
        {
            SqlTable table = null;

            if (!this._cache.TryGetValue(type, out table))
            {
                lock (this._cache)
                {
                    if (!this._cache.TryGetValue(type, out table))
                    {
                        table = BuildTable(type);
                        this._cache.Add(type, table);
                    }
                }
            }

            return table;
        }

        private SqlTable BuildTable(Type type)
        {
            var em = CommonModel.Entities.Get(type);
            if (em == null || em.TableMeta == null)
            {
                throw new LightException("no TableAttribute or found on " + type.FullName);
            }

            var table = new SqlTable(type, em.TableMeta.TableName, null);

            this.ProcessManagedProperties(type, table, em);

            return table;
        }

        #endregion

        private void ProcessManagedProperties(Type type, SqlTable table, EntityMeta em)
        {
            var propertyContainer = ManagedPropertyRepository.Instance.GetTypePropertiesContainer(type);
            var managedProperties = propertyContainer.GetNonReadOnlyCompiledProperties();

            foreach (var property in em.EntityProperties)
            {
                var meta = property.ColumnMeta;
                if (meta == null) continue;

                var propertyName = property.Name;

                //ManagedPropertyBridge
                var runtime = managedProperties.FirstOrDefault(p => p.GetMetaPropertyName(type) == propertyName);
                if (runtime == null) { throw new ArgumentNullException(string.Format("{0}.{1} 属性需要使用托管属性进行编写。", type.FullName, propertyName)); }
                var bridge = new ManagedPropertyBridge(runtime);

                //列名
                var columnName = meta.ColumnName;
                if (string.IsNullOrWhiteSpace(columnName)) columnName = propertyName;

                //属性名
                var mainProperty = propertyName;
                var refProperty = string.Empty;
                var refMeta = runtime.GetMeta(type) as IRefPropertyMetadata;
                if (refMeta != null)
                {
                    mainProperty = refMeta.IdProperty;
                    refProperty = runtime.Name;
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
