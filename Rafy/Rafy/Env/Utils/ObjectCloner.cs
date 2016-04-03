/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120404
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120404
 * 
*******************************************************/

using System;
using System.IO;
using Rafy.Serialization;

namespace Rafy.Utils
{
    internal static class ObjectCloner
    {
        /// <summary>
        /// 使用序列化器来序列化反序列化的方式，深度复制一个对象
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T Clone<T>(T obj)
        {
            using (MemoryStream buffer = new MemoryStream())
            {
                var formatter = SerializationFormatterFactory.GetFormatter();
                formatter.Serialize(buffer, obj);
                buffer.Position = 0;
                object temp = formatter.Deserialize(buffer);
                return (T)temp;
            }
        }
    }
}