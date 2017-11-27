/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120103
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120103
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Rafy;

namespace Rafy.DbMigration.Model
{
    internal class ModelDiffer
    {
        internal IDbIdentifierQuoter IDbIdentifierProvider;

        DbTypeConverter DbTypeConverter;

        internal ModelDiffer(DbTypeConverter _dbTypeConverter)
        {
            DbTypeConverter = _dbTypeConverter;
        }

        /// <summary>
        /// 计算出两个数据库元数据的所有表差别
        /// </summary>
        /// <param name="oldDatabase">旧数据库</param>
        /// <param name="newDatabase">新数据库</param>
        /// <returns></returns>
        public DatabaseChanges Distinguish(Database oldDatabase, DestinationDatabase newDatabase)
        {
            if (!oldDatabase.Removed) { oldDatabase.OrderByRelations(); }
            if (!newDatabase.Removed) { newDatabase.OrderByRelations(); }

            List<TableChanges> result = new List<TableChanges>();

            if (!oldDatabase.Removed && !newDatabase.Removed)
            {
                //先找出所有被删除的表
                foreach (var oldTable in oldDatabase.Tables.Reverse())
                {
                    if (FindTable(newDatabase, oldTable.Name) == null && !newDatabase.IsIgnored(oldTable.Name))
                    {
                        result.Add(new TableChanges(oldTable, null, ChangeType.Removed));
                    }
                }

                foreach (var newTable in newDatabase.Tables)
                {
                    if (!newDatabase.IsIgnored(newTable.Name))
                    {
                        var oldTable = FindTable(oldDatabase, newTable.Name);
                        //如果没有找到旧表，说明这个表是新加的。
                        if (oldTable == null)
                        {
                            result.Add(new TableChanges(null, newTable, ChangeType.Added));
                        }
                        else
                        {
                            //即不是新表，也不是旧表，计算两个表的区别
                            TableChanges record = Distinguish(oldTable, newTable);

                            //如果有区别，则记录下来
                            if (record != null) { result.Add(record); }
                        }
                    }
                }
            }

            return new DatabaseChanges(oldDatabase, newDatabase, result);
        }

        /// <summary>
        /// 计算出新旧表之间的数据列差别
        /// </summary>
        /// <param name="oldTable">旧表</param>
        /// <param name="newTable">新表</param>
        /// <returns>
        /// 返回表之间区别，如果没有区别，则返回null
        /// </returns>
        private TableChanges Distinguish(Table oldTable, Table newTable)
        {
            //if (newTable == null) throw new ArgumentNullException("newTable");
            //if (oldTable == null) throw new ArgumentNullException("oldTable");
            //if (newTable.Name != oldTable.Name) throw new InvalidOperationException("newTable.Name != oldTable.Name must be false.");

            var record = new TableChanges(oldTable, newTable, ChangeType.Modified);

            //先找到已经删除的列
            foreach (var oldColumn in oldTable.Columns)
            {
                if (FindColumn(newTable, oldColumn.Name) == null)
                {
                    record.ColumnsChanged.Add(new ColumnChanges(oldColumn, null, ChangeType.Removed));
                }
            }

            //记录新增的和更改过的列
            foreach (var column in newTable.Columns)
            {
                Column oldColumn = FindColumn(oldTable, column.Name);

                if (oldColumn == null)
                {
                    var columnChanged = new ColumnChanges(null, column, ChangeType.Added);
                    record.ColumnsChanged.Add(columnChanged);
                }
                else
                {
                    var columnChanged = Distinguish(oldColumn, column);
                    //新增的 或者 修改的 列
                    if (columnChanged != null) { record.ColumnsChanged.Add(columnChanged); }
                }
            }

            //如果被修改了，则返回record；否则返回null
            if (record.ColumnsChanged.Count > 0) { return record; }

            return null;
        }

