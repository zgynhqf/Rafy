﻿/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120403
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120403
 * 
*******************************************************/

using System;
namespace OEA
{
    /// <summary>
    /// 跨 C/S，B/S 的服务
    /// </summary>
    public interface IService
    {
        /// <summary>
        /// 调用服务并把返回值转换为指定的类型。
        /// 
        /// （out 参数是为了简化接口调用，编译器直接隐式推断。）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="svcReturn"></param>
        void Invoke<T>(out T svcReturn)
            where T : OEA.IService;
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class ServiceInputAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class ServiceOutputAttribute : Attribute { }
}