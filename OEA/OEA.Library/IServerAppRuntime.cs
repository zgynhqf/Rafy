/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110927
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20110927
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OEA.Server
{
    /// <summary>
    /// 服务端运行时对象实现此接口，然后使用 ServerApp.Register 方法注册即可。
    /// 
    /// 实现此接口的类一般是某种应用程序的 Application 类。
    /// 可用的应用程序：WPF、WinForm、Console、UnitTest.
    /// </summary>
    public interface IServerAppRuntime
    {
        /// <summary>
        /// 显示主窗口
        /// </summary>
        void ShowMainWindow();

        /// <summary>
        /// 主动关闭整个应用程序
        /// </summary>
        void Shutdown();

        /// <summary>
        /// 显示某个应用程序消息。
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        void ShowMessage(string message, string title);

        /// <summary>
        /// 应用程序启动时发生此事件。
        /// </summary>
        event EventHandler AppStartup;

        /// <summary>
        /// 应用程序关闭时发生此事件。
        /// </summary>
        event EventHandler AppExit;
    }
}