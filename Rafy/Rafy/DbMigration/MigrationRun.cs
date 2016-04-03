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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.Data;
using System.Diagnostics;
using System.Data;

namespace Rafy.DbMigration
{
    /// <summary>
    /// 代表每一个数据库升级执行项
    /// </summary>
    public abstract class MigrationRun
    {
        /// <summary>
        /// 通过指定的数据库连接执行
        /// </summary>
        /// <param name="db"></param>
        [DebuggerStepThrough]
        public void Run(IDbAccesser db)
        {
            this.RunCore(db);
        }

        protected abstract void RunCore(IDbAccesser db);
    }
}