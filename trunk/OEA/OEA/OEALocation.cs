using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OEA
{
    /// <summary>
    /// 当前应用程序执行环境的位置
    /// </summary>
    public enum OEALocation
    {
        Client, WPFServer, LocalVersion, WebServer
    }

    public static class OEALocationExtension
    {
        /// <summary>
        /// 判断是否在服务端
        /// 
        /// 单机版，同样返回true。
        /// </summary>
        /// <returns></returns>
        public static bool IsOnServer(this OEALocation l)
        {
            return l != OEALocation.Client;
        }

        /// <summary>
        /// 判断是否在客户端
        /// 
        /// 单机版，同样返回true。
        /// </summary>
        /// <returns></returns>
        public static bool IsOnClient(this OEALocation l)
        {
            return l == OEALocation.Client || l == OEALocation.LocalVersion;
        }
    }
}
