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
using System.ComponentModel;
using System.Linq.Expressions;
using System.Security.Permissions;
using System.Security;
using OEA.Serialization.Mobile;
using System.Reflection;
using OEA.Reflection;

namespace OEA.ManagedProperty
{
    /// <summary>
    /// 托管属性对象
    /// </summary>
    [Serializable]
    [CompiledPropertyDeclarer]
    public abstract partial class ManagedPropertyObject : INotifyPropertyChanged, ICustomTypeDescriptor
    {
        #region Fields

        [NonSerialized]
        private ConsolidatedTypePropertiesContainer _container;

        private ManagedPropertyObjectFieldsManager _fields;

        private IManagedPropertyField[] CompiledFields
        {
            get { return this._fields._compiledFields; }
        }

        #endregion

        public ManagedPropertyObject()
        {
            this._fields = new ManagedPropertyObjectFieldsManager(this);
        }

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

        protected virtual ConsolidatedTypePropertiesContainer FindPropertiesContainer()
        {
            return ManagedPropertyRepository.Instance.GetTypePropertiesContainer(this.GetType());
        }

        #region GetProperty / SetProperty / ClearProperty

        /// <summary>
        /// 重设属性为默认值
        /// </summary>
        /// <param name="property"></param>
        public void ResetProperty(IManagedProperty property)
        {
            this._fields.ResetProperty(property);
        }

        public object GetProperty(IManagedProperty property)
        {
            return this._fields.GetProperty(property);
        }

        public TPropertyType GetProperty<TPropertyType>(ManagedProperty<TPropertyType> property)
        {
            return this._fields.GetProperty(property);
        }

        public void SetProperty(IManagedProperty property, object value, ManagedPropertyChangedSource source = ManagedPropertyChangedSource.FromProperty)
        {
            this._fields.SetProperty(property, value, source);
        }

        public void SetProperty<TPropertyType>(ManagedProperty<TPropertyType> property, TPropertyType value, ManagedPropertyChangedSource source = ManagedPropertyChangedSource.FromProperty)
        {
            this._fields.SetProperty(property, value, source);
        }

        /// <summary>
        /// LoadProperty 直接设置值，不发生 PropertyChanged 事件。
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        public virtual void LoadProperty(IManagedProperty property, object value)
        {
            this._fields.LoadProperty(property, value);
        }

        /// <summary>
        /// LoadProperty 直接设置值，不发生 PropertyChanged 事件。
        /// </summary>
        /// <typeparam name="TPropertyType"></typeparam>
        /// <param name="property"></param>
        /// <param name="value"></param>
        public virtual void LoadProperty<TPropertyType>(ManagedProperty<TPropertyType> property, TPropertyType value)
        {
            this._fields.LoadProperty(property, value);
        }

        #endregion

        #region 使用字符串的 GetProperty / SetProperty

        public IManagedProperty FindProperty(string managedProperty)
        {
            if (string.IsNullOrWhiteSpace(managedProperty)) throw new ArgumentNullException("managedProperty");

            return this.PropertiesContainer.GetAvailableProperties().FirstOrDefault(p => p.Name == managedProperty);
        }

        /// <summary>
        /// 根据托管属性名设置本对象的值。
        /// 
        /// 如果没有这个托管属性还没有加载到对象中，或者这个没有这个托管属性。则返回false。
        /// 如果返回真，表示设置成功。
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TrySetPropertyValue(string propertyName, object value)
        {
            var property = this.FindProperty(propertyName);
            if (property != null)
            {
                this.SetProperty(property, value);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 根据托管属性名读取本对象的值。
        /// 
        /// 如果没有这个托管属性还没有加载到对象中，或者这个没有这个托管属性。则返回false。
        /// 如果返回真，表示设置成功。
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetPropertyValue<T>(string propertyName, out T value)
        {
            value = default(T);
            object objValue = null;

            var property = this.FindProperty(propertyName);
            if (property != null)
            {
                objValue = this.GetProperty(property);

                value = (T)objValue;
                return true;
            }

            return false;
        }

        #endregion

        #region RegisterProperty

        /// <summary>
        /// Register Property
        /// </summary>
        /// <typeparam name="T">Type of Target</typeparam>
        /// <typeparam name="P">Type of property</typeparam>
        /// <param name="propertyLambdaExpression">Property Expression</param>
        /// <param name="defaultValue">Default Value for the property</param>
        /// <returns></returns>
        protected static ManagedProperty<P> RegisterProperty<T, P>(Expression<Func<T, object>> propertyLambdaExpression, P defaultValue)
        {
            var reflectedPropertyInfo = Reflect<T>.GetProperty(propertyLambdaExpression);

            var property = new ManagedProperty<P>(
                typeof(T), reflectedPropertyInfo.Name, new ManagedPropertyMetadata<P>()
                {
                    DefaultValue = defaultValue
                });

            ManagedPropertyRepository.Instance.RegisterProperty(property);

            return property;
        }

        #endregion

        #region PropertyChanged

        internal void RaisePropertyChanged(IManagedPropertyChangedEventArgs e)
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
        protected void OnPropertyChanged(IManagedProperty property, ManagedPropertyChangedSource source = ManagedPropertyChangedSource.FromProperty)
        {
            var propertyInternal = property as IManagedPropertyInternal;
            if (property.IsReadOnly)
            {
                propertyInternal.RaiseReadOnlyPropertyChanged(this, source);
            }
            else
            {
                var defaultValue = property.GetMeta(this).DefaultValue;
                var args = propertyInternal.CreatePropertyChangedArgs(this, defaultValue, defaultValue, source);
                this.RaisePropertyChanged(args);
            }
        }

        /// <summary>
        /// 子类重写此方法实现某个扩展属性变更后的处理函数
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnPropertyChanged(IManagedPropertyChangedEventArgs e)
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

        /// <summary>
        /// Implements a serialization-safe PropertyChanged event.
        /// </summary>
        [NonSerialized]
        private PropertyChangedEventHandler _propertyChangedHandlers;

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

        #region 代理到 _fields 的接口

        public IEnumerable<IManagedPropertyField> GetCompiledPropertyValues()
        {
            return this._fields.GetCompiledPropertyValues();
        }

        public IEnumerable<IManagedPropertyField> GetNonDefaultPropertyValues()
        {
            return this._fields.GetNonDefaultPropertyValues();
        }

        /// <summary>
        /// 是否存在属性值，懒加载还没加载时不存在
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public bool FieldExists(string propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName)) throw new ArgumentNullException("propertyName");

            var property = this.FindProperty(propertyName);
            if (property == null) return false;

            return this._fields.FieldExists(property);
        }

        public bool FieldExists(IManagedProperty property)
        {
            if (property == null) throw new ArgumentNullException("property");

            return this._fields.FieldExists(property);
        }

        //public bool IsDefault(IManagedProperty property)
        //{
        //    if (property.LifeCycle == ManagedPropertyLifeCycle.CompileOrSetup)
        //    {
        //        return this._compiledFields[property.TypeCompiledIndex] != null;
        //    }
        //    else
        //    {
        //        if (this._runtimeFields != null)
        //        {
        //            return this._runtimeFields.ContainsKey(property);
        //        }
        //        return false;
        //    }
        //} 

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
    }
}