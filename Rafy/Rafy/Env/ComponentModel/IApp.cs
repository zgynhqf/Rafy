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
        event EventHandler AppMetaCompleted;

        /// <summary>
        /// 组件的组合操作。
        /// 组合可以在此事件中添加自己的组合逻辑，例如 A 订阅 B 的某个事件。
        /// </summary>
        event EventHandler ComposeOperations;

        /// <summary>
        /// 所有组件组合完毕。
        /// </summary>
        event EventHandler Composed;

        /// <summary>
        /// 应用程序运行时行为开始。
        /// </summary>
        event EventHandler RuntimeStarting;

        /// <summary>
        /// 主过程开始前事件。
        /// </summary>
        event EventHandler MainProcessStarting;

        /// <summary>
        /// AppStartup 完毕
        /// </summary>
        event EventHandler StartupCompleted;

        /// <summary>
        /// 应用程序完全退出
        /// </summary>
        event EventHandler Exit;
    }
}