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
using System.Reflection;
using System.Data;

namespace Rafy.Domain
{
    /// <summary>
    /// 对 LiteDataTable 类型的扩展。
    /// </summary>
    public static class LiteDataTableExtension
    {
        /// <summary>
        /// 将 LiteDataTable 转换为指定的 POCO 类型的实例列表。
        /// </summary>
        /// <typeparam name="T">需要转换的类型。</typeparam>
        /// <param name="table"></param>
        /// <returns></returns>
        public static List<T> ToPOCO<T>(this DataTable table)
        {
            //注意！！！
            //本方法与 ToPOCO<T>(this LiteDataTable table) 方法一模一样。
            //无法提取公共代码，修改者注意同时修改两处。

            //找到实体中与该表格有映射的所有属性。
            var propertyMappings = new List<CLRPropertyMapping>(10);
            var entityType = typeof(T);
            var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty);
            var columns = table.Columns;
            for (int i = 0, c = properties.Length; i < c; i++)
            {
                var propertyInfo = properties[i];
                if (!propertyInfo.CanWrite) continue;

                var propertyName = propertyInfo.Name;

                for (int columnIndex = 0, c2 = columns.Count; columnIndex < c2; columnIndex++)
                {
                    var column = columns[columnIndex];
                    if (column.ColumnName.EqualsIgnoreCase(propertyName))
                    {
                        propertyMappings.Add(new CLRPropertyMapping
                        {
                            Property = propertyInfo,
                            ColumnIndex = columnIndex
                        });

                        break;
                    }
                }
            }

            var list = new List<T>(10);

            //将每一行转换为实体，加入到列表中。
            var rows = table.Rows;
            for (int i = 0, c = rows.Count; i < c; i++)
            {
                var row = rows[i];
                var dto = Activator.CreateInstance<T>();

                for (int j = 0, c2 = propertyMappings.Count; j < c2; j++)
                {
                    var mapping = propertyMappings[j];

                    var value = row[mapping.ColumnIndex];

                    mapping.Property.SetValue(dto, value);
                }

                list.Add(dto);
            }

            return list;
        }

        /// <summary>
        /// 将 LiteDataTable 转换为指定的 POCO 类型的实例列表。
        /// </summary>
        /// <typeparam name="T">需要转换的类型。</typeparam>
        /// <param name="table"></param>
        /// <returns></returns>
        public static List<T> ToPOCO<T>(this LiteDataTable table)
        {
            //注意！！！
            //本方法与 ToPOCO<T>(this DataTable table) 方法一模一样。
            //无法提取公共代码，修改者注意同时修改两处。

            //找到实体中与该表格有映射的所有属性。
            var propertyMappings = new List<CLRPropertyMapping>(10);
            var entityType = typeof(T);
            var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty);
            var columns = table.Columns;
            for (int i = 0, c = properties.Length; i < c; i++)
            {
                var propertyInfo = properties[i];
                if (!propertyInfo.CanWrite) continue;

                var propertyName = propertyInfo.Name;

                for (int columnIndex = 0, c2 = columns.Count; columnIndex < c2; columnIndex++)
                {
                    var column = columns[columnIndex];
                    if (column.ColumnName.EqualsIgnoreCase(propertyName))
                    {
                        propertyMappings.Add(new CLRPropertyMapping
                        {
                            Property = propertyInfo,
                            ColumnIndex = columnIndex
                        });

                        break;
                    }
                }
            }

            var list = new List<T>(10);

            //将每一行转换为实体，加入到列表中。
            var rows = table.Rows;
            for (int i = 0, c = rows.Count; i < c; i++)
            {
                var row = rows[i];
                var dto = Activator.CreateInstance<T>();

                for (int j = 0, c2 = propertyMappings.Count; j < c2; j++)
                {
                    var mapping = propertyMappings[j];

                    var value = row[mapping.ColumnIndex];

                    mapping.Property.SetValue(dto, value);
                }

                list.Add(dto);
            }

