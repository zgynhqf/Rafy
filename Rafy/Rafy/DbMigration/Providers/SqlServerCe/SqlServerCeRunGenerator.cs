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
using Rafy.DbMigration.Operations;
using Rafy.Data;

namespace Rafy.DbMigration.SqlServerCe
{
    /// <summary>
    /// SqlServer 的执行项生成器
    /// </summary>
    public class SqlServerCeRunGenerator : SqlServer.SqlServerRunGenerator
    {
        protected override string ConvertToTypeString(DbType dataType, string length)
        {
            switch (dataType)
            {
                case DbType.String:
                case DbType.AnsiString:
                case DbType.Xml:
                    if (!string.IsNullOrEmpty(length) && !length.EqualsIgnoreCase("MAX"))
                    {
                        return "NVARCHAR(" + length + ')';
                    }
                    return "NVARCHAR(4000)";
                case DbType.Binary:
                    return "VARBINARY(8000)";
                default:
                    return base.ConvertToTypeString(dataType, length);
            }
        }

        protected override void Generate(CreateDatabase op)
        {
            //不需要传入 DataBase 的值，因为 CreateDbMigrationRun 会直接使用连接中指定的数据库名称。
            this.AddRun(new CreateDbMigrationRun());
        }

        protected override void Generate(DropDatabase op)
        {
            //不需要传入 DataBase 的值，因为 CreateDbMigrationRun 会直接使用连接中指定的数据库名称。
            this.AddRun(new DropDbMigrationRun());
        }

        protected override void Generate(UpdateComment op)
        {
            this.AddRun(new GenerationExceptionRun { Message = "不支持为 SQL CE 数据库自动生成注释。" });
        }
    }
}