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

using Rafy.ComponentModel;
using Rafy.MetaModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rafy.Configuration
{
    /// <summary>
    /// 插件的定义可以通过代码及配置文件来完成。
    /// <para>
    /// 插件可使用按需加载功能。
    /// * 通过配置文件时，默认都是按需加载，可以通过设置其 LoadType 属性来定义加载时机。
    /// * 通过代码加载时，全是启动即加载。都需要在 <see cref="AppImplementationBase.InitEnvironment"/> 过程中，向 <see cref="RafyEnvironment.Plugins"/>  集合中添加。
    /// </para>
    /// 
    /// <para><b>如果一个按需加载的插件，还没有加载进 Rafy 环境中时：</b></para>
    /// <list type="bullet">
    /// <item>它的 Intialize 方法没有被调用；</item>
    /// <item>它其中定义的所有 EntityProperties、EntityConfig、ViewConfig、Service、DataProvider 都是没有被 Rafy 扫描并加载的；</item>
    /// <item>由于它的 EntityConfig 虽然没有加载，所以其 EntityMeta 虽然可以通过 <see cref="EntityMetaRepository.Find(Type)"/> 方法创建，但是其元数据是不完整的。</item>
    /// <item>由于 EntityRepository 依赖 EntityMeta，所以其也不会被正确加载。</item>
    /// <item>托管属性框架不依赖 Rafy 环境，所以其定义的托管属性可以在第一次被使用时，正确地自动加载。</item>
    /// </list>
    /// 
    /// <para><b>也就是说，当需要使用到实体的元数据、仓库、数据层等组件时，需要将其对应的插件加载进 Rafy 环境中。</b></para>
    /// <para>另外，需要特别注意的是，实体的一组插件应该同时被配置为按需加载，也要在需要时被同时加载：           </para>
    /// <para>* 按需加载实体插件时，也要记得同时加载基对应的仓库层、数据层、UI 层等插件，不然也会导致元数据不全。 </para>
    /// <para>* 不能只按需加载实体插件，而启动加载仓库等插件。                                                </para>
    /// 
    /// 
    /// <para><b>只有特定的插件需要在启动时加载；若非必要，则插件会在需要的时候再加载。</b></para>
    /// <para> 需要设置为“启动时加载”的情况：                                    </para>
    /// <list type="bullet">
    /// <item>插件中有实体基类的扩展属性；</item>
    /// <item>插件中有仓库扩展；</item>
    /// <item>插件中有实体基类的配置；</item>
    /// <item>插件中有定义模块等元数据时；</item>
    /// </list>
    /// </summary>
    internal interface IPluginConfig
    {
        /// <summary>
        /// 对应的插件的类型。
        /// 可以只填写程序集名称，也可以写出插件类型的全名称。（后者加载更快）
        /// </summary>
        string Plugin { get; }

        /// <summary>
        /// 加载的时机。默认为按需加载。
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