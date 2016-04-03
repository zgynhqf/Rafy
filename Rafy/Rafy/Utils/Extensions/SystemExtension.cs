/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20100326
 * 说明：系统类基本扩展
 * 运行环境：.NET 3.5 SP1
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100326
 * 
*******************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;

namespace Rafy
{
    public static class SystemExtension
    {
        ///// <summary>
        ///// 获取指定属性的值
        ///// </summary>
        ///// <param name="obj"></param>
        ///// <param name="propertyName"></param>
        ///// <returns></returns>
        //public static T GetPropertyValue<T>(this BusinessBase obj, string propertyName)
        //{
        //    return (T)obj.GetPropertyValue(propertyName);
        //}
        ///// <summary>
        ///// 获取指定属性的值
        ///// 
        ///// 使用方法：
        ///// var value = obj.GetStepPropertyValue("Property1.Property2.Property3");
        ///// </summary>
        ///// <param name="obj"></param>
        ///// <param name="propertyName">
        ///// 级联属性过滤串,格式如:属性.子属性.子子属性...
        ///// </param>
        ///// <returns></returns>
        //public static object GetPropertyValue(this BusinessBase obj, string propertyName)
        //{
        //    if (string.IsNullOrEmpty(propertyName)) throw new ArgumentNullException("propertyName");

        //    //如果是层级属性
        //    if (propertyName.Contains('.'))
        //    {
        //        return GetStepPropertyValue(obj, propertyName);
        //    }

        //    object value = null;

        //    if (obj.TryGetPropertyValue(propertyName, out value))
        //    {
        //        return value;
        //    }

        //    var property = obj.GetType().GetProperty(propertyName);
        //    Debug.Assert(null != property, "类型" + obj.GetType().ToString() + "不存在属性" + propertyName);
        //    return property.GetValue(obj, null);
        //}

        //public static bool HasFlag<TEnum>(this TEnum obj, TEnum flag)
        //    where TEnum : Enum
        //{
        //    Enum e = obj;
        //    Enum e2 = obj;
        //    return (e & e2) == e2;
        //    return (obj & flag) == flag;
        //}

        /// <summary>
        /// 强制转换当前对象为指定类型。
        /// 
        /// 传入的对象为空，或者转换失败，则会抛出异常。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T CastTo<T>(this object obj)
            where T : class
        {
            var res = obj as T;

            if (res == null) throw new InvalidCastException("传入的对象为空，或者不能转换为 " + typeof(T).Name + " 类型。");

            return res;
        }
    }
}