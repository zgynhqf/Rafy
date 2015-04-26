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
    /// 在客户端与服务端间提供范围性的数据传输。
    /// </summary>
    public static class DistributionContext
    {
        private static object _syncClientContext = new object();

        private const string _clientContextName = "Rafy.ClientContext";

        private const string _globalContextName = "Rafy.GlobalContext";

        /// <summary>
        /// 客户端提供的范围数据。
        /// </summary>
        /// <remarks>
        /// <para>
        /// 此数据只会从客户端向服务端传输。
        /// </para>
        /// </remarks>
        public static HybridDictionary ClientContext
        {
            get
            {
                var ctx = GetClientContext();
                if (ctx == null)
                {
                    lock (_syncClientContext)
                    {
                        ctx = GetClientContext();
                        if (ctx == null)
                        {
                            ctx = new HybridDictionary();
                            SetClientContext(ctx);
                        }
                    }
                }
                return ctx;
            }
        }

        /// <summary>
        /// 伴随每次传输的上下文数据。
        /// </summary>
        /// <remarks>
        /// <para>
        /// 这些数据会伴随客户端到服务端、服务端到客户端的双向传输。
        /// </para>
        /// </remarks>
        public static HybridDictionary GlobalContext
        {
            get
            {
                var ctx = GetGlobalContext();
                if (ctx == null)
                {
                    ctx = new HybridDictionary();
                    SetGlobalContext(ctx);
                }
                return ctx;
            }
        }

        internal static HybridDictionary GetClientContext()
        {
            if (HttpContext.Current == null)
            {
                if (RafyEnvironment.Location.IsWPFUI)
                {
                    return (HybridDictionary)AppDomain.CurrentDomain.GetData(_clientContextName);
                }
                else
                {
                    var slot = Thread.GetNamedDataSlot(_clientContextName);
                    return (HybridDictionary)Thread.GetData(slot);
                }
            }
            else
            {
                return (HybridDictionary)HttpContext.Current.Items[_clientContextName];
            }
        }

        internal static HybridDictionary GetGlobalContext()
        {
            if (HttpContext.Current == null)
            {
                var slot = Thread.GetNamedDataSlot(_globalContextName);
                return (HybridDictionary)Thread.GetData(slot);
            }
            else
            {
                return (HybridDictionary)HttpContext.Current.Items[_globalContextName];
            }
        }

        internal static void SetClientContext(HybridDictionary clientContext)
        {
            if (HttpContext.Current == null)
            {
                if (RafyEnvironment.Location.IsWPFUI)
                {
                    AppDomain.CurrentDomain.SetData(_clientContextName, clientContext);
                }
                else
                {
                    var slot = Thread.GetNamedDataSlot(_clientContextName);
                    Thread.SetData(slot, clientContext);
                }
            }
            else
            {
                HttpContext.Current.Items[_clientContextName] = clientContext;
            }
        }

        internal static void SetGlobalContext(HybridDictionary globalContext)
        {
            if (HttpContext.Current == null)
            {
                var slot = Thread.GetNamedDataSlot(_globalContextName);
                Thread.SetData(slot, globalContext);
            }
            else
            {
                HttpContext.Current.Items[_globalContextName] = globalContext;
            }
        }

        /// <summary>
        /// Clears all context collections.
        /// </summary>
        internal static void Clear()
        {
            SetClientContext(null);
            SetGlobalContext(null);
            AppContext.Clear();
        }
    }
}