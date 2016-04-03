/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20111110
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20111110
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Rafy.Reflection
{
    public static class MemberInfoExtension
    {
        /// <summary>
        /// 获取程序集中标记了某标记的所有类型。
        /// </summary>
        /// <typeparam name="TAttr"></typeparam>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static IEnumerable<Type> GetTypeMarked<TAttr>(this Assembly assembly) where TAttr : Attribute
        {
            return assembly.GetTypes().Where(t => t.HasMarked<TAttr>());
        }

        /// <summary>
        /// 找到member的指定Attribute的唯一标记实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="member"></param>
        /// <returns></returns>
        public static T GetSingleAttribute<T>(this MemberInfo member) where T : Attribute
        {
            if (member == null) throw new ArgumentNullException("member");

            var attributes = member.GetCustomAttributes(typeof(T), false);
            if (attributes.Length == 1)
            {
                return attributes[0] as T;
            }
            return null;
        }

        /// <summary>
        /// 是否标记了指定的Attribute
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="member"></param>
        /// <returns></returns>
        public static bool HasMarked<T>(this MemberInfo member) where T : Attribute
        {
            return Attribute.IsDefined(member, typeof(T), false);
            // return member.GetSingleAttribute<T>() != null;
        }
    }
}
