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
    public class WPFConfigurationElement : ConfigurationSection
    {
        public WPFConfigurationElement(ConfigurationRoot root, string path) : base(root, path) { }

        ///// <summary>
        ///// 闪屏的时间控制
        ///// </summary>
        //[ConfigurationProperty("splashTimeSpan", DefaultValue = 2500)]
        //public int SplashTimeSpan
        //{
        //    get { return (int)this["splashTimeSpan"]; }
        //    set { this["splashTimeSpan"] = value; }
        //}

        /// <summary>
        /// 是否显示错误的详细信息。
        /// </summary>
        //[ConfigurationProperty("showErrorDetail", DefaultValue = DynamicBoolean.IsDebugging)]
        public DynamicBoolean ShowErrorDetail
        {
            get
            {
                string val = (string)this["showErrorDetail"];
                if (string.IsNullOrEmpty(val))
                {
                    return (DynamicBoolean)Enum.Parse(typeof(DynamicBoolean), val);
                }
                return DynamicBoolean.IsDebugging;
            }
            set
            {
                this["showErrorDetail"] = value.ToString();
            }
        }

        /// <summary>
        /// 使用的皮肤名称。
        /// </summary>
        //[ConfigurationProperty("skin", DefaultValue = "Blue")]
        public string Skin
        {
            get { return this["skin"]; }
            set { this["skin"] = value; }
        }
    }
}
