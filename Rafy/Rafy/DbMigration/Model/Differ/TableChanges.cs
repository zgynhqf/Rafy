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
using Rafy;
using System.Diagnostics;

namespace Rafy.DbMigration.Model
{
    /// <summary>
    /// 两个表的变更记录
    /// </summary>
    [DebuggerDisplay("{Name} : {ChangeType}")]
    public class TableChanges
    {
        public TableChanges(Table oldTable, Table newTable, ChangeType changeType)
        {
            this.OldTable = oldTable;
            this.NewTable = newTable;
            this.ChangeType = changeType;
            this.ColumnsChanged = new List<ColumnChanges>();
        }

        public Table OldTable { get; private set; }

        public Table NewTable { get; private set; }

        public List<ColumnChanges> ColumnsChanged { get; private set; }

        /// <summary>
        /// 当表的字段被增/删/改了，则这个值为Changed。
        /// 其它则是表示表被删除/增加。
        /// </summary>
        public ChangeType ChangeType { get; private set; }

        public string Name
        {
            get { return this.GetCoreTable().Name; }
        }

        private Table GetCoreTable()
        {
            if (this.ChangeType == ChangeType.Removed)
            {
                return this.OldTable;
            }

            return this.NewTable;
        }
    }
}
