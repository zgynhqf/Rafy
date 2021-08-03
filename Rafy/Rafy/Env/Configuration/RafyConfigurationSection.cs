/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130125 11:27
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130125 11:27
 * 编辑文件 崔化栋 20180424 09:50
 * 
*******************************************************/

#if NET45
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Rafy.Configuration
{
    public class RafyConfigurationSection : ConfigurationSection
    {
#region 子元素

        [ConfigurationProperty("wpf")]
        public WPFConfigurationElement WPF
        {
            get { return (WPFConfigurationElement)this["wpf"]; }
            set { this["wpf"] = value; }
        }

        [ConfigurationProperty("web")]
        public WebConfigurationElement Web
        {
            get { return (WebConfigurationElement)this["web"]; }
            set { this["web"] = value; }
        }

        [ConfigurationProperty("domainPlugins")]
        public PluginsConfigurationElement DomainPlugins
        {
            get { return (PluginsConfigurationElement)this["domainPlugins"]; }
            set { this["domainPlugins"] = value; }
        }

        [ConfigurationProperty("uiPlugins")]
        public PluginsConfigurationElement UIPlugins
        {
            get { return (PluginsConfigurationElement)this["uiPlugins"]; }
            set { this["uiPlugins"] = value; }
        }

#endregion

        /// <summary>
        /// 当前显示的语言文化。
        /// 如果没有设置本项，表明使用系统自带的语言文化。
        /// 例如：zh-CN、en-US 等。
        /// </summary>
        [ConfigurationProperty("currentCulture")]
        public string CurrentCulture
        {
            get { return (string)this["currentCulture"]; }
            set { this["currentCulture"] = value; }
        }

        /// <summary>
        /// 在当前语言下是否执行收集操作。
        /// </summary>
        [ConfigurationProperty("collectDevLanguages", DefaultValue = DynamicBoolean.IsDebugging)]
        public DynamicBoolean CollectDevLanguages
        {
            get { return (DynamicBoolean)this["collectDevLanguages"]; }
            set { this["collectDevLanguages"] = value; }
        }

        /// <summary>
        /// 配置使用哪个数据门户代理。
        /// 
        /// 如果直接连接数据源，则需要配置：Local（默认值）。
        /// 如果使用 WCF，则需要配置：Rafy.Domain.DataPortal.WCF.ClientProxy, Rafy.Domain。
        /// </summary>
        [ConfigurationProperty("dataPortalProxy", DefaultValue = "Local")]
        public string DataPortalProxy
        {
            get { return (string)this["dataPortalProxy"]; }
            set { this["dataPortalProxy"] = value; }
        }
    }
}
#endif

#if NS2
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace Rafy.Configuration
{
    public class RafyConfigurationSection
    {
        public WPFConfigurationSection WPF { get; private set; } = new WPFConfigurationSection();

        //public IConfigurationSection Web
        //{
        //    get { return this.GetSection("web"); }
        //}

        /// <summary>
        /// 当前显示的语言文化。
        /// 如果没有设置本项，表明使用系统自带的语言文化。
        /// 例如：zh-CN、en-US 等。
        /// </summary>
        public string CurrentCulture { get; set; }

        /// <summary>
        /// 在当前语言下是否执行收集操作。
        /// </summary>
        public DynamicBoolean CollectDevLanguages { get; set; } = DynamicBoolean.IsDebugging;

        /// <summary>
        /// 配置使用哪个数据门户代理。
        /// 
        /// 如果直接连接数据源，则需要配置：Local（默认值）。
        /// 如果使用 WCF，则需要配置：Rafy.Domain.DataPortal.WCF.ClientProxy, Rafy.Domain。
        /// </summary>
        public string DataPortalProxy { get; set; } = "Local";

        public PluginSection[] DomainPlugins { get; set; }

        public PluginSection[] UIPlugins { get; set; }
    }

    public class PluginSection : IPluginConfig
    {
        /// <summary>
        /// 对应的插件的类型。
        /// 可以只填写程序集名称，也可以写出插件类型的全名称。（后者加载更快）
        /// </summary>
        public string Plugin { get; set; }

        /// <summary>
        /// 加载的时机。
        /// 如果插件中有扩展属性，需要设置为“启动时加载”，否则无法为按需加载的实体进行扩展。
        /// </summary>
        public PluginLoadType LoadType { get; set; }
    }
}
#endif