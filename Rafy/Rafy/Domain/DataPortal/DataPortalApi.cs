/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：2012
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 2012
 * 
*******************************************************/

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Principal;
using System.Threading;
using Rafy;
using Rafy.Reflection;
using Rafy.Serialization;
using Rafy.Utils;

namespace Rafy.Domain.DataPortal
{
    /// <summary>
    /// 数据门户。
    /// 内部封装了对数据层的调用，如果是远程，则使用对应的代理来访问，这使得单机版、网络版的调用完全一致。详见：<see cref="DataPortalCallInterceptor"/>。
    /// </summary>
    public static class DataPortalApi
    {
        /// <summary>
        /// 是否需要在模拟远程调用（进行对象的复制）。
        /// 
        /// 由于开发人员平时会使用单机版本开发，而正式部署时，又会选用 C/S 架构。
        /// 所以需要保证单机版本和 C/S 架构版本的模式是一样的。也就是说，在单机模式下，
        /// 在通过门户访问时，模拟网络版，clone 出一个新的对象。
        /// 这样，在底层 Update 更改 obj 时，不会影响上层的实体。
        /// 而是以返回值的形式把这个被修改的实体返回给上层。
        /// </summary>
        public static bool FakeRemote { get; set; } = false;

        public static object Call(object obj, string method, object[] arguments)
        {
            var proxy = GetDataPortalProxy();

            var dpContext = CreateDataPortalContext();

            var result = proxy.Call(obj, method, arguments, dpContext);

            var res = ReadServerResult(result);

            return res;
        }

        #region Helpers

        private static Type _proxyType;

        private static IDataPortalProxy GetDataPortalProxy()
        {
            if (_proxyType == null)
            {
                _proxyType = Type.GetType(RafyEnvironment.Configuration.Section.DataPortalProxy, true, true);
            }
            return Activator.CreateInstance(_proxyType) as IDataPortalProxy;
        }

        private static object ReadServerResult(DataPortalResult result)
        {
            //同步服务端返回的统一的上下文，到本地的上下文对象中。
            DistributionContext.GlobalContextItem.Value = result.GlobalContext;

            return result.ReturnObject;
        }

        /// <summary>
        /// Creates the data portal context.
        /// </summary>
        /// <returns></returns>
        private static DataPortalContext CreateDataPortalContext()
        {
            var res = new DataPortalContext();

            res.Principal = RafyEnvironment.Principal;
            res.ClientCulture = Thread.CurrentThread.CurrentCulture.Name;
            res.ClientUICulture = Thread.CurrentThread.CurrentUICulture.Name;
            res.ClientContext = DistributionContext.ClientContextItem.Value;
            res.GlobalContext = DistributionContext.GlobalContextItem.Value;

            return res;
        }

        #endregion
    }
}