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
using Rafy.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Data.Common;
using System.Data;

namespace Rafy.DbMigration.SqlServer
{
    [DebuggerDisplay("DROP DATABASE : {Database}")]
    public class DropDbMigrationRun : MigrationRun
    {
        public string Database { get; set; }

        protected override void RunCore(IDbAccesser db)
        {
            //db.Connection.Close();

            //连接到 MASTER 数据库
            var master = new SqlConnectionStringBuilder(db.Connection.ConnectionString) { InitialCatalog = "MASTER" };

            //参考 EntityFramework SqlProviderServices.DbDeleteDatabase()
            SqlConnection.ClearPool(db.Connection as SqlConnection);

            using (var db2 = new DbAccesser(master.ConnectionString, DbSetting.Provider_SqlClient))
            {
                db2.ExecuteText(string.Format("DROP DATABASE [{0}]", this.Database));
            }
        }

        //protected override void RunCore(IDBAccesser db)
        //{
        //    db.Connection.Close();

        //    //连接到 MASTER 数据库
        //    var dbConnection = db.Connection;
        //    var sb = new SqlConnectionStringBuilder(dbConnection.ConnectionString);
        //    sb.InitialCatalog = "MASTER";
        //    dbConnection.ConnectionString = sb.ConnectionString;

        //    //执行 SQL
        //    string sql = string.Format("DROP DATABASE [{0}]", this.Database);
        //    db.ExecuteTextNormal(sql);
        //}
    }
}