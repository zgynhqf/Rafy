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
using System.Diagnostics;
using Rafy.Data;

namespace Rafy.DbMigration
{
    /// <summary>
    /// 在 Generate 的过程中最好不要出现异常，可以使用此类来延迟异常的抛出，在真正开始执行 Sql（Run） 时才抛出异常。
    /// </summary>
    [DebuggerDisplay("Generation Exception : {Message}")]
    public class GenerationExceptionRun : MigrationRun
    {
        public string Message { get; set; }

        protected override void RunCore(IDbAccesser db)
        {
            throw new DbMigrationException(this.Message);
        }
    }
}