using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;
using OEA;
using OEA.Library;
using OEA.MetaModel;
using OEA.RBAC;
using OEA.Server;

namespace ConsoleHost
{
    class Program : AppImplementationBase
    {
        static void Main(string[] args)
        {
            OEAEnvironment.Provider.IsDebuggingEnabled = ConfigurationHelper.GetAppSettingOrDefault("IsDebuggingEnabled", false);

            new Program().OnAppStartup();

            Console.ReadLine();
        }

        protected override void StartMainProcess()
        {
            //本项目中使用了一个 RBAC 的插件，
            //同时在应用层引用 RBAC.dll 即可使用。
            //（注意，只是简单引用，不拷贝到根目录，还放在插件目录。即引用的 CopyLocal = False。）
            var users = RF.Concreate<UserRepository>().GetAll();
        }
    }
}