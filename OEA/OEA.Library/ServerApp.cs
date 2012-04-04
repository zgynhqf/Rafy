/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110927
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20110927
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.Library;
using System.Runtime.Serialization;
using System.ComponentModel;
using System.IO;
using OEA.MetaModel;
using OEA.ManagedProperty;
using System.Configuration;

namespace OEA.Server
{
    public class ServerApp : AppImplementationBase
    {
        #region 构造函数 & 注册

        /// <summary>
        /// 注册服务端应用程序。
        /// 在服务端 Application 类的构造函数中调用此法。
        /// 
        /// 不能继承自类的类型才使用此方法，否则建议继承此类。
        /// </summary>
        /// <param name="serverApp"></param>
        public static void Register(IServerAppRuntime serverApp)
        {
            new ServerApp().AttachTo(serverApp);
        }

        private IServerAppRuntime _runtime;

        protected ServerApp() { }

        private void AttachTo(IServerAppRuntime runtime)
        {
            if (runtime == null) throw new ArgumentNullException("runtime");

            runtime.AppStartup += (s, e) => this.OnAppStartup();
            runtime.AppExit += (s, e) => this.OnExit();

            this._runtime = runtime;
        }

        #endregion

        protected override void StartMainProcess()
        {
            var cancelArg = new CancelEventArgs();
            this.OnMainWindowShowing(cancelArg);
            if (!cancelArg.Cancel) { this._runtime.ShowMainWindow(); }
        }

        public override void Shutdown()
        {
            this._runtime.Shutdown();
        }

        public override void ShowMessage(string message, string title)
        {
            this._runtime.ShowMessage(message, title);
        }
    }
}