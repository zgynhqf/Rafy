/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110109
 * 说明：所有“数据库”相关的操作。
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

namespace Rafy.DbMigration.Operations
{
    public abstract class DatabaseOperation : MigrationOperation
    {
        public string Database { get; set; }

        public override string Description
        {
            get
            {
                return string.Format("{0}: {1}", base.Description, this.Database);
            }
        }
    }

    public class CreateDatabase : DatabaseOperation
    {
        protected override void Down()
        {
            this.AddOperation(new DropDatabase
            {
                Database = this.Database
            });
        }
    }

    public class DropDatabase : DatabaseOperation
    {
        protected override void Down()
        {
            this.AddOperation(new CreateDatabase
            {
                Database = this.Database
            });
        }
    }
}