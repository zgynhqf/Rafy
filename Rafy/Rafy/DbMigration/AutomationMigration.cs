/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110104
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20110104
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.DbMigration.Model;
using Rafy.DbMigration.Operations;
using Rafy.DbMigration.SqlServer;

namespace Rafy.DbMigration
{
    /// <summary>
    /// 职责：通过数据库变更记录来生成迁移操作列表
    /// </summary>
    internal class AutomationMigration
    {
        private List<MigrationOperation> _operations = new List<MigrationOperation>();

        internal DbMigrationContext Context;

        private List<Action> _relationActions = new List<Action>();

        public List<MigrationOperation> Operations
        {
            get { return this._operations; }
        }

        public void GenerateOpertions(DatabaseChanges dbChanges)
        {
            this._operations.Clear();

            switch (dbChanges.ChangeType)
            {
                case ChangeType.Added:
                    this.CreateDatabase(dbChanges);
                    break;
                case ChangeType.Removed:
                    this.DropDatabase(dbChanges);
                    break;
                case ChangeType.Modified:
                    //为了保证外键的变化与表的变化不冲突，按照以下顺序生成操作：添加的表、修改的表（外键）、删除的表。
                    foreach (var item in dbChanges.TablesChanged.Where(t => t.ChangeType == ChangeType.Added))
                    {
                        this.GenerateOpertions(item);
                    }
                    foreach (var item in dbChanges.TablesChanged.Where(t => t.ChangeType == ChangeType.Modified))
                    {
                        this.GenerateOpertions(item);
                    }
                    foreach (var item in dbChanges.TablesChanged.Where(t => t.ChangeType == ChangeType.Removed))
                    {
                        this.GenerateOpertions(item);
                    }
                    break;
                default:
                    break;
            }

            foreach (var action in this._relationActions) { action(); }
            this._relationActions.Clear();
        }

        private void GenerateOpertions(TableChanges tableChanges)
        {
            switch (tableChanges.ChangeType)
            {
                case ChangeType.Added:
                    this.AddTable(tableChanges.NewTable);
                    break;
                case ChangeType.Removed:
                    this.RemoveTable(tableChanges.OldTable);
                    break;
                case ChangeType.Modified:
                    foreach (var column in tableChanges.ColumnsChanged)
                    {
                        this.GenerateOpertions(column);
                    }
                    break;
                default:
                    break;
            }
        }

        private void GenerateOpertions(ColumnChanges columnChanges)
        {
            switch (columnChanges.ChangeType)
            {
                case ChangeType.Added:
                    this.AddColumn(columnChanges.NewColumn);
                    break;
                case ChangeType.Removed:
                    this.RemoveColumn(columnChanges.OldColumn);
                    break;
                case ChangeType.Modified:
                    this.ModifyColumn(columnChanges);
                    break;
                default:
                    break;
            }
        }

        private void CreateDatabase(DatabaseChanges dbChanges)
        {
            this.AddOperation(new CreateDatabase { Database = dbChanges.NewDatabase.Name });

            foreach (var table in dbChanges.NewDatabase.Tables)
            {
                if (!dbChanges.NewDatabase.IsIgnored(table.Name))
                {
                    this.AddTable(table);
                }
            }
        }

        private void DropDatabase(DatabaseChanges dbChanges)
        {
            //反向按表间的引用关系删除表。
            var tables = dbChanges.OldDatabase.Tables;
            for (int i = tables.Count - 1; i >= 0; i--)
            {
                if (!dbChanges.NewDatabase.IsIgnored(tables[i].Name))
                {
                    this.RemoveTable(tables[i]);
                }
            }

            //当版本号嵌入到当前数据库中时，也不支持自动 DropDatabase。
            if (!Context.DbVersionProvider.IsEmbaded())
            {
                this.AddOperation(new DropDatabase { Database = dbChanges.OldDatabase.Name });
            }
        }

        private void AddTable(Table table)
        {
            var op = new CreateTable()
            {
                CopyFromTable = table
            };
            this.AddOperation(op);

            foreach (var column in table.FindNormalColumns())
            {
                this.AddColumn(column);
            }
        }

        private void RemoveTable(Table table)
        {
            if ((Context.RunDataLossOperation & DataLossOperation.DropTable) != 0)
            {
                Context.NotifyDataLoss("删除表");

                foreach (var column in table.FindNormalColumns())
                {
                    this.RemoveColumn(column);
                }

                this.AddOperation(new DropTable
                {
                    CopyFromTable = table
                });
            }
        }

