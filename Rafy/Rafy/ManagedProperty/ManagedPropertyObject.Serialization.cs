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

using Rafy.Reflection;
using Rafy.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;

namespace Rafy.ManagedProperty
{
    /// <summary>
    /// 此文件中代码主要处理 ManagedPropertyObject 对象的序列化相关代码。
    /// 其中包含系统序列化 和 Mobile 序列化
    /// 
    /// ManagedPropertyObject 继承自 MobileObject，使得其支持自定义序列化（JSON等各种格式）
    /// </summary>
    public abstract partial class ManagedPropertyObject : ICustomSerializationObject, IDeserializationCallback
    {
        #region 自定义 System Serialization / Deserialization

        /// <summary>
        /// 序列化数据到 info 中。
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected virtual void Serialize(SerializationInfo info, StreamingContext context)
        {
            //只序列化非默认状态、非默认值的编译期属性（不序列化运行时属性）
            foreach (var field in _compiledFields)
            {
                if (field.IsDefault()) continue;//默认状态

                var property = field.Property;

                var meta = property.GetMeta(this);
                if (!meta.Serializable) continue;//不可序列化。

                var value = field.Serialize();
                if (object.Equals(value, meta.DefaultValue)) continue;//默认值

                var fieldType = value?.GetType() ?? property.PropertyType;
                info.AddValue(property.Name, value, fieldType);
            }

            FieldsSerializationHelper.SerializeFields(this, info);
        }

        /// <summary>
        /// 从 info 中反序列化数据。
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected virtual void Deserialize(SerializationInfo info, StreamingContext context)
        {
            var compiledProperties = this.PropertiesContainer.GetNonReadOnlyCompiledProperties();

            this.InitFields(compiledProperties);

            var clrFields = FieldsSerializationHelper.EnumerateSerializableFields(info.ObjectType);

            //遍历所有已经序列化的属性值序列
            var compiledPropertiesCount = compiledProperties.Count;
            foreach (var serializationEntry in info)
            {
                var name = serializationEntry.Name;

                bool isManagedProperty = false;

                //找到对应的属性赋值。
                for (int i = 0; i < compiledPropertiesCount; i++)
                {
                    var property = compiledProperties[i];
                    if (property.Name == name)
                    {
                        _compiledFields[property.TypeCompiledIndex].Deserialize(serializationEntry.Value);
                        isManagedProperty = true;
                        break;
                    }
                }

                //如果没有打到对应的托管属性设置，则尝试找到私有字段进行设置。
                if (!isManagedProperty)
                {
                    var f = FieldsSerializationHelper.FindSingleField(clrFields, name);
                    if (f != null)
                    {
                        var value = TypeHelper.CoerceValue(f.FieldType, serializationEntry.Value);
                        f.SetValue(this, value);
                    }
                }
            }
        }

        /// <summary>
        /// 所有反序列化完成时（包括对象间的引用关系），会调用此回调函数。 
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnDeserialized(DesirializedArgs e) { }

        ///// <summary>
        ///// 反序列化完成时，调用此函数。
        ///// </summary>
        ///// <param name="context"></param>
        //protected override sealed void OnDeserialized(StreamingContext context)
        //{
        //    if (_compiledFields == null)
        //    {
        //        this.InitFields();
        //    }

        //    this.OnDeserialized(null as DesirializedArgs);
        //}

        #endregion

        #region 系统接口

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            this.Serialize(info, context);
        }

        void ICustomSerializationObject.SetObjectData(SerializationInfo info, StreamingContext context)
        {
            this.Deserialize(info, context);

            //this.OnDeserialized(null as DesirializedArgs);
        }

        void IDeserializationCallback.OnDeserialization(object sender)
        {
            this.OnDeserialized(null as DesirializedArgs);
        }

        //不能使用这个回调。
        //在 BinaryFormatter 中，它在 ICustomSerializationObject.SetObjectData 之前被回调。
        //但是在 NetDataContractSerializer 中，它却在 ICustomSerializationObject.SetObjectData 之后被回调。
        //[OnDeserialized]
        //private void OnDeserialization(StreamingContext context)
        //{
        //    //this.OnDeserialized(null as DesirializedArgs);
        //}

        //NetDataContractSerializer:
        //System.Runtime.Serialization.SerializationException: An object of type 'Rafy.Serialization.DeserializationFactory' which implements IObjectReference returned null from its GetRealObject method. Change the GetRealObject implementation to return a non-null value. 
        //[Serializable]
        //internal class DeserializationFactory : IObjectReference
        //{
        //    private static readonly string RealTypePropertyName = "DeserializationFactory_RealType";

        //    [NonSerialized]
        //    private object _realObject;

        //    public DeserializationFactory(SerializationInfo info, StreamingContext context)
        //    {
        //        var type = info.GetString(RealTypePropertyName);
        //        var realType = Type.GetType(type);
        //        var cso = Domain.Entity.New(realType) as ICustomSerializationObject;
        //        cso.SetObjectData(info, context);
        //        _realObject = cso;
        //    }

        //    internal static void SetFactoryType(SerializationInfo info, object realType)
        //    {
        //        info.SetType(typeof(DeserializationFactory));
        //        info.AddValue(DeserializationFactory.RealTypePropertyName, realType);
        //    }

        //    public object GetRealObject(StreamingContext context)
        //    {
        //        return _realObject;
        //    }
        //}

        #endregion
    }
}