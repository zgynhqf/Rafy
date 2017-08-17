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
using Rafy.MetaModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rafy.Domain
{
    /// <summary>
    /// 对 LiteDataTableExtension 类型的扩展
    /// </summary>
    public static class LiteDataTableExtension
    {
        /// <summary>
        /// LiteDataTableExtension 的扩展方法，实现 LiteDataTableExtension 类型到 EntityList 类型的转换
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
            var resultList = repo.NewList();
            var entityMeta = repo.EntityMeta;
            var columns = liteDataTable.Columns;
            var liteDataTableColumnsNameList = new List<string>();//liteDataTable 中所有列名的集合
            for (int i = 0; i < columns.Count; i++)
            {
                liteDataTableColumnsNameList.Add(columns[i].ColumnName);
            }

            if (!columnMapToProperty)
            {
                List<EntityPropertyMeta> pMetaList = new List<EntityPropertyMeta>();
                IList<EntityPropertyMeta> entityPropertyMetaList = entityMeta.EntityProperties;

                #region 判断 liteDataTable 类型能否转换成 EntityList 类型

                List<string> entitycolumnNameList = new List<string>();
                for (int i = 0; i < entityPropertyMetaList.Count; i++)
                {
                    var propertyMeta = entityPropertyMetaList[i];
                    var columnMeta = propertyMeta.ColumnMeta;

                    var columnName = "";

                    // 这个位置有时候会存在 columnMeta 为空的情况
                    // 很多实体会默认继承 treeIndex  treePId  isphantom  
                    // 而且这些属性没有映射到数据库，所以这个时候取 columnMeta 就是空的
                    // 这个时候如果要转换我就不对这3个属性处理
                    if (columnMeta != null)
                    {
                        //这个位置是针对于时间戳的那几个字段 columnMeta 不为空 但是 映射到数据库的 columnName 这个属性是空的
                        if (columnMeta.ColumnName == null)
                        {
                            columnName = propertyMeta.Name;
                        }
                        else
                        {
                            columnName = columnMeta.ColumnName;
                        }
                        entitycolumnNameList.Add(columnName);
                        pMetaList.Add(propertyMeta);
                    }
                }

                for (int i = 0; i < entitycolumnNameList.Count; i++)
                {
                    var entityColumnName = entitycolumnNameList[i];
                    if (!liteDataTableColumnsNameList.Contains(entityColumnName) && (!LiteDataTableExtension.IsInheritProperty(entityColumnName)))
                    {
                        throw new NotSupportedException("需要转换的 LiteDataTable 不包含 " + entitycolumnNameList[i] + " 列，所以不能转换");
                    }
                }

                #endregion

                foreach (var row in liteDataTable.Rows)
                {
                    var entityItem = repo.New();
                    foreach (var metaProperty in pMetaList)
                    {
                        var manageProperty = metaProperty.ManagedProperty;
                        var propertyName = metaProperty.ColumnMeta.ColumnName;
                        if (propertyName == null)
                        {
                            propertyName = metaProperty.Name;
                        }
                        var rowValue = row[propertyName];
                        entityItem.LoadProperty(manageProperty, rowValue);
                    }
                    resultList.Add(entityItem);
                }
                return resultList as TEntityList;
            }
            else
            {
                #region 判断 liteDataTable 类型能否转换成 EntityList 类型

                var propertyList = entity.PropertiesContainer.GetCompiledProperties();

                for (int i = 0; i < propertyList.Count; i++)
                {
                    var entityPropertyName = propertyList[i].Name;

                    if (!liteDataTableColumnsNameList.Contains(entityPropertyName))
                    {
                        throw new NotSupportedException("需要转换的 LiteDataTable 不包含 " + entityPropertyName + " 列，所以不能转换");
                    }
                }

                #endregion

                foreach (var row in liteDataTable.Rows)
                {
                    var entityItem = repo.New();
                    foreach (var property in propertyList)
                    {
                        var propertyName = property.Name;
                        var rowValue = row[propertyName];
                        entityItem.LoadProperty(property, rowValue);
                    }
                    resultList.Add(entityItem);
                }
                return resultList as TEntityList;
            }
        }

        /// <summary>
        /// 判断属性是否为继承属性 treeIndex  treePId  isphantom
        /// 很多实体会默认继承 treeIndex  treePId  isphantom 
        /// 而且这些属性没有映射到数据库
        /// 所以当类型转换的时候 ，就不需要判断这几个属性
        /// </summary>
        /// <returns></returns>
        public static bool IsInheritProperty(string propertyName)
        {
            if (propertyName == "IsPhantom" || propertyName == "TreeIndex" || propertyName == "TreePId")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
