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
        /// 
        public DynamicBoolean CollectDevLanguages { get; set; } = DynamicBoolean.IsDebugging;

        /// <summary>
        /// 配置使用哪个数据门户代理。
        /// 
        /// 如果直接连接数据源，则需要配置：Local（默认值）。
        /// 如果使用 WCF，则需要配置：Rafy.Domain.DataPortal.WCF.ClientProxy, Rafy.Domain。
        /// </summary>
        public string DataPortalProxy { get; set; } = "Local";

        public string[] DomainPlugins { get; set; }

        public string[] UIPlugins { get; set; }
    }
}
