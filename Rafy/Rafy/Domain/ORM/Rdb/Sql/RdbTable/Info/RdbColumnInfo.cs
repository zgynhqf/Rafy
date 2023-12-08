/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20150313
 * 运行环境：.NET 4.5
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20150313 17:27
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rafy.DbMigration;
using Rafy.Domain.ORM.Query;
using Rafy.Domain.ORM.SqlTree;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.Reflection;

namespace Rafy.Domain.ORM
{
    /// <summary>
    /// 实现 ISqlSelectionColumn 接口，是为了在大量生成列时，直接使用本类的对象，而不再生成新的对象，这样可以大量减少无用对象的生成。
    /// </summary>
    class RdbColumnInfo : IRdbColumnInfo
    {
        /// <summary>
        /// 构造器
        /// </summary>
        /// <param name="name">已经 Prepare 过的列名。</param>
        /// <param name="propertyMeta"></param>
        /// <param name="columnMeta"></param>
        /// <param name="table"></param>
        /// <param name="dbType"></param>
        internal RdbColumnInfo(
            string name,
            EntityPropertyMeta propertyMeta,
            ColumnMeta columnMeta,
            RdbTableInfo table,
            DbType dbType
            )
        {
            this.Table = table;
            this.Name = name;
            this.Meta = columnMeta;
            this.CorePropertyType = TypeHelper.IgnoreNullable(propertyMeta.PropertyType);
            this.DbType = dbType;
            this.Property = propertyMeta.ManagedProperty as IProperty;
        }

        public RdbTableInfo Table { get; private set; }

        public string Name { get; private set; }

        public ColumnMeta Meta { get; private set; }

        public Type CorePropertyType { get; private set; }

        public DbType DbType { get; private set; }

        public IProperty Property { get; private set; }

        public IHasName Owner => this.Table;

        QueryNodeType IQueryNode.NodeType => QueryNodeType.Column;

        SqlNodeType ISqlNode.NodeType => SqlNodeType.SqlColumn;

        string ISqlColumn.ColumnName => this.Name;

        /// <summary>
        /// 使用本对象的列，都没有别名。
        /// </summary>
        string ISqlColumn.Alias => null;

        bool ISqlColumn.HasIndex => this.Meta.HasIndex;

        IHasName ISqlColumn.Table => this.Table;

        IRdbTableInfo IRdbColumnInfo.Table => this.Table;
    }
}
