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
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Rafy;
using Rafy.ComponentModel;
using Rafy.Domain;
using Rafy.DataPortal;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.MetaModel.View;
using Rafy.Threading;
using Rafy.UI;
using Rafy.Utils;
using Rafy.WPF;
using Rafy.WPF.Command;
using Rafy.WPF.Controls;

namespace Rafy.WPF.Shell
{
    public class ClientApp : UIApp, IClientApp
    {
        #region 构造函数 & 注册

        private IClientAppRuntime _wpfApp;

        public ClientApp() { }

        /// <summary>
        /// 注册客户端应用程序。
        /// 在 WPF Application 类的构造函数中调用此法。
        /// </summary>
        /// <param name="wpfApp"></param>
        public void AttachTo(IClientAppRuntime wpfApp)
        {
            if (wpfApp == null) throw new ArgumentNullException("wpfApp");

            wpfApp.Startup += (s, e) => this.Startup();
            wpfApp.Exit += (s, e) => this.ExitApp();

            this._wpfApp = wpfApp;
        }

        #endregion

        /// <summary>
        /// 判断是否在客户端
        /// 单机版，如果还没有进入数据门户中，则同样返回 true。
        /// </summary>
        /// <returns></returns>
        public override bool IsOnClient()
        {
            return !RafyEnvironment.ConnectDataDirectly ||
                !DataPortalApi.IsRunning;
                //RafyEnvironment.ThreadPortalCount == 0;
        }

        public override void Startup()
        {
            _wpfApp.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            if (this.EnableExceptionLog)
            {
                try
                {
                    this.StartupApplication();
                }
                catch (Exception e)
                {
                    var baseException = e.GetBaseException();
                    string msg = baseException.Message;
                    App.MessageBox.Show("程序启动时出现异常，程序即将退出……\r\n" + msg, "程序启动异常");

                    if (Debugger.IsAttached) { Debugger.Break(); }

                    Logger.LogError("程序启动时出现异常。", e);

                    //由于初始化时出现了异常，所以整个程序需要退出。
                    this.Shutdown();
                }
            }
            else
            {
                this.StartupApplication();
            }
        }

        protected void ExitApp()
        {
            //安全地执行一些 Exit 代码。
            try
            {
                this.OnExit();
            }
            catch (Exception ex)
            {
                //由于这里无法弹出窗口，所以简单地记录异常信息。
                Logger.LogError("系统退出代码异常", ex);
            }
        }

        private static bool LanguagePropertyOverrieded = false;

        protected override void InitEnvironment()
        {
            UIEnvironment.IsWPFUI = true;

            //客户端所有线程使用一个身份（上下文）；
            PrincipalAsyncHelper.DisableWrapping = true;
            AppContext.SetProvider(new StaticAppContextProvider());

            base.InitEnvironment();

            if (!LanguagePropertyOverrieded)
            {
                var language = XmlLanguage.GetLanguage(Thread.CurrentThread.CurrentUICulture.Name);
                FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(language));

                LanguagePropertyOverrieded = true;
            }

            this.ShowSplashScreen();

            this.ModifyPrivateBinPath();
        }

        protected override void CreateMeta()
        {
            base.CreateMeta();

            this.InitCommandMeta();
        }

        private void InitCommandMeta()
        {
            UIModel.WPFCommands.AddByAssembly(typeof(ClientApp).Assembly);

            UIModel.InitCommandMetas();

            //UIModel.Commands.SortByName(CommandNames.CommonCommands);
            //UIModel.Commands.SortByLabel("添加", "编辑", "删除", "保存", "刷新");

            this.OnCommandMetasIntialized();
        }

        protected override void OnMetaCreated()
        {
            UIModel.Freeze();

            base.OnMetaCreated();
        }

        protected override void OnRuntimeStarting()
        {
            base.OnRuntimeStarting();

            //运行时行为开始时，先设置好皮肤元素。
            SkinManager.Apply(WPFConfigHelper.Skin);
        }

        protected override void StartMainProcess()
        {
            this.ShowMainWindow();

            this.CloseSplashScreen();

            //由于登录成功后，Windows 不会把 MainWindow 激活，而是把上一个应用程序窗口激活。所以这里需要手动激活一个。
            App.Current.MainWindow.Activate();

            if (this.TryLogin())
            {
                //登录成功后，为当前用户生成相应的模块列表。
                App.Current.InitUserModules();

                this.OnLoginSuccessed();
            }
            else
            {
                this.OnLoginFailed();

                this.Shutdown();
            }
        }

        #region 客户端逻辑

        /// <summary>
        /// 设置本类型可以显示一个登录窗口。
        /// </summary>
        public static Type LoginWindowType;

