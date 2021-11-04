/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20121119 17:34
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20121119 17:34
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Rafy.Reflection
{
    /// <summary>
    /// object 类型的帮助方法。
    /// </summary>
    public static class ObjectHelper
    {
        /// <summary>
        /// 获取指定属性的值
        /// 
        /// 使用方法：
        /// var value = obj.GetStepPropertyValue("Property1.Property2.Property3");
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="propertyName">
        /// 级联属性过滤串,格式如:属性.子属性.子子属性...
        /// </param>
        /// <returns></returns>
        public static object GetPropertyValue(object obj, string propertyName)
        {
            //如果是层级属性
            if (propertyName.Contains('.'))
            {
                return GetStepPropertyValue(obj, propertyName);
            }

            var type = obj.GetType();
            try
            {
                var property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty);
                if (property == null) throw new InvalidProgramException("类型" + obj.GetType().ToString() + "不存在属性" + propertyName);
                return property.GetValue(obj, null);
            }
            catch (AmbiguousMatchException)
            {
                var types = TypeHelper.GetHierarchy(type);
                foreach (var singleType in types)
                {
                    var property = singleType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.DeclaredOnly);
                    if (property == null) continue;
                    return property.GetValue(obj, null);
                }
                throw;
            }
        }

        /// <summary>
        /// 设置指定属性的值
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        public static void SetPropertyValue(object obj, string propertyName, object value)
        {
            var type = obj.GetType();
            try
            {
                var property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty);
                if (property == null) throw new InvalidProgramException("类型" + obj.GetType().ToString() + "不存在属性" + propertyName);
                property.SetValue(obj, value, null);
            }
            catch (AmbiguousMatchException)
            {
                var types = TypeHelper.GetHierarchy(type);
                foreach (var singleType in types)
                {
                    var property = singleType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty | BindingFlags.DeclaredOnly);
                    if (property == null) continue;
                    property.SetValue(obj, value, null);
                    return;
                }
                throw;
            }
        }

        private static object GetStepPropertyValue(object obj, string propertyName)
        {
            var propertiesArray = propertyName.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

            object value = obj;
            for (int i = 0, c = propertiesArray.Length; i < c; i++)
            {
                if (value == null) throw new ArgumentNullException("value");

                var property = propertiesArray[i];
                value = GetPropertyValue(value, property);
            }

            return value;
        }

        //public static object CallMethod(object obj, string method)
        //{
        //    //if (obj == null) throw new ArgumentNullException("obj");

        //    var delegateObj = new LateBoundObject(obj);
        //    return delegateObj.CallMethod(method);
        //}

        //public static object CallMethod(object obj, string method, params object[] parameters)
        //{
        //    //if (obj == null) throw new ArgumentNullException("obj");

        //    var delegateObj = new LateBoundObject(obj);
        //    return delegateObj.CallMethod(method, parameters);
        //}
    }
}
