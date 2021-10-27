/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20140629
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20140629 13:07
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rafy.ComponentModel
{
    /// <summary>
    /// 应用程序生成周期定义
    /// </summary>
    public interface IApp
    {
        /// <summary>
        /// 应用程序当前所处的阶段。
        /// </summary>
        AppPhase Phase { get; }

        /// <summary>
        /// 所有实体元数据初始化完毕，包括实体元数据之间的关系。
        /// </summary>
        event EventHandler StartupPluginsIntialized;

        /// <summary>
        /// 所有初始化期定义的元数据初始化完成时事件。
        /// </summary>
        event EventHandler MetaCreating;

        /// <summary>
        /// 所有初始化工作完成
        /// </summary>
        event EventHandler MetaCreated;

        /// <summary>
        /// 应用程序运行时行为开始。
        /// 
        /// 常见在这个事件执行的操作有：
        /// * 组合逻辑，例如 A 订阅 B 的某个事件。
        /// * 生成数据库。
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
        /// 判断是否在客户端。
        /// </summary>
        /// <returns></returns>
        bool IsOnClient();
    }

    /// <summary>
    /// 应用程序当前所处的阶段。
    /// </summary>
    public enum AppPhase
    {
        /// <summary>
        /// Rafy App 刚被创建，还没有启动。
        /// </summary>
        Created = 10,
        /// <summary>
        /// 开始启动流程
        /// </summary>
        Starting = 20,
        /// <summary>
        /// 启动的插件中的各类元数据已经被创建完成
        /// </summary>
        MetaCreated = 30,
        /// <summary>
        /// 启动完成，开始运行中。
        /// </summary>
        Running = 40,
        /// <summary>
        /// Rafy App 退出完成。
        /// </summary>
        Exited = 50
    }
}