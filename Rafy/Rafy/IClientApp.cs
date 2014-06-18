/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110221
 * 说明：应用程序初始化约定
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100221
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Rafy
{
    /// <summary>
    /// 应用程序生成周期定义
    /// </summary>
    public interface IApp
    {
        /// <summary>
        /// 所有实体元数据初始化完毕，包括实体元数据之间的关系。
        /// </summary>
        event EventHandler AllPluginsIntialized;

        /// <summary>
        /// 所有初始化期定义的元数据初始化完成时事件。
        /// </summary>
        event EventHandler MetaCompiled;

        /// <summary>
        /// 模块的定义先于其它模型的操作。这样可以先设置好模板默认的按钮。
        /// </summary>
        event EventHandler ModuleOperations;

        /// <summary>
        /// 模块的定义完成
        /// </summary>
        event EventHandler ModuleOperationsCompleted;

        /// <summary>
        /// 所有初始化工作完成
        /// </summary>
        event EventHandler AppModelCompleted;

        /// <summary>
        /// 应用程序运行时行为开始。
        /// </summary>
        event EventHandler RuntimeStarting;

        /// <summary>
        /// AppStartup 完毕
        /// </summary>
        event EventHandler StartupCompleted;

        /// <summary>
        /// 应用程序完全退出
        /// </summary>
        event EventHandler Exit;

        /// <summary>
        /// 主过程开始前事件。
        /// </summary>
        event EventHandler MainProcessStarting;

        /// <summary>
        /// 关闭应用程序
        /// </summary>
        void Shutdown();

        /// <summary>
        /// 显示某个应用程序消息。
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        void ShowMessage(string message, string title);
    }

    /// <summary>
    /// 客户端应用程序生命周期定义
    /// </summary>
    public interface IClientApp : IApp
    {
        /// <summary>
        /// 各模块初始化完成
        /// </summary>
        event EventHandler CommandMetasIntialized;

        /// <summary>
        /// 登录成功，主窗口开始显示
        /// </summary>
        event EventHandler LoginSuccessed;

        /// <summary>
        /// 登录失败，准备退出
        /// </summary>
        event EventHandler LoginFailed;
    }

    /// <summary>
    /// 服务端应用程序生命周期定义
    /// </summary>
    public interface IServerApp : IApp { }
}
