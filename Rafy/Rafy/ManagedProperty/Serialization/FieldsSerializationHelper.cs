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
using Rafy.Reflection;
using System.Runtime.Serialization;
using Rafy.Serialization.Mobile;

namespace Rafy.Serialization
{
    /// <summary>
    /// 为了兼容系统的序列化机制，特写此类进行字段的序列化：
    /// 只要不标记 NonSerialized 的字段都进行序列化。
    /// </summary>
    internal static class FieldsSerializationHelper
    {
        internal static FieldInfo FindSingleField(IEnumerable<FieldInfo> fields, string name)
        {
            var result = fields.Where(p => p.Name == name).ToArray();
            if (result.Length > 1)
            {
                throw new InvalidOperationException(string.Format("存在两个同名的字段：{0}.{1}, {2}.{3}，无法支持序列化。",
                    result[0].DeclaringType.Name, result[0].Name,
                    result[1].DeclaringType.Name, result[1].Name
                    ));
            }

            if (result.Length == 1) return result[0];

            return null;
        }

        internal static IEnumerable<FieldInfo> EnumerateSerializableFields(Type objType, params Type[] exceptTypes)
        {
            return EnumerateSerializableFields(objType, ignoreDelegate: false, exceptTypes);
        }

        internal static IEnumerable<FieldInfo> EnumerateSerializableFields(Type objType, bool ignoreDelegate, params Type[] exceptTypes)
        {
            //下行代码中，特殊地，把 exceptTypes 排除，而不是逐一地排除其中的字段。这样性能更好。
            var hierarchy = TypeHelper.GetHierarchy(objType, exceptTypes);

            foreach (var type in hierarchy)
            {
                var fields = type.GetFields(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Public);
                for (int i = 0, c = fields.Length; i < c; i++)
                {
                    var field = fields[i];
                    if (!field.IsDefined(typeof(NonSerializedAttribute), false))
                    {
                        if (typeof(Delegate).IsAssignableFrom(field.FieldType))
                        {
                            if (ignoreDelegate) continue;

                            throw new InvalidOperationException(string.Format(
                                "{0} 类中的字段 {1} 是代理类型，不能直接被序列化，请标记 NonSerializd 并自定义序列化。",
                                field.DeclaringType.Name, field.Name
                                ));
                        }

                        yield return field;
                    }
                }
            }
        }

        internal static void SerializeFields(object obj, SerializationInfo info, params Type[] exceptTypes)
        {
            //同时，还需要序列化未标记 NonSerialized 的字段。
            var clrFields = FieldsSerializationHelper.EnumerateSerializableFields(info.ObjectType, exceptTypes);
            foreach (var f in clrFields)
            {
                var v = f.GetValue(obj);
                if (v == null) continue;

                info.AddValue(f.Name, v, v.GetType());
            }
        }
    }
}
