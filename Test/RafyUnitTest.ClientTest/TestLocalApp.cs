using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rafy.Domain;
using Rafy.Domain.Caching;
using Rafy.UnitTest;
using Rafy.UnitTest.DataProvider;
using Rafy.UnitTest.IDataProvider;
using Rafy.UnitTest.Repository;
using Rafy.UnitTest.WPF;
using Rafy.Utils.Caching;
using Rafy.WPF.Shell;
using Rafy.ComponentModel;
using Rafy.ComponentModel.UnityAdapter;
using Rafy.Domain.EntityPhantom;

namespace RafyUnitTest.ClientTest
{
    public class TestLocalApp : ClientApp
    {
        internal void Start()
        {
            this.StartupApplication();
        }

        protected override void InitEnvironment()
        {
            Logger.EnableSqlObervation = true;

            ConnectionStringNames.RafyPlugins = "Test_RafyPlugins";
            ConnectionStringNames.DbMigrationHistory = "Test_DbMigrationHistory";

            var sqlceFile = @"Rafy_Disk_Cache.sdf";
            CacheInstance.Disk = new Cache(new SQLCompactCacheProvider(sqlceFile));
            CacheInstance.MemoryDisk = new Cache(new HybirdCacheProvider(sqlceFile));

            RafyEnvironment.Provider.IsDebuggingEnabled = true;

            PluginTable.DomainLibraries.AddPlugin<EntityPhantomPlugin>();
            PluginTable.DomainLibraries.AddPlugin<UnityAdapterPlugin>();

            PluginTable.DomainLibraries.AddPlugin<UnitTestPlugin>();
            PluginTable.DomainLibraries.AddPlugin<UnitTestDataProviderPlugin>();
            PluginTable.DomainLibraries.AddPlugin<UnitTestIDataProviderPlugin>();
            PluginTable.DomainLibraries.AddPlugin<UnitTestRepoPlugin>();
            PluginTable.DomainLibraries.AddPlugin<DCPlugin>();
            PluginTable.UILibraries.AddPlugin<UnitTestWPFPlugin>();

            base.InitEnvironment();
        }

        protected override void OnRuntimeStarting()
        {
            TestDbGenerator.GenerateDb();

            base.OnRuntimeStarting();
        }

        protected override void StartMainProcess()
        {
            //不需要启动主窗口。
            //base.StartMainProcess();
        }
    }
}
