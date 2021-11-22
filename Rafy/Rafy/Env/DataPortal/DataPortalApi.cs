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

namespace Rafy.DataPortal
{
    /// <summary>
    /// 数据门户。
    /// 内部封装了对数据层的调用，如果是远程，则使用对应的代理来访问，这使得单机版、网络版的调用完全一致。详见：<see cref="DataPortalCallInterceptor"/>。
    /// </summary>
    public static class DataPortalApi
    {
        [ThreadStatic]
        internal static int _threadPortalCount;

        /// <summary>
        /// 获取当前线程目前已经进入的数据门户层数。
        /// </summary>
        public static int ThreadPortalCount
        {
            get { return _threadPortalCount; }
        }

        /// <summary>
        /// 表示当前线程执行的代码是否正处于数据门户的调用中。
        /// * 当环境为 C/S 架构的服务端时，所有通过服务访问的代码，应该始终处于门户的调用中。 
        /// * 当环境为 C/S 架构的客户端时，所有访问服务以外的代码，应该都不处于门户的调用中。 
        /// * 当环境为 直连数据 架构时，所有代码应该都不处于门户的调用中。 
        /// </summary>
        public static bool IsRunning => _threadPortalCount > 0;

        /// <summary>
        /// 应用程序默认的数据门户模式。
        /// </summary>
        public static DataPortalMode DataPortalMode { get; set; } = DataPortalMode.ConnectDirectly;

        /// <summary>
        /// 是否应用程序直接连接数据。
        /// DataPortalMode == DataPortalMode.ConnectDirectly。
        /// </summary>
        public static bool ConnectDataDirectly
        {
            get { return DataPortalMode == DataPortalMode.ConnectDirectly; }
        }

        internal static bool IsFakeingRemote => GetDataPortalProxy() is FakeRemoteProxy;

        /// <summary>
        /// 远程调用指定对象的指定方法。
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="method"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static object Call(object obj, string method, object[] parameters)
        {
            var proxy = GetDataPortalProxy();

            var dpContext = CreateDataPortalContext();

            var result = proxy.Call(obj, method, parameters, dpContext);

            var res = DealServerResult(result, parameters);

            return res;
        }

        private static Type _proxyType;

        private static IDataPortalProxy GetDataPortalProxy()
        {
            if (_proxyType == null)
            {
                _proxyType = Type.GetType(RafyEnvironment.Configuration.Section.DataPortalProxy, true, true);
            }
            return Activator.CreateInstance(_proxyType) as IDataPortalProxy;
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

        private static object DealServerResult(DataPortalResult result, object[] parameters)
        {
            //同步服务端返回的统一的上下文，到本地的上下文对象中。
            DistributionContext.GlobalContextItem.Value = result.GlobalContext;

            //处理 OutParameters
            var outParameters = result.OutParameters;
            if (outParameters != null)
            {
                for (int i = 0, c = outParameters.Length; i < c; i++)
                {
                    var parameter = outParameters[i];
                    if (parameter != null)
                    {
                        ObjectHelper.CloneFields(parameters[i], outParameters[i]);
                    }
                }
            }

            return result.ReturnObject;
        }
    }
}