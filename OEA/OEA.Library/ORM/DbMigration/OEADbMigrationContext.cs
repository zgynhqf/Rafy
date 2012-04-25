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
using DbMigration;
using DbMigration.History;
using DbMigration.Model;
using Common;
using hxy.Common.Data;

namespace OEA.Library.ORM.DbMigration
{
    /// <summary>
    /// 在 DbMigrationContext 的基础上实现以下功能：
    /// * 读取元数据，获取目标数据库 Schema
    /// * 使用 DbMigrationHistory 库来支持历史日志
    /// </summary>
    public class OEADbMigrationContext : DbMigrationContext
    {
        public OEADbMigrationContext(string dbSetting)
            : base(DbSetting.FindOrCreate(dbSetting))
        {
            //this.DbVersionProvider = new OEADbVersionProvider();
            this.ManualMigrations = new OEAUserMigrations();
            this.ClassMetaReader = new ClassMetaReader(this.DbSetting);
            this.HistoryRepository = new DbHistoryRepository();
        }

        public ClassMetaReader ClassMetaReader { get; private set; }

        public OEADbMigrationContext AutoMigrate()
        {
            //如果这个配置为 true，则执行自动升级
            var enabled = ConfigurationHelper.GetAppSettingOrDefault(
                "DatabaseAutoMigrationEnabled", OEAEnvironment.IsDebuggingEnabled
                );

            if (enabled)
            {
                var classMeta = this.ClassMetaReader.Read();

                this.MigrateTo(classMeta);
            }
            else
            {
                this.MigrateManually();
            }

            return this;
        }
    }
}