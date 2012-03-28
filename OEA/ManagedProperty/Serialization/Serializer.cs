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
using OEA.Serialization;
using OEA.Serialization.Mobile;
using System.Runtime.Serialization;

namespace OEA.Serialization
{
    /// <summary>
    /// 序列化门户 API
    /// </summary>
    public static class Serializer
    {
        public static byte[] Serialize(object value)
        {
            if (value == null) return null;

            var fomatter = SerializationFormatterFactory.GetFormatter();
            var stream = new MemoryStream();
            fomatter.Serialize(stream, value);

            //var dto = Encoding.UTF8.GetString(stream.GetBuffer());
            var dto = stream.ToArray();
            return dto;
        }

        public static object Deserialize(byte[] bytes)
        {
            if (bytes == null) return null;

            //var bytes = Encoding.UTF8.GetBytes(dto as string);
            var stream = new MemoryStream(bytes);

            var bf = SerializationFormatterFactory.GetFormatter();
            var result = bf.Deserialize(stream);

            return result;
        }
    }
}
