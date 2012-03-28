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
using OEA.Serialization.Mobile;
using System.Reflection;
using System.Runtime.Serialization;
using OEA.Serialization;

namespace OEA.ManagedProperty
{
    /// <summary>
    /// 此文件中代码主要处理 ManagedPropertyObject 对象的序列化相关代码。
    /// 其中包含系统序列化 和 Mobile 序列化
    /// 
    /// ManagedPropertyObject 继承自 MobileObject，使得其支持自定义序列化（JSON等各种格式）
    /// </summary>
    [MobileNonSerialized]
    public abstract partial class ManagedPropertyObject : MobileObject, ISerializationNotification
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
            this._fields = new ManagedPropertyObjectFieldsManager(this);

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
            foreach (var field in this.CompiledFields)
            {
                if (field != null)
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

                    var property = compiledProperties.FirstOrDefault(p => p.Name == name);
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

                    var property = compiledProperties.FirstOrDefault(p => p.Name == name);
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

        #region System Serialization

        [OnDeserialized]
        private void OnDeserializedHandler(StreamingContext context)
        {
            this.OnDeserialized(null);
        }

        #endregion

        protected virtual void OnDeserialized(DesirializedArgs e) { }
    }
}