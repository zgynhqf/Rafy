/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20211024
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.Net Standard 2.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20211024 06:56
 * 
*******************************************************/

using Rafy.ManagedProperty;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Rafy.Serialization
{
    /// <summary>
    /// 托管属性对象序列化时的选择器。
    /// </summary>
    public class CustomSurrogateSelector : SurrogateSelector
    {
        public override ISerializationSurrogate GetSurrogate(Type type, StreamingContext context, out ISurrogateSelector selector)
        {
            selector = this;

            if (typeof(ICustomSerializationObject).IsAssignableFrom(type))
            {
                //https://social.msdn.microsoft.com/Forums/en-US/8558a5d4-de9d-466d-a9a6-7eb37f31aa21/deserializing-problem-where-to-find-the-bugfix
                return FormatterServices.GetSurrogateForCyclicalReference(CustomSerializationSurrogate.Instance);
                //return CustomSerializationSurrogate.Instance;
            }

            return base.GetSurrogate(type, context, out selector);
        }
    }
}
