/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110321
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100321
 * 
*******************************************************/

using Rafy.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Rafy.ComponentModel
{
    /// <summary>
    /// Rafy 插件的运行时对象。
    /// 
    /// 关于插件的定义、配置、加载时机。见 <see cref="IPluginConfig"/>。
    /// </summary>
    public interface IPlugin
    {
        /// <summary>
        /// 插件对应的程序集。
        /// </summary>
        Assembly Assembly { get; }

        ///// <summary>
        ///// 插件的启动级别
        ///// </summary>
        //int SetupLevel { get; }

        /// <summary>
        /// 插件的初始化方法。
        /// 框架会在启动时根据启动级别顺序调用本方法。
        /// 
        /// 方法有两个职责：
        /// 1.依赖注入。
        /// 2.注册 app 生命周期中事件，进行特定的初始化工作。
        /// 
        /// 如果这个插件是按需加载的。那么不会发生所有的事件，而是只会发生当前时间后面的事件。
        /// 如果一个插件想要监听启动周期中比较靠前的事件，那么请将这个插件设置为启动时加载。
        /// </summary>
        /// <param name="app">应用程序对象。</param>
        void Initialize(IApp app);
    }

    //[Flags]
    //public enum PluginComponent
    //{
    //    None = 0,
    //    Entity = 1,
    //    EntityRepository = 2,
    //    EntityRepositoryDataProvider = 4,
    //    EntityConfiguration = 8,


    //    ViewConfiguration = 1024,

    //    All = Entity | EntityRepository | EntityRepositoryDataProvider | EntityConfiguration
    //        | ViewConfiguration
    //}

    ///// <summary>
    ///// 本领域插件提供的所有
    ///// </summary>
    //public virtual DomainPluginComponent Components
    //{
    //    get { return DomainPluginComponent.All; }
    //}

    //public abstract class Plugin
    //{
    //    System.Reflection.BindingFlags
    //}
}