            return list;
        }

        /// <summary>
        /// LiteDataTable 类型的扩展方法，实现 LiteDataTable 类型到 EntityList 类型的转换。
        /// </summary>
        /// <typeparam name="TEntityList">The type of the entity list.</typeparam>
        /// <param name="table">The table.</param>
        /// <param name="columnMapToProperty">
        /// 此参数表示表格中的列名是否直接映射实体的属性名。
        /// true：表格列名就是属性名。
        /// false：表格中列名映射的是实体对应的数据库表的列名，而非属性名。
        /// 默认为 true。
        /// </param>
        /// <returns></returns>
        public static TEntityList ToEntityList<TEntityList>(this LiteDataTable table, bool columnMapToProperty = true) where TEntityList : class, IEntityList
        {
            //属性和对应列的键值对集合,为后面填充实体用。
            var propertyMappings = new List<PropertyToColumnMapping>(10);
            var entityMatrix = EntityMatrix.FindByList(typeof(TEntityList));
            var repo = RepositoryFacade.Find(entityMatrix.EntityType);
            var properties = repo.EntityMeta.EntityProperties;
            var columns = table.Columns;
            for (int i = 0, c = properties.Count; i < c; i++)
            {
                var propertyMeta = properties[i];
                var property = propertyMeta.ManagedProperty;
                if (!property.IsReadOnly)
                {
                    //这个位置是针对于时间戳的那几个字段 columnMeta 不为空 ，但是映射到数据库的 columnName 这个属性是空的。
                    //还有种情况是 columnMeta 为空，比如当对应的属性为 treeIndex。
                    var columnName = property.Name;
                    if (!columnMapToProperty)
                    {
                        var columnMeta = propertyMeta.ColumnMeta;
                        columnName = columnMeta == null ? propertyMeta.Name : columnMeta.ColumnName;
                    }

                    for (int columnIndex = 0, c2 = columns.Count; columnIndex < c2; columnIndex++)
                    {
                        var column = columns[columnIndex];
                        if (column.ColumnName.EqualsIgnoreCase(columnName))
                        {
                            propertyMappings.Add(new PropertyToColumnMapping
                            {
                                Property = property,
                                ColumnName = columnName,
                                ColumnIndex = columnIndex
                            });

                            break;
                        }
                    }
                }
            }

            return ConvertEntitiesIntoList(table, repo, propertyMappings, columnMapToProperty) as TEntityList;
        }

        /// <summary>
        /// 填充实体
        /// </summary>
        /// <param name="table">需要转换实体的 LiteDataTable</param>
        /// <param name="repo">实体的仓储</param>
        /// <param name="propertyToColumnMappings">属性和对应列的键值对集合</param>
        /// <param name="columnMapToProperty">
        /// 此参数表示表格中的列的数据类型和是否直接映射实体的属性数据类型。
        /// 如果传入 false，表示表格中的列数据类型和实体属性数据类型不一致。
        /// </param>
        /// <returns></returns>
        private static IEntityList ConvertEntitiesIntoList(
            LiteDataTable table,
            EntityRepository repo,
            IList<PropertyToColumnMapping> propertyToColumnMappings,
            bool columnMapToProperty = true
            )
        {
            var resultList = repo.NewList();

            var rdbDataProvider = ORM.RdbDataProvider.Get(repo);
            var columns = rdbDataProvider.DbTable.Columns;

            for (int i = 0, c = table.Rows.Count; i < c; i++)
            {
                var row = table.Rows[i];

                var entityItem = repo.New();

                for (int j = 0, c2 = propertyToColumnMappings.Count; j < c2; j++)
                {
                    var mapping = propertyToColumnMappings[j];
                    var rowValue = row[mapping.ColumnIndex];
                    if (!columnMapToProperty)
                    {
                        for (int z = 0, z2 = columns.Count; z < z2; z++)
                        {
                            var column = columns[z];
                            //通过列名称相等匹配找到 rdbcolumn。
                            if (column.Name.EqualsIgnoreCase(mapping.ColumnName))
                            {
                                column.WritePropertyValue(entityItem, rowValue);
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
            public int ColumnIndex;
        }

        /// <summary>
        /// 属性名和与之对应的列名。
        /// </summary>
        private struct CLRPropertyMapping
        {
            public PropertyInfo Property;
            public int ColumnIndex;
        }
    }
}
