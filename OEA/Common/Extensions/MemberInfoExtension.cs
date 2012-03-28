using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace OEA
{
    public static class MemberInfoExtension
    {
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
