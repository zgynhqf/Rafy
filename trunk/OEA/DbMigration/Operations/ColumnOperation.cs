/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120102
 * 说明：所有“列”相关的操作。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120102
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbMigration.Model;
using System.Xml.Serialization;
using System.Data;

namespace DbMigration.Operations
{
    public abstract class ColumnOperation : MigrationOperation
    {
        public string TableName { get; set; }

        public string ColumnName { get; set; }

        public DbType DataType { get; set; }

        internal Column CopyFromColumn
        {
            set
            {
                if (value != null)
                {
                    this.TableName = value.Table.Name;
                    this.ColumnName = value.Name;
                    this.DataType = value.DataType;
                }
            }
        }

        internal ColumnOperation CopyFrom
        {
            set
            {
                if (value != null)
                {
                    this.TableName = value.TableName;
                    this.ColumnName = value.ColumnName;
                    this.DataType = value.DataType;
                }
            }
        }

        protected override string GetDescription()
        {
            return string.Format("{0}: {1}.{2}", base.GetDescription(), this.TableName, this.ColumnName);
        }
    }

    public class CreateNormalColumn : ColumnOperation
    {
        public bool IsPrimaryKey { get; set; }

        protected override void Down()
        {
            this.AddOperation(new DropNormalColumn
            {
                CopyFrom = this,
                IsPrimaryKey = this.IsPrimaryKey
            });
        }
    }

    public class DropNormalColumn : ColumnOperation
    {
        public bool IsPrimaryKey { get; set; }

        protected override void Down()
        {
            this.AddOperation(new CreateNormalColumn
            {
                CopyFrom = this,
                IsPrimaryKey = this.IsPrimaryKey
            });
        }
    }

    public class AddPKConstraint : ColumnOperation
    {
        protected override void Down()
        {
            this.AddOperation(new RemovePKConstraint
            {
                CopyFrom = this,
            });
        }
    }

    public class RemovePKConstraint : ColumnOperation
    {
        protected override void Down()
        {
            this.AddOperation(new AddPKConstraint
            {
                CopyFrom = this,
            });
        }
    }

    public class AddNotNullConstraint : ColumnOperation
    {
        protected override void Down()
        {
            this.AddOperation(new RemoveNotNullConstraint
            {
                CopyFrom = this,
            });
        }
    }

    public class RemoveNotNullConstraint : ColumnOperation
    {
        protected override void Down()
        {
            this.AddOperation(new AddNotNullConstraint
            {
                CopyFrom = this,
            });
        }
    }

    public class AddNotNullConstraintFK : ColumnOperation
    {
        protected override void Down()
        {
            this.AddOperation(new RemoveNotNullConstraintFK
            {
                CopyFrom = this,
            });
        }
    }

    public class RemoveNotNullConstraintFK : ColumnOperation
    {
        protected override void Down()
        {
            this.AddOperation(new AddNotNullConstraintFK
            {
                CopyFrom = this,
            });
        }
    }

    public class AlterColumnType : ColumnOperation
    {
        public DbType NewType { get; set; }

        public bool IsRequired { get; set; }

        protected override void Down()
        {
            this.AddOperation(new AlterColumnType
            {
                CopyFrom = this,
                IsRequired = this.IsRequired,
                DataType = this.NewType,
                NewType = this.DataType
            });
        }

        protected override string GetDescription()
        {
            return base.GetDescription() + " From " + this.DataType + " To " + this.NewType;
        }
    }
}