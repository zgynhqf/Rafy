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
    /// 两个列的区别记录
    /// </summary>
    [DebuggerDisplay("{TableName}.{Name} => {ChangeType}")]
    public class ColumnChanges
    {
        public ColumnChanges(Column oldColumn, Column newColumn, ChangeType changeType)
        {
            this.OldColumn = oldColumn;
            this.NewColumn = newColumn;
            this.ChangeType = changeType;
        }

        /// <summary>
        /// 旧列
        /// </summary>
        public Column OldColumn { get; private set; }

        /// <summary>
        /// 新列
        /// </summary>
        public Column NewColumn { get; private set; }

        public string TableName
        {
            get { return this.GetCoreColumn().Table.Name; }
        }

        public string Name
        {
            get { return this.GetCoreColumn().Name; }
        }

        public ChangeType ChangeType { get; private set; }

        /// <summary>
        /// 是否更新了是否为空的设定
        /// </summary>
        public bool IsRequiredChanged { get; set; }

        /// <summary>
        /// 是否是主键改变
        /// </summary>
        public bool IsPrimaryKeyChanged { get; set; }

        /// <summary>
        /// 是否是类型改变
        /// </summary>
        public bool IsDbTypeChanged { get; set; }

        /// <summary>
        /// 添加/删除/修改了 外键关系
        /// </summary>
        public ChangeType ForeignRelationChangeType { get; set; }

        private Column GetCoreColumn()
        {
            if (this.ChangeType == ChangeType.Removed)
            {
                return this.OldColumn;
            }

            return this.NewColumn;
        }
    }
}