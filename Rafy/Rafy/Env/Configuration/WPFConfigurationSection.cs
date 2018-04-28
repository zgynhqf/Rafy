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

#if NETSTANDARD2_0 || NETCOREAPP2_0
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace Rafy.Configuration
{
    public class WPFConfigurationSection
    {
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
        public DynamicBoolean ShowErrorDetail { get; set; } = DynamicBoolean.IsDebugging;

        /// <summary>
        /// 使用的皮肤名称。
        /// </summary>
        public string Skin { get; set; } = "Blue";
    }
}
#endif
