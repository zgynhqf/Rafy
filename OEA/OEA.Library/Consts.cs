using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OEA
{
    /// <summary>
    /// 数据库使用的连接字符串名
    /// 统一管理Connection字符串
    /// </summary>
    public static class ConnectionStringNames
    {
        /// <summary>
        /// 默认情况下 OEA 插件使用的库配置名。
        /// 
        /// 外界可以在程序启动时修改这个值以使得它和其它的库使用同一个配置。
        /// </summary>
        public static string OEAPlugins = "OEAPlugins";

        /// <summary>
        /// 默认情况下迁移记录库使用的库配置名。
        /// 
        /// 外界可以在程序启动时修改这个值以使得它和其它的库使用同一个配置。
        /// </summary>
        public static string DbMigrationHistory = "DbMigrationHistory";
    }

    public static class ComposableNames
    {
        public const string MainWindow = "OEA.MainWindow";

        public const string MainWindow_TopBanner = "MainWindow_TopBanner";

        public const string LoginWindow = "OEA.LoginWindow";
    }
}