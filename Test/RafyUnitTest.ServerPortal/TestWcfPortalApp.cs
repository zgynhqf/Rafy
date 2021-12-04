/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20211124
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20211124 12:23
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
using Rafy.Data;
using Rafy.ComponentModel.UnityAdapter;

namespace RafyUnitTest.ServerPortal
{
    public class TestWcfPortalApp : DomainApp
    {
        internal void Start()
        {
            Logger.SetImplementation(new FileLogger
            {
                WriteConcole = true,
            });

            this.StartupApplication();

            RafyEnvironment.EnsureAllPluginsLoaded();
        }

        protected override void InitEnvironment()
        {
            DbAccesserInterceptor.ObserveSql = true;

            DbSettingNames.RafyPlugins = "Test_RafyPlugins";
            DbSettingNames.DbMigrationHistory = "Test_DbMigrationHistory";

            RafyEnvironment.Provider.IsDebuggingEnabled = true;

            RafyEnvironment.Plugins.Add(new StampPlugin());
            RafyEnvironment.Plugins.Add(new EntityPhantomPlugin());
            RafyEnvironment.Plugins.Add(new UnityAdapterPlugin());
            RafyEnvironment.Plugins.Add(new UnitTestPlugin());

            base.InitEnvironment();
        }

        protected override void OnRuntimeStarting()
        {
            TestDbGenerator.GenerateDb();

            base.OnRuntimeStarting();
        }
    }
}