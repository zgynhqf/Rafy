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
using Rafy.Data;

namespace RafyUnitTest.ClientTest
{
    /// <summary>
    /// 客户端应用程序。
    /// 不支持生成数据库。（使用服务测试来生成数据库。）
    /// </summary>
    public class TestLocalApp : ClientApp
    {
        internal void Start()
        {
            this.StartupApplication();
        }

        protected override void InitEnvironment()
        {
            DbAccesserInterceptor.ObserveSql = true;

            DbSettingNames.RafyPlugins = "Test_RafyPlugins";
            DbSettingNames.DbMigrationHistory = "Test_DbMigrationHistory";

            var sqlceFile = @"Rafy_Disk_Cache.sdf";
            CacheInstances.Disk = new SQLCompactCache(sqlceFile);
            CacheInstances.MemoryDisk = new HybirdCache(sqlceFile);

            RafyEnvironment.Provider.IsDebuggingEnabled = true;

            RafyEnvironment.Plugins.Add(new StampPlugin());
            RafyEnvironment.Plugins.Add(new EntityPhantomPlugin());
            RafyEnvironment.Plugins.Add(new UnityAdapterPlugin());

            RafyEnvironment.Plugins.Add(new UnitTestPlugin());
            //RafyEnvironment.DomainPlugins.Add(new UnitTestDataProviderPlugin());
            //RafyEnvironment.DomainPlugins.Add(new UnitTestIDataProviderPlugin());
            //RafyEnvironment.DomainPlugins.Add(new UnitTestRepoPlugin());
            //RafyEnvironment.DomainPlugins.Add(new DiskCachingPlugin());

            RafyEnvironment.Plugins.Add(new UnitTestWPFPlugin());

            base.InitEnvironment();
        }

        protected override void StartMainProcess()
        {
            //不需要启动主窗口。
            //base.StartMainProcess();
        }
    }
}
