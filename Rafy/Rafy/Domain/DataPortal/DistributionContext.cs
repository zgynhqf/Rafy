/*******************************************************
 * 
 * 作者：CSLA
 * 创建时间：2008
 * 说明：
 * 运行环境：.NET 3.5 SP1
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 CSLA 2008
 * 实现线程安全(同步字段：_logicalExecutionLocation，_principal) 胡庆访 20100526
 * 不再使用名称 ApplicationContext。 胡庆访 20130508
 * 
*******************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Security.Principal;
using System.Threading;
using System.Web;

namespace Rafy.Domain.DataPortal
{
    /// <summary>
    /// 分布式数据上下文。
    /// 在这个类中控制：调用者端（客户端）与被调用者端（服务端）之间的范围性数据的传输。
    /// </summary>
    public static class DistributionContext
    {
        private static object _clientContextLock = new object();

        internal static readonly AppContextItem<Dictionary<string, object>> ClientContextItem =
            new AppContextItem<Dictionary<string, object>>("Rafy.Domain.DataPortal.ClientContext");

        internal static readonly AppContextItem<Dictionary<string, object>> GlobalContextItem =
            new AppContextItem<Dictionary<string, object>>("Rafy.Domain.DataPortal.GlobalContext");

        /// <summary>
        /// 客户端提供的范围数据。
        /// 这些数据只会从客户端向服务端传输。
        /// </summary>
        public static Dictionary<string, object> ClientContext
        {
            get
            {
                var ctx = ClientContextItem.Value;
                if (ctx == null)
                {
                    lock (_clientContextLock)
                    {
                        ctx = ClientContextItem.Value;
                        if (ctx == null)
                        {
                            ctx = new Dictionary<string, object>();
                            ClientContextItem.Value = ctx;
                        }
                    }
                }
                return ctx;
            }
        }

        /// <summary>
        /// 伴随每次传输的上下文数据。
        /// 这些数据会伴随客户端到服务端、服务端到客户端的双向传输。
        /// </summary>
        public static Dictionary<string, object> GlobalContext
        {
            get
            {
                var ctx = GlobalContextItem.Value;
                if (ctx == null)
                {
                    ctx = new Dictionary<string, object>();
                    GlobalContextItem.Value = ctx;
                }
                return ctx;
            }
        }
    }
}