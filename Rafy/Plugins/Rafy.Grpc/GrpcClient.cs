using Google.Protobuf;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rafy.Grpc
{
    internal class GrpcClient
    {
        /// <summary>
        /// Target of the channel
        /// </summary>
        public string Target { get; set; }

        public byte[] CallCore(byte[] data)
        {
            Channel channel = new Channel(this.Target, ChannelCredentials.Insecure);

            var client = new DataTransferService.DataTransferServiceClient(channel);
            var reply = client.Call(new GrpcData { Data = ByteString.CopyFrom(data) });

            channel.ShutdownAsync();

            return reply.Data.ToByteArray();
        }
    }
}