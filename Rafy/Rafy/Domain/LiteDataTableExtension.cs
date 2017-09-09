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
        /// </param>
        /// <returns></returns>
        public static TEntityList ToEntityList<TEntityList>(this LiteDataTable liteDataTable, bool columnMapToProperty = true) where TEntityList : EntityList
        {
            var entityMatrix = EntityMatrix.FindByList(typeof(TEntityList));
            var repo = RepositoryFacade.Find(entityMatrix.EntityType);

            //属性和对应列的键值对集合,为后面填充实体用。
            var propertyToColumnMappings = new List<PropertyToColumnMapping>(10);

            //初始化 liteDataTable 中所有列名的集合，为后面初始化 propertyToColumnMappings 和判断属性用 。
            var columns = liteDataTable.Columns;
            var tableColumnsNameList = new List<string>();
            for (int i = 0, c = columns.Count; i < c; i++)
            {
                tableColumnsNameList.Add(columns[i].ColumnName);
            }

            //当表格中的列名映射的是实体对应的数据库表的列名，而非属性名的转换方法。
            if (!columnMapToProperty)
            {
                var entityPropertyMetaList = repo.EntityMeta.EntityProperties;
                for (int i = 0, c = entityPropertyMetaList.Count; i < c; i++)
                {
                    var propertyMeta = entityPropertyMetaList[i];
                    var manageProperty = propertyMeta.ManagedProperty;
                    if (!manageProperty.IsReadOnly)
                    {
                        //这个位置是针对于时间戳的那几个字段 columnMeta 不为空 ，但是映射到数据库的 columnName 这个属性是空的。
                        //还有种情况是 columnMeta 为空，比如当对应的属性为 treeIndex。
                        var columnMeta = propertyMeta.ColumnMeta;
                        var columnName = columnMeta == null ? propertyMeta.Name : columnMeta.ColumnName ?? propertyMeta.Name;

                        for (int j = 0, c2 = tableColumnsNameList.Count; j < c2; j++)
                        {
                            if (tableColumnsNameList.Contains(columnName))
                            {
                                propertyToColumnMappings.Add(new PropertyToColumnMapping
                                {
                                    Property = manageProperty,
                                    ColumnName = columnName
                                });
                            }
                        }
                    }
                }
            }
            else
            {
                var propertyList = repo.EntityMeta.ManagedProperties.GetCompiledProperties();
                for (int i = 0, c = propertyList.Count; i < c; i++)
                {
                    var property = propertyList[i];
                    var propertyName = property.Name;
                    if (tableColumnsNameList.Contains(propertyName) && !property.IsReadOnly)
                    {
                        propertyToColumnMappings.Add(new PropertyToColumnMapping
                        {
                            Property = property,
                            ColumnName = propertyName
                        });
                    }
                }
            }

            return ConvertEntitiesIntoList(liteDataTable, repo, propertyToColumnMappings, columnMapToProperty) as TEntityList;
        }

        /// <summary>
        /// 填充实体
        /// </summary>
        /// <param name="liteDataTable">需要转换实体的 LiteDataTable</param>
        /// <param name="repo">实体的仓储</param>
        /// <param name="propertyToColumnMappings">属性和对应列的键值对集合</param>
        /// <param name="columnMapToProperty">
        /// 此参数表示表格中的列的数据类型和是否直接映射实体的属性数据类型。
        /// 如果传入 false，表示表格中的列数据类型和实体属性数据类型不一致。
        /// </param>
        /// <returns></returns>
        private static EntityList ConvertEntitiesIntoList(LiteDataTable liteDataTable, EntityRepository repo, IList<PropertyToColumnMapping> propertyToColumnMappings, bool columnMapToProperty = true)
        {
            var resultList = repo.NewList();

            var rdbDataProvider = ORM.RdbDataProvider.Get(repo);
            var columns = rdbDataProvider.DbTable.Columns;

            for (int i = 0, c = liteDataTable.Rows.Count; i < c; i++)
            {
                var row = liteDataTable.Rows[i];

                var entityItem = repo.New();
                for (int j = 0, c2 = propertyToColumnMappings.Count; j < c2; j++)
                {
                    var mapping = propertyToColumnMappings[j];
                    var rowValue = row[mapping.ColumnName];
                    if (!columnMapToProperty)
                    {
                        for (int z = 0, z2 = columns.Count; z < z2; z++)
                        {
                            var column = columns[z];
                            //通过列名称相等匹配找到 rdbcolumn。
                            if (column.Name == mapping.ColumnName)
                            {
                                column.LoadValue(entityItem, rowValue);
                                break;
                            }
                        }
                    }
                    else
                    {
                        entityItem.LoadProperty(mapping.Property, rowValue);
                    }
                }

                resultList.Add(entityItem);
            }

            return resultList;
        }

        /// <summary>
        /// 属性名和与之对应的列名。
        /// </summary>
        private struct PropertyToColumnMapping
        {
            public IManagedProperty Property;
            public string ColumnName;
        }
    }
}
