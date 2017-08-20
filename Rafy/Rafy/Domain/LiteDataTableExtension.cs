/*******************************************************
 * 
 * 作者：胡康
 * 创建日期：20170729
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡康 20170729 17:02
 * 
*******************************************************/

using Rafy.Data;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rafy.Domain
{
    /// <summary>
    /// 对 LiteDataTable 类型的扩展。
    /// </summary>
    public static class LiteDataTableExtension
    {
        /// <summary>
        /// LiteDataTable 类型的扩展方法，实现 LiteDataTable 类型到 EntityList 类型的转换。
        /// </summary>
        /// <typeparam name="TEntityList"></typeparam>
        /// <param name="liteDataTable"></param>
        /// <param name="columnMapToProperty">
        /// 此参数表示表格中的列名是否直接映射实体的属性名。
        /// 如果传入 false，表示表格中的列名映射的是实体对应的数据库表的列名，而非属性名。
        /// 默认为 true。
        /// 。</param>
        /// <returns></returns>
        public static TEntityList ToEntityList<TEntityList>(this LiteDataTable liteDataTable, bool columnMapToProperty = true) where TEntityList : EntityList
        {
            var entityMatrix = EntityMatrix.FindByList(typeof(TEntityList));
            var repo = RepositoryFacade.Find(entityMatrix.EntityType);
            var entity = repo.New();
            var entityMeta = repo.EntityMeta;

            //初始化 liteDataTable 中所有列名的集合 tableColumnsNameList。
            var columns = liteDataTable.Columns;
            var tableColumnsNameList = new List<string>();
            for (int i = 0; i < columns.Count; i++)
            {
                tableColumnsNameList.Add(columns[i].ColumnName);
            }

            var resultList = repo.NewList();
            if (!columnMapToProperty)
            {
                var pMetaList = new List<EntityPropertyMeta>();
                var entityPropertyMetaList = entityMeta.EntityProperties;

                //初始化 liteDateTable 能够映射到实体属性列名集合 pMetaList，
                //并检验 liteDataTable 是否能转换为相应的 entitylist。
                for (int i = 0; i < entityPropertyMetaList.Count; i++)
                {
                    var propertyMeta = entityPropertyMetaList[i];
                    var manageProperty = propertyMeta.ManagedProperty;
                    if (!manageProperty.IsReadOnly)
                    {
                        var columnMeta = propertyMeta.ColumnMeta;

                        //这个位置是针对于时间戳的那几个字段 columnMeta 不为空 ，但是映射到数据库的 columnName 这个属性是空的。
                        //还有种情况是 columnMeta 为空，比如当对应的属性为 treeIndex。
                        var columnName = columnMeta == null ? propertyMeta.Name : columnMeta.ColumnName ?? propertyMeta.Name;

                        for (int j = 0; j < tableColumnsNameList.Count; j++)
                        {
                            if (tableColumnsNameList.Contains(columnName))
                            {
                                pMetaList.Add(propertyMeta);
                            }
                        }
                    }
                    //如果 liteDataTable 的列集合和 entitylist 映射在数据库中的列（以及没映射属性）集合没有任何交集那么就不能转换。
                    if (pMetaList.Count == 0)
                    {
                        throw new NotSupportedException("liteDataTable 的列集合和 entitylist 映射在数据库中的列（以及没映射属性）集合没有任何交集");
                    }
                }

                // 通过 liteDataTable 填充 entitylist
                for (int i = 0; i < liteDataTable.Rows.Count; i++)
                {
                    var row = liteDataTable.Rows[i];
                    var entityItem = repo.New();
                    for (int j = 0; j < pMetaList.Count; j++)
                    {
                        var metaItem = pMetaList[j];
                        var manageProperty = metaItem.ManagedProperty;
                        var columnMeta = metaItem.ColumnMeta;
                        var propertyName = string.Empty;
                        propertyName = columnMeta == null ? metaItem.Name : columnMeta.ColumnName ?? metaItem.Name;
                        var rowValue = row[propertyName];
                        entityItem.LoadProperty(manageProperty, rowValue);
                    }
                    resultList.Add(entityItem);
                }
                return resultList as TEntityList;
            }

            //初始化 liteDateTable 能够映射到实体属性列名集合 pMetaList，
            //并检验 liteDataTable 是否能转换为相应的 entitylist。
            var propertyList = entity.PropertiesContainer.GetCompiledProperties();
            var availablePropertyList = new List<IManagedProperty>();
            for (int i = 0; i < propertyList.Count; i++)
            {
                var property = propertyList[i];
                var entityPropertyName = property.Name;
                if (tableColumnsNameList.Contains(entityPropertyName) && (!property.IsReadOnly))
                {
                    availablePropertyList.Add(property);
                }
            }
            if (availablePropertyList.Count == 0)
            {
                throw new NotSupportedException("liteDataTable 的列集合和 entitylist 映射在数据库中的列（以及没映射属性）集合没有任何交集");
            }

            // 通过 liteDataTable 填充 entitylist
            for (int i = 0; i < liteDataTable.Rows.Count; i++)
            {
                var entityItem = repo.New();
                var row = liteDataTable.Rows[i];
                for (int j = 0; j < availablePropertyList.Count; j++)
                {
                    var item = availablePropertyList[j];
                    var propertyName = item.Name;
                    var rowValue = row[propertyName];
                    entityItem.LoadProperty(item, rowValue);
                }
                resultList.Add(entityItem);
            }
            return resultList as TEntityList;
        }
    }
}
