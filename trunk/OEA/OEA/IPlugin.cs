/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110321
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100321
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OEA
{
    /// <summary>
    /// 插件
    /// </summary>
    public interface IPlugin
    {
        /// <summary>
        /// 插件的 721 级别
        /// </summary>
        ReuseLevel ReuseLevel { get; }
    }
}
