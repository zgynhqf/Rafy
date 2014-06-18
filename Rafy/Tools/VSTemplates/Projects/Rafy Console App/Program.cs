using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy;
using Rafy.Domain;

namespace $domainNamespace$
{
    class Program
    {
        static void Main(string[] args)
        {
            //启动领域项目
            PluginTable.DomainLibraries.AddPlugin<$domainName$Plugin>();
            new DomainApp().Startup();

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
}
