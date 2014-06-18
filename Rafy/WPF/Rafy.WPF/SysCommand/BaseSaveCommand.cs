/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20101126
 * 说明：保存数据的基类
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20101126
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Rafy.WPF;
using Rafy.Domain;

namespace Rafy.WPF.Command
{
    /// <summary>
    /// 保存数据的基类。
    /// 
    /// 在这里添加了切换叶签时的保存提示。
    /// </summary>
    public abstract class BaseSaveCommand : ViewCommand
    {
        private LogicalView _ownerView;
        private WorkspaceWindow _ownerWindow;
        private bool _saveSuccessed;

        /// <summary>
        /// 是否在窗口切换时也弹出提示。默认为 false。
        /// </summary>
        public bool EnableAlertOnWindowActiving { get; set; }

        public override bool CanExecute(LogicalView view)
        {
            var data = view.Data as IDirtyAware;

            return data != null && data.IsDirty;
        }

        #region 元素加载时，监听页签事件

        protected override void OnCreated(LogicalView param)
        {
            _ownerView = param;

            base.OnCreated(param);
        }

        protected override void OnUIElementGenerated(FrameworkElement value)
        {
            if (value != null)
            {
                value.Loaded += OnLoaded;
            }

            base.OnUIElementGenerated(value);
        }

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            //只监听一次 Loaded 事件。
            (sender as FrameworkElement).Loaded -= OnLoaded;

            Attach();
        }

        private void Attach()
        {
            _ownerWindow = WorkspaceWindow.GetOuterWorkspaceWindow(_ownerView.Control);

            var workSpace = App.Current.Workspace;
            if (this.EnableAlertOnWindowActiving)
            {
                workSpace.WindowActiving += Workspace_WindowActiving;
            }
            workSpace.WindowClosing += workSpace_WindowClosing;
            workSpace.WindowClosed += workSpace_WindowClosed;
        }

        private void Dettach()
        {
            var workSpace = App.Current.Workspace;
            workSpace.WindowActiving -= Workspace_WindowActiving;
            workSpace.WindowClosing -= workSpace_WindowClosing;
            workSpace.WindowClosed -= workSpace_WindowClosed;
        }

        void Workspace_WindowActiving(object sender, WorkspaceWindowActivingEventArgs e)
        {
            if (e.DeactiveWindow == this._ownerWindow)
            {
                this.SaveIfDirty(() => e.Cancel = true);
            }
        }

        void workSpace_WindowClosing(object sender, WorkspaceWindowClosingEventArgs e)
        {
            if (e.Window == this._ownerWindow)
            {
                this.SaveIfDirty(() => e.Cancel = true);
            }
        }

        void workSpace_WindowClosed(object sender, WorkspaceWindowChangedEventArgs e)
        {
            if (e.DeactiveWindow == this._ownerWindow)
            {
                this.Dettach();
            }
        }

        #endregion

        private void SaveIfDirty(Action actionIfCancel)
        {
            if (this.CanExecute(_ownerView))
            {
                var res = AlertDataDirty();
                switch (res)
                {
                    case MessageBoxResult.Cancel:
                        actionIfCancel();
                        break;
                    case MessageBoxResult.Yes:
                        _saveSuccessed = false;

                        this.Execute(_ownerView);

                        //如果执行过程中没有调用 OnSaveSuccessed 方法，说明该次执行并未成功，则需要取消活动。
                        if (!_saveSuccessed)
                        {
                            actionIfCancel();
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// 执行成功后，需要调用此方法来通知执行功能。
        /// </summary>
        protected void OnSaveSuccessed()
        {
            _saveSuccessed = true;
        }

        private static MessageBoxResult AlertDataDirty()
        {
            var res = App.MessageBox.Show(
                "您的操作已使数据改变，是否要保存？".Translate(), "是否保存".Translate(),
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question
                );
            return res;
        }
    }
}
