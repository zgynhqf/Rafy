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

namespace SimpleCsla
{
    /// <summary>
    /// 一个适配 Serializer 接口的类
    /// </summary>
    internal static class InnerSerializer
    {
        internal static object SerializeObject(object value)
        {
            //如果想在 CompactMessageEncoder 中查看原始的 XML 值，
            //则把 InnerSerializer 内部的功能禁用掉，这样，WCF就会直接用 XML 把整个对象进行序列化。
            //return value;

            return Serializer.Serialize(value);
        }

        internal static object DeserializeObject(object dto)
        {
            //return dto;

            return Serializer.Deserialize(dto as byte[]);
        }
    }
}
