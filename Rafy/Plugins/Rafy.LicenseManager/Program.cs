/*******************************************************
 * 
 * 作者：宋军瑞
 * 创建日期：20160921
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.5
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 宋军瑞 20160921 10:00
 * 
*******************************************************/

using System;
using System.IO;
using System.Windows.Forms;
using Rafy.LicenseManager.Infrastructure;
using Rafy.LicenseManager.UI;

namespace Rafy.LicenseManager
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += _CurrentDomain_UnhandledException;

            //启动领域项目
            new LicenseManagerApp().Startup();

            //不能在 Main 方法中直接使用领域相关的类型，而是需要封装额外的方法（MainDomainProcess）。
            //这会导致 User 类型的静态构造函数在 ConsoleHostApp.Startup() 方法之前运行，
            //从而引发 System.Reflection.TargetInvocationException 异常。
            //var repo = RF.Concrete<UserRepository>();

            MainDomainProcess();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ManagerForm());
        }

        private static void _CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            if (exception != null)
            {
                using (var writer = File.AppendText($"{AppDomain.CurrentDomain.BaseDirectory}error.log"))
                {
                    writer.WriteLine(exception.Message);
                }

                throw new Exception("应用程序未处理异常。", exception);
            }
        }

        private static void MainDomainProcess()
        {
            ////本项目中使用了一个 RBAC 的插件，
            ////同时在应用层引用 RBAC.dll 即可使用。
            ////（注意，只是简单引用，不拷贝到根目录，还放在插件目录。即引用的 CopyLocal = False。）
            //var users = RF.Concrete<UserRepository>().GetAll();

            //LicenseManagerApp.GeneratorPublicPrivateKey();
        }
    }
}