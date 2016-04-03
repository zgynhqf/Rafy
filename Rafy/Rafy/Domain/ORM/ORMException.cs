/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：201204
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 201204
 * 
*******************************************************/

using System;
using System.Runtime.Serialization;

namespace Rafy.Domain.ORM
{
    /// <summary>
    /// ORM 操作异常
    /// </summary>
    [Serializable]
    public class ORMException : Exception
    {
        public ORMException(string message) : base(message) { }

        public ORMException(string message, Exception cause) : base(message, cause) { }

        public ORMException(Exception cause) : base(cause.Message, cause) { }

        protected ORMException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}