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
                IList<EntityPropertyMeta> entityPropertyMetaList = entityMeta.EntityProperties;

                #region 判断 liteDataTable 类型能否转换成 EntityList 类型

                List<string> entitycolumnNameList = new List<string>();
                for (int i = 0; i < entityPropertyMetaList.Count; i++)
                {
                    var columnName = entityPropertyMetaList[i].ColumnMeta.ColumnName;
                    entitycolumnNameList.Add(columnName);
                }

                for (int i = 0; i < entitycolumnNameList.Count; i++)
                {
                    if (!liteDataTableColumnsNameList.Contains(entitycolumnNameList[i]))
                    {
                        throw new NotSupportedException("需要转换的 LiteDataTable 不包含 " + entitycolumnNameList[i] + " 列，所以不能转换");
                    }
                }

                #endregion

                foreach (var row in liteDataTable.Rows)
                {
                    var entityItem = repo.New();
                    foreach (var metaProperty in entityPropertyMetaList)
                    {
                        var manageProperty = metaProperty.ManagedProperty;
                        var propertyName = metaProperty.ColumnMeta.ColumnName;
                        entityItem.LoadProperty(manageProperty, row[propertyName]);
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
    }
}
