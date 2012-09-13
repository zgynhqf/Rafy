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
using System.ComponentModel;
using System.Runtime;

namespace OEA.Reflection
{
    public static class TypeHelper
    {
        /// <summary>
        /// 获取继承层次列表，从子类到基类
        /// </summary>
        /// <param name="from"></param>
        /// <param name="exceptFrom"></param>
        /// <returns></returns>
        public static IEnumerable<Type> GetHierarchy(Type from, params Type[] exceptTypes)
        {
            var needExcept = exceptTypes.Length > 0;

            Type current = from;
            while (current != null && (!needExcept || !InExcept(current, exceptTypes)))
            {
                yield return current;
                current = current.BaseType;
            }
        }

        private static bool InExcept(Type current, Type[] exceptTypes)
        {
            for (int i = 0, c = exceptTypes.Length; i < c; i++)
            {
                var exceptType = exceptTypes[i];

                //如果是泛型定义，则需要 current 类型是这个泛型的实例也可以。
                if (exceptType.IsGenericTypeDefinition)
                {
                    if (current.IsGenericType && current.GetGenericTypeDefinition() == exceptType) return true;
                }
                else
                {
                    if (exceptType == current) return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 判断指定的类型是不是数字类型。
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsNumber(Type type)
        {
            return type == typeof(int) ||
                type == typeof(long) ||
                type == typeof(float) ||
                type == typeof(double) ||
                type == typeof(decimal) ||
                type == typeof(uint) ||
                type == typeof(ulong) ||
                type == typeof(byte) ||
                type == typeof(sbyte);
        }

        public static object GetDefaultValue(Type targetType)
        {
            if (targetType.IsValueType) return Activator.CreateInstance(targetType);
            return null;
        }

        /// <summary>
        /// 如果是 Nullable 泛型类型，则返回内部的真实类型。
        /// </summary>
        /// <param name="targetType"></param>
        /// <returns></returns>
        public static Type IgnoreNullable(Type targetType)
        {
            if (IsNullable(targetType)) { return targetType.GetGenericArguments()[0]; }

            return targetType;
        }

        /// <summary>
        /// 判断某个类型是否为 Nullable 泛型类型。
        /// </summary>
        /// <param name="targetType"></param>
        /// <returns></returns>
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static bool IsNullable(Type targetType)
        {
            return targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        /// <summary>
        /// 判断指定的类型是否是一个枚举类型，或者是一个可空的枚举类型。
        /// </summary>
        /// <param name="targetType"></param>
        /// <returns></returns>
        public static bool IsEnumNullable(Type targetType)
        {
            var enumType = TypeHelper.IgnoreNullable(targetType);
            return enumType.IsEnum;
        }

        #region  CoerceValue

        /// <summary>
        /// 强制把 value 的值变换为 desiredType
        /// </summary>
        /// <param name="desiredType"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static object CoerceValue(Type desiredType, object value)
        {
            if (value != null)
            {
                value = CoerceValue(desiredType, value.GetType(), value);
            }
            else
            {
                value = TypeHelper.GetDefaultValue(desiredType);
            }

            return value;
        }

        /// <summary>
        /// Attempts to coerce a value of one type into
        /// a value of a different type.
        /// </summary>
        /// <param name="desiredType">
        /// Type to which the value should be coerced.
        /// </param>
        /// <param name="valueType">
        /// Original type of the value.
        /// </param>
        /// 
        /// <param name="value">
        /// The value to coerce.
        /// </param>
        /// <remarks>
        /// <para>
        /// If the desired type is a primitive type or Decimal, 
        /// empty string and null values will result in a 0 
        /// or equivalent.
        /// </para>
        /// <para>
        /// If the desired type is a Nullable type, empty string
        /// and null values will result in a null result.
        /// </para>
        /// <para>
        /// If the desired type is an enum the value's ToString()
        /// result is parsed to convert into the enum value.
        /// </para>
        /// </remarks>
        public static object CoerceValue(Type desiredType, Type valueType, object value)
        {
            // types match, just return value
            if (desiredType.IsAssignableFrom(valueType)) { return value; }

            if (desiredType == typeof(string) && value != null) return value.ToString();

            if (desiredType.IsGenericType && desiredType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                desiredType = Nullable.GetUnderlyingType(desiredType);

                if (value == null ||
                    valueType.Equals(typeof(string)) && Convert.ToString(value) == string.Empty) return null;
            }

            if (desiredType.IsEnum) { return Enum.Parse(desiredType, value.ToString()); }

            //空字符串转换为数字 0
            if ((desiredType.IsPrimitive || desiredType.Equals(typeof(decimal))) &&
                valueType.Equals(typeof(string)) && string.IsNullOrEmpty((string)value))
                value = 0;

            if (desiredType == typeof(Guid) && value is string) { return Guid.Parse(value as string); }

            try
            {
                return Convert.ChangeType(value, desiredType);
            }
            catch
            {
                var cnv = TypeDescriptor.GetConverter(desiredType);
                if (cnv != null && cnv.CanConvertFrom(valueType)) return cnv.ConvertFrom(value);

                throw;
            }
        }

        /// <summary>
        /// Attempts to coerce a value of one type into
        /// a value of a different type.
        /// </summary>
        /// <typeparam name="T">
        /// Type to which the value should be coerced.
        /// </typeparam>
        /// <param name="valueType">
        /// Original type of the value.
        /// </param>
        /// 
        /// <param name="value">
        /// The value to coerce.
        /// </param>
        /// <remarks>
        /// <para>
        /// If the desired type is a primitive type or Decimal, 
        /// empty string and null values will result in a 0 
        /// or equivalent.
        /// </para>
        /// <para>
        /// If the desired type is a Nullable type, empty string
        /// and null values will result in a null result.
        /// </para>
        /// <para>
        /// If the desired type is an enum the value's ToString()
        /// result is parsed to convert into the enum value.
        /// </para>
        /// </remarks>
        public static T CoerceValue<T>(Type valueType, object value)
        {
            return (T)(CoerceValue(typeof(T), valueType, value));
        }

        public static T CoerceValue<T>(object value)
        {
            if (value == null) throw new ArgumentNullException("value");

            return (T)(CoerceValue(typeof(T), value.GetType(), value));
        }

        #endregion
    }
}
