/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20211125
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20211125 10:08
 * 
*******************************************************/

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rafy.Reflection;
using Rafy.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Rafy.Domain
{
    internal class ControllerSerializer
    {
        public static readonly ControllerSerializer Instance = new ControllerSerializer();

        private ControllerSerializer() { }

        public object Serialize(DomainController controller)
        {
            var typeName = TypeSerializer.Serialize(controller.GetType().BaseType);

            var data = new Dictionary<string, object>();

            SerializeSettings(controller, data);

            //有状态，则序列化 Dictionary<string, object> 进行传输。
            if (data.Count > 0)
            {
                data.Add("__T", typeName);

                var bytes = BinarySerializer.SerializeBytes(data);

                return bytes;
            }

            //无状态，则传输类名即可。
            return typeName;
        }

        public DomainController Deserialize(object content, Func<Type, DomainController> factory)
        {
            //仅传输类名时，无需要状态回写。
            if (content is string)
            {
                var controllerType2 = TypeSerializer.Deserialize(content as string);
                var controller2 = factory(controllerType2);
                return controller2;
            }

            //如果是 byte[]，说明有状态需要回写。
            var bytes = content as byte[];
            var data = BinarySerializer.DeserializeBytes(bytes) as IDictionary<string, object>;

            var typeName = data["__T"] as string;

            var controllerType = TypeSerializer.Deserialize(typeName);
            var controller = factory(controllerType);

            foreach (var kv in data)
            {
                var value = kv.Value;
                var field = TypeHelper.GetProperty(controllerType, kv.Key);
                if (field != null && value != null)
                {
                    field.SetValue(controller, value);
                }
            }

            return controller;
        }

        //private static Type[] _baseTypes = new Type[]
        //{
        //    typeof(DomainController),
        //};

        private static void SerializeSettings(object obj, IDictionary<string, object> data)
        {
            var properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty);
            foreach (var property in properties)
            {
                if (property.HasMarked<ControllerClientSettingsAttribute>())
                {
                    var v = property.GetValue(obj);
                    if (v == null) continue;

                    data.Add(property.Name, v);
                }
            }
            //var clrFields = FieldsSerializationHelper.EnumerateSerializableFields(obj.GetType(), ignoreDelegate: true, _baseTypes);
            //foreach (var f in clrFields)
            //{
            //    var fieldType = f.FieldType;
            //    if (TypeHelper.IsPrimitive(fieldType))//只支持原生类型的状态传输。
            //    {
            //        var v = f.GetValue(obj);
            //        if (v == null) continue;

            //        data.Add(f.Name, v);
            //    }
            //}
        }
    }
}