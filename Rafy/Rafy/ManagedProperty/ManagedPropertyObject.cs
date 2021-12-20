/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20111110
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：2.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20111110
 * 2.0版本 胡庆访 20121118
 *      属性的存储由引用类型 IManagedPropertyField 的数组变为结构体 ManagedPropertyField 的数组。
 *      属性变更事件参数，从泛型加接口的方式改变为纯结构体。
 * 2.1版本 胡庆访 20121119
 *      为了减少数据传输量，同时为 LazyEntityRef 的重构做准备：删除 FieldManager 类型，代码直接放到 ManagedPropertyObject 中。
 *      同时，由于自定义反序列化函数也被放到本类中，所以所有子类都需要添加反序列化构造函数。
 * 2.2 胡庆访 20121120
 *      PropertyMeta 中支持以 Serializable 属性来配置是否序列化。
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Security.Permissions;
using System.Security;
using Rafy.Serialization.Mobile;
using System.Reflection;
using Rafy.Reflection;
using Rafy;
using System.Runtime.Serialization;
using System.Runtime;
using System.Diagnostics;
using Rafy.Serialization;

namespace Rafy.ManagedProperty
{
    /// <summary>
    /// 托管属性对象
    /// 
    /// 从此类继承的子类，可以以托管属性的方式声明自己的所有属性。同时，其它的类型可以为其在扩展编译期属性，也可以在运行时动态扩展属性，
    /// </summary>
    [CompiledPropertyDeclarer]
    [DebuggerDisplay("{DebuggerDisplay}")]
    [DebuggerTypeProxy(typeof(ManagedPropertyObjectTypeProxy))]
    public abstract partial class ManagedPropertyObject : IManagedPropertyObject, INotifyPropertyChanged, ICustomTypeDescriptor
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="ManagedPropertyObject"/> class.
        /// </summary>
        protected ManagedPropertyObject()
        {
            this.InitFields();
        }

        private void InitFields()
        {
            var compiledProperties = this.PropertiesContainer.GetNonReadOnlyCompiledProperties();

            this.InitFields(compiledProperties);
        }

        #endregion

        #region PropertiesContainer

        [NonSerialized]
        private ConsolidatedTypePropertiesContainer _container;

        /// <summary>
        /// 本对象所有的属性容器
        /// </summary>
        public ConsolidatedTypePropertiesContainer PropertiesContainer
        {
            get
            {
                if (_container == null) { _container = this.FindPropertiesContainer(); }

                return _container;
            }
        }

        /// <summary>
        /// 子类重写此方法以使用更高效的属性容器查找方法。
        /// </summary>
        /// <returns></returns>
        protected virtual ConsolidatedTypePropertiesContainer FindPropertiesContainer()
        {
            return ManagedPropertyRepository.Instance.GetTypePropertiesContainer(this.GetType());
        }

        #endregion

        #region 属性值访问核心逻辑

        /// <summary>
        /// 编译期属性以数组方式存储，使得检索速度是 O(1)
        /// </summary>
        private ManagedPropertyField[] _compiledFields;

        /// <summary>
        /// 由于运行时属性不会很多，所以使用 Dictionary 类来进行更快速的检索。
        /// </summary>
        private Dictionary<IManagedProperty, ManagedPropertyField> _runtimeFields;

        /// <summary>
        /// 通过编译期属性来初始化字段数组。
        /// </summary>
        /// <param name="compiledProperties"></param>
        private void InitFields(IList<IManagedProperty> compiledProperties)
        {
            var compiledFieldsCount = compiledProperties.Count;

            _compiledFields = new ManagedPropertyField[compiledFieldsCount];

            for (int i = 0; i < compiledFieldsCount; i++)
            {
                var property = compiledProperties[i];
                _compiledFields[i]._property = property;
            }
        }

        /// <summary>
        /// 获取指定托管属性的字段状态
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public ManagedPropertyField GetField(IManagedProperty property)
        {
            if (!property.IsReadOnly)
            {
                if (property.LifeCycle == ManagedPropertyLifeCycle.Compile)
                {
                    return _compiledFields[property.TypeCompiledIndex];
                }

                if (_runtimeFields != null && _runtimeFields.TryGetValue(property, out ManagedPropertyField field))
                {
                    return field;
                }
            }

            //返回一个默认状态的字段。
            return new ManagedPropertyField { _property = property };
        }

