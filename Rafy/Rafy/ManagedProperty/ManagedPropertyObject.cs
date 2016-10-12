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

namespace Rafy.ManagedProperty
{
    /// <summary>
    /// 托管属性对象
    /// 
    /// 从此类继承的子类，可以以托管属性的方式声明自己的所有属性。同时，其它的类型可以为其在扩展编译期属性，也可以在运行时动态扩展属性，
    /// </summary>
    [Serializable]
    [CompiledPropertyDeclarer]
    [DebuggerDisplay("{DebuggerDisplay}")]
    [DebuggerTypeProxy(typeof(ManagedPropertyObjectTypeProxy))]
    public abstract partial class ManagedPropertyObject : CustomSerializationObject, IManagedPropertyObject, INotifyPropertyChanged, ICustomTypeDescriptor
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
                if (this._container == null) { this._container = this.FindPropertiesContainer(); }

                return this._container;
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

            this._compiledFields = new ManagedPropertyField[compiledFieldsCount];

            for (int i = 0; i < compiledFieldsCount; i++)
            {
                var property = compiledProperties[i];
                this._compiledFields[i].ResetToProperty(property);
            }
        }

        /// <summary>
        /// 重设为默认值
        /// </summary>
        /// <param name="property"></param>
        private void _ResetProperty(IManagedProperty property)
        {
            CheckEditing(property);

            if (property.LifeCycle == ManagedPropertyLifeCycle.Compile)
            {
                this._compiledFields[property.TypeCompiledIndex].ResetValue();
            }
            else
            {
                if (this._runtimeFields != null) { this._runtimeFields.Remove(property); }
            }
        }

        private object _GetProperty(IManagedProperty property)
        {
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
                    var field = this._compiledFields[property.TypeCompiledIndex];
                    if (field.HasValue)
                    {
                        result = field.Value;
                        useDefault = false;
                    }
                }
                else
                {
                    if (this._runtimeFields != null)
                    {
                        ManagedPropertyField field;
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

            result = meta.CoerceGetValue(this, result);

            return result;
        }

        /// <summary>
        /// 设置某个属性的值。
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        /// <param name="source"></param>
        /// <returns>返回最终使用的值。</returns>
        private object _SetProperty(IManagedProperty property, object value, ManagedPropertyChangedSource source)
        {
            object finalValue = null;

            CheckEditing(property);

            var meta = property.GetMeta(this) as IManagedPropertyMetadataInternal;
            finalValue = meta.DefaultValue;

            value = CoerceType(property, value);

            bool isReset = false;
            if (NeedReset(property, value))
            {
                isReset = true;
                value = meta.DefaultValue;
            }

            bool cancel = meta.RaisePropertyChangingMetaEvent(this, ref value, source);
            if (!cancel)
            {
                bool hasOldValue = false;
                object oldValue = null;

                //这个 if 块中的代码：查找或创建对应 property 的 field，同时记录可能存在的历史值。
                if (property.LifeCycle == ManagedPropertyLifeCycle.Compile)
                {
                    var index = property.TypeCompiledIndex;
                    var field = this._compiledFields[index];
                    if (field.HasValue)
                    {
                        oldValue = field.Value;
                        hasOldValue = true;
                    }

                    if (isReset)
                    {
                        this._compiledFields[index].ResetValue();
                    }
                    else
                    {
                        this._compiledFields[index]._value = value;
                    }
                }
                else
                {
                    if (this._runtimeFields == null)
                    {
                        if (!isReset)
                        {
                            this._runtimeFields = new Dictionary<IManagedProperty, ManagedPropertyField>();
                        }
                    }
                    else
                    {
                        var oldField = new ManagedPropertyField();
                        if (this._runtimeFields.TryGetValue(property, out oldField))
                        {
                            oldValue = oldField.Value;
                            hasOldValue = true;
                        }
                    }

                    if (isReset)
                    {
                        if (hasOldValue)
                        {
                            this._runtimeFields.Remove(property);
                        }
                    }
                    else
                    {
                        //使用新的 field
                        var field = new ManagedPropertyField
                        {
                            _property = property,
                            _value = value
                        };
                        if (hasOldValue)
                        {
                            this._runtimeFields[property] = field;
                        }
                        else
                        {
                            this._runtimeFields.Add(property, field);
                        }
                    }
                }

                if (!hasOldValue) { oldValue = meta.DefaultValue; }

                if (!object.Equals(oldValue, value))
                {
                    var args = new ManagedPropertyChangedEventArgs(property, oldValue, value, source);

                    //发生 Meta 中的回调事件
                    meta.RaisePropertyChangedMetaEvent(this, args);

                    //发生外部事件
                    this.RaisePropertyChanged(args);

                    finalValue = value;
                }
            }

            return finalValue;
        }

        /// <summary>
        /// LoadProperty 直接设置值，不发生 PropertyChanged 事件。
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        private void _LoadProperty(IManagedProperty property, object value)
        {
            CheckEditing(property);

            value = CoerceType(property, value);

            if (NeedReset(property, value))
            {
                this._ResetProperty(property);
                return;
            }

            if (property.LifeCycle == ManagedPropertyLifeCycle.Compile)
            {
                this._compiledFields[property.TypeCompiledIndex]._value = value;
            }
            else
            {
                if (this._runtimeFields == null)
                {
                    this._runtimeFields = new Dictionary<IManagedProperty, ManagedPropertyField>();
                }

                var field = new ManagedPropertyField
                {
                    _property = property,
                    _value = value
                };
                this._runtimeFields[property] = field;
            }
        }

        //[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        private void CheckEditing(IManagedProperty property)
        {
            if (!property.OwnerType.IsInstanceOfType(this))
            {
                throw new InvalidProgramException("属性与当前对象的类型不符，赋值错误！");
            }
            if (property.IsReadOnly) throw new InvalidOperationException("属性是只读的！");
        }

        /// <summary>
        /// 如果把 null 赋值给一个值类型，则直接还原此属性为默认值。
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private static bool NeedReset(IManagedProperty property, object value)
        {
            if (value == null)
            {
                var propertyType = property.PropertyType;
                if (propertyType.IsValueType && (!propertyType.IsGenericType || propertyType.GetGenericTypeDefinition() != typeof(Nullable<>)))
                {
                    return true;
                }
            }

            return false;
        }

        private static object CoerceType(IManagedProperty property, object value)
        {
            if (value != null)
            {
                value = TypeHelper.CoerceValue(property.PropertyType, value.GetType(), value);
            }
            return value;
        }

        #endregion

        #region GetProperty / SetProperty / ClearProperty

        /// <summary>
        /// 重设属性为默认值
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
        /// <param name="source">本次值设置的来源。</param>
        /// <returns>返回最终使用的值。</returns>
        public object SetProperty(ManagedProperty<bool> property, bool value, ManagedPropertyChangedSource source = ManagedPropertyChangedSource.FromProperty)
        {
            //使用 BooleanBoxes 来防止装箱操作。
            return this._SetProperty(property, BooleanBoxes.Box(value), source);
        }

        /// <summary>
        /// 设置某个托管属性的值。
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        /// <param name="source">本次值设置的来源。</param>
        /// <returns>返回最终使用的值。</returns>
        public virtual object SetProperty(IManagedProperty property, object value, ManagedPropertyChangedSource source = ManagedPropertyChangedSource.FromProperty)
        {
            return this._SetProperty(property, value, source);
        }

        /// <summary>
        /// LoadProperty 以最快的方式直接加载值，不发生 PropertyChanged 事件。
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        public virtual void LoadProperty(IManagedProperty property, object value)
        {
            this._LoadProperty(property, value);
        }

        #endregion

        #region PropertyChanged

        /// <summary>
        /// 对所有的属性都发生属性变更事件。
        /// </summary>
        public void NotifyAllPropertiesChanged()
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
                    dp.RaiseReadOnlyPropertyChanged(this, e.Source);
                }
            }
        }

        /// <summary>
        /// 向子类公布一个方法，这样子类可以使用 IManagedProperty 来进行属性变更通知。
        /// 注意，这个方法发布的事件，NewValue、OldValue 将不可用。
        /// </summary>
        /// <param name="property"></param>
        /// <param name="source"></param>
        public void NotifyPropertyChanged(IManagedProperty property, ManagedPropertyChangedSource source = ManagedPropertyChangedSource.FromProperty)
        {
            var propertyInternal = property as IManagedPropertyInternal;
            if (property.IsReadOnly)
            {
                propertyInternal.RaiseReadOnlyPropertyChanged(this, source);
            }
            else
            {
                var defaultValue = property.GetMeta(this).DefaultValue;
                var args = new ManagedPropertyChangedEventArgs(property, defaultValue, defaultValue, source);
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
            var handler = this._propertyChangedHandlers;
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

        #region public CompiledPropertyValuesEnumerator GetCompiledPropertyValues

        /// <summary>
        /// 获取编译期属性值集合
        /// </summary>
        /// <returns></returns>
        public CompiledPropertyValuesEnumerator GetCompiledPropertyValues()
        {
            return new CompiledPropertyValuesEnumerator(this);
        }

        public struct CompiledPropertyValuesEnumerator
        {
            private ManagedPropertyObject _mpo;
            private ManagedPropertyField _current;
            private IList<IManagedProperty> _properties;
            private int _index;

            internal CompiledPropertyValuesEnumerator(ManagedPropertyObject mpo)
            {
                _index = -1;
                _mpo = mpo;
                _current = new ManagedPropertyField();
                _properties = mpo.PropertiesContainer.GetCompiledProperties();
            }

            public ManagedPropertyField Current
            {
                get { return _current; }
            }

            public bool MoveNext()
            {
                if (++_index >= _properties.Count) return false;

                var property = _properties[_index];
                if (property.IsReadOnly)
                {
                    _current = new ManagedPropertyField
                    {
                        _property = property,
                        _value = (property as IManagedPropertyInternal).ProvideReadOnlyValue(_mpo)
                    };
                }
                else
                {
                    _current = _mpo._compiledFields[property.TypeCompiledIndex];
                    if (!_current.HasValue)
                    {
                        _current = new ManagedPropertyField
                        {
                            _property = property,
                            _value = property.GetMeta(_mpo).DefaultValue
                        };
                    }
                }

                return true;
            }

            public CompiledPropertyValuesEnumerator GetEnumerator()
            {
                //添加此方法，使得可以使用 foreach 循环
                return this;
            }
        }

        #endregion

        #region public NonDefaultPropertyValuesEnumerator GetNonDefaultPropertyValues

        /// <summary>
        /// 获取当前对象所有非默认值的属性值集合。
        /// </summary>
        /// <returns></returns>
        public NonDefaultPropertyValuesEnumerator GetNonDefaultPropertyValues()
        {
            return new NonDefaultPropertyValuesEnumerator(this);
        }

        public struct NonDefaultPropertyValuesEnumerator
        {
            private ManagedPropertyObject _mpo;
            private ManagedPropertyField[] _runtimeFields;
            private ManagedPropertyField _current;
            private int _index;
            private bool _enumerateRuntimeFields;

            internal NonDefaultPropertyValuesEnumerator(ManagedPropertyObject mpo)
            {
                _index = -1;
                _mpo = mpo;
                _current = new ManagedPropertyField();
                _runtimeFields = null;
                _enumerateRuntimeFields = false;
            }

            public ManagedPropertyField Current
            {
                get { return _current; }
            }

            public bool MoveNext()
            {
                _index++;

                var compiledFields = _mpo._compiledFields;

                //编译期属性
                var compiledFieldLength = compiledFields.Length;
                while (_index < compiledFieldLength)
                {
                    var field = compiledFields[_index];
                    if (field.HasValue)
                    {
                        var defaultValue = field.Property.GetMeta(this).DefaultValue;
                        if (!object.Equals(field.Value, defaultValue))
                        {
                            _current = field;
                            return true;
                        }
                    }

                    _index++;
                }

                //第一次进入运行时属性遍历
                if (!_enumerateRuntimeFields)
                {
                    var rf = _mpo._runtimeFields;
                    if (rf != null)
                    {
                        var values = rf.Values;
                        _runtimeFields = new ManagedPropertyField[values.Count];
                        values.CopyTo(_runtimeFields, 0);
                    }

                    _enumerateRuntimeFields = true;
                }

                //开始遍历运行时属性。
                if (_runtimeFields != null)
                {
                    var runtimeFieldsCount = _runtimeFields.Length;
                    while (true)
                    {
                        var runtimeIndex = _index - compiledFieldLength;
                        if (runtimeIndex >= runtimeFieldsCount) { break; }

                        var field = _runtimeFields[runtimeIndex];
                        if (field.HasValue)
                        {
                            var defaultValue = field.Property.GetMeta(this).DefaultValue;
                            if (!object.Equals(field.Value, defaultValue))
                            {
                                _current = field;
                                return true;
                            }
                        }

                        _index++;
                    }
                }

                return false;
            }

            public NonDefaultPropertyValuesEnumerator GetEnumerator()
            {
                //添加此方法，使得可以使用 foreach 循环
                return this;
            }
        }

        #endregion

        /// <summary>
        /// 是否存在主动设置/加载的字段值（本地值）。
        /// </summary>
        /// <param name="property">托管属性</param>
        /// <returns></returns>
        public bool FieldExists(IManagedProperty property)
        {
            if (property == null) throw new ArgumentNullException("property");

            if (property.IsReadOnly) return true;

            if (property.LifeCycle == ManagedPropertyLifeCycle.Compile)
            {
                return this._compiledFields[property.TypeCompiledIndex].HasValue;
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
            return null;
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
                    var value = this.GetProperty(nameProperty);
                    if (value != null)
                    {
                        res += " Name:" + value;
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