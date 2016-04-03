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
    /// 表示某列对应外键关系
    /// </summary>
    [DebuggerDisplay("{FKColumn.Name} => {PKTable.Name}.{PKColumn.Name}  ConstraintName: {ConstraintName}")]
    public class ForeignConstraint : Extendable
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="primaryKeyColumn">这个外键对应的主键表的列（不一定是主键列，可以是unique列等）</param>
        public ForeignConstraint(Column primaryKeyColumn)
        {
            if (primaryKeyColumn == null) throw new ArgumentNullException("primaryKeyColumn");
            this.PKColumn = primaryKeyColumn;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fkColumn">这个列的外键</param>
        internal void Init(Column fkColumn)
        {
            if (fkColumn == null) throw new ArgumentNullException("fkColumn");
            this.FKColumn = fkColumn;

            if (string.IsNullOrWhiteSpace(this.ConstraintName))
            {
                this.ConstraintName = string.Format(
                    "FK_{0}_{1}_{2}_{3}",
                    this.FKColumn.Table.Name,
                    this.FKColumn.Name,
                    this.PKColumn.Table.Name,
                    this.PKColumn.Name
                    );
            }
        }

        /// <summary>
        /// 约束名
        /// </summary>
        public string ConstraintName { get; set; }

        /// <summary>
        /// 外键关系的所有者，外键列。
        /// </summary>
        public Column FKColumn { get; private set; }

        /// <summary>
        /// 这个外键对应的主键表的列（不一定是主键列，可以是unique列等）
        /// </summary>
        public Column PKColumn { get; private set; }

        /// <summary>
        /// 是否级联删除
        /// </summary>
        public bool NeedDeleteCascade { get; set; }

        /// <summary>
        /// 这个外键对应的主键表
        /// </summary>
        public Table PKTable
        {
            get { return this.PKColumn.Table; }
        }
    }
}