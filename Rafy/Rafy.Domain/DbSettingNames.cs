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
    /// 数据库使用的连接字符串名
    /// 统一管理Connection字符串
    /// </summary>
    public static class DbSettingNames
    {
        /// <summary>
        /// 默认情况下 Rafy 插件使用的库配置名。
        /// 
        /// 外界可以在程序启动时修改这个值以使得它和其它的库使用同一个配置。
        /// </summary>
        public static string RafyPlugins = "RafyPlugins";

        /// <summary>
        /// 默认情况下迁移记录库使用的库配置名。
        /// 
        /// 外界可以在程序启动时修改这个值以使得它和其它的库使用同一个配置。
        /// </summary>
        public static string DbMigrationHistory = "DbMigrationHistory";
    }
}