/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20211125
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20211125 12:35
 * 
*******************************************************/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rafy.Reflection;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RafyUnitTest
{
    internal class AssertAdapter : DynamicObject
    {
        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            var methodName = binder.Name;
            if (methodName != "IsNull")
            {
                var argTypes = args.Select(a => a?.GetType()).ToArray();
                var method = typeof(Assert).GetMethod(methodName, argTypes);

                if (method != null)
                {
                    result = method.Invoke(null, args);
                    return true;
                }
            }

            return base.TryInvokeMember(binder, args, out result);
        }

        public void IsNull(object value)
        {
            Assert.IsNull(value);
        }

        public void IsNull(object value, string message)
        {
            Assert.IsNull(value, message);
        }
    }
}