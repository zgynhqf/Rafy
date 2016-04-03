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
using System.Threading;

namespace Rafy.DbMigration.SqlServer
{
    [DebuggerDisplay("CREATE DATABASE : {Database}")]
    public class CreateDbMigrationRun : MigrationRun
    {
        public string Database { get; set; }

        //protected override void RunCore(IDBAccesser db)
        //{
        //    //连接到 MASTER 数据库
        //    var master = new SqlConnectionStringBuilder(db.Connection.ConnectionString) { InitialCatalog = "MASTER" };

        //    ////手动打开连接，并不关闭，让连接一直处于打开的状态，否则不能立刻连接到新的数据库上
        //    //db.Connection.ConnectionString = master.ConnectionString;
        //    //db.Connection.Open();
        //    //db.ExecuteTextNormal(string.Format("CREATE DATABASE [{0}]", this.Database));
        //    //db.ExecuteTextNormal("USE " + this.Database);

        //    using (var db2 = new DBAccesser(master.ConnectionString, "System.Data.SqlClient"))
        //    {
        //        db2.ExecuteText(string.Format("CREATE DATABASE [{0}]", this.Database));
        //        SqlConnection.ClearPool(db2.Connection as SqlConnection);
        //    }
        //}

        protected override void RunCore(IDbAccesser db)
        {
            //连接到 MASTER 数据库
            var builder = new SqlConnectionStringBuilder(db.ConnectionSchema.ConnectionString) { InitialCatalog = "MASTER" };

            //手动打开连接，并不关闭，让连接一直处于打开的状态，否则不能立刻连接到新的数据库上
            db.Connection.ConnectionString = builder.ConnectionString;
            db.Connection.Open();
            db.RawAccesser.ExecuteText(string.Format("CREATE DATABASE [{0}]", this.Database));
            db.RawAccesser.ExecuteText("USE " + this.Database);

            //另外，为了保证其它的连接能够连接上数据库而不会报错，最好等待 5 s。（具体原因未知。）
            Thread.Sleep(5000);
        }
    }
}
