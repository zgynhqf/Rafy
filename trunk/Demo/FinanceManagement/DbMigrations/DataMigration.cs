/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120427
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120427
 * 
*******************************************************/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy.DbMigration;

namespace FM.DbMigrations
{
    public abstract class DataMigration : ManualDbMigration
    {
        public override string DbSetting
        {
            get { return FMEntityRepository.DbSettingName; }
        }

        public override ManualMigrationType Type
        {
            get { return ManualMigrationType.Data; }
        }

        protected override void Down() { }
    }
}
