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
 * 
*******************************************************/

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
        /// Gets or sets the full type name (or 'Local') of
        /// the data portal proxy object to be used when
        /// communicating with the data portal server.
        /// </summary>
        /// <value>Fully qualified assembly/type name of the proxy class
        /// or 'Local'.</value>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// If this value is empty or null, a new value is read from the 
        /// application configuration file with the key value 
        /// "DataPortalProxy".
        /// </para><para>
        /// The proxy class must implement SimpleCsla.Server.IDataPortalServer.
        /// </para><para>
        /// The value "Local" is a shortcut to running the DataPortal
        /// "server" in the client process.
        /// </para><para>
        /// Other built-in values include:
        /// <list>
        /// <item>
        /// <term>SimpleCsla,SimpleCsla.DataPortalClient.RemotingProxy</term>
        /// <description>Use .NET Remoting to communicate with the server</description>
        /// </item>
        /// <item>
        /// <term>SimpleCsla,SimpleCsla.DataPortalClient.EnterpriseServicesProxy</term>
        /// <description>Use Enterprise Services (DCOM) to communicate with the server</description>
        /// </item>
        /// <item>
        /// <term>SimpleCsla,SimpleCsla.DataPortalClient.WebServicesProxy</term>
        /// <description>Use Web Services (asmx) to communicate with the server</description>
        /// </item>
        /// </list>
        /// Each proxy type does require that the DataPortal server be hosted using the appropriate
        /// technology. For instance, Web Services and Remoting should be hosted in IIS, while
        /// Enterprise Services must be hosted in COM+.
        /// </para>
        /// </remarks>
        [ConfigurationProperty("dataPortalProxy", DefaultValue = "Local")]
        public string DataPortalProxy
        {
            get { return (string)this["dataPortalProxy"]; }
            set { this["dataPortalProxy"] = value; }
        }
    }
}