        /// <summary>
        /// 重设为默认值、未变更状态。
        /// </summary>
        /// <param name="property"></param>
        private void _ResetProperty(IManagedProperty property)
        {
            this.CheckEditable(property);

            if (property.LifeCycle == ManagedPropertyLifeCycle.Compile)
            {
                var index = property.TypeCompiledIndex;
                _compiledFields[index].ResetValue();
                _compiledFields[index].IsChanged = false;
            }
            else
            {
                if (_runtimeFields != null) { _runtimeFields.Remove(property); }
            }
        }

        private object _GetProperty(IManagedProperty property)
        {
            this.CheckHasProperty(property);

            var useDefault = true;
            object result = null;

            if (property.IsReadOnly)
            {
                result = (property as IManagedPropertyInternal).ProvideReadOnlyValue(this);
                useDefault = false;
            }
            else
            {
                if (property.LifeCycle == ManagedPropertyLifeCycle.Compile)
                {
                    var field = _compiledFields[property.TypeCompiledIndex];
                    if (field.IsDisabled) { ThrowPropertyDisabled(property); }

                    if (field.HasLocalValue)
                    {
                        result = field.Value;
                        useDefault = false;
                    }
                }
                else
                {
                    if (_runtimeFields != null && _runtimeFields.TryGetValue(property, out ManagedPropertyField field))
                    {
                        if (field.IsDisabled) { ThrowPropertyDisabled(property); }

                        if (field.HasLocalValue)
                        {
                            result = field.Value;
                            useDefault = false;
                        }
                    }
                }
            }

            var meta = property.GetMeta(this) as IManagedPropertyMetadataInternal;

            if (useDefault) result = meta.DefaultValue;

            result = meta.CoerceGetValue(this, result);

            return result;
        }

        /// <summary>
        /// 设置某个属性的值。
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        /// <param name="resetDisabledStatus"></param>
        /// <returns>返回最终使用的值。</returns>
        private object _SetProperty(IManagedProperty property, object value, bool resetDisabledStatus)
        {
            object finalValue;

            this.CheckEditable(property);

            var meta = property.GetMeta(this) as IManagedPropertyMetadataInternal;
            var defaultValue = meta.DefaultValue;
            finalValue = defaultValue;

            value = CoerceType(property, value);

            bool cancel = meta.RaisePropertyChangingMetaEvent(this, ref value);
            if (cancel) return finalValue;

            object oldValue = defaultValue;
            bool valueChanged;

            //这个 if 块中的代码：查找或创建对应 property 的 field，同时记录可能存在的历史值。
            if (property.LifeCycle == ManagedPropertyLifeCycle.Compile)
            {
                var index = property.TypeCompiledIndex;
                var field = _compiledFields[index];
                if (field.IsDisabled)
                {
                    if (resetDisabledStatus)
                    {
                        _compiledFields[index].IsDisabled = false;
                    }
                    else
                    {
                        ThrowPropertyDisabled(property);
                    }
                }

                if (field.HasLocalValue)
                {
                    oldValue = field.Value;
                }

                if (object.Equals(defaultValue, value))
                {
                    field.ResetValue();
                }
                else
                {
                    field.Value = value;
                }

                valueChanged = !object.Equals(oldValue, value);
                if (valueChanged)
                {
                    field.IsChanged = true;
                }

                _compiledFields[index] = field;
            }
            else
            {
                bool hasOldValue = false;
                ManagedPropertyField oldField = default(ManagedPropertyField);
                if (_runtimeFields == null)
                {
                    _runtimeFields = new Dictionary<IManagedProperty, ManagedPropertyField>();
                }
                else
                {
                    if (_runtimeFields.TryGetValue(property, out oldField))
                    {
                        if (oldField.IsDisabled)
                        {
                            if (resetDisabledStatus)
                            {
                                oldField.IsDisabled = false;
                                _runtimeFields[property] = oldField;
                            }
                            else
                            {
                                ThrowPropertyDisabled(property);
                            }
                        }

                        oldValue = oldField.Value;
                        hasOldValue = true;
                    }
                }
                valueChanged = !object.Equals(oldValue, value);

                //使用新的 field
                var field = new ManagedPropertyField
                {
                    _property = property,
                    Value = value,
                    IsChanged = valueChanged || hasOldValue && oldField.IsChanged//值不论是之前已经改过，还是现在才改的，都需要设置为 true
                };

                if (hasOldValue)
                {
                    _runtimeFields[property] = field;
                }
                else
                {
                    _runtimeFields.Add(property, field);
                }
            }

