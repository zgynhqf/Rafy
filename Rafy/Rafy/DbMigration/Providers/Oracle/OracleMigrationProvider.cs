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

namespace Rafy.DbMigration.Oracle
{
    /// <summary>
    /// Oracle 的数据库迁移提供程序
    /// </summary>
    public class OracleMigrationProvider : DbMigrationProvider
    {
        public override IMetadataReader CreateSchemaReader()
        {
            return new OracleMetaReader(this.DbSetting);
        }

        public override RunGenerator CreateRunGenerator()
        {
            return new OracleRunGenerator();
        }

        public override IDbBackuper CreateDbBackuper()
        {
            throw new NotSupportedException("暂时不支持 Oracle 数据库的备份。");
        }
    }
}