/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110218
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100218
 * 
*******************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using Rafy.MetaModel;
using Rafy.MetaModel.View;
using Rafy.WPF.Controls;
using Rafy.Utils;
using System.Windows.Input;

namespace Rafy.WPF
{
    /// <summary>
    /// 客户端应用程序API集合
    /// 
    /// 考虑到上层开发人员经常使用此类，所以命名不再使用 RafyApplication/RafyApp/ClientApp
    /// 将来服务端的App类请使用 ServerApp 命名
    /// </summary>
    public partial class App
    {
        public static readonly App Current = new App();

        /// <summary>
        /// Windows 相关的 API
        /// </summary>
        public static readonly RafyWindows Windows = new RafyWindows();

        /// <summary>
        /// 弹出框相关的 API
        /// </summary>
        public static readonly RafyMessageBox MessageBox = new RafyMessageBox();

        #region Shutdown & MainWindow

        private Window _mainWindow;

        /// <summary>
        /// 应用程序的主窗体。
        /// </summary>
        public Window MainWindow
        {
            get { return this._mainWindow; }
        }

        /// <summary>
        /// 设置本属性可以设置主窗口的类型。
        /// </summary>
        public static Type MainWindowType;

        internal void InitMainWindow()
        {
            if (MainWindowType == null) throw new InvalidProgramException("插件必须设置 App.MainWindowType 来设置主窗口的类型。");

            _mainWindow = Activator.CreateInstance(MainWindowType) as Window;
            if (_mainWindow == null) { throw new InvalidOperationException("MainWindowType 必须是一个 Window 类型的窗体。"); }

            WPFHelper.SetTrackFocusScope(_mainWindow);

            _mainWindow.CommandBindings.Add(new CommandBinding(CloseWorkspaceWindowCommand, OnCloseWorkspaceWindowCommandExecuted));
            _mainWindow.InputBindings.Add(new InputBinding(CloseWorkspaceWindowCommand, new KeyGesture(Key.W, ModifierKeys.Control)));

            _mainWindow.Closing += (s, e) => e.Cancel = !this.TryShutdownAllWindows();
        }

        /// <summary>
        /// 关闭当前窗口或者指定窗口的命令
        /// </summary>
        public static readonly RoutedCommand CloseWorkspaceWindowCommand = new RoutedCommand("CloseWorkspaceWindowCommand", typeof(App));

        private static void OnCloseWorkspaceWindowCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            //如果是 windows 触发的命令，那么应该关闭当前窗口。
            //如果是其它界面元素触发的命令，则命令的参数就是需要关闭的窗口元素。
            var ws = App.Current.Workspace;
            var windowControl = e.Parameter as FrameworkElement;
            WorkspaceWindow window = null;
            if (windowControl != null)
            {
                window = WorkspaceWindow.GetWorkspaceWindow(windowControl);
            }
            else
            {
                window = ws.ActiveWindow;
            }
            if (window != null) { ws.TryRemove(window); }
        }

        /// <summary>
        /// 尝试关闭整个应用程序。返回是否成功。
        /// 
        /// 注意：
        /// * 程序可以处理 Shutingdown 事件来阻止关闭。
        /// * 程序可以阻止某一个打开的工作区页签关闭，来间接阻止整个应用程序关闭。
        /// </summary>
        /// <returns></returns>
        public bool TryShutdown(Action beforeShutdown = null)
        {
            var success = this.TryShutdownAllWindows();

            if (success)
            {
                if (beforeShutdown != null) { beforeShutdown(); }
                var app = Application.Current;
                if (app != null) app.Shutdown();
            }

            return success;
        }

        /// <summary>
        /// 两个职责：
        /// * 通知所有应用程序关闭的事件。
        /// * 尝试关闭所有页签。
        /// </summary>
        private bool TryShutdownAllWindows()
        {
            //先通知所有应用程序关闭的事件
            var args = new CancelEventArgs();
            this.OnShutingdown(args);
            if (args.Cancel) return false;

            //没有程序阻止应用程序级别的关闭，开始关闭每一个页签。
            var workSpace = this.Workspace;
            var windowsCache = workSpace.Windows.ToArray();
            foreach (var window in windowsCache)
            {
                var success = workSpace.TryRemove(window);

                //任何一个页签无法关闭，整个程序的关闭都将被阻止。
                if (!success) return false;
            }

            return true;
        }

        public event EventHandler<CancelEventArgs> Shutingdown;

        private void OnShutingdown(CancelEventArgs e)
        {
            var handler = this.Shutingdown;
            if (handler != null) handler(this, e);
        }

        #endregion

        #region RafyWindows

        /// <summary>
        /// Windows 相关的 API
        /// </summary>
        public class RafyWindows : IEnumerable<Window>
        {
            private List<Window> _popupWindows = new List<Window>();

            internal RafyWindows() { }

            /// <summary>
            /// 当前是否有弹出窗口
            /// </summary>
            public bool HasPopup
            {
                get { return this._popupWindows.Count > 0; }
            }

            /// <summary>
            /// 当前激活的窗口
            /// </summary>
            public Window Current
            {
                get
                {
                    return this._popupWindows.FirstOrDefault(w => w.IsActive)
                        ?? App.Current.MainWindow;
                }
            }

