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
        public const string OEAPlugins = "OEAPlugins";

        public const string DbMigrationHistory = "DbMigrationHistory";
    }

    public static class ComposableNames
    {
        public const string MainWindow = "OEA.MainWindow";

        public const string MainWindow_TopBanner = "MainWindow_TopBanner";

        public const string LoginWindow = "OEA.LoginWindow";
    }
}