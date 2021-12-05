/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20211205
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20211205 17:18
 * 
*******************************************************/

using Rafy.Reflection;
using Rafy.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

#if NET45

namespace Rafy.DataPortal.WCF
{
    internal class WCFSerializationWrapper
    {
        public static void Serialize(CallRequest request)
        {
            if (WCFSettings.EnableBinarySerialization)
            {
                var arguments = request.Arguments;

                if (arguments != null && arguments.Length > 0)
                {
                    var copiedParameters = new object[arguments.Length];
                    for (int i = 0; i < arguments.Length; i++)
                    {
                        var argument = arguments[i];
                        if (argument != null)
                        {
                            byte[] bytes = Serialize(argument);
                            copiedParameters[i] = bytes ?? argument;
                        }
                    }
                    request.Arguments = copiedParameters;
                }
            }
        }

        public static void Deserialize(CallRequest request)
        {
            if (WCFSettings.EnableBinarySerialization)
            {
                var arguments = request.Arguments;
                if (arguments != null && arguments.Length > 0)
                {
                    var copiedParameters = new object[arguments.Length];
                    for (int i = 0; i < arguments.Length; i++)
                    {
                        var argument = arguments[i];
                        if (argument is byte[] bytes)
                        {
                            var arg = Deserialize(bytes);
                            copiedParameters[i] = arg;
                        }
                        else
                        {
                            copiedParameters[i] = argument;
                        }
                    }
                    request.Arguments = copiedParameters;
                }
            }
        }

        public static void Serialize(WcfResponse response)
        {
            if (WCFSettings.EnableBinarySerialization)
            {
                var result = response.Result;
                if (result != null)
                {
                    byte[] bytes = Serialize(result);

                    response.Result = bytes;
                }
            }
        }

        public static void Deserialize(WcfResponse response)
        {
            if (WCFSettings.EnableBinarySerialization)
            {
                if (response.Result is byte[] bytes)
                {
                    response.Result = Deserialize(bytes);
                }
            }
        }

        private static byte[] Serialize(object result)
        {
            var isPrimitive = TypeHelper.IsPrimitive(result.GetType());
            if (!isPrimitive)
            {
                var bytes = BinarySerializer.SerializeBytes(result);

                if (WCFSettings.EnableCompacting)
                {
                    bytes = Compress(bytes);
                }

                return bytes;
            }

            return null;
        }

        private static object Deserialize(byte[] bytes)
        {
            if (WCFSettings.EnableCompacting)
            {
                bytes = Decompress(bytes);
            }

            var result = BinarySerializer.DeserializeBytes(bytes);

            return result;
        }

        private static byte[] Compress(byte[] bytes)
        {
            MemoryStream memoryStream = new MemoryStream();

            using (GZipStream gzStream = new GZipStream(memoryStream, CompressionMode.Compress, true))
            {
                gzStream.Write(bytes, 0, bytes.Length);
            }

            return memoryStream.ToArray();
        }

        private static byte[] Decompress(byte[] bytes)
        {
            MemoryStream decompressedStream = new MemoryStream();

            int blockSize = 1024;
            byte[] tempBuffer = new byte[blockSize];
            using (GZipStream gzStream = new GZipStream(new MemoryStream(bytes), CompressionMode.Decompress))
            {
                while (true)
                {
                    int bytesRead = gzStream.Read(tempBuffer, 0, blockSize);
                    if (bytesRead == 0) break;

                    decompressedStream.Write(tempBuffer, 0, bytesRead);
                }
            }

            return decompressedStream.ToArray();
        }
    }
}

#endif