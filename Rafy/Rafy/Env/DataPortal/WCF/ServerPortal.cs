﻿/*******************************************************
 * 
 * 作者：CSLA
 * 创建日期：2008
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 周金根 2008
 * 
*******************************************************/
#if NET45

using System;
using System.ServiceModel;
using System.ServiceModel.Activation;

namespace Rafy.DataPortal.WCF
{
    /// <summary>
    /// 使用 WCF 实现的统一的数据门户。
    /// 
    /// 标记了 ConcurrencyMode.Multiple 来表示多线程进行
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class ServerPortal : IWcfPortal
    {
        /// <summary>
        /// Get an existing business object.
        /// </summary>
        /// <param name="request">The request parameter object.</param>
        public WcfResponse Call(CallRequest request)
        {
            var portal = new FinalDataPortal();

            object result;
            try
            {
                result = portal.Call(request.Instance, request.Method, request.Arguments, request.Context);
            }
            catch (Exception ex)
            {
                result = ex;
            }

            return new WcfResponse { Result = result };
        }

        public string Test(string msg)
        {
            return msg + " recieved";
        }
    }
}

#endif