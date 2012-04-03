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
using System.ComponentModel;
using SimpleCsla.Server;

using SimpleCsla.Core;


using SimpleCsla.DataPortalClient;
using OEA.ManagedProperty;
using SimpleCsla;

namespace OEA
{
    /// <summary>
    /// 跨 C/S，B/S 的服务基类
    /// </summary>
    [Serializable]
    public abstract class Service
    {
        /// <summary>
        /// 子类重写此方法实现具体的业务逻辑
        /// </summary>
        internal protected abstract void Execute();

        public Service Invoke()
        {
            return DataPortal.Update(this) as Service;
        }

        public void Invoke<T>(out T svcReturn)
            where T : Service
        {
            svcReturn = this.Invoke() as T;
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class ServiceInputAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class ServiceOutputAttribute : Attribute { }
}