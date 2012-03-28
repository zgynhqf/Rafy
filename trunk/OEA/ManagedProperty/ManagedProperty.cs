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
using System.Diagnostics;

namespace OEA.ManagedProperty
{
    /// <summary>
    /// 托管属性标记
    /// </summary>
    public interface IManagedProperty
    {
        /// <summary>
        /// 定义此属性的类型
        /// </summary>
        Type OwnerType { get; }

        /// <summary>
        /// 属性名
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 属性类型
        /// </summary>
        Type PropertyType { get; }

        /// <summary>
        /// 全局索引
        /// </summary>
        int GlobalIndex { get; }

        /// <summary>
        /// 为本属性所定义的生命周期
        /// </summary>
        ManagedPropertyLifeCycle LifeCycle { get; }

        /// <summary>
        /// 表示当前属性是否已经被注销、不再使用。
        /// </summary>
        bool IsUnregistered { get; }

        /// <summary>
        /// 是否为扩展属性（一个类中为另一个类定义的扩展发展）
        /// </summary>
        bool IsExtension { get; }

        /// <summary>
        /// 是否只读
        /// </summary>
        bool IsReadOnly { get; }

        /// <summary>
        /// 如果此属性即是编译期属性，也不是只读的，则这个值表示该属性在对象中的索引。
        /// </summary>
        int TypeCompiledIndex { get; }

        /// <summary>
        /// 为某个对象获取本属性的元数据
        /// </summary>
        /// <param name="owner"></param>
        /// <returns></returns>
        IManagedPropertyMetadata GetMeta(object owner);

        /// <summary>
        /// 为某个类型获取本属性的元数据
        /// </summary>
        /// <param name="ownerType"></param>
        /// <returns></returns>
        IManagedPropertyMetadata GetMeta(Type ownerType);
    }

    /// <summary>
    /// 内部使用的接口
    /// </summary>
    internal interface IManagedPropertyInternal : IManagedProperty
    {
        new int TypeCompiledIndex { get; set; }

        new bool IsUnregistered { get; set; }

        IManagedPropertyField CreateField();

        IList<IManagedProperty> ReadonlyDependencies { get; }

        object ProvideReadOnlyValue(ManagedPropertyObject component);

        void RaiseReadOnlyPropertyChanged(ManagedPropertyObject sender, ManagedPropertyChangedSource source);

        IManagedPropertyChangedEventArgs CreatePropertyChangedArgs(
            ManagedPropertyObject sender,
            object oldValue, object newValue, ManagedPropertyChangedSource source
            );
    }

    [DebuggerDisplay("{OwnerType.Name}.{Name}")]
    public class ManagedProperty<TPropertyType> : IManagedProperty, IManagedPropertyInternal
    {
        private int _typeCompiledIndex;

        public ManagedProperty(Type ownerType, string propertyName, ManagedPropertyMetadata<TPropertyType> defaultMeta)
        {
            this._typeCompiledIndex = -1;
            this.GlobalIndex = -1;
            this.LifeCycle = ManagedPropertyLifeCycle.CompileOrSetup;
            this.OwnerType = ownerType;
            this.Name = propertyName;
            this._defaultMeta = defaultMeta;
            this._defaultMeta.SetProperty(this);
        }

        /// <summary>
        /// 定义此属性的类型
        /// </summary>
        public Type OwnerType { get; private set; }

        /// <summary>
        /// 属性名
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// 表示当前属性是否已经被注销、不再使用。
        /// </summary>
        public bool IsUnregistered { get; private set; }

        /// <summary>
        /// 为本属性所定义的生命周期
        /// </summary>
        public ManagedPropertyLifeCycle LifeCycle { get; internal set; }

        /// <summary>
        /// 全局索引
        /// </summary>
        public int GlobalIndex { get; internal set; }

        /// <summary>
        /// 是否为扩展属性（一个类中为另一个类定义的扩展发展）
        /// </summary>
        public bool IsExtension { get; set; }

        /// <summary>
        /// 如果此属性即是编译期属性，也不是只读的，则这个值表示该属性在对象中的索引。
        /// </summary>
        public int TypeCompiledIndex
        {
            get { return this._typeCompiledIndex; }
        }

        /// <summary>
        /// 属性类型
        /// </summary>
        public Type PropertyType
        {
            get { return typeof(TPropertyType); }
        }

        #region IsReadOnly

        private Func<ManagedPropertyObject, TPropertyType> _readOnlyValueProvider;

        private List<IManagedProperty> _readonlyDependencies = new List<IManagedProperty>();

        /// <summary>
        /// 是否只读
        /// </summary>
        public bool IsReadOnly
        {
            get { return this._readOnlyValueProvider != null; }
        }

