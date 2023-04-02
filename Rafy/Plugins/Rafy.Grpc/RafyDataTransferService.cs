using Google.Protobuf;
using Grpc.Core;
using Rafy.DataPortal;
using Rafy.Serialization;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Rafy.Grpc
{
    /// <summary>
    /// Rafy DataPortal 的核心 Grpc 服务。
    /// </summary>
    public class RafyDataTransferService : DataTransferService.DataTransferServiceBase
    {
        public override Task<GrpcData> Call(GrpcData GrpcCallRequest, ServerCallContext context)
        {
            var requestBytes = GrpcCallRequest.Data.ToByteArray();
            var request = BinarySerializer.DeserializeBytes(requestBytes) as GrpcCallRequest;

            LogCalling(request);

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

            var responseBytes = BinarySerializer.SerializeBytes(result);

            if (!(result is Exception))
            {
                LogCalled(request, responseBytes);
            }

            return Task.FromResult(new GrpcData
            {
                Data = ByteString.CopyFrom(responseBytes)
            });
        }

        private void LogCalling(GrpcCallRequest request)
        {
            var content = $"Grpc Call invoking. request: method:{request.Method}, instance: {request.Instance}, arguments.length:{request.Arguments.Length}";
            var totalBytes = GetTotalBytes(request);
            if (totalBytes > 0)
            {
                content += $", arguments bytes:{totalBytes}";
            }
            Logger.LogInfo(content);
        }

        private void LogCalled(GrpcCallRequest request, byte[] response)
        {
            var content = $"Grpc Call invoked. request: method:{request.Method}, instance: {request.Instance}, arguments.length:{request.Arguments.Length}";
            if (response is byte[] bytes)
            {
                content += $", result bytes:{bytes.Length}";
            }
            Logger.LogInfo(content);
        }

        private void LogException(GrpcCallRequest request, Exception ex)
        {
            Logger.LogException($"Grpc Call Exception occurred! request: method:{request.Method}, instance: {request.Instance}, arguments.length:{request.Arguments.Length}.", ex);
        }

        private int GetTotalBytes(GrpcCallRequest request)
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