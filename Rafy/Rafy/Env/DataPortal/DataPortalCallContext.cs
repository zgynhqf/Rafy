/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20211114
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20211114 23:12
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Rafy.DataPortal
{
    /// <summary>
    /// 数据门户调用参数
    /// </summary>
    public class DataPortalCallContext
    {
        /// <summary>
        /// 用户上下文对象，开发者可使用此对象进行数据扩展。
        /// </summary>
        public object UserContext { get; set; }

        /// <summary>
        /// 当前最终确定的调用类型（远程、本地）
        /// </summary>
        public PortalCallType CallType { get; internal set; }

        /// <summary>
        /// 正在调用的方法。
        /// </summary>
        public MethodInfo Method { get; internal set; }

        /// <summary>
        /// 正在调用的参数列表。
        /// </summary>
        public object[] Arguments { get; internal set; }

        /// <summary>
        /// 本次调用的结果对象。
        /// 如果在 <see cref="IDataPortalTarget.OnPortalCalling(DataPortalCallContext)"/> 中设置了这个值，将不会再调用目标方法。
        /// </summary>
        public object Result { get; set; }
    }

    /// <summary>
    /// 当前最终确定的调用类型（远程、本地）
    /// </summary>
    public enum PortalCallType
    {
        Local,
        Remote
    }
}