            if (valueChanged)
            {
                var args = new ManagedPropertyChangedEventArgs(property, oldValue, value);

                //发生 Meta 中的回调事件
                meta.RaisePropertyChangedMetaEvent(this, args);

                //发生外部事件
                this.RaisePropertyChanged(args);

                finalValue = value;
            }

            return finalValue;
        }

        /// <summary>
        /// 注释见：<see cref="LoadProperty(IManagedProperty, object)"/>
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        private void _LoadProperty(IManagedProperty property, object value)
        {
            this.CheckEditable(property);

            value = CoerceType(property, value);

            if (!object.Equals(value, property.GetMeta(this).DefaultValue))
            {
                if (property.LifeCycle == ManagedPropertyLifeCycle.Compile)
                {
                    _compiledFields[property.TypeCompiledIndex].Value = value;
                }
                else
                {
                    if (_runtimeFields == null)
                    {
                        _runtimeFields = new Dictionary<IManagedProperty, ManagedPropertyField>();
                    }

                    if (!_runtimeFields.TryGetValue(property, out ManagedPropertyField field))
                    {
                        field = new ManagedPropertyField
                        {
                            _property = property
                        };
                    }

                    field.Value = value;
                    _runtimeFields[property] = field;
                }
            }
            else
            {
                //如果传入的是默认值，那么直接 ResetValue
                //性能：这样虽然损失了一点加载速度，但是所有装箱的值类型的对象，都不会存储在内存中。
                if (property.LifeCycle == ManagedPropertyLifeCycle.Compile)
                {
                    _compiledFields[property.TypeCompiledIndex].ResetValue();
                }
                else
                {
                    if (_runtimeFields.TryGetValue(property, out ManagedPropertyField field))
                    {
                        field.ResetValue();
                        _runtimeFields[property] = field;
                    }
                }
            }
        }

        private void CheckEditable(IManagedProperty property)
        {
            this.CheckHasProperty(property);
            if (property.IsReadOnly) throw new InvalidOperationException($"属性赋值错误，{property.OwnerType.FullName}.{property.Name}属性是只读的！");
        }

        /// <summary>
        /// 确保当前对象，拥有指定的属性。
        /// </summary>
        /// <param name="property"></param>
        /// <exception cref="InvalidOperationException"></exception>
        private void CheckHasProperty(IManagedProperty property)
        {
            if (!property.OwnerType.IsInstanceOfType(this))
            {
                throw new InvalidOperationException($"无法在类型为 {this.GetType().FullName} 的对象上操作属性：{property.OwnerType.FullName}.{property.Name}，因为该对象并不拥有这个属性！");
            }
        }

        private static object CoerceType(IManagedProperty property, object value)
        {
            if (value != null)
            {
                value = TypeHelper.CoerceValue(property.PropertyType, value.GetType(), value);
            }
            else
            {
                //如果给一个非 Nullable 的值类型设置 null，则需要抛出异常。
                var propertyType = property.PropertyType;
                if (propertyType.IsValueType && !TypeHelper.IsNullable(propertyType))
                {
                    throw new InvalidProgramException($"不能给值类型属性 {property.OwnerType}.{property.Name} 设置 null。");
                }
            }
            return value;
        }

        #endregion

        #region GetProperty / SetProperty / ClearProperty

