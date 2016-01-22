/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20151209
 * 运行环境：.NET 4.5
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20151209 13:43
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Rafy.Domain
{
    /// <summary>
    /// 领域逻辑异常
    /// </summary>
    [Serializable]
    public class DomainException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DomainException"/> class.
        /// </summary>
        public DomainException() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DomainException"/> class.
        /// </summary>
        /// <param name="res">The resource.</param>
        /// <param name="parameters">The parameters.</param>
        public DomainException(Result res, params object[] parameters)
        {
            this.Result = res;
            this.Parameters = parameters;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DomainException"/> class.
        /// </summary>
        /// <param name="res">The resource.</param>
        /// <param name="inner">The inner.</param>
        /// <param name="parameters">The parameters.</param>
        public DomainException(Result res, Exception inner, params object[] parameters) : base(res.Message, inner)
        {
            this.Result = res;
            this.Parameters = parameters;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DomainException"/> class.
        /// </summary>
        /// <param name="info">The information.</param>
        /// <param name="context">The context.</param>
        protected DomainException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        /// <summary>
        /// 表示领域逻辑的异常分支的结果。
        /// </summary>
        public Result Result { get; set; }

        /// <summary>
        /// 如果 <see cref="Result"/> 中的 <see cref="Result.Message"/> 是一个格式化字符串，那么这个参数表示对应需要格式化的数据。
        /// </summary>
        public object[] Parameters { get; set; }

        public static implicit operator DomainException(int statusCode)
        {
            return new DomainException
            {
                Result = statusCode
            };
        }

        public static implicit operator DomainException(Result result)
        {
            return new DomainException
            {
                Result = result
            };
        }
    }
}
