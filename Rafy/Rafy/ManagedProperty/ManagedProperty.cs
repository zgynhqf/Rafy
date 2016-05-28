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
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Rafy;
using Rafy.Reflection;

namespace Rafy.ManagedProperty
{
    /// <summary>
    /// 托管属性标记
    /// </summary>
    public interface IManagedProperty : IHasName
    {
        /// <summary>
        /// 定义此属性的类型
        /// </summary>
        Type OwnerType { get; }

        /// <summary>
        /// 申明这个属性的类型
        /// 
        /// 如果这个属性是一个扩展属性，则这个值与 OwnerType 不同。
        /// </summary>
        Type DeclareType { get; }

        /// <summary>
        /// 属性类型
        /// </summary>
        Type PropertyType { get; }

        /// <summary>
        /// 全局索引
        /// 
        /// 只在本次运行中有效，每次运行不保证一致。
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
        /// 是否为扩展属性（一个类中为另一个类定义的扩展属性）
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
        /// 获取 this.OwnerType 类型所对应的元数据。
        /// </summary>
        IManagedPropertyMetadata DefaultMeta { get; }

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

        IList<IManagedProperty> ReadonlyDependencies { get; }

        object ProvideReadOnlyValue(ManagedPropertyObject component);

        void RaiseReadOnlyPropertyChanged(ManagedPropertyObject sender, ManagedPropertyChangedSource source);
    }

    [DebuggerDisplay("{OwnerType.Name}.{Name}")]
    public class ManagedProperty<TPropertyType> : IManagedProperty, IManagedPropertyInternal
    {
        private int _typeCompiledIndex;

        public ManagedProperty(Type ownerType, string propertyName, ManagedPropertyMetadata<TPropertyType> defaultMeta)
            : this(ownerType, ownerType, propertyName, defaultMeta) { }

        public ManagedProperty(Type ownerType, Type declareType, string propertyName, ManagedPropertyMetadata<TPropertyType> defaultMeta)
        {
            if (ownerType == null) throw new ArgumentNullException("ownerType");
            if (declareType == null) throw new ArgumentNullException("declareType");
            if (string.IsNullOrEmpty(propertyName)) throw new ArgumentNullException("propertyName");
            if (defaultMeta == null) throw new ArgumentNullException("defaultMeta");

            this._typeCompiledIndex = -1;
            this.GlobalIndex = -1;
            this.LifeCycle = ManagedPropertyLifeCycle.Compile;
            this.OwnerType = ownerType;
            this.DeclareType = declareType;
            this._defaultMeta = defaultMeta;
            this._defaultMeta.SetProperty(this);
            this.Name = propertyName;
            if (ownerType != declareType)
            {
                this.IsExtension = true;
            }
        }

        /// <summary>
        /// 定义此属性的类型
        /// </summary>
        public Type OwnerType { get; private set; }

        /// <summary>
        /// 申明这个属性的类型
        /// 
        /// 如果这个属性是一个扩展属性，则这个值与 OwnerType 不同。
        /// </summary>
        public Type DeclareType { get; private set; }

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
        /// 
        /// 只在本次运行中有效，每次运行不保证一致。
        /// </summary>
        public int GlobalIndex { get; internal set; }

        /// <summary>
        /// 是否为扩展属性（一个类中为另一个类定义的扩展发展）
        /// </summary>
        public bool IsExtension { get; private set; }

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
            if (this.GlobalIndex >= 0) throw new InvalidOperationException("属性已经注册完毕，不能修改！");

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
            //直接装箱，以免下行代码装箱两次。
            object nonValue = default(TPropertyType);
            sender.RaisePropertyChanged(new ManagedPropertyChangedEventArgs(this, nonValue, nonValue, source));
        }

        #endregion

        #region Metadata

        private ManagedPropertyMetadata<TPropertyType> _defaultMeta;

        private Dictionary<Type, ManagedPropertyMetadata<TPropertyType>> _overridedMetas;

        /// <summary>
        /// 获取 this.OwnerType 类型所对应的元数据。
        /// </summary>
        public ManagedPropertyMetadata<TPropertyType> DefaultMeta
        {
            get { return this._defaultMeta; }
        }

