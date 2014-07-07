/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110109
 * 说明：所有外键相关的操作。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20110109
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.DbMigration.Model;

namespace Rafy.DbMigration.Operations
{
    public abstract class FKConstraintOperation : MigrationOperation
    {
        public string ConstraintName { get; set; }

        /// <summary>
        /// FK Table
        /// </summary>
        public string DependentTable { get; set; }

        /// <summary>
        /// FK Table Column
        /// </summary>
        public string DependentTableColumn { get; set; }

        /// <summary>
        /// PK Table
        /// </summary>
        public string PrincipleTable { get; set; }

        /// <summary>
        /// PK Table Column
        /// </summary>
        public string PrincipleTableColumn { get; set; }

        public bool NeedDeleteCascade { get; set; }

        internal ForeignConstraint CopyFromConstraint
        {
            set
            {
                if (value != null)
                {
                    this.DependentTable = value.FKColumn.Table.Name;
                    this.DependentTableColumn = value.FKColumn.Name;
                    this.PrincipleTable = value.PKColumn.Table.Name;
                    this.PrincipleTableColumn = value.PKColumn.Name;
                    this.NeedDeleteCascade = value.NeedDeleteCascade;
                    this.ConstraintName = value.ConstraintName;
                }
            }
        }

        internal FKConstraintOperation CopyFrom
        {
            set
            {
                if (value != null)
                {
                    this.DependentTable = value.DependentTable;
                    this.DependentTableColumn = value.DependentTableColumn;
                    this.PrincipleTable = value.PrincipleTable;
                    this.PrincipleTableColumn = value.PrincipleTableColumn;
                    this.NeedDeleteCascade = value.NeedDeleteCascade;
                    this.ConstraintName = value.ConstraintName;
                }
            }
        }

        public override string Description
        {
            get
            {
                return string.Format("{0}: {1}", base.Description, this.ConstraintName);
            }
        }
    }

    public class AddFKConstraint : FKConstraintOperation
    {
        protected override void Down()
        {
            this.AddOperation(new RemoveFKConstraint
            {
                CopyFrom = this
            });
        }
    }

    public class RemoveFKConstraint : FKConstraintOperation
    {
        protected override void Down()
        {
            this.AddOperation(new AddFKConstraint
            {
                CopyFrom = this
            });
        }
    }
}