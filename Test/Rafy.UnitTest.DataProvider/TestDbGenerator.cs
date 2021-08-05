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
        public static bool ForceAllPluginsLoaded = false;

        private static bool ClearDb = true;

        public static void GenerateDb()
        {
            if (ConfigurationHelper.GetAppSettingOrDefault("Test_GenerateDb", false))
            {
                //生成数据库时，为简单起见，需要先加载所有的插件。
                //注意，此行代码会导致按需加载的单元测试全部通过。
                //所以平时应该通过配置文件来关闭数据库生成功能。
                RafyEnvironment.EnsureAllPluginsLoaded();
                ForceAllPluginsLoaded = true;

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
