/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20231206
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.Net Standard 2.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20231206 19:45
 * 
*******************************************************/

using Rafy.Domain.ORM.Query.Impl;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;

namespace Rafy.Domain.ORM.Query
{
    /// <summary>
    /// <see cref="ColumnNode"/> 列的缓存懒加载
    /// <see cref="TableSource"/> 在构造时，不必立刻为所有属性生成相应的列。必须使用懒加载。
    /// </summary>
    internal class ColumnNodeCache
    {
        public static readonly ColumnNodeCache Instance = new ColumnNodeCache();

        private ColumnNodeCache() { }

        /// <summary>
        /// 所有表中的所有可用的列。
        /// 第一层 Key：Entity Name；
        /// 第一层 Key：Column Name + Alias；
        /// Value：ColumnNode
        /// </summary>
        private IDictionary<string, IDictionary<string, IColumnNode>> _columnsCache = new Dictionary<string, IDictionary<string, IColumnNode>>();

        /// <summary>
        /// 通过缓存获取指定表、指定属性、指定别名对应的列。
        /// 如果没找到，则创建一个新的并缓存。
        /// </summary>
        /// <param name="tableSource"></param>
        /// <param name="property"></param>
        /// <param name="aliasNeed"></param>
        /// <param name="throwIfNotFound"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public IColumnNode Get(TableSource tableSource, IManagedProperty property, string aliasNeed = null, bool throwIfNotFound = true)
        {
            //如果表、列都没有别名的情况下，不再生成额外的对象，直接使用元数据对象。
            var tryUseMeta = tableSource.Alias == null;

            var columns = tableSource.TableInfo.Columns;
            for (int i = 0, c = columns.Count; i < c; i++)
            {
                var column = columns[i];
                if (column.Property == property)
                {
                    if (tryUseMeta && aliasNeed == null) return column;

                    //如果别名不同，也需要生成一个新的 Column，这时，只存储最近一个使用 Column。
                    var entityType = tableSource.EntityRepository.EntityType;
                    var tableKey = entityType.FullName + ":" + tableSource.Alias;
                    if (!_columnsCache.TryGetValue(tableKey, out var columnsDic))
                    {
                        lock (_columnsCache)
                        {
                            if (!_columnsCache.TryGetValue(tableKey, out columnsDic))
                            {
                                columnsDic = new Dictionary<string, IColumnNode>();
                                _columnsCache.Add(tableKey, columnsDic);
                            }
                        }
                    }

                    var columnKey = column.Name + ":" + aliasNeed;
                    if (!columnsDic.TryGetValue(columnKey, out var node))
                    {
                        lock (columnsDic)
                        {
                            if (!columnsDic.TryGetValue(columnKey, out node))
                            {
                                node = NewColumn(tableSource, column, aliasNeed);
                                columnsDic.Add(columnKey, node);
                            }
                        }
                    }

                    if (node != null) return node;
                }
            }

            //如有列都遍历完成，没有找到与 property 对应的列，则返回 null。
            if (throwIfNotFound)
            {
                throw new InvalidOperationException($"没有在实体数据源 {tableSource.Alias}({tableSource.TableName}) 中找到 {property.Name} 属性对应的列。");
            }

            return null;
        }

        private static IColumnNode NewColumn(TableSource table, IRdbColumnInfo dbColumn, string alias)
        {
            var res = new ColumnNode();
            res.Table = table;
            res.Property = dbColumn.Property;
            res.ColumnName = dbColumn.Name;
            res.DbType = dbColumn.DbType;
            res.HasIndex = dbColumn.Meta.HasIndex;
            res.Alias = alias;
            return res;
        }
    }
}