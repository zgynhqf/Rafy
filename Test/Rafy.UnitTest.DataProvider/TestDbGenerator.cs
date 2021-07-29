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
        private static bool ClearDb = true;

        public static void GenerateDb()
        {
            if (ConfigurationHelper.GetAppSettingOrDefault("Test_GenerateDb", false))
            {
                if (ClearDb && ConfigurationHelper.GetAppSettingOrDefault("Test_GenerateDb_Clear", false))
                {
                    //不想手工去删除数据库，可以使用下面这个方法来在程序中删除所有的表。
                    DropAllTables();

                    ClearDb = false;
                }

                using (var c = new RafyDbMigrationContext(DbSettingNames.DbMigrationHistory))
                {
                    c.RunDataLossOperation = DataLossOperation.All;
                    c.AutoMigrate();
                }
                using (var c = new RafyDbMigrationContext(DbSettingNames.RafyPlugins))
                {
                    if (c.DbSetting.ProviderName != DbSetting.Provider_SQLite)
                    {
                        c.HistoryRepository = new DbHistoryRepository();
                    }
                    c.RunDataLossOperation = DataLossOperation.All;
                    c.AutoMigrate();
                }
                using (var c = new RafyDbMigrationContext(UnitTestEntityRepositoryDataProvider.DbSettingName))
                {
                    if (c.DbSetting.ProviderName != DbSetting.Provider_SQLite)
                    {
                        c.HistoryRepository = new DbHistoryRepository();
                    }
                    c.RunDataLossOperation = DataLossOperation.All;
                    c.AutoMigrate();
                }
                UnitTestPlugin.InitailizeSequences();

                using (RdbDataProvider.RedirectDbSetting(
                    UnitTestEntityRepositoryDataProvider.DbSettingName,
                    UnitTestEntityRepositoryDataProvider.DbSettingName_Duplicate
                    ))
                {
                    using (var c = new RafyDbMigrationContext(UnitTestEntityRepositoryDataProvider.DbSettingName_Duplicate))
                    {
                        c.ClassMetaReader.EntityDbSettingName = UnitTestEntityRepositoryDataProvider.DbSettingName;
                        c.ClassMetaReader.IsGeneratingForeignKey = false;
                        if (c.DbSetting.ProviderName != DbSetting.Provider_SQLite)
                        {
                            c.HistoryRepository = new DbHistoryRepository();
                        }
                        c.RunDataLossOperation = DataLossOperation.All;
                        c.AutoMigrate();
                    }
                    UnitTestPlugin.InitailizeSequences();
                }
                using (var c = new RafyDbMigrationContext(UnitTest2EntityRepositoryDataProvider.DbSettingName))
                {
                    if (c.DbSetting.ProviderName != DbSetting.Provider_SQLite)
                    {
                        c.HistoryRepository = new DbHistoryRepository();
                    }
                    c.RunDataLossOperation = DataLossOperation.All;
                    c.AutoMigrate();
                }
                using (var c = new RafyDbMigrationContext(StringTestEntityDataProvider.DbSettingName))
                {
                    if (c.DbSetting.ProviderName != DbSetting.Provider_SQLite)
                    {
                        c.HistoryRepository = new DbHistoryRepository();
                    }
                    c.RunDataLossOperation = DataLossOperation.All;
                    c.AutoMigrate();
                }
            }
        }

        public static void DropAllTables()
        {
            using (var c = new RafyDbMigrationContext(DbSettingNames.DbMigrationHistory))
            {
                DropTables(c);
            }
            using (var c = new RafyDbMigrationContext(DbSettingNames.RafyPlugins))
            {
                DropTables(c);
            }
            using (var c = new RafyDbMigrationContext(UnitTestEntityRepositoryDataProvider.DbSettingName))
            {
                DropTables(c);
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
                    DropTables(c);
                }
            }
            using (var c = new RafyDbMigrationContext(UnitTest2EntityRepositoryDataProvider.DbSettingName))
            {
                DropTables(c);
            }
            using (var c = new RafyDbMigrationContext(StringTestEntityDataProvider.DbSettingName))
            {
                DropTables(c);
            }
        }

        private static void DropTables(RafyDbMigrationContext c)
        {
            c.RunDataLossOperation = DataLossOperation.All;
            c.MigrateTo(new DestinationDatabase(c.DbSetting.Name));
        }
    }
}
