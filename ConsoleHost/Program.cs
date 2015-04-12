using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy;
using Rafy.ComponentModel;
using Rafy.DbMigration;
using Rafy.Domain;
using Rafy.Domain.ORM.DbMigration;
using Rafy.MetaModel;
using Rafy.RBAC;

namespace ConsoleHost
{
    class Program : AppImplementationBase
    {
        static void Main(string[] args)
        {
            //启动领域项目
            new ConsoleHostApp().Startup();

            ////本项目中使用了一个 RBAC 的插件，
            ////同时在应用层引用 RBAC.dll 即可使用。
            ////（注意，只是简单引用，不拷贝到根目录，还放在插件目录。即引用的 CopyLocal = False。）
            //var users = RF.Concrete<UserRepository>().GetAll();

            Console.WriteLine("执行完毕，回车退出。");
            Console.ReadLine();
        }
    }

    class ConsoleHostApp : DomainApp
    {
        protected override void InitEnvironment()
        {
            RafyEnvironment.Provider.IsDebuggingEnabled = ConfigurationHelper.GetAppSettingOrDefault("IsDebuggingEnabled", false);

            //PluginTable.DomainLibraries.AddPlugin<FundMngPlugin>();

            base.InitEnvironment();
        }

        protected override void OnRuntimeStarting()
        {
            base.OnRuntimeStarting();

            //if (ConfigurationHelper.GetAppSettingOrDefault("ConsoleHost_AutoUpdateDb", true))
            //{
            //    var svc = ServiceFactory.Create<MigrateService>();
            //    svc.Options = new MigratingOptions
            //    {
            //        //ReserveHistory = true,//ReserveHistory 表示是否需要保存所有数据库升级的历史记录
            //        RunDataLossOperation = DataLossOperation.All,//要支持数据库表、字段的删除操作，取消本行注释。
            //        Databases = new string[] { FundMngEntityRepositoryDataProvider.DbSettingName }
            //    };
            //    svc.Invoke();
            //}
        }
    }
}