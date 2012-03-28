/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20100612
 * 说明：友好消息的异常类。
 * 运行环境：.NET 3.5 SP1
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100612
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace OEA
{
    /// <summary>
    /// 如果是使用这个类，传入的消息则会直接友好地显示给用户。
    /// 
    /// 注意，当使用这个异常时，必须保证中断后的流程不会出现安全问题！
    /// </summary>
    [Serializable]
    public class FriendlyMessageException : Exception
    {
        public FriendlyMessageException() { }
        public FriendlyMessageException(string message) : base(message) { }
        public FriendlyMessageException(string message, Exception inner) : base(message, inner) { }
        protected FriendlyMessageException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
