using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy
{
    /// <summary>
    /// 数据门户模式。
    /// </summary>
    public enum DataPortalMode
    {
        /// <summary>
        /// 应用程序直接连接数据。
        /// </summary>
        ConnectDirectly = 0,
        /// <summary>
        /// 应用程序通过服务来连接数据。
        /// </summary>
        ThroughService = 1

        ///// <summary>
        ///// 当前应用程序是否是 WCF 的客户端。
        ///// </summary>
        //public bool IsClient { get; set; }

        ///// <summary>
        ///// 当前应用程序是否是直接连接数据库的版本
        ///// </summary>
        //public bool IsLocalVersion { get; set; }

        ///// <summary>
        ///// 当前应用程序是否作为 WCF 的服务端。
        ///// </summary>
        //public bool IsServer { get; set; }
    }

    ///// <summary>
    ///// 当前应用程序执行环境的位置
    ///// </summary>
    //[Obsolete("请使用 LocationInformation！")]
    //public enum RafyLocation
    //{
    //    /// <summary>
    //    /// C-S 架构中的 WPF 客户端。
    //    /// </summary>
    //    WPFClient,
    //    /// <summary>
    //    /// C-S 架构中的服务端。
    //    /// </summary>
    //    WCFServer,
    //    /// <summary>
    //    /// 单机版，同时具有数据访问、界面层代码。
    //    /// </summary>
    //    LocalVersion,
    //    /// <summary>
    //    /// Web 服务器。
    //    /// 
    //    /// 其中的界面层代码就是放在实体 dll 中的 js 代码。
    //    /// 
    //    /// 本服务端不能作为 C-S 架构的服务端。
    //    /// 同时，它虽然加载 Module 中的 dll，但是不会初始化这些插件。
    //    /// </summary>
    //    WebServer
    //}
}
