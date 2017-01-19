/*******************************************************
 * 
 * 作者：刘雷
 * 创建日期：20161229
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 刘雷 20161229 14:02
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rafy.Data;

namespace Rafy.DbMigration.MySql
{
    /// <summary>
    /// MySql改变当前使用的数据库
    /// </summary>
    public sealed class MySqlChangeDbMigrationRun : MigrationRun
    {
        /// <summary>
        /// 获取或者设置当前要操作的数据库
        /// </summary>
        public string Database { get; set; }

        /// <summary>
        /// 运行修改当前运行的数据库操作
        /// </summary>
        /// <param name="db">数据库访问对象</param>
        protected override void RunCore(IDbAccesser db)
        {
            db.RawAccesser.ExecuteText("USE " + this.Database+";");
        }
    }
}