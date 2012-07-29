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
using System.Runtime.Serialization;
using System.ComponentModel;
using System.Security.Permissions;
using OEA.Reflection;

namespace OEA.ManagedProperty
{
    /// <summary>
    /// 托管字段容器。
    /// 
    /// 提取此类的目的：
    /// * 把字段的管理方法都提取出来，使得 ManagedPropertyObject 类更加清晰。
    /// * 为了自定义序列化的数据，同时兼容历史代码（在实体类中直接标记字段的 NonSerializedAttribute 来控制序列化）。
    /// </summary>
    [Serializable]
    internal sealed class ManagedPropertyObjectFieldsManager : CustomSerializationObject
    {
        #region Fields

        private ManagedPropertyObject _owner;

        /// <summary>
        /// 编译期属性以数组方式存储，使得检索速度是 O(1)
        /// </summary>
        internal IManagedPropertyField[] _compiledFields;

        /// <summary>
        /// <![CDATA[
        /// 由于 _runtimeFields 不会很多，所以使用 Dictionary<IManagedProperty, IManagedPropertyField> 类来进行更快速的检索，
        /// 空间不会浪费很多，并不简单地使用 List<IManagedPropertyField>。
        /// ]]>
        /// </summary>
        internal Dictionary<IManagedProperty, IManagedPropertyField> _runtimeFields;

        #endregion

        #region 构造函数

        internal ManagedPropertyObjectFieldsManager(ManagedPropertyObject owner)
        {
            this._owner = owner;

            this.InitFields();
        }

        private void InitFields()
        {
            var compiledFieldsCount = this._owner.PropertiesContainer.GetNonReadOnlyCompiledProperties().Count;

            this._compiledFields = new IManagedPropertyField[compiledFieldsCount];
        }

        #endregion

        #region GetProperty / SetProperty / LoadProperty / ResetProperty

        /// <summary>
        /// 重设为默认值
        /// </summary>
        /// <param name="property"></param>
        internal void ResetProperty(IManagedProperty property)
        {
            if (!property.IsReadOnly)
            {
                if (property.LifeCycle == ManagedPropertyLifeCycle.CompileOrSetup)
                {
                    this._compiledFields[property.TypeCompiledIndex] = null;
                }
                else
                {
                    if (this._runtimeFields != null) { this._runtimeFields.Remove(property); }
                }
            }
        }

        internal object GetProperty(IManagedProperty property)
        {
            var useDefault = true;
            object result = null;

            if (property.IsReadOnly)
            {
                result = (property as IManagedPropertyInternal).ProvideReadOnlyValue(this._owner);
                useDefault = false;
            }
            else
            {
                if (property.LifeCycle == ManagedPropertyLifeCycle.CompileOrSetup)
                {
                    var field = this._compiledFields[property.TypeCompiledIndex];
                    if (field != null)
                    {
                        result = field.Value;
                        useDefault = false;
                    }
                }
                else
                {
                    if (this._runtimeFields != null)
                    {
                        IManagedPropertyField field;
                        if (this._runtimeFields.TryGetValue(property, out field))
                        {
                            result = field.Value;
                            useDefault = false;
                        }
                    }
                }
            }

            var meta = property.GetMeta(this) as IManagedPropertyMetadataInternal;

            if (useDefault) result = meta.DefaultValue;

            result = meta.CoerceGetValue(this._owner, result);

            return result;
        }

        internal TPropertyType GetProperty<TPropertyType>(ManagedProperty<TPropertyType> property)
        {
            var useDefault = true;
            TPropertyType result = default(TPropertyType);

            if (property.IsReadOnly)
            {
                result = (property as ManagedProperty<TPropertyType>).ProvideReadOnlyValue(this._owner);
                useDefault = false;
            }
            else
            {
                if (property.LifeCycle == ManagedPropertyLifeCycle.CompileOrSetup)
                {
                    var field = this._compiledFields[property.TypeCompiledIndex] as ManagedPropertyField<TPropertyType>;
                    if (field != null)
                    {
                        result = field.Value;
                        useDefault = false;
                    }
                }
                else
                {
                    if (this._runtimeFields != null)
                    {
                        IManagedPropertyField f;
                        if (this._runtimeFields.TryGetValue(property, out f))
                        {
                            var field = f as ManagedPropertyField<TPropertyType>;
                            result = field.Value;
                            useDefault = false;
                        }
                    }
                }
            }

            var meta = property.GetMeta(this);

            if (useDefault) result = meta.DefaultValue;

            result = meta.CoerceGetValue(this._owner, result);

            return result;
        }

