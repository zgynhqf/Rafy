/*******************************************************
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
    //[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class ServerPortal : IWcfPortal
    {
        public string Test(string msg)
        {
            Logger.LogInfo("WCF Test invoked. msg : " + msg);

            return msg + " recieved";
        }

        /// <summary>
        /// Get an existing business object.
        /// </summary>
        /// <param name="request">The request parameter object.</param>
        public WcfResponse Call(CallRequest request)
        {
            LogCalling(request);

            WCFSerializationWrapper.Deserialize(request);

            var portal = new FinalDataPortal();

            object result;
            try
            {
                result = portal.Call(request.Instance, request.Method, request.Arguments, request.Context);
            }
            catch (Exception ex)
            {
                result = ex;
                LogException(request, ex);
            }

            var response = new WcfResponse { Result = result };

            WCFSerializationWrapper.Serialize(response);

            if (!(result is Exception))
            {
                LogCalled(request, response);
            }

            return response;
        }

        private void LogCalling(CallRequest request)
        {
            var content = $"WCF Call invoking. request: method:{request.Method}, instance: {request.Instance}, arguments.length:{request.Arguments.Length}";
            var totalBytes = GetTotalBytes(request);
            if (totalBytes > 0)
            {
                content += $", arguments bytes:{totalBytes}";
            }
            Logger.LogInfo(content);
        }

        private void LogCalled(CallRequest request, WcfResponse response)
        {
            var content = $"WCF Call invoked. request: method:{request.Method}, instance: {request.Instance}, arguments.length:{request.Arguments.Length}";
            if (response.Result is byte[] bytes)
            {
                content += $", result bytes:{bytes.Length}";
            }
            Logger.LogInfo(content);
        }

        private void LogException(CallRequest request, Exception ex)
        {
            Logger.LogException($"WCF Call Exception occurred! request: method:{request.Method}, instance: {request.Instance}, arguments.length:{request.Arguments.Length}.", ex);
        }

        private int GetTotalBytes(CallRequest request)
        {
            var totalBytes = 0;

            var arguments = request.Arguments;
            for (int i = 0, c = arguments.Length; i < c; i++)
            {
                var argument = arguments[i];
                if (argument is byte[] bytes)
                {
                    totalBytes += bytes.Length;
                }
            }

            return totalBytes;
        }
    }
}

#endif