        /// <summary>
        /// 声明本属性为只读属性
        /// </summary>
        /// <param name="readOnlyValueProvider"></param>
        /// <param name="dependencies"></param>
        public void AsReadOnly(Func<ManagedPropertyObject, TPropertyType> readOnlyValueProvider, params IManagedProperty[] dependencies)
        {
            if (readOnlyValueProvider == null) throw new ArgumentNullException("readOnlyValueProvider");

            this._readOnlyValueProvider = readOnlyValueProvider;

            foreach (IManagedPropertyInternal property in dependencies)
            {
                var list = property.ReadonlyDependencies;
                if (!list.Contains(this)) list.Add(this);
            }
        }

        internal TPropertyType ProvideReadOnlyValue(ManagedPropertyObject component)
        {
            return this._readOnlyValueProvider(component);
        }

        object IManagedPropertyInternal.ProvideReadOnlyValue(ManagedPropertyObject component)
        {
            return this._readOnlyValueProvider(component);
        }

        void IManagedPropertyInternal.RaiseReadOnlyPropertyChanged(ManagedPropertyObject sender, ManagedPropertyChangedSource source)
        {
            var nonValue = default(TPropertyType);
            sender.RaisePropertyChanged(new ManagedPropertyChangedEventArgs<TPropertyType>(this, nonValue, nonValue, source));
        }

        #endregion

        #region Metadata

        private ManagedPropertyMetadata<TPropertyType> _defaultMeta;

        private Dictionary<Type, ManagedPropertyMetadata<TPropertyType>> _overridedMetas;

        /// <summary>
        /// 为某个对象获取本属性的元数据
        /// </summary>
        /// <param name="ownerType">
        /// 当前属性的声明类或者它的子类的实例
        /// </param>
        /// <returns></returns>
        public ManagedPropertyMetadata<TPropertyType> GetMeta(object owner)
        {
            return this.GetMeta(owner.GetType());
        }

        /// <summary>
        /// 为某个类型获取本属性的元数据
        /// </summary>
        /// <param name="ownerType">
        /// 当前属性的声明类或者它的子类
        /// </param>
        /// <returns></returns>
        public ManagedPropertyMetadata<TPropertyType> GetMeta(Type ownerType)
        {
            ManagedPropertyMetadata<TPropertyType> result = this._defaultMeta;

            var myOwnerType = this.OwnerType;

            if (this._overridedMetas != null && ownerType != myOwnerType)
            {
                var t = ownerType;
                while (t != myOwnerType && t != typeof(ManagedPropertyObject) && t != null)
                {
                    if (this._overridedMetas.TryGetValue(t, out result)) { break; }

                    t = t.BaseType;
                }

                result = result ?? this._defaultMeta;
            }

            return result;
        }

        /// <summary>
        /// 为某个子类重写元数据
        /// </summary>
        /// <param name="ownerSubType"></param>
        /// <param name="propertyMeta"></param>
        public void OverrideMeta(Type ownerSubType, ManagedPropertyMetadata<TPropertyType> propertyMeta)
        {
            if (ownerSubType == null) throw new ArgumentNullException("ownerSubType");
            if (propertyMeta == null) throw new ArgumentNullException("propertyMeta");

            if (this._overridedMetas == null) this._overridedMetas = new Dictionary<Type, ManagedPropertyMetadata<TPropertyType>>();

            this._overridedMetas[ownerSubType] = propertyMeta;
            propertyMeta.SetProperty(this);
        }

        IManagedPropertyMetadata IManagedProperty.GetMeta(object owner)
        {
            return this.GetMeta(owner);
        }

        IManagedPropertyMetadata IManagedProperty.GetMeta(Type ownerType)
        {
            return this.GetMeta(ownerType);
        }

        #endregion

        int IManagedPropertyInternal.TypeCompiledIndex
        {
            get { return this._typeCompiledIndex; }
            set { this._typeCompiledIndex = value; }
        }

        IList<IManagedProperty> IManagedPropertyInternal.ReadonlyDependencies
        {
            get { return this._readonlyDependencies; }
        }

        internal ManagedPropertyField<TPropertyType> CreateField()
        {
            return new ManagedPropertyField<TPropertyType>(this);
        }

        IManagedPropertyField IManagedPropertyInternal.CreateField()
        {
            return this.CreateField();
        }

        bool IManagedPropertyInternal.IsUnregistered
        {
            get { return this.IsUnregistered; }
            set { this.IsUnregistered = value; }
        }

        #region 重写 GetHashCode 以提高在 Dictionary 中作为 Key 时的速度

        public override int GetHashCode()
        {
            return this.GlobalIndex.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var mp = obj as ManagedProperty<TPropertyType>;
            if (mp != null) return mp.GlobalIndex == this.GlobalIndex;

            return base.Equals(obj);
        }

        #endregion

        IManagedPropertyChangedEventArgs IManagedPropertyInternal.CreatePropertyChangedArgs(
            ManagedPropertyObject sender, object oldValue, object newValue, ManagedPropertyChangedSource source)
        {
            return new ManagedPropertyChangedEventArgs<TPropertyType>(this, (TPropertyType)oldValue, (TPropertyType)newValue, source);
        }
    }
}