        /// <summary>
        /// 为某个对象获取本属性的元数据
        /// </summary>
        /// <param name="owner">
        /// 当前属性的声明类或者它的子类的实例
        /// </param>
        /// <returns></returns>
        public ManagedPropertyMetadata<TPropertyType> GetMeta(object owner)
        {
            var result = this._defaultMeta;

            if (this._overridedMetas != null)
            {
                result = this.GetOverridedMeta(owner.GetType());
            }

            return result;
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
            var result = this._defaultMeta;

            if (_overridedMetas != null)
            {
                result = this.GetOverridedMeta(ownerType);
            }

            return result;
        }

        private ManagedPropertyMetadata<TPropertyType> GetOverridedMeta(Type ownerType)
        {
            var result = this._defaultMeta;

            var myOwnerType = this.OwnerType;
            if (ownerType != myOwnerType)
            {
                var t = ownerType;
                while (t != myOwnerType && t != typeof(ManagedPropertyObject) && t != null)
                {
                    if (_overridedMetas.TryGetValue(t, out result)) { break; }

                    t = t.BaseType;
                }

                result = result ?? this._defaultMeta;
            }

            return result;
        }

        /// <summary>
        /// 为某个子类重写元数据
        /// </summary>
        /// <typeparam name="TMeta">The type of the meta.</typeparam>
        /// <param name="ownerSubType">子类的类型。</param>
        /// <param name="overrideMeta">一个全新的元数据对象。</param>
        /// <param name="overrideValues">覆盖某些属性的方法。</param>
        /// <exception cref="System.ArgumentNullException">ownerSubType
        /// or
        /// overrideValues</exception>
        public void OverrideMeta<TMeta>(
            Type ownerSubType, TMeta overrideMeta, Action<TMeta> overrideValues
            )
            where TMeta : ManagedPropertyMetadata<TPropertyType>
        {
            if (ownerSubType == null) throw new ArgumentNullException("ownerSubType");
            if (overrideValues == null) throw new ArgumentNullException("overrideValues");

            //TMeta overrideMeta = new TMeta();
            //try
            //{
            //    overrideMeta = Activator.CreateInstance<TMeta>();
            //}
            //catch
            //{
            //    throw new InvalidProgramException(string.Format(
            //        "{0} 类型必须实现无参的构造函数，否则无法进行元数据覆盖。",
            //        typeof(TMeta)
            //        ));
            //}

            //复制、重写某些属性。
            CloneMeta(ownerSubType, overrideMeta);
            overrideValues(overrideMeta);

            //添加到集合中。
            if (_overridedMetas == null) _overridedMetas = new Dictionary<Type, ManagedPropertyMetadata<TPropertyType>>();
            _overridedMetas[ownerSubType] = overrideMeta;
            overrideMeta.SetProperty(this);

            //冻结。
            overrideMeta.Freeze();
        }

        /// <summary>
        /// 把当前的元数据的值全部合并到 overrideMeta 上。
        /// </summary>
        /// <typeparam name="TMeta"></typeparam>
        /// <param name="ownerSubType"></param>
        /// <param name="overrideMeta"></param>
        private void CloneMeta<TMeta>(Type ownerSubType, TMeta overrideMeta)
        {
            //获取当前的元数据。
            var baseMeta = this.GetMeta(ownerSubType);

            //检测元数据类型的兼容性。
            var overrideMetaType = overrideMeta.GetType();
            var baseMetaType = baseMeta.GetType();
            if (overrideMetaType != baseMetaType && !overrideMetaType.IsSubclassOf(baseMetaType))
            {
                throw new InvalidProgramException(string.Format(
                    "在 {0} 中为 {1} 的 {2} 属性重写元数据时，元数据类型必须是一个 {3} 类型。",
                    ownerSubType, this.OwnerType, this.Name, baseMeta.GetType()
                    ));
            }

            //对于当前元数据的所有字段，进行复制。
            //获取字段时，不能通过 GetFields 方法一次性获取到所有的字段，所以只能由基类到子类，逐个类型进行遍历。
            var hierachy = TypeHelper.GetHierarchy(baseMetaType, typeof(FreezableMeta)).Reverse();
            foreach (var type in hierachy)
            {
                var fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                foreach (var field in fields)
                {
                    var value = field.GetValue(baseMeta);
                    field.SetValue(overrideMeta, value);
                }
            }
        }

        IManagedPropertyMetadata IManagedProperty.DefaultMeta
        {
            get { return this._defaultMeta; }
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
            var mp = obj as IManagedProperty;
            if (mp != null) return mp.GlobalIndex == this.GlobalIndex;

            return base.Equals(obj);
        }

        #endregion
    }
}
