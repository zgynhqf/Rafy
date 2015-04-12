using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy;
using Rafy.ComponentModel;
using Rafy.DbMigration;
using Rafy.Domain;
using Rafy.Domain.ORM.DbMigration;

namespace $domainNamespace$
{
    class Program
    {
        static void Main(string[] args)
        {
            //启动领域项目
            new $domainNamespace$App().Startup();

            //在领域项目启动后，就可以使用领域模型了：
            //var repo = RF.Concrete<XXXRepository>();
            //repo.Save(new XXX { Name = "Name" });
            //var items = repo.CountAll();
            //Console.WriteLine("实体存储成功，目前数据库中存在 {0} 条数据。", items);
            //var list = repo.GetAll();
            //foreach (var item in list)
            //{
            //    Console.WriteLine(item.Name);
            //}
        }
    }

    class $domainNamespace$App : DomainApp
    {
        protected override void InitEnvironment()
        {
            PluginTable.DomainLibraries.AddPlugin<$domainNamespace$Plugin>();

            base.InitEnvironment();
        }

        protected override void OnRuntimeStarting()
        {
            base.OnRuntimeStarting();
    
            if (ConfigurationHelper.GetAppSettingOrDefault("$domainNamespace$_AutoUpdateDb", true))
            {
                var svc = ServiceFactory.Create<MigrateService>();
                svc.Options = new MigratingOptions
                {
                    //ReserveHistory = true,//ReserveHistory 表示是否需要保存所有数据库升级的历史记录
                    RunDataLossOperation = DataLossOperation.All,//要支持数据库表、字段的删除操作，取消本行注释。
                    Databases = new string[] { $domainNamespace$EntityRepositoryDataProvider.DbSettingName }
                };
                svc.Invoke();
            }
        }
    }
}
