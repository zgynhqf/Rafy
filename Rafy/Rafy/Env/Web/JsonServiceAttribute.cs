/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120220
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120220
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Rafy;

namespace Rafy.Web
{
    /// <summary>
    /// 此标记用于标记于 Service 之上。
    /// </summary>
    [Serializable]
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class JsonServiceAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JsonServiceAttribute"/> class.
        /// </summary>
        public JsonServiceAttribute() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonServiceAttribute"/> class.
        /// </summary>
        /// <param name="clientName">Name of the client.</param>
        public JsonServiceAttribute(string clientName)
        {
            this.ClientName = clientName;
        }

        /// <summary>
        /// 获取或设置在客户端对应调用的服务的名称标记。
        /// 如果没有不标记，则默认以类的全名称为服务名。
        /// </summary>
        public string ClientName { get; set; }
    }
}