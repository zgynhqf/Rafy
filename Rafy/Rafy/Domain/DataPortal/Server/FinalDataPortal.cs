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
        public DataPortalResult Call(object obj, string method, object[] arguments, DataPortalContext context)
        {
            try
            {
                SetContext(context);

                var result = DoCall(obj, method, arguments);
                return new DataPortalResult(result);
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
        /// <param name="arguments"></param>
        /// <returns></returns>
        internal static object DoCall(object obj, string method, object[] arguments)
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
                return MethodCaller.CallMethod(obj, method, arguments);
            }
            finally
            {
                RafyEnvironment.ThreadPortalCount--;
            }
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
}