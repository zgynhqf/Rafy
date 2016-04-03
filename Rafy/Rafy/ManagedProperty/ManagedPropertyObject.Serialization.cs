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
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy.Reflection;
using Rafy.Serialization;
using Rafy.Serialization.Mobile;

namespace Rafy.ManagedProperty
{
    /// <summary>
    /// 此文件中代码主要处理 ManagedPropertyObject 对象的序列化相关代码。
    /// 其中包含系统序列化 和 Mobile 序列化
    /// 
    /// ManagedPropertyObject 继承自 MobileObject，使得其支持自定义序列化（JSON等各种格式）
    /// </summary>
    [MobileNonSerialized]
    public abstract partial class ManagedPropertyObject : ISerializationNotification
    {
        #region Mobile Serialization

        protected override void OnMobileSerializeRef(ISerializationContext context)
        {
            base.OnMobileSerializeRef(context);

            this.SerialzeCompiledProperties(context);
        }

        protected override void OnMobileSerializeState(ISerializationContext context)
        {
            base.OnMobileSerializeState(context);

            this.SerialzeCompiledProperties(context);
        }

        protected override void OnMobileDeserializeState(ISerializationContext context)
        {
            this.InitFields();

            this.DeserialzeCompiledProperties(context);

            base.OnMobileDeserializeState(context);
        }

        protected override void OnMobileDeserializeRef(ISerializationContext context)
        {
            this.DeserialzeCompiledProperties(context);

            base.OnMobileDeserializeRef(context);
        }

        void ISerializationNotification.Deserialized(ISerializationContext context)
        {
            this.OnDeserialized(null);
        }

        private void SerialzeCompiledProperties(ISerializationContext context)
        {
            var formatter = context.RefFormatter;
            bool isState = context.IsProcessingState;

            //只序列化 compiled property, 不序列化 runtime property
            foreach (var field in this._compiledFields)
            {
                if (field.HasValue)
                {
                    var p = field.Property;

                    //如果是需要的类型
                    if (context.IsState(p.PropertyType) == isState)
                    {
                        var v = field.Value;

                        var defaultValue = p.GetMeta(this).DefaultValue;

                        //如果不是默认值
                        if (!object.Equals(v, defaultValue))
                        {
                            if (isState)
                            {
                                context.AddState(p.Name, v);
                            }
                            else
                            {
                                context.AddRef(p.Name, v);
                            }
                        }
                    }
                }
            }
        }

        private void DeserialzeCompiledProperties(ISerializationContext context)
        {
            var formatter = context.RefFormatter;
            bool isState = context.IsProcessingState;

            var compiledProperties = this._container.GetNonReadOnlyCompiledProperties();

            if (isState)
            {
                var allStates = context.States;
                foreach (var kv in allStates)
                {
                    var name = kv.Key;

                    var property = compiledProperties.Find(name);
                    if (property != null)
                    {
                        var state = kv.Value;
                        this.LoadProperty(property, state);
                    }
                }
            }
            else
            {
                var allReferences = context.References;
                foreach (var kv in allReferences)
                {
                    var name = kv.Key;

                    var property = compiledProperties.Find(name);
                    if (property != null)
                    {
                        var refId = kv.Value;
                        var v = formatter.GetObject(refId);
                        this.LoadProperty(property, v);
                    }
                }
            }
        }

        #endregion

        #region 自定义 System Serialization / Deserialization

        /// <summary>
        /// 序列化数据到 info 中。
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected override void Serialize(SerializationInfo info, StreamingContext context)
        {
            base.Serialize(info, context);

            //只序列化非默认值的编译期属性, 不序列化运行时属性
            foreach (var field in this._compiledFields)
            {
                if (field.HasValue)
                {
                    var property = field.Property;

                    var meta = property.GetMeta(this);
                    if (meta.Serializable)
                    {
                        var value = field.Value;
                        if (!object.Equals(value, meta.DefaultValue))
                        {
                            info.AddValue(property.Name, value, property.PropertyType);
                        }
                    }
                }
            }

            //同时，还需要序列化未标记 NonSerialized 的字段。
            var clrFields = FieldsSerializationHelper.EnumerateSerializableFields(info.ObjectType);
            foreach (var f in clrFields)
            {
                var v = f.GetValue(this);
                var vType = v != null ? v.GetType() : f.FieldType;

                info.AddValue(f.Name, v, vType);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ManagedPropertyObject"/> class.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected ManagedPropertyObject(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            var compiledProperties = this.PropertiesContainer.GetNonReadOnlyCompiledProperties();

            this.InitFields(compiledProperties);

            var clrFields = FieldsSerializationHelper.EnumerateSerializableFields(info.ObjectType);

            //遍历所有已经序列化的属性值序列
            var compiledPropertiesCount = compiledProperties.Count;
            var allValues = info.GetEnumerator();
            while (allValues.MoveNext())
            {
                var serializationEntry = allValues.Current;
                var name = serializationEntry.Name;

                bool isManagedProperty = false;

                //找到对应的属性赋值。
                for (int i = 0; i < compiledPropertiesCount; i++)
                {
                    var property = compiledProperties[i];
                    if (property.Name == name)
                    {
                        this._LoadProperty(property, serializationEntry.Value);
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
        /// 反序列化完成时，调用此函数。
        /// </summary>
        /// <param name="context"></param>
        protected override sealed void OnDeserialized(StreamingContext context)
        {
            if (this._compiledFields == null)
            {
                this.InitFields();
            }

            this.OnDeserialized(null as DesirializedArgs);
        }

        #endregion

        /// <summary>
        /// 反序列化完成后的回调函数。
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnDeserialized(DesirializedArgs e) { }
    }
}