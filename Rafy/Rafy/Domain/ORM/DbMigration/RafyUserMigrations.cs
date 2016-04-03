/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110106
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20110106
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.DbMigration;

namespace Rafy.Domain.ORM.DbMigration
{
    internal class RafyUserMigrations : ManualMigrationsContainer
    {
        protected override void OnInit()
        {
            base.OnInit();

            foreach (var item in RafyEnvironment.AllPlugins)
            {
                this.AddByAssembly(item.Assembly);
            }
        }
    }
}