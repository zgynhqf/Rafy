/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20211024
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.Net Standard 2.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20211024 17:18
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Rafy.Serialization
{
    /// <summary>
    /// 一个可自定义序列化、反序列化的对象。
    /// </summary>
    public interface ICustomSerializationObject : ISerializable
    {
        /// <summary>
        /// 反序列化时，读取序列化信息中的内容到本对象。
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        void SetObjectData(SerializationInfo info, StreamingContext context);
    }
}