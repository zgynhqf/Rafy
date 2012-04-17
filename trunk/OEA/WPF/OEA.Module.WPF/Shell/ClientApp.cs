/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110323
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100323
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Common;
using Microsoft.Practices.Unity;
using OEA.Library;
using OEA.ManagedProperty;
using OEA.MetaModel;
using OEA.MetaModel.View;
using OEA.Module.WPF;
using OEA.Server;
using OEA.Utils;
using OEA.WPF.Command;

namespace OEA.Module.WPF.Shell
{
    public class ClientApp : AppImplementationBase
    {
        #region 构造函数 & 注册

        private IClientAppRuntime _wpfApp;

        private ClientApp() { }

        /// <summary>
        /// 注册客户端应用程序。
        /// 在 WPF Application 类的构造函数中调用此法。
        /// </summary>
        /// <param name="wpfClientApp"></param>
        public static void Register(IClientAppRuntime wpfClientApp)
        {
            new ClientApp().AttachTo(wpfClientApp);
        }

        private void AttachTo(IClientAppRuntime wpfApp)
        {
            if (wpfApp == null) throw new ArgumentNullException("wpfApp");

            wpfApp.Startup += (s, e) => this.OnAppStartup();
            wpfApp.Exit += (s, e) => this.OnExit();

            this._wpfApp = wpfApp;
        }

        #endregion

