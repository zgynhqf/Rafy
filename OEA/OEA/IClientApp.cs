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

namespace OEA
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
        /// 所有实体类初始化完成
        /// </summary>
        event EventHandler EntityMetasInitialized;

        /// <summary>
        /// 所有实体元数据初始化完毕，包括实体元数据之间的关系。
        /// </summary>
        event EventHandler AllPluginsMetaIntialized;

        /// <summary>
        /// 元数据客户化操作
        /// Module 挂接此事件完成实际的元数据操作
        /// </summary>
        event EventHandler ModelOpertions;

        /// <summary>
        /// 元数据客户化完成
        /// </summary>
        event EventHandler ModelOpertionsCompleted;

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
        /// 数据库自动迁移操作，
        /// 此操作只会在 单机版 和 服务器端执行。
        /// </summary>
        event EventHandler DbMigratingOperations;

        /// <summary>
        /// AppStartup 完毕
        /// </summary>
        event EventHandler StartupCompleted;

        /// <summary>
        /// 应用程序完全退出
        /// </summary>
        event EventHandler Exit;

        event CancelEventHandler MainWindowShowing;

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
        /// 界面组合完成
        /// </summary>
        event EventHandler Composed;

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

        /// <summary>
        /// 主窗体的 Loaded 事件
        /// </summary>
        event EventHandler MainWindowLoaded;
    }

    /// <summary>
    /// 服务端应用程序生命周期定义
    /// </summary>
    public interface IServerApp : IApp { }
}
