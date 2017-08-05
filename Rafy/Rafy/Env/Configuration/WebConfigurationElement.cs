/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130125 11:32
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130125 11:32
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
    public class WebConfigurationElement : ConfigurationSection
    {
        public WebConfigurationElement(ConfigurationRoot root, string path) : base(root, path) { }
    }
}