        protected override void InitEnvironment()
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("zh-CHS");
            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement),
                new FrameworkPropertyMetadata(XmlLanguage.GetLanguage("zh-CN")));

            OEAEnvironment.Location = ApplicationContext.DataPortalProxy == "Local" ?
                OEALocation.LocalVersion : OEALocation.Client;

            base.InitEnvironment();

            this.ModifyPrivateBinPath();
        }

        protected override void InitAllPlugins()
        {
            base.InitAllPlugins();

            OEAEnvironment.StartupModulePlugins();
        }

        protected override void InitEntityMeta()
        {
            //Command 需要在实体前初始化好
            this.InitCommandMetas();

            base.InitEntityMeta();
        }

        private void InitCommandMetas()
        {
            UIModel.WPFCommands.AddByAssembly(typeof(ClientApp).Assembly);

            UIModel.InitCommandMetas();

            //UIModel.Commands.SortByName(CommandNames.CommonCommands);
            //UIModel.Commands.SortByLabel("添加", "编辑", "删除", "保存", "刷新");

            if (OEAEnvironment.IsDebuggingEnabled)
            {
                WPFCommandNames.TreeCommands.Insert(0, WPFCommandNames.CustomizeUI);
                WPFCommandNames.CommonCommands.Insert(0, WPFCommandNames.CustomizeUI);
            }

            this.OnCommandMetasIntialized();
        }

        protected override void OnAllPluginsMetaIntialized()
        {
            UIModel.NotifyPluginsMetaIntialized();

            base.OnAllPluginsMetaIntialized();
        }

        protected override void RaiseModuleOpertions()
        {
            //Module 的初始化在 Compose 之前执行，这样就能保证其中的内容也能参加到 Compose 过程中。
            this.Compose();
            this.OnComposed();

            base.RaiseModuleOpertions();
        }

        protected override void OnAppModelCompleted()
        {
            UIModel.Freeze();

            base.OnAppModelCompleted();
        }

        protected override void RaiseDbMigratingOperations()
        {
            if (OEAEnvironment.Location == OEALocation.LocalVersion)
            {
                base.RaiseDbMigratingOperations();
            }
        }

        protected override void StartMainProcess()
        {
            if (this.TryLogin())
            {
                this.OnLoginSuccessed();

                this.ShowSplashScreen();

                var cancelArg = new CancelEventArgs();
                this.OnMainWindowShowing(cancelArg);
                if (!cancelArg.Cancel)
                {
                    this.ShowMainWindow();
                }
            }
            else
            {
                this.OnLoginFailed();

                this.Shutdown();
            }
        }

        protected override void OnExit()
        {
            if (this._container != null) { this._container.Dispose(); }

            base.OnExit();
        }

        #region 重点私有方法

        private CompositionContainer _container;

        /// <summary>
        /// 组合所有的 Module
        /// </summary>
        /// <returns></returns>
        private void Compose()
        {
            var catalog = new AggregateCatalog();

            catalog.Catalogs.Add(new AssemblyCatalog(this.GetType().Assembly));
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(UIModel).Assembly));
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(AutoUI).Assembly));
            catalog.Catalogs.Add(new AssemblyCatalog(System.Reflection.Assembly.GetEntryAssembly()));
            if (Directory.Exists(@".\Module")) catalog.Catalogs.Add(new DirectoryCatalog(@".\Module"));

            //加入所有客户化的DLL
            foreach (var dir in OEAEnvironment.GetModuleDlls())
            {
                if (Directory.Exists(dir)) catalog.Catalogs.Add(new DirectoryCatalog(dir));
            }

            this._container = new CompositionContainer(catalog);
            this._container.Compose(new CompositionBatch());

            OEA.Module.WPF.App.Current.InitCompositionContainer(this._container);
        }

        /// <summary>
        /// 根据客户化的信息，动态设置私有程序集路径
        /// 1、不设置的话会出现找不到加载的dll的情况
        /// 2、直接SetData不起作用，需要手动刷新下
        /// </summary>
        private void ModifyPrivateBinPath()
        {
            var dlls = OEAEnvironment.GetEntityDlls(false);
            if (dlls.Length > 0)
            {
                var pathes = new List<string> { 
                    "Library", "Module", "Files", Path.GetDirectoryName(dlls[0]) + @"\Library"
                };

                var path = string.Join(";", pathes);

                PathHelper.ModifyPrivateBinPath(path);
            }
        }

        private void ShowMainWindow()
        {
            var mainWin = OEA.Module.WPF.App.Current.MainWindow;
            if (Application.Current != null)
            {
                Application.Current.MainWindow = mainWin;
                mainWin.Show();
                mainWin.Loaded += (o, e) => this.OnMainWindowLoaded();
            }
        }

        /// <summary>
        /// 弹出登录窗口，并返回是否登录成功
        /// </summary>
        /// <returns></returns>
        private bool TryLogin()
        {
            this._wpfApp.ShutdownMode = ShutdownMode.OnExplicitShutdown;  //防止登录后Application退出

            var loginWindow = this._container.TryGetExportedLastVersionValue<Window>(ComposableNames.LoginWindow);
            var result = loginWindow.ShowDialog().GetValueOrDefault();

            this._wpfApp.ShutdownMode = ShutdownMode.OnMainWindowClose;

            return result;
        }

        #endregion

        #region 其它方法

        public override void Shutdown()
        {
            App.Current.TryShutdown();
        }

        public override void ShowMessage(string message, string title)
        {
            App.MessageBox.Show(message, title);
        }

        /// <summary>
        /// 闪屏
        /// </summary>
        private void ShowSplashScreen()
        {
            //闪屏不支持内容文件
            //var splashScreen = new SplashScreen("images/login_g.jpg");
            //splashScreen.Show(true);

            var log = OEAEnvironment.CustomerProvider.FindCustomerFile("Images/ProductSplash.jpg");
            if (log != null)
            {
                var img = new BitmapImage(new Uri(log));

                var window = new Window()
                {
                    Content = new Image()
                    {
                        Source = img
                    },
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    WindowStyle = WindowStyle.None,
                    BorderThickness = new Thickness(0d),
                    ShowInTaskbar = false,
                    Width = 500,
                    Height = 300,
                    ResizeMode = ResizeMode.NoResize
                };
                window.Show();

                Application.Current.MainWindow = null;

                Action<Window> action = w => w.Close();
                Dispatcher.CurrentDispatcher.BeginInvoke(action, DispatcherPriority.Loaded, window);
            }
        }

        #endregion
    }

    /// <summary>
    /// 为了兼容单元测试，提取此接口
    /// </summary>
    public interface IClientAppRuntime
    {
        ShutdownMode ShutdownMode { get; set; }

        /// <summary>
        /// 应用程序启动时发生此事件。
        /// </summary>
        event StartupEventHandler Startup;

        /// <summary>
        /// 应用程序关闭时发生此事件。
        /// </summary>
        event ExitEventHandler Exit;
    }
}