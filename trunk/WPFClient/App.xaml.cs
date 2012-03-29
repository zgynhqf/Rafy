using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using OEA.Module.WPF;
using OEA.Module.WPF.Shell;
using System.Windows.Threading;
using OEA;

namespace WPFClient
{
    public partial class App : Application, IClientAppRuntime
    {
        public App()
        {
            OEAEnvironment.Provider.IsDebuggingEnabled = true;

            ClientApp.Register(this);
            this.DispatcherUnhandledException += OnDispatcherUnhandledException;
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
