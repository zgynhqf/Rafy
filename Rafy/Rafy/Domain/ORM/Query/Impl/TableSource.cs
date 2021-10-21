/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20131212
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20131212 12:18
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.Domain.ORM.SqlTree;
using Rafy.ManagedProperty;

namespace Rafy.Domain.ORM.Query.Impl
{
    class TableSource : SqlTable, ITableSource
    {
        public TableSource(IRdbTableInfo tableInfo, IRepository entityRepository)
        {
            _tableInfo = tableInfo;
            _tableInfoColumns = tableInfo.Columns;
            this.TableName = tableInfo.Name;
            this.EntityRepository = entityRepository;
        }

        public IRdbTableInfo TableInfo { get => _tableInfo; }

        public IRepository EntityRepository { get; private set; }

        public IColumnNode IdColumn
        {
            get { return this.Column(Entity.IdProperty); }
        }

        public IColumnNode Column(IManagedProperty property, string alias)
        {
            return this.FindColumn(property, alias, true);
        }

        public IColumnNode Column(IManagedProperty property)
        {
            return this.FindColumn(property, null, true);
        }

        public IColumnNode FindColumn(IManagedProperty property)
        {
            return this.FindColumn(property, null, false);
        }

        QueryNodeType IQueryNode.NodeType
        {
            get { return QueryNodeType.TableSource; }
        }

        #region 列的懒加载

        /*********************** 代码块解释 *********************************
         * TableSource 在构造时，不必立刻为所有属性生成相应的列。必须使用懒加载。
        *********************************************************************/

        /// <summary>
        /// 对应 ORM 中的运行时表。
        /// </summary>
        private IRdbTableInfo _tableInfo;
        private IReadOnlyList<IRdbColumnInfo> _tableInfoColumns;

        /// <summary>
        /// 这个表中的所有可用的列。
        /// 数组中元素可能是 null、RdbColumnInfo、ColumnNode 三种情况。
        /// 要解决如果表中的列过多，而不生成过多无用的 ColumnNode 的情况。所以，在 Table.Alias、Column.Alias 都是空的情况下，直接使用 RdbColumnInfo 即可。
        /// </summary>
        private IColumnNode[] _columnsCache;

        public IColumnNode FindColumn(IManagedProperty property, string alias = null, bool throwIfNotFound = false)
        {
            for (int i = 0, c = _tableInfoColumns.Count; i < c; i++)
            {
                if (_tableInfoColumns[i].Property == property)
                {
                    return GetCache(i, alias);
                }
            }

            //如有列都遍历完成，没有找到与 property 对应的列，则返回 null。
            if (throwIfNotFound)
            {
                throw new InvalidOperationException($"没有在实体数据源 {this.Name} 中找到 {property.Name} 属性对应的列。");
            }

            return null;
        }

        /// <summary>
        /// 返回指定的用于查询的列，不包含 LOB 列。
        /// </summary>
        /// <param name="readProperties">需要返回的列所对应的属性的列表。如果为 null，表示查询全部属性。</param>
        /// <returns></returns>
        internal IEnumerable<IColumnNode> CacheSelectionColumnsExceptsLOB(List<IManagedProperty> readProperties)
        {
            for (int i = 0, c = _tableInfoColumns.Count; i < c; i++)
            {
                var property = _tableInfoColumns[i].Property;

                if (property.Category == PropertyCategory.LOB) continue;
                if (property != Entity.IdProperty && readProperties != null &&  !readProperties.Contains(property)) continue;

                var item = GetCache(i, null, true);
                yield return item;
            }
        }

        private IColumnNode GetCache(int index, string aliasNeed, bool tryUseMeta = false)
        {
            //如果数组还没初始化，则先初始化好这个列表。
            if (_columnsCache == null)
            {
                _columnsCache = new IColumnNode[_tableInfoColumns.Count];
            }

            //数组中元素可能是 null、ColumnNode 三种情况。
            var item = _columnsCache[index];

            //如果该位置的列还没有生成，则即刻生成一个该列的对象，并插入到数组对应的位置中。
            if (item == null || item.Alias != aliasNeed)
            {
                var dbColumn = _tableInfoColumns[index];

                //如果表、列都没有别名的情况下，不再生成额外的对象，直接使用元数据对象。
                if (this.Alias == null && aliasNeed == null && tryUseMeta) return dbColumn;

                //如果别名不同，也需要生成一个新的 Column，这时，只存储最近一个使用 Column。
                item = NewColumn(dbColumn, aliasNeed);
                _columnsCache[index] = item;
            }

            return item;
        }

        ///// <summary>
        ///// 由于 IRdbColumnInfo 中的表名都是没有使用别名的，所以表的别名一旦被设置，IRdbColumnInfo 都不能再使用。
        ///// 所有使用 IRdbColumnInfo 的列缓存，都得清空。
        ///// </summary>
        //private void ResetCacheIfAliasChanged()
        //{
        //    if (_columnsCache != null)
        //    {
        //        for (int i = 0, c = _columnsCache.Length; i < c; i++)
        //        {
        //            var item = _columnsCache[i];
        //            if (item is IRdbColumnInfo)
        //            {
        //                _columnsCache[i] = null;
        //            }
        //        }
        //    }
        //}

        private IColumnNode NewColumn(IRdbColumnInfo dbColumn, string alias = null)
        {
            var res = new ColumnNode();
            res.Table = this;
            res.Property = dbColumn.Property;
            res.ColumnName = dbColumn.Name;
            res.DbType = dbColumn.DbType;
            res.HasIndex = dbColumn.HasIndex;
            res.Alias = alias;
            return res;
        }

        #endregion

        private TableSourceFinder _finder;

        ITableSource ISource.FindTable(IRepository repo, string alias)
        {
            if (_finder == null) { _finder = new TableSourceFinder(this); }
            return _finder.Find(repo, alias);
        }
    }
}