        /// <summary>
        /// 重设属性为默认值、未变更状态。
        /// </summary>
        /// <param name="property"></param>
        public void ResetProperty(IManagedProperty property)
        {
            this._ResetProperty(property);
        }

        /// <summary>
        /// 获取某个托管属性的值。
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public object GetProperty(IManagedProperty property)
        {
            return this._GetProperty(property);
        }

        /// <summary>
        /// 获取某个托管属性的值。
        /// </summary>
        /// <typeparam name="TPropertyType"></typeparam>
        /// <param name="property"></param>
        /// <returns></returns>
        public TPropertyType GetProperty<TPropertyType>(ManagedProperty<TPropertyType> property)
        {
            //属性拆箱
            return (TPropertyType)this._GetProperty(property);
        }

        /// <summary>
        /// 设置某个 bool 类型托管属性的值。
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        /// <returns>返回最终使用的值。</returns>
        public object SetProperty(ManagedProperty<bool> property, bool value)
        {
            //使用 BooleanBoxes 来防止装箱操作。
            return this.SetProperty(property, BooleanBoxes.Box(value), true);
        }

        /// <summary>
        /// 设置某个托管属性的值。
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        /// <returns>返回最终使用的值。</returns>
        public object SetProperty(IManagedProperty property, object value)
        {
            return this.SetProperty(property, value, true);
        }

        /// <summary>
        /// 设置某个托管属性的值。
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        /// <param name="resetDisabledStatus">如果本字段处于禁用状态，那么是否在设置新值时，将禁用状态解除？</param>
        /// <returns>返回最终使用的值。</returns>
        public virtual object SetProperty(IManagedProperty property, object value, bool resetDisabledStatus)
        {
            return this._SetProperty(property, value, resetDisabledStatus);
        }

        /// <summary>
        /// LoadProperty 将以最快的方式直接加载值。
        /// * 不发生 PropertyChanged 事件；
        /// * 不变更属性的“变更状态”；
        /// * 不检查属性的“禁用状态”。
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        public virtual void LoadProperty(IManagedProperty property, object value)
        {
            this._LoadProperty(property, value);
        }

        #endregion

        #region 属性禁用状态

        /// <summary>
        /// 禁用或解禁某个属性。
        /// 属性一旦进入禁用状态，则对这个属性调用 Get、Set 都将会抛出异常；除非重新解禁。
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        public void Disable(IManagedProperty property, bool value = true)
        {
            this.DisableCore(property, value);
        }
        internal virtual void DisableCore(IManagedProperty property, bool value = true)
        {
            if (property.IsReadOnly) throw new InvalidOperationException("禁用只读属性时，无需调用此方法。只需要禁用其任一依赖的属性即可。");

            if (property.LifeCycle == ManagedPropertyLifeCycle.Compile)
            {
                _compiledFields[property.TypeCompiledIndex].IsDisabled = value;
                return;
            }

            if (_runtimeFields == null)
            {
                _runtimeFields = new Dictionary<IManagedProperty, ManagedPropertyField>();
            }

            if (!_runtimeFields.TryGetValue(property, out ManagedPropertyField field))
            {
                field._property = property;
                //var meta = property.GetMeta(this) as IManagedPropertyMetadataInternal;
                //field._value = meta.DefaultValue;
            }

            field.IsDisabled = value;
            _runtimeFields[property] = field;
        }

        /// <summary>
        /// 获取指定的属性的值是否可用状态。
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public bool IsDisabled(IManagedProperty property)
        {
            if (property.IsReadOnly)
            {
                var dependencies = (property as IManagedPropertyInternal).Dependencies;
                for (int i = 0, c = dependencies.Count; i < c; i++)
                {
                    var dependency = dependencies[i];
                    if (this.IsDisabled(dependency)) return true;
                }
                return false;
            }

            if (property.LifeCycle == ManagedPropertyLifeCycle.Compile)
            {
                return _compiledFields[property.TypeCompiledIndex].IsDisabled;
            }

            if (_runtimeFields != null && _runtimeFields.TryGetValue(property, out ManagedPropertyField field))
            {
                return field.IsDisabled;
            }

            return false;
        }

