/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130107 09:51
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130107 09:51
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.DbMigration;
using Rafy;
using Rafy.Domain;

namespace Rafy.Domain.ORM.DbMigration
{
    /// <summary>
    /// 在服务端升级数据库的服务
    /// </summary>
    [Serializable]
    [Contract, ContractImpl]
    public class MigrateService : Service
    {
        public MigratingOptions Options { get; set; }

        protected override void Execute()
        {
            var migrateList = this.Options;
            if (migrateList.Databases.Length > 0)
            {
                if (Options.ReserveHistory)
                {
                    using (var c = new RafyDbMigrationContext(DbSettingNames.DbMigrationHistory))
                    {
                        c.RunDataLossOperation = DataLossOperation.All;

                        c.AutoMigrate();
                    }
                }

                foreach (var config in migrateList.Databases)
                {
                    using (var c = new RafyDbMigrationContext(config))
                    {
                        if (Options.ReserveHistory)
                        {
                            c.HistoryRepository = new DbHistoryRepository();
                        }

                        //    c.RollbackAll();
                        //    c.ResetHistory();
                        //    c.ResetDbVersion();
                        if (Options.IgnoreTables != null)
                        {
                            c.ClassMetaReader.IgnoreTables.AddRange(Options.IgnoreTables);
                        }

                        c.RunDataLossOperation = migrateList.RunDataLossOperation;
                        c.AutoMigrate();
                    }
                }
            }
        }
    }
}