        private void AddColumn(Column column)
        {
            this.AddOperation(new CreateNormalColumn
            {
                CopyFromColumn = column,
                IsPrimaryKey = column.IsPrimaryKey,
                IsIdentity = column.IsIdentity,
            });

            //自增列必然是不可空的，在创建列时已经同时把不可空约束给创建好了，所以这里不需要重复添加了。
            if (column.IsRequired && !column.IsIdentity)
            {
                this.AddOperation(new AddNotNullConstraint
                {
                    CopyFromColumn = column,
                });
            }

            if (column.IsForeignKey)
            {
                this.AddRelationAction(() =>
                {
                    this.AddOperation(new AddFKConstraint
                    {
                        CopyFromConstraint = column.ForeignConstraint
                    });
                });
            }
        }

        private void RemoveColumn(Column column)
        {
            if ((Context.RunDataLossOperation & DataLossOperation.DropColumn) != 0)
            {
                Context.NotifyDataLoss("删除列");

                if (column.IsForeignKey)
                {
                    this.AddOperation(new RemoveFKConstraint
                    {
                        CopyFromConstraint = column.ForeignConstraint
                    });
                }

                if (column.IsRequired)
                {
                    this.AddOperation(new RemoveNotNullConstraint
                    {
                        CopyFromColumn = column,
                    });
                }

                this.AddOperation(new DropNormalColumn
                {
                    CopyFromColumn = column,
                    IsPrimaryKey = column.IsPrimaryKey,
                    IsIdentity = column.IsIdentity,
                });
            }
        }

        private void ModifyColumn(ColumnChanges columnChanges)
        {
            //数据类型
            if (columnChanges.IsDbTypeChanged)
            {
                this.AddOperation(new AlterColumnType
                {
                    CopyFromColumn = columnChanges.OldColumn,
                    NewType = columnChanges.NewColumn.DataType,
                    IsRequired = columnChanges.OldColumn.IsRequired,
                });
            }

            //是否主键
            if (columnChanges.IsPrimaryKeyChanged)
            {
                var column = columnChanges.NewColumn;
                if (column.IsPrimaryKey)
                {
                    this.AddOperation(new AddPKConstraint
                    {
                        CopyFromColumn = column,
                    });
                }
                else
                {
                    this.AddOperation(new RemovePKConstraint
                    {
                        CopyFromColumn = column,
                    });
                }
            }

            //可空性
            if (columnChanges.IsRequiredChanged)
            {
                this.ModifyColumnRequired(columnChanges);
            }

            //外键
            if (columnChanges.ForeignRelationChangeType != ChangeType.UnChanged)
            {
                this.ModifyColumnForeignConstraint(columnChanges);
            }
        }

        private void ModifyColumnRequired(ColumnChanges columnChanges)
        {
            if (columnChanges.NewColumn.IsRequired)
            {
                if (columnChanges.OldColumn.IsForeignKey)
                {
                    this.AddOperation(new AddNotNullConstraintFK
                    {
                        CopyFromColumn = columnChanges.NewColumn
                    });
                }
                else
                {
                    this.AddOperation(new AddNotNullConstraint
                    {
                        CopyFromColumn = columnChanges.NewColumn
                    });
                }
            }
            else
            {
                if (columnChanges.OldColumn.IsForeignKey)
                {
                    this.AddOperation(new RemoveNotNullConstraintFK
                    {
                        CopyFromColumn = columnChanges.NewColumn
                    });
                }
                else
                {
                    this.AddOperation(new RemoveNotNullConstraint
                    {
                        CopyFromColumn = columnChanges.NewColumn
                    });
                }
            }
        }

        private void ModifyColumnForeignConstraint(ColumnChanges columnChanges)
        {
            var value = columnChanges.ForeignRelationChangeType;
            switch (value)
            {
                case ChangeType.Added:
                    this.AddOperation(new AddFKConstraint
                    {
                        CopyFromConstraint = columnChanges.NewColumn.ForeignConstraint
                    });
                    break;
                case ChangeType.Removed:
                    this.AddOperation(new RemoveFKConstraint
                    {
                        CopyFromConstraint = columnChanges.OldColumn.ForeignConstraint
                    });
                    break;
                case ChangeType.Modified:
                    //throw new NotSupportedException("暂时不支持外键修改。");
                    this.AddOperation(new RemoveFKConstraint
                    {
                        CopyFromConstraint = columnChanges.OldColumn.ForeignConstraint
                    });
                    this.AddOperation(new AddFKConstraint
                    {
                        CopyFromConstraint = columnChanges.NewColumn.ForeignConstraint
                    });
                    break;
                default:
                    break;
            }
        }

        #region 私有方法

        private void AddOperation(MigrationOperation operation)
        {
            this._operations.Add(operation);
        }

        private void AddRelationAction(Action action)
        {
            this._relationActions.Add(action);
        }

        #endregion
    }
}