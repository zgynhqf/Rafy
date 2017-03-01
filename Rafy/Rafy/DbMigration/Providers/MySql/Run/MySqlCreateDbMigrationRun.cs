/*******************************************************
 * 
 * 作者：刘雷
 * 创建日期：20170103
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 刘雷 20170103 16:25
 * 
*******************************************************/

using Rafy.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rafy.DbMigration.MySql
{
    /// <summary>
    /// MySql创建数据库
    /// </summary>
    [DebuggerDisplay("CREATE DATABASE {Database}")]
    public sealed class MySqlCreateDbMigrationRun : MigrationRun
    {
        /// <summary>
        /// 设置或者获取当前操作的数据库
        /// </summary>
        public string Database { get; set; }

        /// <summary>
        /// 执行数据库的创建操作
        /// </summary>
        /// <param name="db">数据库访问对象</param>
        protected override void RunCore(IDbAccesser db)
        {
            string dbConnString = db.ConnectionSchema.ConnectionString;
            db.Connection.ConnectionString = dbConnString.Replace(db.Connection.Database, "mysql");
            //手动打开连接，并不关闭，让连接一直处于打开的状态，否则不能立刻连接到新的数据库上
            db.Connection.Open();
            db.RawAccesser.ExecuteText(string.Format("CREATE DATABASE IF NOT EXISTS {0} CHARACTER SET UTF8;", this.Database));
            db.RawAccesser.ExecuteText("USE " + this.Database + ";");
            db.Connection.Close();
            db.Connection.ConnectionString = dbConnString;
        }
    }
}