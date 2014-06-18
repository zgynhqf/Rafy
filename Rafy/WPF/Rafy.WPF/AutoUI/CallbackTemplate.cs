/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120415
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120415
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.MetaModel.View;
using Rafy.WPF;

using Rafy.WPF.Command;
using Rafy.Domain;

namespace Rafy.WPF
{
    /// <summary>
    /// 一个添加了许多系统内置回调的自定义模块基类。
    /// </summary>
    public abstract class CallbackTemplate : UITemplate
    {
        private ControlResult _ui;

        protected ListLogicalView ListView
        {
            get { return this._ui.MainView as ListLogicalView; }
        }

        /// <summary>
        /// 是否不创建所有 CallBack 功能。
        /// 子类可以在其构造函数中设置该值。
        /// </summary>
        public bool SuppressCallback { get; set; }

        /// <summary>
        /// 子类可以重写此方法来添加当前模块中 UI 的初始化逻辑。
        /// 
        /// 当使用自动生成的 UI 时，此方法会被调用。
        /// </summary>
        /// <param name="ui"></param>
        internal protected override void OnUIGenerated(ControlResult ui)
        {
            this._ui = ui;

            if (!this.SuppressCallback)
            {
                this.CreateCallback();
            }

            base.OnUIGenerated(ui);
        }

        private void CreateCallback()
        {
            this._mainViewCommands = this._ui.MainView.Commands;

            this.ListView.ItemCreated += (o, e) => this.OnItemCreated(e.Item);

            IfHas<AddCommand>(cmd =>
            {
                cmd.Executed += (o, e) => this.OnAdded();
            });

            IfHas<PopupAddCommand>(cmd =>
            {
                cmd.Executed += (o, e) => this.OnAdded();
            });

            IfHas<DeleteListObjectCommand>(cmd =>
            {
                cmd.Executed += (o, e) => this.OnDeleted();
            });

            IfHas<EditDetailCommand>(cmd =>
            {
                cmd.Executed += (o, e) => this.OnEdited();
            });

            IfHas<SaveListCommand>(cmd =>
            {
                cmd.Executing += (o, e) => this.OnSaving();
                cmd.Executed += (o, e) => this.OnSaveSuccessed();
                cmd.ExecuteFailed += (o, e) => this.OnSaveFailed();
            });
        }

        #region General Callback

        protected virtual void OnItemCreated(Entity entity) { }

        protected virtual void OnAdded() { }

        protected virtual void OnDeleted() { }

        protected virtual void OnEdited() { }

        protected virtual void OnSaving() { }

        protected virtual void OnSaveSuccessed() { }

        protected virtual void OnSaveFailed() { }

        #endregion

        #region IfHasCommand

        private ClientCommandCollection _mainViewCommands;

        protected void IfHas<TCommandType>(Action<TCommandType> action)
            where TCommandType : ClientCommand
        {
            var cmd = this._mainViewCommands.Find<TCommandType>();
            if (cmd != null) { action(cmd); }
        }

        protected void IfHas(Type cmdType, Action<ClientCommand> action)
        {
            var cmd = this._mainViewCommands.Find(cmdType);
            if (cmd != null) { action(cmd); }
        }

        #endregion
    }
}