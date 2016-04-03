/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120819 13:16
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120819 13:16
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Rafy.Domain.DataPortal
{
    /// <summary>
    /// 数据访问异常
    /// </summary>
    [Serializable]
    public class DataPortalException : Exception
    {
        public DataPortalException() { }
        public DataPortalException(string message) : base(message) { }
        public DataPortalException(string message, Exception inner) : base(message, inner) { }
        protected DataPortalException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
