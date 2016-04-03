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
using System.Reflection;

namespace Rafy.Reflection
{
    /// <summary>
    /// 类型的一些帮助方法。
    /// </summary>
    public static class TypeHelper
    {
        /// <summary>
        /// 获取继承层次列表，从子类到基类
        /// </summary>
        /// <param name="from">From.</param>
        /// <param name="exceptTypes">The except types.</param>
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
                type == typeof(double) ||
                type == typeof(long) ||
                type == typeof(float) ||
                type == typeof(decimal) ||
                type == typeof(uint) ||
                type == typeof(ulong) ||
                type == typeof(byte) ||
                type == typeof(sbyte);
        }

        /// <summary>
        /// 获取指定类型的默认值。
        /// </summary>
        /// <param name="targetType">Type of the target.</param>
        /// <returns></returns>
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
        /// <param name="targetType">需要判断的目标类型。</param>
        /// <returns></returns>
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static bool IsNullable(Type targetType)
        {
            //本代码拷贝自：Nullable.GetUnderlyingType.
            return targetType.IsGenericType && !targetType.IsGenericTypeDefinition && targetType.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        /// <summary>
        /// 判断指定的类型是否是一个指定的泛型类型。
        /// </summary>
        /// <param name="targetType">需要判断的目标类型。</param>
        /// <param name="genericType">泛型类型。</param>
        /// <returns></returns>
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static bool IsGenericType(Type targetType, Type genericType)
        {
            return targetType.IsGenericType && targetType.GetGenericTypeDefinition() == genericType;
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

        /// <summary>
        /// 根据引用关系来排列程序集。
        /// </summary>
        /// <param name="assemblies"></param>
        /// <returns></returns>
        public static List<Assembly> SortByReference(IEnumerable<Assembly> assemblies)
        {
            //items 表示待处理列表。
            var items = assemblies.ToList();
            var sorted = new List<Assembly>(items.Count);

            while (items.Count > 0)
            {
                for (int i = 0, c = items.Count; i < c; i++)
                {
                    var item = items[i];
                    bool referencesOther = false;
                    var refItems = item.GetReferencedAssemblies();
                    for (int j = 0, c2 = items.Count; j < c2; j++)
                    {
                        if (i != j)
                        {
                            if (refItems.Any(ri => ri.FullName == items[j].FullName))
                            {
                                referencesOther = true;
                                break;
                            }
                        }
                    }
                    //没有被任何一个程序集引用，则把这个加入到结果列表中，并从待处理列表中删除。
                    if (!referencesOther)
                    {
                        sorted.Add(item);
                        items.RemoveAt(i);

                        //跳出循环，从新开始。
                        break;
                    }
                }
            }

            return sorted;
        }

        #region CoerceValue

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
            //类型匹配时，直接返回值。
            if (desiredType.IsAssignableFrom(valueType)) { return value; }

            //字符串类型，直接使用 ToString 进行转换。
            if (desiredType == typeof(string))
            {
                return value != null ? value.ToString() : null;
            }

            //处理 Nullable 类型。
            if (desiredType.IsGenericType && desiredType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                desiredType = Nullable.GetUnderlyingType(desiredType);

                //空字符串转换为 null。
                if (value == null ||
                    value is string && value as string == string.Empty)
                    return null;
            }

            //处理枚举类型
            if (desiredType.IsEnum) { return Enum.Parse(desiredType, value.ToString()); }

            //处理数字类型。（空字符串转换为数字 0）
            if ((desiredType.IsPrimitive || desiredType == typeof(decimal)) &&
                value is string && string.IsNullOrEmpty(value as string))
            {
                value = 0;
            }

            //处理 Guid
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

        /// <summary>
        /// Attempts to coerce a value of one type into
        /// a value of a different type.
        /// </summary>
        /// <typeparam name="T">Type to which the value should be coerced.</typeparam>
        /// <param name="value">The value to coerce.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">value</exception>
        /// <remarks>
        ///   <para>
        /// If the desired type is a primitive type or Decimal,
        /// empty string and null values will result in a 0
        /// or equivalent.
        ///   </para>
        ///   <para>
        /// If the desired type is a Nullable type, empty string
        /// and null values will result in a null result.
        ///   </para>
        ///   <para>
        /// If the desired type is an enum the value's ToString()
        /// result is parsed to convert into the enum value.
        ///   </para>
        /// </remarks>
        public static T CoerceValue<T>(object value)
        {
            if (value == null) throw new ArgumentNullException("value");

            return (T)CoerceValue(typeof(T), value.GetType(), value);
        }

        #endregion
    }
}