        internal void SetProperty(IManagedProperty property, object value, ManagedPropertyChangedSource source)
        {
            ForceNotReadOnly(property);

            var meta = property.GetMeta(this) as IManagedPropertyMetadataInternal;

            value = CoerceType(property, value);

            bool isReset = false;
            if (value == null && property.PropertyType.IsValueType)
            {
                isReset = true;
                value = meta.DefaultValue;
            }

            bool cancel = meta.RaisePropertyChangingMetaEvent(this._owner, ref value, source);
            if (cancel) return;

            //以下代码与泛型版本的 SetProperty 方法一模一样，所以没有注释。

            object oldValue = null;
            IManagedPropertyField field = null;

            if (property.LifeCycle == ManagedPropertyLifeCycle.CompileOrSetup)
            {
                field = this._compiledFields[property.TypeCompiledIndex];

                if (field == null)
                {
                    field = (property as IManagedPropertyInternal).CreateField();
                    this._compiledFields[property.TypeCompiledIndex] = field;
                }
                else
                {
                    oldValue = field.Value;
                }
            }
            else
            {
                if (this._runtimeFields == null)
                {
                    this._runtimeFields = new Dictionary<IManagedProperty, IManagedPropertyField>();
                }
                else
                {
                    if (this._runtimeFields.TryGetValue(property, out field))
                    {
                        oldValue = field.Value;
                    }
                }

                if (field == null)
                {
                    field = (property as IManagedPropertyInternal).CreateField();
                    this._runtimeFields.Add(property, field);
                }
            }

            if (isReset)
            {
                this.ResetProperty(property);
            }
            else
            {
                field.Value = value;
            }

            if (oldValue == null) { oldValue = meta.DefaultValue; }

            if (!object.Equals(oldValue, value))
            {
                var args = meta.RaisePropertyChangedMetaEvent(
                    this._owner, oldValue, value, source
                    );

                this._owner.RaisePropertyChanged(args);
            }
        }

        internal void SetProperty<TPropertyType>(ManagedProperty<TPropertyType> property, TPropertyType value, ManagedPropertyChangedSource source)
        {
            ForceNotReadOnly(property);

            var meta = property.GetMeta(this);

            bool cancel = meta.RaisePropertyChangingMetaEvent(this._owner, ref value, source);
            if (cancel) return;

            var hasOldValue = false;
            TPropertyType oldValue = default(TPropertyType);

            ManagedPropertyField<TPropertyType> field = null;

            //这个 if 块中的代码：查找或创建对应 property 的 field，同时记录可能存在的历史值。
            if (property.LifeCycle == ManagedPropertyLifeCycle.CompileOrSetup)
            {
                field = this._compiledFields[property.TypeCompiledIndex] as ManagedPropertyField<TPropertyType>;

                if (field == null)
                {
                    //不管是不是默认值，都进行存储。
                    //不需要检测默认值更加快速，但是浪费了一些小的空间。
                    //默认值的检测，在 GetNonDefaultPropertyValues 方法中进行实现。
                    field = property.CreateField();
                    this._compiledFields[property.TypeCompiledIndex] = field;
                }
                else
                {
                    oldValue = field.Value;
                    hasOldValue = true;
                }
            }
            else
            {
                if (this._runtimeFields == null)
                {
                    this._runtimeFields = new Dictionary<IManagedProperty, IManagedPropertyField>();
                }
                else
                {
                    IManagedPropertyField f;
                    if (this._runtimeFields.TryGetValue(property, out f))
                    {
                        field = f as ManagedPropertyField<TPropertyType>;
                        oldValue = field.Value;
                        hasOldValue = true;
                    }
                }

                if (field == null)
                {
                    field = property.CreateField();
                    this._runtimeFields.Add(property, field);
                }
            }

            field.Value = value;

            if (!hasOldValue) { oldValue = meta.DefaultValue; }

            if (!object.Equals(oldValue, value))
            {
                //发生 Meta 中的事件
                var args = meta.RaisePropertyChangedMetaEvent(
                    this._owner, oldValue, value, source
                    );

                //发生事件
                this._owner.RaisePropertyChanged(args);
            }
        }

        /// <summary>
        /// LoadProperty 直接设置值，不发生 PropertyChanged 事件。
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        internal void LoadProperty(IManagedProperty property, object value)
        {
            ForceNotReadOnly(property);

            value = CoerceType(property, value);
            //如果把 null 赋值给一个值类型，则直接还原此属性为默认值。
            if (value == null && property.PropertyType.IsValueType)
            {
                this.ResetProperty(property);
                return;
            }

            var field = this.FindOrCreateField(property);
            field.Value = value;
        }

