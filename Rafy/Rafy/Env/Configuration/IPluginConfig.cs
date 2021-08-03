/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20210801
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20210801 22:59
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Text;

namespace Rafy.Configuration
{
    internal interface IPluginConfig
    {
        /// <summary>
        /// 对应的插件的类型。
        /// 可以只填写程序集名称，也可以写出插件类型的全名称。（后者加载更快）
        /// </summary>
        string Plugin { get; }

        /// <summary>
        /// 加载的时机。
        /// 需要设置为“启动时加载”的情况：
        /// * 插件中有实体基类的扩展属性
        /// * 插件中有仓库扩展
        /// * 插件中有实体基类的配置
        /// * 插件中有定义模块等元数据时。
        /// 
        /// 如果一个运行时插件还没有加载进 Rafy 环境中时，它其中的所有 Entity、EntityConfig、ViewConfig 都是没有加载的。
        /// 只有特定的插件需要在启动时加载；若非必要，则插件会在需要的时候再加载。
        /// </summary>
        PluginLoadType LoadType { get; }
    }

    /// <summary>
    /// 插件加载的类型
    /// </summary>
    public enum PluginLoadType
    {
        /// <summary>
        /// 按需再载。
        /// </summary>
        AsRequired,
        /// <summary>
        /// 系统启动时就加载。一般为重要的插件需要设置为启动时加载。
        /// </summary>
        AtStartup
    }
}