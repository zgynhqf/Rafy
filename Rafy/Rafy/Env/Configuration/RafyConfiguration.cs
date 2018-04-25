/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20121217 17:39
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20121217 17:39
 * 编辑文件 崔化栋 20180424 09:50
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
#if NETSTANDARD2_0 || NETCOREAPP2_0
using Microsoft.Extensions.Configuration;
#endif
using Rafy;
using Rafy.Configuration;

namespace Rafy
{
    /// <summary>
    /// Rafy 的配置。
    /// </summary>
    public class RafyConfiguration
    {
        private RafyConfigurationSection _section;

        public RafyConfiguration()
        {
            this.DevCulture = "zh-CN";
        }

        /// <summary>
        /// 配置文件节对应的 WMSSection 配置节。相当于 Instance.WMSSection。
        /// </summary>
        public RafyConfigurationSection Section
        {
            get
            {
                EnsureLoaded();
                return _section;
            }
        }

        private void EnsureLoaded()
        {
            if (_section == null)
            {
                //在 WCFServer 下，以下代码不起作用。所以所有属性都暂时只支持只读。
                //_configurtaion = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
#if NET45
                _section = ConfigurationManager.GetSection("rafy") as RafyConfigurationSection;
                if (_section == null) _section = new RafyConfigurationSection();
#endif
#if NETSTANDARD2_0 || NETCOREAPP2_0
                var rafyRawSection = ConfigurationHelper.Configuration.GetSection("rafy");
                if (rafyRawSection == null)
                {
                    throw new InvalidProgramException("配置文件中没有 rafy 配置节，请检查配置文件。");
                }
                _section = new RafyConfigurationSection();
                rafyRawSection.Bind(_section);
#endif
            }
        }

        /// <summary>
        /// 开发语言文化代码，默认值是 zh-CN。
        /// </summary>
        public string DevCulture { get; set; }
    }
}
