/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110928
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20110928
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Rafy;
using Rafy.ComponentModel;
using Rafy.DbMigration;
using Rafy.Domain;
using Rafy.Domain.Caching;
using Rafy.Domain.EntityPhantom;
using Rafy.Domain.ORM.DbMigration;
using Rafy.Domain.Stamp;
using Rafy.MetaModel;
using Rafy.UnitTest;
using Rafy.UnitTest.DataProvider;
using Rafy.UnitTest.IDataProvider;
using Rafy.UnitTest.Repository;
using UT;
using Rafy.FileStorage;
using Rafy.SerialNumber;
using Rafy.Accounts;
using Rafy.DataArchiver;
using Rafy.SystemSettings;

namespace RafyUnitTest
{
    public class TestServerApp : AppImplementationBase
    {
        internal void Start()
        {
            this.StartupApplication();

            RafyEnvironment.LoadPlugin(typeof(UnitTestDataProviderPlugin).Assembly);
            RafyEnvironment.LoadPlugin(typeof(UnitTestIDataProviderPlugin).Assembly);
            RafyEnvironment.LoadPlugin(typeof(UnitTestRepoPlugin).Assembly);
        }

        protected override void InitEnvironment()
        {
            Logger.EnableSqlObervation = true;

            DbSettingNames.RafyPlugins = "Test_RafyPlugins";
            DbSettingNames.DbMigrationHistory = "Test_DbMigrationHistory";

            RafyEnvironment.Provider.IsDebuggingEnabled = true;

            //故意把下面两个插件的位置放反。测试 Config 中配置插件的顺序是否成功。
            RafyEnvironment.Plugins.Add(new EntityPhantomPlugin());
            RafyEnvironment.Plugins.Add(new StampPlugin());

            RafyEnvironment.Plugins.Add(new UnitTestPlugin());
            //RafyEnvironment.DomainPlugins.Add(new UnitTestDataProviderPlugin());//load as required
            //RafyEnvironment.DomainPlugins.Add(new UnitTestIDataProviderPlugin());//load as required
            //RafyEnvironment.DomainPlugins.Add(new UnitTestRepoPlugin());//load as required

            //load as required, config in appsetting.json
            //RafyEnvironment.DomainPlugins.Add(new DCPlugin());
            //RafyEnvironment.DomainPlugins.Add(new AccountsPlugin());
            //RafyEnvironment.DomainPlugins.Add(new SystemSettingsPlugin());
            //RafyEnvironment.DomainPlugins.Add(new SerialNumberPlugin());
            //RafyEnvironment.DomainPlugins.Add(new FileStoragePlugin());

            //RafyEnvironment.DomainPlugins.Add(new RoleManagementPlugin());
            //RafyEnvironment.DomainPlugins.Add(new GroupManagementPlugin());
            //RafyEnvironment.DomainPlugins.Add(new UserRoleManagementPlugin());
            //RafyEnvironment.DomainPlugins.Add(new DataPermissionManagementPlugin());

            ////为了多次修改 Location 值，需要把修改值的操作放到 InitEnvironment 中。
            //RafyEnvironment.Location.IsWebUI = false;
            //RafyEnvironment.Location.IsWPFUI = false;
            //RafyEnvironment.Location.DataPortalMode = DataPortalMode.ConnectDirectly;

            base.InitEnvironment();
        }

        protected override void OnRuntimeStarting()
        {
            TestDbGenerator.GenerateDb();

            base.OnRuntimeStarting();
        }
    }
}