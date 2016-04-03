/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110109
 * 说明：所有“表”相关的操作。
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
using System.Data;

namespace Rafy.DbMigration.Operations
{
    public abstract class TableOperation : MigrationOperation
    {
        public string TableName { get; set; }

        /// <summary>
        /// 如果有主键，则这个字段表示主键的名称
        /// 目前只简单地支持单一主键
        /// 
        /// 注意，这个主键目前还会是自增长的列。
        /// </summary>
        public string PKName { get; set; }

        /// <summary>
        /// 如果有主键，则这个字段表示主键的名称
        /// 目前只简单地支持单一主键
        /// </summary>
        public DbType PKDataType { get; set; }

        /// <summary>
        /// 主键列的长度。
        /// </summary>
        public string PKLength { get; set; }

        /// <summary>
        /// 主键是否为自增列。
        /// </summary>
        public bool PKIdentity { get; set; }

        public override string Description
        {
            get
            {
                return string.Format("{0}: {1}", base.Description, this.TableName);
            }
        }

        internal Table CopyFromTable
        {
            set
            {
                if (value != null)
                {
                    this.TableName = value.Name;
                    var pk = value.FindPrimaryColumn();
                    if (pk != null)
                    {
                        this.PKName = pk.Name;
                        this.PKDataType = pk.DataType;
                        this.PKIdentity = pk.IsIdentity;
                        this.PKLength = pk.Length;
                    }
                }
            }
        }

        internal TableOperation CopyFrom
        {
            set
            {
                if (value != null)
                {
                    this.TableName = value.TableName;
                    this.PKName = value.PKName;
                    this.PKDataType = value.PKDataType;
                    this.PKIdentity = value.PKIdentity;
                    this.PKLength = value.PKLength;
                }
            }
        }
    }

    public class CreateTable : TableOperation
    {
        protected override void Down()
        {
            this.AddOperation(new DropTable
            {
                CopyFrom = this
            });
        }
    }

    public class DropTable : TableOperation
    {
        protected override void Down()
        {
            this.AddOperation(new CreateTable
            {
                CopyFrom = this
            });
        }
    }
}