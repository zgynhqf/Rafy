/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：2011
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 2011
 * 
*******************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Rafy.Domain;
using Rafy.MetaModel;
using Rafy.MetaModel.View;
using Rafy.WPF;
using Rafy.WPF.Controls;

namespace Rafy.WPF.Command
{
    public abstract class ViewCommandBase<TView> : ClientCommand
        where TView : LogicalView
    {
        /// <summary>
        /// 本命令对应的参数视图。
        /// </summary>
        public TView View
        {
            get { return base.Parameter as TView; }
        }

        protected override sealed bool CanExecuteCore()
        {
            return this.CanExecute(View);
        }

        protected override sealed void ExecuteCore()
        {
            this.Execute(View);
        }

        protected override sealed void OnCreatedCore()
        {
            base.OnCreatedCore();

            this.OnCreated(View);
        }

        /// <summary>
        /// 当本对象被创建时，调用此方法。
        /// </summary>
        /// <param name="view">为这个参数创建的本对象</param>
        protected virtual void OnCreated(TView view) { }

        /// <summary>
        /// 子类重写此方法来返回是否可执行的逻辑。
        /// 默认返回 true。
        /// </summary>
        /// <param name="view">此命令对应的视图对象。</param>
        /// <returns></returns>
        public virtual bool CanExecute(TView view) { return true; }

        /// <summary>
        /// 子类重写此方法来执行具体的逻辑。
        /// </summary>
        /// <param name="view">此命令对应的视图对象。</param>
        public abstract void Execute(TView view);
    }

    /// <summary>
    /// 参数为 LogicalView 的客户端命令
    /// </summary>
    public abstract class ViewCommand : ViewCommandBase<LogicalView> { }

    /// <summary>
    /// 参数为 ListLogicalView 的客户端命令
    /// </summary>
    public abstract class ListViewCommand : ViewCommandBase<ListLogicalView> { }
}