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
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Rafy.Data;
using System.IO;
using System.Data.SqlServerCe;

namespace Rafy.DbMigration.SqlServerCe
{
    [DebuggerDisplay("DROP DATABASE : {Database}")]
    public class DropDbMigrationRun : MigrationRun
    {
        protected override void RunCore(IDbAccesser db)
        {
            var sqlCeCon = db.Connection as SqlCeConnection;
            if (sqlCeCon == null) throw new InvalidOperationException("需要使用 SqlCe 的连接，才能正确创建数据库。");

            File.Delete(db.Connection.Database);
        }
    }
}