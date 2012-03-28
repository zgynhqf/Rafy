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
using OEA.Server;
using System.Windows;
using System.Windows.Threading;

namespace OEAUnitTest
{
    public class TestApp : IServerAppRuntime//, IClientAppRuntime
    {
        public TestApp()
        {
            //ClientApp.Register(this);
            ServerApp.Register(this);
        }

        internal void Start()
        {
            var handler = this.AppStartup;
            if (handler != null) handler(this, EventArgs.Empty);

            var handler2 = this.Startup;
            if (handler2 != null) handler2(this, null);
        }

        #region 适配接口实现

        void IServerAppRuntime.Shutdown() { }

        void IServerAppRuntime.ShowMessage(string message, string title) { }

        void IServerAppRuntime.ShowMainWindow() { }

        public event EventHandler AppStartup;

        public event EventHandler AppExit;

        //ShutdownMode IClientAppRuntime.ShutdownMode { get; set; }

        public event StartupEventHandler Startup;

        public event ExitEventHandler Exit;

        public event DispatcherUnhandledExceptionEventHandler DispatcherUnhandledException;

        #endregion
    }
}