        private static void ThrowPropertyDisabled(IManagedProperty property)
        {
            throw new InvalidOperationException($"{property.OwnerType.FullName}.{property.Name} 属性的值被禁用，不能使用。");
        }

        #endregion

        #region PropertyChanged

        /// <summary>
        /// 标记所有的属性为未变更状态。
        /// </summary>
        public void MarkPropertiesUnchanged()
        {
            this.MarkPropertiesChangedStatus(false);
        }

        /// <summary>
        /// 标记所有的属性为变更状态。（主要用于想要强制更新所有属性时的场景。）
        /// </summary>
        public void MarkPropertiesChanged()
        {
            this.MarkPropertiesChangedStatus(true);
        }

        private void MarkPropertiesChangedStatus(bool value)
        {
            for (int i = 0, c = _compiledFields.Length; i < c; i++)
            {
                _compiledFields[i].IsChanged = value;
            }

            if (_runtimeFields != null)
            {
                foreach (var kv in _runtimeFields)
                {
                    var field = kv.Value;
                    if (field.IsChanged)
                    {
                        field.IsChanged = value;
                        _runtimeFields[kv.Key] = field;
                    }
                }
            }
        }

        /// <summary>
        /// 标记指定的属性的变更状态。
        /// </summary>
        /// <param name="property"></param>
        /// <param name="isChanged"></param>
        public void MarkChangedStatus(IManagedProperty property, bool isChanged)
        {
            if (property.IsReadOnly) return;

            if (property.LifeCycle == ManagedPropertyLifeCycle.Compile)
            {
                _compiledFields[property.TypeCompiledIndex].IsChanged = isChanged;
            }
            else
            {
                if (_runtimeFields == null)
                {
                    _runtimeFields = new Dictionary<IManagedProperty, ManagedPropertyField>();
                }

                if (!_runtimeFields.TryGetValue(property, out ManagedPropertyField field))
                {
                    field._property = property;
                    //var meta = property.GetMeta(this) as IManagedPropertyMetadataInternal;
                    //field._value = meta.DefaultValue;
                }

                field.IsChanged = isChanged;
                _runtimeFields[property] = field;
            }
        }

        /// <summary>
        /// 获取指定的属性的变更状态。
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public bool IsChanged(IManagedProperty property)
        {
            if (property.IsReadOnly) return false;

            if (property.LifeCycle == ManagedPropertyLifeCycle.Compile)
            {
                return _compiledFields[property.TypeCompiledIndex].IsChanged;
            }

            if (_runtimeFields != null && _runtimeFields.TryGetValue(property, out ManagedPropertyField field))
            {
                return field.IsChanged;
            }

            return false;
        }

        /// <summary>
        /// 对所有的属性都发生属性变更事件。
        /// </summary>
        internal void NotifyAllPropertiesChanged()
        {
            var properties = this.PropertiesContainer.GetAvailableProperties();
            foreach (var property in properties)
            {
                if (!property.IsReadOnly)
                {
                    this.NotifyPropertyChanged(property);
                }
            }
        }

        internal void RaisePropertyChanged(ManagedPropertyChangedEventArgs e)
        {
            this.OnPropertyChanged(e);

            var p = e.Property as IManagedPropertyInternal;

            var dependencies = p.ReadonlyDependencies;
            if (dependencies.Count > 0)
            {
                for (int i = 0, c = dependencies.Count; i < c; i++)
                {
                    var dp = dependencies[i] as IManagedPropertyInternal;
                    dp.RaiseReadOnlyPropertyChanged(this);
                }
            }
        }

        /// <summary>
        /// 向子类公布一个方法，这样子类可以使用 IManagedProperty 来进行属性变更通知。
        /// 注意，这个方法发布的事件，NewValue、OldValue 将不可用。
        /// </summary>
        /// <param name="property"></param>
        public void NotifyPropertyChanged(IManagedProperty property)
        {
            var propertyInternal = property as IManagedPropertyInternal;
            if (property.IsReadOnly)
            {
                propertyInternal.RaiseReadOnlyPropertyChanged(this);
            }
            else
            {
                var defaultValue = property.GetMeta(this).DefaultValue;
                var args = new ManagedPropertyChangedEventArgs(property, defaultValue, defaultValue);
                this.RaisePropertyChanged(args);
            }
        }

