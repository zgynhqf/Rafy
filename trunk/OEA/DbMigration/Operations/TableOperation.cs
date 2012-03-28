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
using DbMigration.Model;
using System.Data;

namespace DbMigration.Operations
{
    public abstract class TableOperation : MigrationOperation
    {
        public string TableName { get; set; }

        /// <summary>
        /// 如果有主键，则这个字段表示主键的名称
        /// 目前只简单地支持单一主键
        /// </summary>
        public string PKName { get; set; }

        /// <summary>
        /// 如果有主键，则这个字段表示主键的名称
        /// 目前只简单地支持单一主键
        /// </summary>
        public DbType PKDataType { get; set; }

        protected override string GetDescription()
        {
            return string.Format("{0}: {1}", base.GetDescription(), this.TableName);
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