        private ColumnChanges Distinguish(Column oldColumn, Column newColumn)
        {
            //if (newColumn == null) throw new ArgumentNullException("newColumn");
            //if (newColumn.Name.EqualsIgnoreCase(oldColumn.Name)) throw new InvalidOperationException("newColumn.Name.EqualsIgnoreCase(oldColumn.Name) must be false.");
            //if (newColumn.Table.Name.EqualsIgnoreCase(oldColumn.Table.Name)) throw new InvalidOperationException("newColumn.Table.Name.EqualsIgnoreCase(oldColumn.Table.Name) must be false.");

            ColumnChanges columnChanged = null;
            if (!Equals(newColumn, oldColumn))
            {
                columnChanged = new ColumnChanges(oldColumn, newColumn, ChangeType.Modified);

                if (newColumn.IsRequired != oldColumn.IsRequired) { columnChanged.IsRequiredChanged = true; }

                if (newColumn.IsPrimaryKey != oldColumn.IsPrimaryKey) { columnChanged.IsPrimaryKeyChanged = true; }

                if (!DbTypeConverter.IsCompatible(newColumn.DbType, oldColumn.DbType)) { columnChanged.IsDbTypeChanged = true; }

                //ForeignRelationChangeType
                columnChanged.ForeignRelationChangeType = ChangeType.UnChanged;
                if (!newColumn.IsForeignKey && oldColumn.IsForeignKey)
                {
                    columnChanged.ForeignRelationChangeType = ChangeType.Removed;
                }
                else if (newColumn.IsForeignKey && !oldColumn.IsForeignKey)
                {
                    columnChanged.ForeignRelationChangeType = ChangeType.Added;
                }
                else if (newColumn.IsForeignKey && oldColumn.IsForeignKey)
                {
                    if (!Equals(newColumn.ForeignConstraint, oldColumn.ForeignConstraint))
                    {
                        columnChanged.ForeignRelationChangeType = ChangeType.Modified;
                    }
                }
            }
            return columnChanged;
        }

        private bool Equals(Column a, Column b)
        {
            if (a.Table.Name.EqualsIgnoreCase(b.Table.Name) &&
                a.Name.EqualsIgnoreCase(b.Name) &&
                DbTypeConverter.IsCompatible(a.DbType, b.DbType) &&
                //a.DataType == b.DataType &&
                a.IsRequired == b.IsRequired &&
                a.IsForeignKey == b.IsForeignKey &&
                a.IsPrimaryKey == b.IsPrimaryKey
                )
            {
                //判断外键是否相等
                //暂时不考虑NeedDeleteCascade是否相同的问题
                if (a.IsForeignKey &&
                    !a.ForeignConstraint.PKColumn.Table.Name.EqualsIgnoreCase(b.ForeignConstraint.PKColumn.Table.Name))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        private bool Equals(ForeignConstraint a, ForeignConstraint b)
        {
            if (a.NeedDeleteCascade == b.NeedDeleteCascade &&
                a.PKTable.Name.EqualsIgnoreCase(b.PKTable.Name))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 不使用 <see cref="Database.FindTable(string)"/> 的原因是因为要考虑标识符的截断。
        /// </summary>
        /// <param name="database"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private Table FindTable(Database database, string name)
        {
            var tables = database.Tables;

            for (int i = 0, c = tables.Count; i < c; i++)
            {
                var table = tables[i];

                var tableName = this.Prepare(table.Name);
                name = this.Prepare(name);

                if (tableName.EqualsIgnoreCase(name)) { return table; }
            }

            return null;
        }

        /// <summary>
        /// 不使用 <see cref="Table.FindColumn(string)"/> 的原因是因为要考虑标识符的截断。
        /// </summary>
        /// <param name="table"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private Column FindColumn(Table table, string name)
        {
            var columns = table.Columns;

            for (int i = 0, c = columns.Count; i < c; i++)
            {
                var column = columns[i];

                var columnName = this.Prepare(column.Name);
                name = this.Prepare(name);

                if (columnName.EqualsIgnoreCase(name)) { return column; }
            }

            return null;
        }

        /// <summary>
        /// 对比时需要考虑截断。
        /// 由于在数据库生成时，有可能额外地对标识符进行一些处理，所以这里需要对这些情况做兼容性处理。
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        private string Prepare(string identifier)
        {
            return this.IDbIdentifierProvider != null ?
                this.IDbIdentifierProvider.Prepare(identifier) :
                identifier;
        }
    }
}