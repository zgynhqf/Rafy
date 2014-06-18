/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110426
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20110426
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.MetaModel;
using System.Windows;

namespace Rafy.WPF
{
    /// <summary>
    /// 用户界面主要工作区
    /// </summary>
    public interface IWorkspace
    {
        /// <summary>
        /// 从某个页签切换到另一个页签前发生此事件。
        /// </summary>
        event EventHandler<WorkspaceWindowActivingEventArgs> WindowActiving;

        /// <summary>
        /// 从某个页签切换到另一个页签后发生此事件。
        /// </summary>
        event EventHandler<WorkspaceWindowActivedEventArgs> WindowActived;

        /// <summary>
        /// 关闭某个页签时发生。
        /// </summary>
        event EventHandler<WorkspaceWindowClosingEventArgs> WindowClosing;

        /// <summary>
        /// 关闭某个页签后发生。
        /// </summary>
        event EventHandler<WorkspaceWindowChangedEventArgs> WindowClosed;

        /// <summary>
        /// 工作区中的所有页签
        /// </summary>
        IEnumerable<WorkspaceWindow> Windows { get; }

        /// <summary>
        /// 已经选中的页签
        /// </summary>
        WorkspaceWindow ActiveWindow { get; }

        /// <summary>
        /// 添加一个页签。
        /// </summary>
        /// <param name="window"></param>
        void Add(WorkspaceWindow window);

        /// <summary>
        /// 尝试移除一个页签，返回是否成功。
        /// （有时候某些模块可以监听 WindowClosing 事件来阻止此操作，）
        /// </summary>
        /// <param name="window"></param>
        bool TryRemove(WorkspaceWindow window);

        /// <summary>
        /// 尝试激活某一个页签，返回是否成功。（展示给用户）
        /// （有时候某些模块可以监听 WindowActiving 事件来阻止此操作，）
        /// </summary>
        /// <param name="window"></param>
        bool TryActive(WorkspaceWindow window);

        /// <summary>
        /// 在所有页签中查找指定名称的页签。
        /// </summary>
        /// <param name="windowTitle"></param>
        /// <returns></returns>
        WorkspaceWindow GetWindow(string windowTitle);
    }
}