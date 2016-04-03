/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110109
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20110109
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.Data;

namespace Rafy.DbMigration.Operations
{
    /// <summary>
    /// 操作执行某个具体的代码段
    /// </summary>
    public class RunAction : MigrationOperation
    {
        public Action<IDbAccesser> Action { get; set; }

        protected override void Down() { }
    }
}