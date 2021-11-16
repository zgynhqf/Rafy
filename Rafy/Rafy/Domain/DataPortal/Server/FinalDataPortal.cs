/*******************************************************
 * 
 * 作者：CSLA
 * 创建时间：2008
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 2010
 * 
*******************************************************/

using Rafy.Reflection;
using System;
using System.Configuration;
using System.Globalization;
using System.Security.Principal;
using System.Threading;

namespace Rafy.Domain.DataPortal
{
    /// <summary>
    /// 最终调用实体的 IDataPortalServer 门户实现。
    /// 不论是通过 WCFProxy、还是通过 FakeRemoteProxy，都会调用到 FinalDataPortal 中。
    /// </summary>
    internal class FinalDataPortal : IDataPortalServer
    {
        public DataPortalResult Call(object obj, string method, object[] parameters, DataPortalContext context)
        {
            try
            {
                SetContext(context);

                var result = DoCall(obj, method, parameters);

                return result;
            }
            finally
            {
                ClearContext(context);
            }
        }

        /// <summary>
        /// 核心实现
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="method"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        internal static DataPortalResult DoCall(object obj, string method, object[] parameters)
        {
            try
            {
                RafyEnvironment.ThreadPortalCount++;

                //如果目标对象需要使用工厂，那么先找到其对应的工厂，然后再通过工厂来获取对应的对象。
                var factoryInfo = obj as DataPortalTargetFactoryInfo;
                if (factoryInfo != null)
                {
                    var factory = DataPortalTargetFactoryRegistry.Get(factoryInfo.FactoryName);
                    obj = factory.GetTarget(factoryInfo);
                }

                //非工厂模式下，直接使用反射进行调用。
                var res = MethodCaller.CallMethod(obj, method, parameters);
                var outArgs = ReadOutParameters(parameters);

                return new DataPortalResult(res, outArgs);
            }
            finally
            {
                RafyEnvironment.ThreadPortalCount--;
            }
        }

        private static object[] ReadOutParameters(object[] parameters)
        {
            if (parameters == null) return null;

            object[] res = null;

            for (int i = 0; i < parameters.Length; i++)
            {
                var arg = parameters[i];
                if (arg is IDataPortalOutArgument && (arg as IDataPortalOutArgument).NeedTransferToClient())
                {
                    if (res == null) res = new object[parameters.Length];
                    res[i] = arg;
                }
            }

            return res;
        }

        private static void SetContext(DataPortalContext context)
        {
            // set the app context to the value we got from the
            // client
            DistributionContext.ClientContextItem.Value = context.ClientContext;
            DistributionContext.GlobalContextItem.Value = context.GlobalContext;

            // set the thread's culture to match the client
            Thread.CurrentThread.CurrentCulture = new CultureInfo(context.ClientCulture);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(context.ClientUICulture);

            RafyEnvironment.Principal = context.Principal;
        }

        private static void ClearContext(DataPortalContext context)
        {
            DistributionContext.ClientContextItem.Value = null;
            DistributionContext.GlobalContextItem.Value = null;
            RafyEnvironment.Principal = null;
        }
    }

    /// <summary>
    /// 如果远程调用方法时，某个传入的参数需要再次被传输到客户端时，需要将参数类型实现这个接口。
    /// </summary>
    public interface IDataPortalOutArgument
    {
        /// <summary>
        /// 可以根据当前对象的状态。来决定当前是否需要将参数回传到客户端。
        /// </summary>
        /// <returns></returns>
        bool NeedTransferToClient();
    }
}