/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130405 23:41
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130405 23:41
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.Domain
{
    /// <summary>
    /// 数据访问层执行的地点
    /// </summary>
    public enum DataPortalLocation
    {
        /// <summary>
        /// 根据 RafyEnvironment.Location 而判断是否在远程服务端执行。
        /// 
        /// 此种状态下，目前只有 RafyLocation.WPFClient 的位置时，才会选择在远程服务器执行。20130118
        /// </summary>
        Dynamic,
        /// <summary>
        /// 将在当前机器执行。
        /// </summary>
        Local,
    }
}
