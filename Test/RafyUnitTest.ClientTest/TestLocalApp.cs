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
using Rafy.Domain.Stamp;

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

            DbSettingNames.RafyPlugins = "Test_RafyPlugins";
            DbSettingNames.DbMigrationHistory = "Test_DbMigrationHistory";

            var sqlceFile = @"Rafy_Disk_Cache.sdf";
            CacheInstances.Disk = new SQLCompactCache(sqlceFile);
            CacheInstances.MemoryDisk = new HybirdCache(sqlceFile);

            RafyEnvironment.Provider.IsDebuggingEnabled = true;

            RafyEnvironment.DomainPlugins.Add(new StampPlugin());
            RafyEnvironment.DomainPlugins.Add(new EntityPhantomPlugin());
            RafyEnvironment.DomainPlugins.Add(new UnityAdapterPlugin());

            RafyEnvironment.DomainPlugins.Add(new UnitTestPlugin());
            RafyEnvironment.DomainPlugins.Add(new UnitTestDataProviderPlugin());
            RafyEnvironment.DomainPlugins.Add(new UnitTestIDataProviderPlugin());
            RafyEnvironment.DomainPlugins.Add(new UnitTestRepoPlugin());
            RafyEnvironment.DomainPlugins.Add(new DCPlugin());

            RafyEnvironment.DomainPlugins.Add(new UnitTestWPFPlugin());

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