        /// <summary>
        /// LoadProperty 直接设置值，不发生 PropertyChanged 事件。
        /// </summary>
        /// <typeparam name="TPropertyType"></typeparam>
        /// <param name="property"></param>
        /// <param name="value"></param>
        internal void LoadProperty<TPropertyType>(ManagedProperty<TPropertyType> property, TPropertyType value)
        {
            ForceNotReadOnly(property);

            var field = this.FindOrCreateField(property) as ManagedPropertyField<TPropertyType>;
            field.Value = value;
        }

        private static void ForceNotReadOnly(IManagedProperty property)
        {
            if (property.IsReadOnly) throw new InvalidOperationException("属性是只读的！");
        }

        private static object CoerceType(IManagedProperty property, object value)
        {
            var targetType = property.PropertyType;
            Type valueType = value != null ? value.GetType() : targetType;
            value = TypeHelper.CoerceValue(targetType, valueType, value);
            return value;
        }

        private IManagedPropertyField FindOrCreateField(IManagedProperty property)
        {
            if (property.LifeCycle == ManagedPropertyLifeCycle.CompileOrSetup)
            {
                var field = this._compiledFields[property.TypeCompiledIndex];

                if (field == null)
                {
                    field = (property as IManagedPropertyInternal).CreateField();
                    this._compiledFields[property.TypeCompiledIndex] = field;
                }

                return field;
            }
            else
            {
                IManagedPropertyField field = null;

                if (this._runtimeFields == null)
                {
                    this._runtimeFields = new Dictionary<IManagedProperty, IManagedPropertyField>();
                }
                else
                {
                    this._runtimeFields.TryGetValue(property, out field);
                }

                if (field == null)
                {
                    field = (property as IManagedPropertyInternal).CreateField();
                    this._runtimeFields[property] = field;
                }

                return field;
            }
        }

        #endregion

        #region 其它接口

        internal bool FieldExists(IManagedProperty property)
        {
            if (property.IsReadOnly) return true;

            if (property.LifeCycle == ManagedPropertyLifeCycle.CompileOrSetup)
            {
                return this._compiledFields[property.TypeCompiledIndex] != null;
            }
            else
            {
                if (this._runtimeFields != null)
                {
                    return this._runtimeFields.ContainsKey(property);
                }
                return false;
            }
        }

        internal IEnumerable<IManagedPropertyField> GetCompiledPropertyValues()
        {
            foreach (var property in this._owner.PropertiesContainer.GetNonReadOnlyCompiledProperties())
            {
                var field = this._compiledFields[property.TypeCompiledIndex];

                if (field == null)
                {
                    field = (property as IManagedPropertyInternal).CreateField();
                    field.Value = property.GetMeta(this).DefaultValue;
                }

                yield return field;
            }
        }

        internal IEnumerable<IManagedPropertyField> GetNonDefaultPropertyValues()
        {
            foreach (var field in this._compiledFields)
            {
                if (field != null)
                {
                    var defaultValue = field.Property.GetMeta(this).DefaultValue;

                    if (!object.Equals(field.Value, defaultValue))
                    {
                        yield return field;
                    }
                }
            }

            if (this._runtimeFields != null)
            {
                foreach (var kv in this._runtimeFields)
                {
                    var field = kv.Value;

                    var defaultValue = field.Property.GetMeta(this).DefaultValue;

                    if (!object.Equals(field.Value, defaultValue))
                    {
                        yield return field;
                    }
                }
            }
        }

        #endregion

        #region 自定义 Serialization / Deserialization

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        private ManagedPropertyObjectFieldsManager(SerializationInfo info, StreamingContext context) : base(info, context) { }

        protected override void Serialize(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("owner", this._owner, typeof(ManagedPropertyObject));

            base.Serialize(info, context);

            //只序列化非默认值的编译期属性, 不序列化运行时属性
            foreach (var field in this._compiledFields)
            {
                if (field != null)
                {
                    var p = field.Property;
                    var v = field.Value;

                    var defaultValue = p.GetMeta(this).DefaultValue;

                    if (!object.Equals(v, defaultValue))
                    {
                        info.AddValue(p.Name, v, p.PropertyType);
                    }
                }
            }
        }

        protected override void Deserialize(SerializationInfo info, StreamingContext context)
        {
            this._owner = info.GetValue<ManagedPropertyObject>("owner");

            this.InitFields();

            base.Deserialize(info, context);

            var compiledProperties = this._owner.PropertiesContainer.GetNonReadOnlyCompiledProperties();

            var allValues = info.GetEnumerator();
            while (allValues.MoveNext())
            {
                var cur = allValues.Current;
                var name = cur.Name;
                var property = compiledProperties.FirstOrDefault(p => p.Name == name);
                if (property != null) { this.LoadProperty(property, cur.Value); }
            }
        }

        #endregion
    }
}
