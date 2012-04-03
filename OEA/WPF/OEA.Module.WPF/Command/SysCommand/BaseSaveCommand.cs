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
using OEA.Module;
using OEA.Core;
using System.Windows;
using OEA.Module.WPF;
using OEA.Library;

namespace OEA.WPF.Command
{
    /// <summary>
    /// 保存数据的基类。
    /// 
    /// 在这里添加了切换叶签时的保存提示。
    /// </summary>
    public abstract class BaseSaveCommand : ViewCommand
    {
        private bool _eventAttached = false;

        /// <summary>
        /// 在第一次CanExecute方法执行时，进行初始化。
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public override bool CanExecute(ObjectView view)
        {
            if (!_eventAttached)
            {
                _eventAttached = true;
                this.AttachEventToWindow(view);
            }

            var data = this.GetCheckData(view);

            return base.CanExecute(view) &&
                data != null &&
                data.IsDirty;
        }

        protected virtual IDirtyAware GetCheckData(ObjectView view)
        {
            return view.Data as IDirtyAware;
        }

        #region 在切换/关闭叶签时，提示用户

        private IWorkspaceWindow _ownerWindow;

        private ObjectView _ownerView;

        /// <summary>
        /// 初始化所有需要的事件。
        /// </summary>
        /// <param name="view"></param>
        private void AttachEventToWindow(ObjectView view)
        {
            //IWndowTemplate : IFrameTemplate, IWindow
            this._ownerWindow = view.GetWorkspaceWindow();
            this._ownerView = view;

            var workSpace = App.Current.Workspace;
            workSpace.WindowActiving -= Workspace_WindowActiving;
            workSpace.WindowActiving += Workspace_WindowActiving;
            workSpace.WindowClosing -= workSpace_WindowClosing;
            workSpace.WindowClosing += workSpace_WindowClosing;
        }

        private void workSpace_WindowClosing(object sender, WorkspaceWindowClosingEventArgs e)
        {
            if (e.Window == this._ownerWindow)
            {
                this.CheckData(() => e.Cancel = true);
            }
        }

        private void Workspace_WindowActiving(object sender, WorkspaceWindowActivingEventArgs e)
        {
            if (e.DeactiveWindow == this._ownerWindow)
            {
                this.CheckData(() => e.Cancel = true);
            }
        }

        private void CheckData(Action actionIfCancel)
        {
            var data = this.GetCheckData(this._ownerView);
            if (data != null && data.IsDirty)
            {
                var res = MessageBox.Show(
                    "您的操作已使数据改变，是否要保存？", "是否保存",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question
                    );
                switch (res)
                {
                    case MessageBoxResult.Cancel:
                        actionIfCancel();
                        break;
                    case MessageBoxResult.Yes:
                        this.Execute(this._ownerView);
                        break;
                    default:
                        break;
                }
            }
        }

        #endregion
    }
}
