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
using Rafy.MetaModel;
using Rafy.Reflection;

namespace Rafy.Domain.ORM
{
    class PersistanceColumnInfo : IPersistanceColumnInfo
    {
        internal PersistanceColumnInfo(
            string name,
            EntityPropertyMeta propertyMeta,
            ColumnMeta columnMeta,
            PersistanceTableInfo table,
            DbTypeConverter dbTypeConverter
            )
        {
            this.Table = table;
            this.Name = name;
            this.Meta = columnMeta;
            this.PropertyType = TypeHelper.IgnoreNullable(propertyMeta.PropertyType);
            this.DbType = columnMeta.DataType ?? dbTypeConverter.FromClrType(this.PropertyType);
            this.Property = propertyMeta.ManagedProperty as IProperty;
        }

        public PersistanceTableInfo Table { get; private set; }

        public string Name { get; private set; }

        public ColumnMeta Meta { get; private set; }

        public Type PropertyType { get; private set; }

        public DbType DbType { get; private set; }

        public bool IsIdentity => this.Meta.IsIdentity;

        public bool IsPrimaryKey => this.Meta.IsPrimaryKey;

        public IProperty Property { get; private set; }

        IPersistanceTableInfo IPersistanceColumnInfo.Table
        {
            get { return this.Table; }
        }
    }
}