        /// <summary>
        /// 根据客户化的信息，动态设置私有程序集路径
        /// 1、不设置的话会出现找不到加载的dll的情况
        /// 2、直接SetData不起作用，需要手动刷新下
        /// </summary>
        private void ModifyPrivateBinPath()
        {
            var dlls = UIEnvironment.GetCustomerEntityDlls(false);
            if (dlls.Length > 0)
            {
                var pathes = new List<string> {
                    "Library", "Module", "Files", Path.GetDirectoryName(dlls[0]) + @"\Library"
                };

                var path = string.Join(";", pathes);

                //ModifyPrivateBinPath
                PathHelper.ModifyPrivateBinPath(path);
            }
        }

        private void ShowMainWindow()
        {
            //初始化主窗口
            App.Current.InitMainWindow();

            var mainWin = App.Current.MainWindow;
            _wpfApp.MainWindow = mainWin;

            mainWin.Closed += (o, e) => { this.Shutdown(); };

            mainWin.Show();
        }

        /// <summary>
        /// 弹出登录窗口，并返回是否登录成功
        /// </summary>
        /// <returns></returns>
        private bool TryLogin()
        {
            if (LoginWindowType != null)
            {
                var loginWindow = Activator.CreateInstance(LoginWindowType) as Window;
                if (loginWindow == null) throw new InvalidProgramException("LoginWindowType 必须是一个 Window 类型。");

                loginWindow.Owner = _wpfApp.MainWindow;

                WPFHelper.SetTrackFocusScope(loginWindow);

                var result = loginWindow.ShowDialog().GetValueOrDefault();
                return result;
            }

            return true;
        }

        #endregion

        #region 闪屏

        /// <summary>
        /// 如果在客户端应用程序启动前设置了这个属性，则将会使用这个对象表示的图片来作为程序的闪屏。
        /// </summary>
        public static SplashScreen SplashScreen;

        /// <summary>
        /// 本属性用于表示闪屏对应的内容文件名。
        /// </summary>
        public static string SplashScreenContentFile = "Images/ProductSplash.jpg";

        private Window _splash;

        /// <summary>
        /// 闪屏
        /// </summary>
        private void ShowSplashScreen()
        {
            if (SplashScreen != null)
            {
                //闪屏类 SplashScreen 不支持内容文件，暂留
                //var splashScreen = new SplashScreen("images/login_g.jpg");
                SplashScreen.Show(false);
            }
            else
            {
                var log = UIEnvironment.BranchProvider.FindCustomerFile(SplashScreenContentFile);
                if (log != null)
                {
                    //创建并显示一个闪屏窗口
                    _splash = new Window()
                    {
                        Content = new Image()
                        {
                            Source = new BitmapImage(new Uri(log))
                        },
                        WindowStartupLocation = WindowStartupLocation.CenterScreen,
                        WindowStyle = WindowStyle.None,
                        BorderThickness = default(Thickness),
                        ShowInTaskbar = false,
                        Width = 500,
                        Height = 300,
                        Topmost = true,
                        ResizeMode = ResizeMode.NoResize
                    };
                    _splash.Show();

                    ////固定时间后关闭闪屏。
                    //var splashTimer = new DispatcherTimer();
                    //splashTimer.Interval = TimeSpan.FromMilliseconds(RafyEnvironment.Configuration.Section.WPF.SplashTimeSpan);
                    //splashTimer.Tick += (o, e) =>
                    //{
                    //    Action<Window> action = w => w.Close();
                    //    Dispatcher.CurrentDispatcher.BeginInvoke(action, _splash);

                    //    splashTimer.Stop();
                    //};
                    //splashTimer.Start();
                }
            }
        }

        private void CloseSplashScreen()
        {
            if (SplashScreen != null)
            {
                SplashScreen.Close(TimeSpan.FromSeconds(1d));
                //SplashScreen.Close(TimeSpan.FromMilliseconds(RafyEnvironment.Configuration.Section.WPF.SplashTimeSpan));
            }
            else if (_splash != null)
            {
                _splash.Close();
            }
        }

        #endregion

        #region 其它方法

        public void ShowMessage(string message, string title)
        {
            App.MessageBox.Show(message, title);
        }

        public void Shutdown()
        {
            _wpfApp.Shutdown();
        }

        #endregion

        #region IClientApp 成员

        public event EventHandler CommandMetaIntialized;

        protected virtual void OnCommandMetasIntialized()
        {
            var handler = this.CommandMetaIntialized;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public event EventHandler LoginSuccessed;

        protected virtual void OnLoginSuccessed()
        {
            var handler = this.LoginSuccessed;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public event EventHandler LoginFailed;

        protected virtual void OnLoginFailed()
        {
            var handler = this.LoginFailed;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        #endregion
    }

    /// <summary>
    /// 为了兼容单元测试，提取此接口
    /// </summary>
    public interface IClientAppRuntime
    {
        /// <summary>
        /// 应用程序关闭的模式。
        /// </summary>
        ShutdownMode ShutdownMode { get; set; }

        /// <summary>
        /// 应用程序的主窗体。
        /// </summary>
        Window MainWindow { get; set; }

        /// <summary>
        /// 关闭应用程序。
        /// </summary>
        void Shutdown();

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