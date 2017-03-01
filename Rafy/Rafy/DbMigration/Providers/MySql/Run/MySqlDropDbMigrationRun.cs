/*******************************************************
 * 
 * 作者：刘雷
 * 创建日期：20170103
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 刘雷 20170103 16:12
 * 
*******************************************************/

using Rafy.Data;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rafy.DbMigration.MySql
{
    /// <summary>
    /// MySql删除数据库
    /// </summary>
    [DebuggerDisplay("DROP DATABASE {Database}")]
    public sealed class MySqlDropDbMigrationRun: MigrationRun
    {
        /// <summary>
        /// 获取或者设置当前使用的数据库
        /// </summary>
        public string Database { get; set; }

        /// <summary>
        /// 运行删除数据库的核心方法
        /// </summary>
        /// <param name="db">数据库操作对象</param>
        protected override void RunCore(IDbAccesser db)
        {
            //var mysqlInstance = new MySqlConnectionStringBuilder(db.Connection.ConnectionString);
            //MySqlConnection.ClearPool(db.Connection as MySqlConnection);
          
            using (var db2 = new DbAccesser(db.ConnectionSchema.ConnectionString, DbSetting.Provider_MySql))
            {
                db2.ExecuteText(string.Format("DROP DATABASE IF EXISTS {0};", this.Database));
            }
        }
    }
}