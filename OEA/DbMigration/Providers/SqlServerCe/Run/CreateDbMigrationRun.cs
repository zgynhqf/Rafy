/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110104
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20110104
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using hxy.Common.Data;
using System.Data.SqlServerCe;
using System.Diagnostics;
using System.Threading;

namespace DbMigration.SqlServerCe
{
    [DebuggerDisplay("CREATE DATABASE : {Database}")]
    public class CreateDbMigrationRun : MigrationRun
    {
        public string Database { get; set; }

        protected override void RunCore(IDBAccesser db)
        {
            var engine = new SqlCeEngine(db.Connection.ConnectionString);
            engine.CreateDatabase();
        }
    }
}
