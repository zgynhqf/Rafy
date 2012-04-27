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
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using DbMigration.Operations;
using hxy.Common.Data;

namespace DbMigration.SqlServerCe
{
    /// <summary>
    /// SqlServer 的执行项生成器
    /// </summary>
    public class SqlServerCeRunGenerator : SqlServer.SqlServerRunGenerator
    {
        protected override string ConvertToTypeString(DbType dataType)
        {
            switch (dataType)
            {
                case DbType.String:
                case DbType.AnsiString:
                    return "NVARCHAR(4000)";
                case DbType.Binary:
                    return "VARBINARY(8000)";
                default:
                    return base.ConvertToTypeString(dataType);
            }
        }

        protected override void Generate(CreateDatabase op)
        {
            this.AddRun(new CreateDbMigrationRun { Database = op.Database });
        }

        protected override void Generate(DropDatabase op)
        {
            this.AddRun(new DropDbMigrationRun { Database = op.Database });
        }
    }
}