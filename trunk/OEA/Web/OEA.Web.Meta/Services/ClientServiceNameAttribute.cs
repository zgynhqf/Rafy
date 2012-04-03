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
using System.Linq;
using System.Text;
using OEA;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using OEA.Web.ClientMetaModel;
using OEA.Web.Json;

namespace OEA.Web.Services
{
    /// <summary>
    /// 此标记用于标记于 JsonService 之上。
    /// JsonService 用于 Web 时，在客户端对应调用的服务的名称标记。
    /// </summary>
    [Serializable]
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class ClientServiceNameAttribute : Attribute
    {
        private readonly string _name;

        public ClientServiceNameAttribute(string name)
        {
            this._name = name;
        }

        public string Name
        {
            get { return this._name; }
        }
    }
}