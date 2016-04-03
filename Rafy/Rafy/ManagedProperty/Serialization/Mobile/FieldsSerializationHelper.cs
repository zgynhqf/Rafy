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
using Rafy.ManagedProperty;
using Rafy.Reflection;
using System.Runtime.Serialization;

namespace Rafy.Serialization.Mobile
{
    /// <summary>
    /// 为了兼容系统的序列化机制，特写此类进行字段的序列化：
    /// 只要不标记 NonSerialized 的字段都进行序列化。
    /// </summary>
    internal static class FieldsSerializationHelper
    {
        internal static void SerialzeFields(object obj, ISerializationContext info)
        {
            bool isState = info.IsProcessingState;

            var fields = EnumerateSerializableFields(obj.GetType());
            foreach (var f in fields)
            {
                var v = f.GetValue(obj);
                var vType = v != null ? v.GetType() : f.FieldType;

                if (isState)
                {
                    if (info.IsState(vType)) { info.AddState(f.Name, v); }
                }
                else
                {
                    if (!info.IsState(vType)) { info.AddRef(f.Name, v); }
                }
            }
        }

        internal static void DeserialzeFields(object obj, ISerializationContext info)
        {
            var formatter = info.RefFormatter;
            bool isState = info.IsProcessingState;

            var fields = EnumerateSerializableFields(obj.GetType());

            if (isState)
            {
                var allStates = info.States;
                foreach (var kv in allStates)
                {
                    var name = kv.Key;

                    var f = FindSingleField(fields, name);
                    if (f != null)
                    {
                        var v = TypeHelper.CoerceValue(f.FieldType, kv.Value);
                        f.SetValue(obj, v);
                    }
                }
            }
            else
            {
                var allReferences = info.References;
                foreach (var kv in allReferences)
                {
                    var name = kv.Key;

                    var f = FindSingleField(fields, name);
                    if (f != null)
                    {
                        var v = formatter.GetObject(kv.Value);
                        if (v != null) { f.SetValue(obj, v); }
                    }
                }
            }
        }

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

        internal static IEnumerable<FieldInfo> EnumerateSerializableFields(Type objType)
        {
            var hierarchy = TypeHelper.GetHierarchy(objType,
                typeof(ManagedPropertyObject),
                typeof(MobileObject), typeof(MobileCollection<>), typeof(MobileList<>), typeof(MobileDictionary<,>)
                );

            foreach (var type in hierarchy)
            {
                if (type.IsDefined(typeof(MobileNonSerializedAttribute), false)) break;

                //由于本函数只为兼容，所以没有标记 [Serializable] 的类型就直接忽略它里面的所有字段。
                if (type.IsSerializable)
                {
                    var fields = type.GetFields(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Public);
                    for (int i = 0, c = fields.Length; i < c; i++)
                    {
                        var field = fields[i];
                        if (!field.IsDefined(typeof(NonSerializedAttribute), false))
                        {
                            if (typeof(Delegate).IsAssignableFrom(field.FieldType))
                            {
                                throw new InvalidOperationException(string.Format(
                                    "{0} 类中的字段 {1} 是代理类型，不能直接被序列化，请标记 NonSerializd 并重写 OnSerializeState 以自定义序列化。",
                                    field.DeclaringType.Name, field.Name
                                    ));
                            }

                            yield return field;
                        }
                    }
                }
            }
        }
    }
}
