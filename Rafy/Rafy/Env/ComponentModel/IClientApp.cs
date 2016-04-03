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

namespace Rafy.ComponentModel
{
    /// <summary>
    /// 客户端应用程序生命周期定义
    /// </summary>
    public interface IClientApp : IApp
    {
        /// <summary>
        /// 所有命令元数据初始化完成
        /// </summary>
        event EventHandler CommandMetaIntialized;

        /// <summary>
        /// 登录成功，主窗口开始显示
        /// </summary>
        event EventHandler LoginSuccessed;

        /// <summary>
        /// 登录失败，准备退出
        /// </summary>
        event EventHandler LoginFailed;

        /// <summary>
        /// 显示某个应用程序消息。
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        void ShowMessage(string message, string title);

        /// <summary>
        /// 关闭应用程序
        /// </summary>
        void Shutdown();
    }

    /// <summary>
    /// 服务端应用程序生命周期定义
    /// </summary>
    public interface IServerApp : IApp { }
}
