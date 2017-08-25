/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20140614
 * 说明：见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20140614 20:23
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rafy;
using Rafy.Data;
using Rafy.DbMigration;
using Rafy.DbMigration.Model;
using Rafy.Domain;
using Rafy.Domain.ORM;
using Rafy.Domain.ORM.BatchSubmit.Oracle;
using Rafy.Domain.ORM.DbMigration;
using UT;

namespace Rafy.UnitTest.DataProvider
{
    public static class TestDbGenerator
    {
        public static void GenerateDb()
        {
            if (ConfigurationHelper.GetAppSettingOrDefault("单元测试-生成数据库", false))
            {
                using (var c = new RafyDbMigrationContext(DbSettingNames.DbMigrationHistory))
                {
                    c.RunDataLossOperation = DataLossOperation.All;
                    c.AutoMigrate();
                }
                using (var c = new RafyDbMigrationContext(DbSettingNames.RafyPlugins))
                {
                    c.HistoryRepository = new DbHistoryRepository();
                    c.RunDataLossOperation = DataLossOperation.All;
                    c.AutoMigrate();
                }
                using (var c = new RafyDbMigrationContext(UnitTestEntityRepositoryDataProvider.DbSettingName))
                {
                    c.HistoryRepository = new DbHistoryRepository();
                    c.RunDataLossOperation = DataLossOperation.All;
                    c.AutoMigrate();
                }
                using (RdbDataProvider.RedirectDbSetting(
                    UnitTestEntityRepositoryDataProvider.DbSettingName,
                    UnitTestEntityRepositoryDataProvider.DbSettingName_Duplicate
                    ))
                {
                    using (var c = new RafyDbMigrationContext(UnitTestEntityRepositoryDataProvider.DbSettingName_Duplicate))
                    {
                        c.ClassMetaReader.EntityDbSettingName = UnitTestEntityRepositoryDataProvider.DbSettingName;
                        c.ClassMetaReader.IsGeneratingForeignKey = false;
                        c.HistoryRepository = new DbHistoryRepository();
                        c.RunDataLossOperation = DataLossOperation.All;
                        c.AutoMigrate();
                    }
                }
                using (var c = new RafyDbMigrationContext(UnitTest2EntityRepositoryDataProvider.DbSettingName))
                {
                    c.HistoryRepository = new DbHistoryRepository();
                    c.RunDataLossOperation = DataLossOperation.All;
                    c.AutoMigrate();
                }
                using (var c = new RafyDbMigrationContext(StringTestEntityDataProvider.DbSettingName))
                {
                    c.HistoryRepository = new DbHistoryRepository();
                    c.RunDataLossOperation = DataLossOperation.All;
                    c.AutoMigrate();
                }

                //启用批量更新
                var dbSetting = DbSetting.FindOrCreate(UnitTestEntityRepositoryDataProvider.DbSettingName);
                if (DbSetting.IsOracleProvider(dbSetting))
                {
                    OracleBatchImporter.EnableBatchSequence(RF.ResolveInstance<BookRepository>());
                    OracleBatchImporter.EnableBatchSequence(RF.ResolveInstance<InvoiceRepository>());
                }
            }
        }

        public static void DropAllTables()
        {
            using (var c = new RafyDbMigrationContext(DbSettingNames.DbMigrationHistory))
            {
                c.RunDataLossOperation = DataLossOperation.All;
                c.MigrateTo(new DestinationDatabase(DbSettingNames.DbMigrationHistory));
            }
            using (var c = new RafyDbMigrationContext(DbSettingNames.RafyPlugins))
            {
                c.RunDataLossOperation = DataLossOperation.All;
                c.MigrateTo(new DestinationDatabase(DbSettingNames.RafyPlugins));
            }
            using (var c = new RafyDbMigrationContext(UnitTestEntityRepositoryDataProvider.DbSettingName))
            {
                c.RunDataLossOperation = DataLossOperation.All;
                c.MigrateTo(new DestinationDatabase(UnitTestEntityRepositoryDataProvider.DbSettingName));
            }
            using (RdbDataProvider.RedirectDbSetting(
                UnitTestEntityRepositoryDataProvider.DbSettingName,
                UnitTestEntityRepositoryDataProvider.DbSettingName_Duplicate
                ))
            {
                using (var c = new RafyDbMigrationContext(UnitTestEntityRepositoryDataProvider.DbSettingName_Duplicate))
                {
                    c.ClassMetaReader.EntityDbSettingName = UnitTestEntityRepositoryDataProvider.DbSettingName;
                    c.ClassMetaReader.IsGeneratingForeignKey = false;
                    c.RunDataLossOperation = DataLossOperation.All;
                    c.MigrateTo(new DestinationDatabase(UnitTestEntityRepositoryDataProvider.DbSettingName_Duplicate));
                }
            }
            using (var c = new RafyDbMigrationContext(UnitTest2EntityRepositoryDataProvider.DbSettingName))
            {
                c.RunDataLossOperation = DataLossOperation.All;
                c.MigrateTo(new DestinationDatabase(UnitTest2EntityRepositoryDataProvider.DbSettingName));
            }
            using (var c = new RafyDbMigrationContext(StringTestEntityDataProvider.DbSettingName))
            {
                c.RunDataLossOperation = DataLossOperation.All;
                c.MigrateTo(new DestinationDatabase(StringTestEntityDataProvider.DbSettingName));
            }
        }
    }
}
