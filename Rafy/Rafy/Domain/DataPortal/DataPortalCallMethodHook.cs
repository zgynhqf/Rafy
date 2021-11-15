/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20211115
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20211115 10:06
 * 
*******************************************************/

using Castle.DynamicProxy;
using Rafy.Reflection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Rafy.Domain.DataPortal
{
    /// <summary>
    /// 只有标记了 <see cref="DataPortalCallAttribute"/> 标记的方法，才需要进行拦截。
    /// </summary>
    internal class DataPortalCallMethodHook : AllMethodsHook
    {
        public static readonly DataPortalCallMethodHook Instance = new DataPortalCallMethodHook();

        private DataPortalCallMethodHook() { }

        public override bool ShouldInterceptMethod(Type type, MethodInfo methodInfo)
        {
            return methodInfo.HasMarked<DataPortalCallAttribute>(); 

            //var res = base.ShouldInterceptMethod(type, methodInfo);
            //if (res)
            //{
            //    res = methodInfo.HasMarked<DataPortalCallAttribute>();
            //}
            //return res;
        }
    }
}
