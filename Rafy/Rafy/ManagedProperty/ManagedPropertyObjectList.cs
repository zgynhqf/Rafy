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
using System.Collections.ObjectModel;
using System.ComponentModel;
using Rafy.Serialization.Mobile;
using System.Reflection;
using Rafy.Serialization;
using System.Collections;
using System.Runtime.Serialization;
using Rafy.Reflection;

namespace Rafy.ManagedProperty
{
    /// <summary>
    /// 托管属性对象的集合基类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class ManagedPropertyObjectList<T> : ObservableCollection<T>, ICustomSerializationObject
    {
        /// <summary>
        /// 序列化数据到 info 中。
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected virtual void Serialize(SerializationInfo info, StreamingContext context)
        {
            FieldsSerializationHelper.SerializeFields(this, info);
        }

        /// <summary>
        /// 从 info 中反序列化数据。
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected virtual void Deserialize(SerializationInfo info, StreamingContext context)
        {
            var clrFields = FieldsSerializationHelper.EnumerateSerializableFields(info.ObjectType);

            //遍历所有已经序列化的属性值序列
            var allValues = info.GetEnumerator();
            while (allValues.MoveNext())
            {
                var serializationEntry = allValues.Current;

                var f = FieldsSerializationHelper.FindSingleField(clrFields, serializationEntry.Name);
                if (f != null)
                {
                    var value = TypeHelper.CoerceValue(f.FieldType, serializationEntry.Value);
                    f.SetValue(this, value);
                }
            }
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            this.Serialize(info, context);
        }

        void ICustomSerializationObject.SetObjectData(SerializationInfo info, StreamingContext context)
        {
            this.Deserialize(info, context);
        }
    }
}