        /// <summary>
        /// 子类重写此方法实现某个扩展属性变更后的处理函数
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnPropertyChanged(ManagedPropertyChangedEventArgs e)
        {
            this.OnPropertyChanged(e.Property.Name);
        }

        /// <summary>
        /// 子类重写此方法实现某个属性变更后的处理函数
        /// 默认实现中，会触发本对象的 PropertyChanged 事件。
        /// </summary>
        /// <param name="propertyName"></param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = _propertyChangedHandlers;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        [NonSerialized]
        private PropertyChangedEventHandler _propertyChangedHandlers;

        /// <summary>
        /// 属性变更后事件。
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                _propertyChangedHandlers = (PropertyChangedEventHandler)
                  System.Delegate.Combine(_propertyChangedHandlers, value);
            }
            remove
            {
                _propertyChangedHandlers = (PropertyChangedEventHandler)
                  System.Delegate.Remove(_propertyChangedHandlers, value);
            }
        }

        #endregion

        #region 其它接口

        /// <summary>
        /// 返回是否存在本地值。（开发者是否存在主动设置/加载的字段值（本地值）。）
        /// 没有本地值的属性，是不占用过多的内存的，在序列化、反序列化的过程中也将被忽略，网络传输时，也不需要传输值。
        /// * 一个属性，如果调用过 LoadProperty、SetProperty 更新了值后，字段都会有本地值。
        /// * 如果调用过 <see cref="ResetProperty(IManagedProperty)"/>  来设置默认值，都会清空其本地值；那么，此时这个方法也会返回 false。
        /// 见：<see cref="ManagedPropertyField.HasLocalValue"/>
        /// </summary>
        /// <param name="property">托管属性</param>
        /// <returns></returns>
        public bool HasLocalValue(IManagedProperty property)
        {
            if (property == null) throw new ArgumentNullException("property");

            var field = this.GetField(property);

            return field.HasLocalValue;
        }

        #endregion

        #region ICustomTypeDescriptor Members

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
        {
            return PropertyDescriptorFactory.Current.GetProperties(this.PropertiesContainer);
        }

        //以下实现复制自 DataRowView

        AttributeCollection ICustomTypeDescriptor.GetAttributes()
        {
            return new AttributeCollection(null);
        }
        string ICustomTypeDescriptor.GetClassName()
        {
            return null;
        }
        string ICustomTypeDescriptor.GetComponentName()
        {
            return null;
        }
        TypeConverter ICustomTypeDescriptor.GetConverter()
        {
            return null;
        }
        EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
        {
            return null;
        }
        PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
        {
            return TypeDescriptor.GetDefaultProperty(this, true);
        }
        object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
        {
            return null;
        }
        EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
        {
            return new EventDescriptorCollection(null);
        }
        EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
        {
            return new EventDescriptorCollection(null);
        }
        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
        {
            return ((ICustomTypeDescriptor)this).GetProperties(null);
        }
        object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
        {
            return this;
        }

        #endregion

        #region 调试

        /// <summary>
        /// 调试器显示文本
        /// </summary>
        internal string DebuggerDisplay
        {
            get
            {
                var res = this.GetType().Name;

                //尝试读取Name属性。
                var nameProperty = this.PropertiesContainer.GetAvailableProperties().Find("Name");
                if (nameProperty != null)
                {
                    if (!this.IsDisabled(nameProperty))
                    {
                        var value = this.GetProperty(nameProperty);
                        if (value != null)
                        {
                            res += " Name:" + value;
                        }
                    }
                }

                return res;
            }
        }

        /// <summary>
        /// 返回 this.DebuggerDisplay。
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            //有时候实体的显示不走 DebuggerDisplay 属性（例如 GridTreeViewRow 中的 Header），
            //所以这里重写 ToString 方法，方便调试。
            return this.DebuggerDisplay;
        }

        #endregion
    }
}