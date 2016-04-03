/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120424
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120424
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.DbMigration.Model;
using Rafy.Data;
using Rafy.Data.Providers;
using Rafy.DbMigration.SqlServer;

namespace Rafy.DbMigration.SqlServerCe
{
    /// <summary>
    /// SqlServer CE4 的数据库迁移提供程序
    /// </summary>
    public class SqlServerCeMigrationProvider : DbMigrationProvider
    {
        public override IMetadataReader CreateSchemaReader()
        {
            return new SqlServerCeMetaReader(this.DbSetting);
        }

        public override RunGenerator CreateRunGenerator()
        {
            return new SqlServerCeRunGenerator();
        }

        public override IDbBackuper CreateDbBackuper()
        {
            //TODO 实现 .sdf 文件的复制即可
            throw new NotSupportedException("暂时未完成此方法的实现。");
        }
    }
}