using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using Rafy;
using Rafy.WPF;
using Rafy.WPF.Shell;

namespace WPFClient
{
    public partial class App : Application, IClientAppRuntime
    {
        public App()
        {
            var app = new WPFClientApp();
            app.AttachTo(this);

            this.DispatcherUnhandledException += OnDispatcherUnhandledException;
        }

        class WPFClientApp : ClientApp
        {
            protected override void InitEnvironment()
            {
                Rafy.WPF.App.MainWindowType = typeof(DefaultShell);
                ClientApp.LoginWindowType = typeof(DefaultLoginWindow);
                //ClientApp.SplashScreen = new SplashScreen("Shell/ProductSplash.jpg");

                RafyEnvironment.Provider.IsDebuggingEnabled = ConfigurationHelper.GetAppSettingOrDefault("WPFClient.IsDebuggingEnabled", false);

                base.InitEnvironment();
            }

            protected override void OnLoginSuccessed()
            {
                base.OnLoginSuccessed();

                //登录成功时，需要绑定主窗口的模块列表。
                var mainWin = App.Current.MainWindow as DefaultShell;
                if (mainWin != null)
                {
                    mainWin.ShowModules();
                }
            }
        }

        #region 处理异常

        /// <summary>
        /// 应用程序异常处理。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Exception.Alert();
            e.Handled = true;
        }

        #endregion
    }
}
