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
        /// <summary>
        /// 对应 ORM 中的运行时表。
        /// </summary>
        private IRdbTableInfo _tableInfo;

        public TableSource(IRdbTableInfo tableInfo, IRepository entityRepository)
        {
            _tableInfo = tableInfo;
            this.TableName = tableInfo.Name;
            this.EntityRepository = entityRepository;
        }

        public IRdbTableInfo TableInfo => _tableInfo;

        public IRepository EntityRepository { get; private set; }

        /// <summary>
        /// 如果当前表是由一个 Join 引入的数据源，则这里表示其所在的 Join。
        /// 如果此值为 null，则表示当前表为主表。
        /// </summary>
        public IJoin Join { get; internal set; }

        /// <summary>
        /// 如果当前表是由一个引用属性对应的 Join 数据源，则这里表示其对应的引用属性。
        /// </summary>
        public IRefProperty RefProperty { get; internal set; }

        public IColumnNode IdColumn
        {
            get { return this.Column(Entity.IdProperty); }
        }

        public IColumnNode Column(IManagedProperty property, string alias)
        {
            return this.CacheColumn(property, alias, true);
        }

        public IColumnNode Column(IManagedProperty property)
        {
            return this.CacheColumn(property, null, true);
        }

        public IColumnNode FindColumn(IManagedProperty property)
        {
            return this.CacheColumn(property, null, false);
        }

        QueryNodeType IQueryNode.NodeType
        {
            get { return QueryNodeType.TableSource; }
        }

        private IColumnNode CacheColumn(IManagedProperty property, string alias = null, bool throwIfNotFound = false)
        {
            return ColumnNodeCache.Instance.Get(this, property, alias, throwIfNotFound);
        }
    }
}