/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120330
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 适配到 Entity、托管属性上。 胡庆访 20120330
 * 
*******************************************************/

using System;

namespace Rafy.Domain.Validation
{
    /// <summary>
    /// Exception class indicating that there was a validation
    /// problem with a business object.
    /// </summary>
    [Serializable]
    public class ValidationException : Exception
    {
        /// <summary>
        /// Creates an instance of the object.
        /// </summary>
        public ValidationException() { }

        /// <summary>
        /// Creates an instance of the object.
        /// </summary>
        /// <param name="message">Message describing the exception.</param>
        public ValidationException(string message) : base(message) { }

        /// <summary>
        /// Creates an instance of the object.
        /// </summary>
        /// <param name="message">Message describing the exception.</param>
        /// <param name="innerException">Inner exception object.</param>
        public ValidationException(string message, Exception innerException)
            : base(message, innerException) { }

        /// <summary>
        /// Creates an instance of the object for serialization.
        /// </summary>
        /// <param name="context">Serialization context.</param>
        /// <param name="info">Serialization info.</param>
        protected ValidationException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}