/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110314
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100314
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Linq.Expressions;

namespace OEA.Utils
{
    public static class ExpressionHelper
    {
        public static PropertyInfo GetRuntimeProperty<T>(Expression<Func<T, object>> expProperty)
        {
            var member = expProperty.Body as MemberExpression;
            if (member == null)
            {
                member = (expProperty.Body as UnaryExpression).Operand as MemberExpression;
                if (member == null) throw new ArgumentNullException("property");
            }

            var property = member.Member as PropertyInfo;
            if (property == null) throw new ArgumentNullException("property");
            return property;
        }

        public static string GetProperty<T>(Expression<Func<T, object>> expProperty)
        {
            return GetRuntimeProperty(expProperty).Name;
        }
    }
}
