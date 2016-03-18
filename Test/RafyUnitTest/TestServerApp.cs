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

namespace RafyUnitTest
{
    public class TestServerApp : AppImplementationBase
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

            RafyEnvironment.Provider.IsDebuggingEnabled = true;

            //故意把下面两个插件的位置放反。测试 Config 中配置插件的顺序是否成功。
            RafyEnvironment.DomainPlugins.Add(new EntityPhantomPlugin());
            RafyEnvironment.DomainPlugins.Add(new StampPlugin());

            RafyEnvironment.DomainPlugins.Add(new UnitTestPlugin());
            RafyEnvironment.DomainPlugins.Add(new UnitTestDataProviderPlugin());
            RafyEnvironment.DomainPlugins.Add(new UnitTestIDataProviderPlugin());
            RafyEnvironment.DomainPlugins.Add(new UnitTestRepoPlugin());
            RafyEnvironment.DomainPlugins.Add(new DCPlugin());

            RafyEnvironment.DomainPlugins.Add(new Rafy.SerialNumber.SerialNumberPlugin());

            ////为了多次修改 Location 值，需要把修改值的操作放到 InitEnvironment 中。
            //RafyEnvironment.Location.IsWebUI = false;
            //RafyEnvironment.Location.IsWPFUI = false;
            //RafyEnvironment.Location.DataPortalMode = DataPortalMode.ConnectDirectly;

            DataSaver.SubmitInterceptors.Add(typeof(Rafy.Domain.Stamp.StampSubmitInterceptor));

            base.InitEnvironment();
        }

        protected override void OnRuntimeStarting()
        {
            TestDbGenerator.GenerateDb();

            base.OnRuntimeStarting();
        }
    }
}