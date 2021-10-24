/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20211024
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.Net Standard 2.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20211024 17:14
 * 
*******************************************************/

using Rafy.ManagedProperty;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Rafy.Serialization
{
    internal class CustomSerializationSurrogate : ISerializationSurrogate
    {
        public static readonly CustomSerializationSurrogate Instance = new CustomSerializationSurrogate();

        private CustomSerializationSurrogate() { }

        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            (obj as ISerializable).GetObjectData(info, context);
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            (obj as ICustomSerializationObject).SetObjectData(info, context);
            return obj;
        }
    }
}
