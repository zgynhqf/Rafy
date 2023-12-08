/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20231208
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.Net Standard 2.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20231208 13:20
 * 
*******************************************************/

using Rafy.Domain.ORM.Query;
using Rafy.ManagedProperty;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.Domain.ORM
{
    internal static class JoinAutoGenerator
    {
        /// <summary>
        /// 自动处理 query.From 中的 Join。
        /// </summary>
        /// <param name="query"></param>
        /// <param name="fromJoinColumns">所有可用的 FromJoin 表。</param>
        internal static void AutoJoin(IQuery query, List<IRdbColumnInfo> fromJoinColumns)
        {
            //找到在 query 中用到的 FromJoin 列。
            var fromJoinUsed = FromJoinColumnFinder.FindColumns(query, fromJoinColumns);

            //如果有 FromJoin 列被用到了，则需要为这些列生成 Join。并将查询中的这些列进行替换。
            if (fromJoinUsed.Count > 0)
            {
                var allTables = query.From.SearchAllTables();

                var toReplace = new Dictionary<IProperty, IColumnNode>();
                for (int i = 0, c = fromJoinColumns.Count; i < c; i++)
                {
                    var fromJoinColumn = fromJoinColumns[i];
                    var columnInJoin = EnsureJoinCreated(query, fromJoinColumn, allTables);
                    toReplace.Add(fromJoinColumn.Property, columnInJoin);
                }

                //将使用到的 FromJoin 表、T.* 等都替换为 Join 中的列
                FromJoinColumnReplacer.Replace(query, toReplace);
            }
        }

        /// <summary>
        /// 确保指定的 fromJoinColumn 的关系链都已经生成成功。
        /// </summary>
        /// <param name="query"></param>
        /// <param name="fromJoinColumn"></param>
        /// <param name="allTables"></param>
        /// <returns></returns>
        private static IColumnNode EnsureJoinCreated(IQuery query, IRdbColumnInfo fromJoinColumn, List<ITableSource> allTables)
        {
            var f = QueryFactory.Instance;
            var meta = fromJoinColumn.Meta;

            var from = query.From;
            var path = meta.RefValuePath;
            ITableSource lastTable = query.MainTable;
            var refProperties = path.GetRefProperties();

            //遍历所有引用属性，如果发现第一个没有生成的引用属性，则开始创建这个属性及其之后的引用属性对应的 Join。
            bool needCreate = false;
            foreach (var refProperty in refProperties)
            {
                if (!needCreate)
                {
                    var table = allTables.FirstOrDefault(t => t.RefProperty == refProperty);
                    if (table == null)
                    {
                        needCreate = true;
                    }
                    else
                    {
                        lastTable = table;
                    }
                }

                if (needCreate)
                {
                    var repo = RF.Find(refProperty.RefEntityType);
                    var table = f.Table(repo);

                    //如果有重复，则生成别名：表名 + 这个表已经被在这个查询中出现了几次
                    var entityTablesCount = allTables.Count(t => t.EntityRepository == repo);
                    if (entityTablesCount > 0)
                    {
                        table.Alias = table.Name + "_" + entityTablesCount;
                    }

                    from = f.Join(from, table, refProperty, lastTable);

                    lastTable = table;
                    allTables.Add(table);
                }
            }
            query.From = from;

            var res = lastTable.Column(path.ValueProperty.Property, meta.ColumnName);
            return res;
        }

        private class FromJoinColumnFinder : QueryNodeVisitor
        {
            /// <summary>
            /// 在结点树中查找所有用到的列。
            /// </summary>
            /// <param name="node"></param>
            /// <param name="columns">仅限于在这个集合中查找。</param>
            /// <returns></returns>
            public static IList<IRdbColumnInfo> FindColumns(IQueryNode node, IReadOnlyList<IRdbColumnInfo> columns)
            {
                var finder = new FromJoinColumnFinder();
                finder._result = new List<IRdbColumnInfo>();
                finder._columns = columns;
                finder.Visit(node);
                return finder._result;
            }

            private IReadOnlyList<IRdbColumnInfo> _columns;
            private IList<IRdbColumnInfo> _result;

            protected override IQueryNode VisitSelectAll(ISelectAll node)
            {
                foreach (var item in _columns)
                {
                    _result.Add(item);
                }
                return node;
            }

            protected override IQueryNode VisitColumn(IColumnNode node)
            {
                for (int i = 0, c = _columns.Count; i < c; i++)
                {
                    var column = _columns[i];
                    if (column.Property == node.Property)
                    {
                        if (!_result.Contains(column))
                        {
                            _result.Add(column);
                        }
                    }
                }
                return node;
            }
        }

        private class FromJoinColumnReplacer : QueryNodeVisitor
        {
            /// <summary>
            /// 将使用到的 FromJoin 表、T.* 等都替换为 Join 中的列
            /// </summary>
            public static void Replace(IQuery query, IDictionary<IProperty, IColumnNode> fromJoinColumnsToReplace)
            {
                var replacer = new FromJoinColumnReplacer();
                replacer._fromJoinColumnsToReplace = fromJoinColumnsToReplace;
                replacer.Visit(query);
            }

            private QueryFactory f = QueryFactory.Instance;
            private IDictionary<IProperty, IColumnNode> _fromJoinColumnsToReplace;

            protected override IQueryNode VisitSelectAll(ISelectAll node)
            {
                var selection = new List<IQueryNode>() { node };

                foreach (var kv in _fromJoinColumnsToReplace)
                {
                    var columnToReplace = kv.Value;
                    selection.Add(columnToReplace);
                }

                return f.Array(selection);
            }
            protected override IQueryNode VisitColumn(IColumnNode node)
            {
                if (node is IColumnNode column && _fromJoinColumnsToReplace.TryGetValue(column.Property, out var columnToReplace))
                {
                    return columnToReplace;
                }

                return node;
            }
        }
    }
}