            /// <summary>
            /// 使用一个通用的Dialog来显示某个UI元素。
            /// </summary>
            /// <param name="content"></param>
            /// <param name="windowSetter">
            /// 可以在这个 setter 中设置窗体的初始属性。
            /// </param>
            /// <returns></returns>
            public WindowButton ShowDialog(FrameworkElement content, Action<ViewDialog> windowSetter = null)
            {
                var window = this.CreateWindow(content);

                window.ShowAsDialog = true;

                window.ShowInTaskbar = false;

                if (windowSetter != null) windowSetter(window);

                var activeWindow = Application.Current.Windows.Cast<Window>().FirstOrDefault(w => w.IsActive);
                if (activeWindow != null) window.Owner = activeWindow;

                var res = window.ShowDialog();
                if (res == null) return WindowButton.Cancel;
                return res.Value ? WindowButton.Yes : WindowButton.No;
            }

            /// <summary>
            /// 使用一个通用的窗体来显示某个UI元素。
            /// </summary>
            /// <param name="content"></param>
            /// <param name="windowSetter">
            /// 可以在这个 setter 中设置窗体的初始属性。
            /// </param>
            /// <returns></returns>
            public ViewDialog ShowWindow(FrameworkElement content, Action<ViewDialog> windowSetter = null)
            {
                var window = this.CreateWindow(content);

                if (windowSetter != null) windowSetter(window);

                window.Show();

                return window;
            }

            private ViewDialog CreateWindow(FrameworkElement view)
            {
                if (view == null) throw new ArgumentNullException("view");

                view.RemoveFromParent();

                var screenSize = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea;
                var dialog = new ViewDialog()
                {
                    InnerContent = view,
                    MinWidth = 400,
                    MinHeight = 200,
                    Width = screenSize.Width * 0.7,
                    Height = screenSize.Height * 0.7,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                };

                Zoom.EnableZoom(view);

                this.OnWindowCreated(dialog);

                return dialog;
            }

            private void OnWindowCreated(Window win)
            {
                this._popupWindows.Add(win);

                win.Closed += (o, e) =>
                {
                    this._popupWindows.Remove(o as Window);
                };
            }

            #region IEnumerable<Window>

            IEnumerator<Window> IEnumerable<Window>.GetEnumerator()
            {
                return this._popupWindows.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this._popupWindows.GetEnumerator();
            }

            #endregion
        }

        #endregion

        #region RafyMessageBox

        /// <summary>
        /// 使用用WPF扩展的MessageBox
        /// </summary>
        public class RafyMessageBox
        {
            public MessageBoxResult Show(string messageText)
            {
                return Show(messageText, "提示".Translate(), MessageBoxButton.OK, MessageBoxImage.Information);
            }

            public MessageBoxResult Show(string messageText, MessageBoxButton button)
            {
                return Show(messageText, "提示".Translate(), button, MessageBoxImage.Information);
            }

            public MessageBoxResult Show(string messageText, MessageBoxImage icon)
            {
                return Show(messageText, "提示".Translate(), MessageBoxButton.OK, icon);
            }

            public MessageBoxResult Show(string messageText, MessageBoxButton button, MessageBoxImage icon)
            {
                return Show(messageText, "提示".Translate(), button, icon);
            }

            public MessageBoxResult Show(string messageText, string caption)
            {
                return Show(messageText, caption, MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            public MessageBoxResult Show(string messageText, string caption, MessageBoxButton button)
            {
                return Show(messageText, caption, button, MessageBoxImage.Warning);
            }

            public MessageBoxResult Show(string messageText, string caption, MessageBoxButton button, MessageBoxImage icon)
            {
                //var wpfApp = Application.Current;
                //if (wpfApp.Windows.OfType<Window>().All(w => !w.IsActive))
                //{
                //    var l = new ActivatorListener
                //    {
                //        Window = wpfApp.MainWindow,
                //        Action = () =>
                //        {
                //            Microsoft.Windows.Controls.MessageBox.Show(messageText, caption, button, icon);
                //        }
                //    };
                //    l.ListenOnce();
                //    wpfApp.MainWindow.Activate();
                //}

                //messageText = messageText.Translate();
                //caption = caption.Translate();

                return Xceed.Wpf.Toolkit.MessageBox.Show(messageText, caption, button, icon);

                //return System.Windows.MessageBox.Show(messageText, caption, button, icon);
            }

            //private class ActivatorListener
            //{
            //    internal Window Window;
            //    internal Action Action;

            //    public void ListenOnce()
            //    {
            //        Window.Activated += this.Activated;
            //    }

            //    private void Activated(object sender, EventArgs e)
            //    {
            //        Window.Activated -= this.Activated;

            //        Action();
            //    }
            //}
        }

        #endregion

        #region Status

        private string _status;

        /// <summary>
        /// 用于描述系统当前的状态。
        /// 界面层可直接显示此信息。
        /// </summary>
        public string Status
        {
            get { return this._status; }
            set
            {
                this._status = value;
                this.OnStatusChanged();
            }
        }

        public event EventHandler StatusChanged;

        protected virtual void OnStatusChanged()
        {
            var handler = this.StatusChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        #endregion

        #region Progress

        private double _progress;

        /// <summary>
        /// 描述当前异步流程的进度，值从0-100.
        /// 界面层可直接显示此信息。
        /// </summary>
        public double Progress
        {
            get { return this._progress; }
            set
            {
                this._progress = value;
                this.OnProgressChanged();
            }
        }

        public event EventHandler ProgressChanged;

        protected virtual void OnProgressChanged()
        {
            var handler = this.ProgressChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        #endregion
    }

    /// <summary>
    /// 最后的结果按钮
    /// </summary>
    public enum WindowButton
    {
        Yes, No, Cancel
    }
}