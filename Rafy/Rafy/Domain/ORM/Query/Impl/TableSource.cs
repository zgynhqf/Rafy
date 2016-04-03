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
        public IRepository EntityRepository { get; set; }

        public IColumnNode IdColumn
        {
            get { return this.Column(Entity.IdProperty); }
        }

        string ITableSource.Alias
        {
            get
            {
                return base.Alias;
            }
            set
            {
                base.Alias = value;
            }
        }

        public IColumnNode Column(IManagedProperty property)
        {
            var res = this.FindColumn(property);
            if (res == null) throw new InvalidOperationException(string.Format("没有在实体数据源 {0} 中找到 {1} 属性对应的列。", this.GetName(), property.Name));
            return res;
        }

        public IColumnNode Column(IManagedProperty property, string alias)
        {
            var res = this.Column(property);
            res.Alias = alias;
            return res;
        }

        QueryNodeType IQueryNode.NodeType
        {
            get { return QueryNodeType.TableSource; }
        }

        string INamedSource.GetName()
        {
            return base.GetName();
        }

        #region 列的懒加载

        /*********************** 代码块解释 *********************************
         * EntitySource 在构造时，不必立刻为所有属性生成相应的列。必须使用懒加载。
        *********************************************************************/

        /// <summary>
        /// 对应 ORM 中的运行时表。
        /// </summary>
        internal IPersistanceTableInfo _tableInfo;

        /// <summary>
        /// 这个表中的所有可用的列。
        /// 一次性生成这些表，方便查找。
        /// </summary>
        private ColumnNode[] _columns;

        public IColumnNode FindColumn(IManagedProperty property)
        {
            for (int i = 0, c = _tableInfo.Columns.Count; i < c; i++)
            {
                var dbColumn = _tableInfo.Columns[i];
                if (dbColumn.Property == property)
                {
                    if (_columns == null)
                    {
                        _columns = new ColumnNode[_tableInfo.Columns.Count];
                    }

                    var res = _columns[i];

                    //如果该位置的列还没有生成，则即刻生成一个该列的对象，并插入到数组对应的位置中。
                    if (res == null)
                    {
                        res = NewColumn(dbColumn);
                        _columns[i] = res;
                    }
                    return res;
                }
            }

            //如有列都遍历完成，没有找到与 property 对应的列，则返回 null。
            return null;
        }

        /// <summary>
        /// 返回所有列对应的节点。
        /// 本方法会生成所有还没有生成的列的节点。
        /// </summary>
        /// <returns></returns>
        internal IList<ColumnNode> LoadAllColumns()
        {
            var c = _tableInfo.Columns.Count;

            if (_columns == null)
            {
                _columns = new ColumnNode[c];
            }

            for (int i = 0; i < c; i++)
            {
                var dbColumn = _tableInfo.Columns[i];

                //如果该位置的列还没有生成，则即刻生成一个该列的对象，并插入到数组对应的位置中。
                if (_columns[i] == null)
                {
                    var res = NewColumn(dbColumn);
                    _columns[i] = res;
                }
            }

            return _columns;
        }

        private ColumnNode NewColumn(IPersistanceColumnInfo dbColumn)
        {
            var res = new ColumnNode();
            res.Table = this;
            res.Property = dbColumn.Property;
            res.ColumnName = dbColumn.Name;
            res.DbColumn = dbColumn;
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