/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20111110
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20111110
 * 
*******************************************************/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Rafy.Serialization;
using Rafy.Serialization.Mobile;
using System.Runtime.Serialization;

namespace Rafy.Serialization
{
    /// <summary>
    /// 序列化门户 API
    /// </summary>
    public static class Serializer
    {
        /// <summary>
        /// 使用二进制序列化对象。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] SerializeBytes(object value)
        {
            if (value == null) return null;

            var stream = new MemoryStream();
            new BinaryFormatter().Serialize(stream, value);

            //调试使用
            //var dto = Encoding.UTF8.GetString(stream.GetBuffer());

            var bytes = stream.ToArray();
            return bytes;
        }

        /// <summary>
        /// 使用二进制反序列化对象。
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static object DeserializeBytes(byte[] bytes)
        {
            if (bytes == null) return null;

            //调试使用
            //var bytes = Encoding.UTF8.GetBytes(dto as string);

            var stream = new MemoryStream(bytes);

            var result = new BinaryFormatter().Deserialize(stream);

            return result;
        }
    }
}
