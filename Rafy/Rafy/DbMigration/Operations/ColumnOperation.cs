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
using Rafy.DbMigration.Model;
using System.Xml.Serialization;
using System.Data;

namespace Rafy.DbMigration.Operations
{
    public abstract class ColumnOperation : MigrationOperation
    {
        public string TableName { get; set; }

        public string ColumnName { get; set; }

        public DbType DataType { get; set; }

        public string Length { get; set; }

        public bool IsForeignKey { get; set; }

        internal Column CopyFromColumn
        {
            set
            {
                if (value != null)
                {
                    this.TableName = value.Table.Name;
                    this.ColumnName = value.Name;
                    this.DataType = value.DataType;
                    this.Length = value.Length;
                    this.IsForeignKey = value.IsForeignKey;
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
                    this.Length = value.Length;
                    this.IsForeignKey = value.IsForeignKey;
                }
            }
        }

        public override string Description
        {
            get
            {
                return string.Format("{0}: {1}.{2}", base.Description, this.TableName, this.ColumnName);
            }
        }
    }

    public class CreateNormalColumn : ColumnOperation
    {
        public bool IsPrimaryKey { get; set; }
        public bool IsIdentity { get; set; }

        protected override void Down()
        {
            this.AddOperation(new DropNormalColumn
            {
                CopyFrom = this,
                IsPrimaryKey = this.IsPrimaryKey,
                IsIdentity = this.IsIdentity
            });
        }
    }

    public class DropNormalColumn : ColumnOperation
    {
        public bool IsPrimaryKey { get; set; }
        public bool IsIdentity { get; set; }

        protected override void Down()
        {
            this.AddOperation(new CreateNormalColumn
            {
                CopyFrom = this,
                IsPrimaryKey = this.IsPrimaryKey,
                IsIdentity = this.IsIdentity
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

        public override string Description
        {
            get
            {
                return base.Description + " From " + this.DataType + " To " + this.NewType;
            }
        }
    }
}