/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120310
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120310
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace OEA.MetaModel
{
    /// <summary>
    /// 这个类为 ClientApp、ServerApp、WebApp 等类提供了一致的基类。
    /// </summary>
    public abstract class AppImplementationBase : IServerApp, IClientApp
    {
        protected void OnAppStartup()
        {
            this.InitEnvironment();

            this.InitAllPlugins();
            this.OnAllPluginsIntialized();

            OEAEnvironment.InitManagedProperties();
            OEAEnvironment.NotifyIntialized();

            this.InitEntityMeta();
            this.OnEntityMetasInitialized();

            this.OnAllPluginsMetaIntialized();

            //定义模块列表
            this.RaiseModuleOpertions();
            this.OnModuleOpertionsCompleted();

            this.RaiseModelOpertions();
            this.OnModelOpertionsCompleted();

            CommonModel.Modules.Freeze();
            CommonModel.Entities.Freeze();
            this.OnAppModelCompleted();

            this.RaiseDbMigratingOperations();

            this.StartMainProcess();

            this.OnStartupCompleted();
        }

        protected virtual void InitEnvironment()
        {
            OEAEnvironment.InitCustomizationPath();

            OEAEnvironment.InitApp(this);
        }

        /// <summary>
        /// 初始化所有Plugins
        /// </summary>
        protected virtual void InitAllPlugins()
        {
            OEAEnvironment.StartupEntityPlugins();
        }

        protected virtual void InitEntityMeta()
        {
            CommonModel.InitEntityMetas();
        }

        protected abstract void StartMainProcess();

        protected void OnAppExit()
        {
            this.OnExit();
        }

        #region IClientApp IServerApp 的成员

        public virtual void ShowMessage(string message, string title) { }

        public virtual void Shutdown() { }

        #endregion

        #region IClientApp IServerApp 的事件

        public event EventHandler AllPluginsIntialized;

        protected virtual void OnAllPluginsIntialized()
        {
            var handler = this.AllPluginsIntialized;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public event EventHandler Composed;

        protected virtual void OnComposed()
        {
            var handler = this.Composed;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public event EventHandler EntityMetasInitialized;

        protected virtual void OnEntityMetasInitialized()
        {
            var handler = this.EntityMetasInitialized;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public event EventHandler CommandMetasIntialized;

        protected virtual void OnCommandMetasIntialized()
        {
            var handler = this.CommandMetasIntialized;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public event EventHandler AllPluginsMetaIntialized;

        protected virtual void OnAllPluginsMetaIntialized()
        {
            var handler = this.AllPluginsMetaIntialized;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public event EventHandler ModuleOperations;

        protected virtual void RaiseModuleOpertions()
        {
            var handler = this.ModuleOperations;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public event EventHandler ModuleOperationsCompleted;

        protected virtual void OnModuleOpertionsCompleted()
        {
            var handler = this.ModuleOperationsCompleted;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public event EventHandler ModelOpertions;

        protected virtual void RaiseModelOpertions()
        {
            var handler = this.ModelOpertions;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public event EventHandler ModelOpertionsCompleted;

        protected virtual void OnModelOpertionsCompleted()
        {
            var handler = this.ModelOpertionsCompleted;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public event EventHandler AppModelCompleted;

        protected virtual void OnAppModelCompleted()
        {
            var handler = this.AppModelCompleted;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public event EventHandler DbMigratingOperations;

        protected virtual void RaiseDbMigratingOperations()
        {
            var handler = this.DbMigratingOperations;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public event EventHandler Exit;

        protected virtual void OnExit()
        {
            var handler = this.Exit;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public event EventHandler LoginSuccessed;

        protected virtual void OnLoginSuccessed()
        {
            var handler = this.LoginSuccessed;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public event CancelEventHandler MainWindowShowing;

        protected virtual void OnMainWindowShowing(CancelEventArgs e)
        {
            var handler = this.MainWindowShowing;
            if (handler != null) handler(this, e);
        }

        public event EventHandler MainWindowLoaded;

        protected virtual void OnMainWindowLoaded()
        {
            var handler = this.MainWindowLoaded;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public event EventHandler LoginFailed;

        protected virtual void OnLoginFailed()
        {
            var handler = this.LoginFailed;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public event EventHandler StartupCompleted;

        protected virtual void OnStartupCompleted()
        {
            var handler = this.StartupCompleted;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        #endregion
    }
}