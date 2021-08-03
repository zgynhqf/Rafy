/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20151209
 * 运行环境：.NET 4.5
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20151209 20:28
 * 编辑文件 崔化栋 20180502 14:00
 * 
*******************************************************/

#if NET45
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rafy.Configuration
{
    [ConfigurationCollection(typeof(PluginElement))]
    public class PluginsConfigurationElement : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new PluginElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as PluginElement).Plugin;
        }
    }

    public class PluginElement : ConfigurationElement, IPluginConfig
    {
        /// <summary>
        /// 可以只填写插件程序集的全名称，也可以写出具体的插件类型的全名称。
        /// </summary>
        [ConfigurationProperty("plugin", IsKey = true, IsRequired = true)]
        public string Plugin
        {
            get { return (string)this["plugin"]; }
            set { this["plugin"] = value; }
        }

        /// <summary>
        /// 加载的时机。
        /// 如果插件中有扩展属性，需要设置为“启动时加载”，否则无法为按需加载的实体进行扩展。
        /// </summary>
        [ConfigurationProperty("loadType")]
        public PluginLoadType LoadType
        {
            get { return (PluginLoadType)this["loadType"]; }
            set { this["loadType"] = value; }
        }
    }
}
#endif