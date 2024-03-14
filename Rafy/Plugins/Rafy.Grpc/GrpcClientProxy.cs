/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20220226
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20220226 19:23
 * 
*******************************************************/

using Rafy.DataPortal;
using Rafy.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rafy.Grpc
{
    /// <summary>
    /// 使用 Grpc 客户端来实现的数据门户代理。
    /// </summary>
    public class GrpcClientProxy : IDataPortalProxy
    {
        public DataPortalResult Call(object obj, string method, object[] arguments, DataPortalContext context)
        {
            var request = new GrpcCallRequest
            {
                Instance = obj,
                Method = method,
                Arguments = arguments,
                Context = context,
            };

            var requestBytes = BinarySerializer.SerializeBytes(request);

            var client = GetClient();

            var responseBytes = client.CallCore(requestBytes);

            var response = BinarySerializer.DeserializeBytes(responseBytes);

            if (response is Exception responseEx)
            {
                throw new DataPortalException("连接服务器时，发生异常，请检查 InnerException。", responseEx);
            }
            return (DataPortalResult)response;
        }

        /// <summary>
        /// 获取服务端地址。
        /// 子类可重写此方法实现获取服务地址的逻辑。例如，可以使用 Consul 等服务发现。
        /// 默认实现为读取 JM:Grpc:GrpcClientProxy:Taget 配置。
        /// </summary>
        /// <returns></returns>
        protected virtual string GetServerAddress()
        {
            return ConfigurationHelper.GetAppSettingOrDefault("Rafy:Grpc:GrpcClientProxy:Taget", "127.0.0.1:9007");
        }

        private GrpcClient GetClient()
        {
            var client = new GrpcClient();

            client.Target = this.GetServerAddress();

            return client;
        }
    }
}
