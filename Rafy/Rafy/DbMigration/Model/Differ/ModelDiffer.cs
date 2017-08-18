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
    internal static class ModelDiffer
    {
        /// <summary>
        /// 计算出两个数据库元数据的所有表差别
        /// </summary>
        /// <param name="oldDatabase">旧数据库</param>
        /// <param name="newDatabase">新数据库</param>
        /// <returns></returns>
        public static DatabaseChanges Distinguish(Database oldDatabase, DestinationDatabase newDatabase)
        {
            if (!oldDatabase.Removed) { oldDatabase.OrderByRelations(); }
            if (!newDatabase.Removed) { newDatabase.OrderByRelations(); }

            List<TableChanges> result = new List<TableChanges>();

            if (!oldDatabase.Removed && !newDatabase.Removed)
            {
                //先找出所有被删除的表
                foreach (var oldTable in oldDatabase.Tables.Reverse())
                {
                    if (newDatabase.FindTable(oldTable.Name) == null && !newDatabase.IsIgnored(oldTable.Name))
                    {
                        result.Add(new TableChanges(oldTable, null, ChangeType.Removed));
                    }
                }

                foreach (var newTable in newDatabase.Tables)
                {
                    if (!newDatabase.IsIgnored(newTable.Name))
                    {
                        var oldTable = oldDatabase.FindTable(newTable.Name);
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
        private static TableChanges Distinguish(Table oldTable, Table newTable)
        {
            //if (newTable == null) throw new ArgumentNullException("newTable");
            //if (oldTable == null) throw new ArgumentNullException("oldTable");
            //if (newTable.Name != oldTable.Name) throw new InvalidOperationException("newTable.Name != oldTable.Name must be false.");

            var record = new TableChanges(oldTable, newTable, ChangeType.Modified);

            //先找到已经删除的列
            foreach (var oldColumn in oldTable.Columns)
            {
                if (newTable.FindColumn(oldColumn.Name) == null)
                {
                    record.ColumnsChanged.Add(new ColumnChanges(oldColumn, null, ChangeType.Removed));
                }
            }

            //记录新增的和更改过的列
            foreach (var column in newTable.Columns)
            {
                Column oldColumn = oldTable.FindColumn(column.Name);

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

        private static ColumnChanges Distinguish(Column oldColumn, Column newColumn)
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

                if (!DbTypeHelper.IsCompatible(newColumn.DataType, oldColumn.DataType)) { columnChanged.IsDbTypeChanged = true; }

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

        public static bool Equals(Column a, Column b)
        {
            if (a.Table.Name.EqualsIgnoreCase(b.Table.Name) &&
                a.Name.EqualsIgnoreCase(b.Name) &&
                DbTypeHelper.IsCompatible(a.DataType, b.DataType) &&
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

        public static bool Equals(ForeignConstraint a, ForeignConstraint b)
        {
            if (a.NeedDeleteCascade == b.NeedDeleteCascade &&
                a.PKTable.Name.EqualsIgnoreCase(b.PKTable.Name))
            {
                return true;
            }

            return false;
        }
    }
}