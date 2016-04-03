/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120102
 * 说明：此文件只包含一个类，具体内容见类型注释。
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
using Rafy;
using System.Diagnostics;

namespace Rafy.DbMigration.Model
{
    /// <summary>
    /// 数据库的变更记录
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay}")]
    public class DatabaseChanges
    {
        private IList<TableChanges> _allRecords;

        internal DatabaseChanges(Database oldDatabase, DestinationDatabase newDatabase, IList<TableChanges> tableChanges)
        {
            this.OldDatabase = oldDatabase;
            this.NewDatabase = newDatabase;
            this.ChangeType = ChangeType.UnChanged;

            if (!oldDatabase.Removed || !newDatabase.Removed)
            {
                if (oldDatabase.Removed)
                {
                    this.ChangeType = ChangeType.Added;
                    return;
                }

                if (newDatabase.Removed)
                {
                    this.ChangeType = ChangeType.Removed;
                    return;
                }

                if (tableChanges != null && tableChanges.Count > 0)
                {
                    this.ChangeType = ChangeType.Modified;

                    this._allRecords = tableChanges;
                }
            }
        }

        public Database OldDatabase { get; private set; }

        public DestinationDatabase NewDatabase { get; private set; }

        public ChangeType ChangeType { get; private set; }

        public IList<TableChanges> TablesChanged
        {
            get { return this._allRecords; }
        }

        public TableChanges FindTable(string tableName)
        {
            return this._allRecords.FirstOrDefault(t => t.Name == tableName);
        }

        protected string DebuggerDisplay
        {
            get
            {
                switch (this.ChangeType)
                {
                    case ChangeType.Added:
                        return this.NewDatabase.Name + " Added!";
                    case ChangeType.Removed:
                        return this.OldDatabase.Name + " Removed!";
                    case ChangeType.Modified:
                        return string.Format(
                            "All:{0}, Added:{1}, Removed:{2}, Changed:{3}",
                            this._allRecords.Count,
                            this._allRecords.Count(r => r.ChangeType == ChangeType.Added),
                            this._allRecords.Count(r => r.ChangeType == ChangeType.Removed),
                            this._allRecords.Count(r => r.ChangeType == ChangeType.Modified)
                            );
                    default:
                        return string.Empty;
                }
            }
        }
    }
}
