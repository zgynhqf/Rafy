/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20121120 20:11
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20121120 20:11
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.Reflection;

namespace Rafy.Domain
{
    /// <summary>
    /// 引用实体属性的实体标记
    /// </summary>
    /// <typeparam name="TRefEntity">引用实体的类型</typeparam>
    public sealed class RefEntityProperty<TRefEntity> : Property<Entity>, IRefProperty, IRefPropertyInternal
        where TRefEntity : Entity
    {
        private IManagedProperty _refKeyProperty;
        private IManagedProperty _keyPropertyOfRefEntity = Entity.IdProperty;

        /// <summary>
        /// 自定义加载器。
        /// </summary>
        private RefEntityLoader _loader;

        /// <summary>
        /// 为了提高性能，在这个属性上添加一个 IRepository 的缓存字段。
        /// </summary>
        private IRepository _defaultLoader;

        private IKeyProvider _keyProvider;

        internal RefEntityProperty(Type ownerType, Type declareType, string propertyName, ManagedPropertyMetadata<Entity> defaultMeta) : base(ownerType, declareType, propertyName, defaultMeta) { }

        internal RefEntityProperty(Type ownerType, string propertyName, ManagedPropertyMetadata<Entity> defaultMeta) : base(ownerType, propertyName, defaultMeta) { }

        public override PropertyCategory Category => PropertyCategory.ReferenceEntity;

        /// <summary>
        /// 实体引用的类型
        /// </summary>
        public ReferenceType ReferenceType { get; internal set; }

        /// <summary>
        /// 引用实体的键对应的托管属性。
        /// </summary>
        public IManagedProperty KeyPropertyOfRefEntity { get => _keyPropertyOfRefEntity; internal set => _keyPropertyOfRefEntity = value ?? Entity.IdProperty; }

        /// <summary>
        /// 引用的实体的主键的算法程序。
        /// </summary>
        public IKeyProvider KeyProvider
        {
            get
            {
                if (_keyProvider == null)
                {
                    _keyProvider = KeyProviders.Get(TypeHelper.IgnoreNullable(_refKeyProperty.PropertyType));
                }
                return _keyProvider;
            }
        }

        public IManagedProperty RefKeyProperty
        {
            get { return _refKeyProperty; }
            internal set
            {
                _refKeyProperty = value;
                (value as IManagedPropertyInternal).RefEntityProperty = this;
            }
        }

        /// <summary>
        /// 自定义的引用实体加载器。
        /// </summary>
        public RefEntityLoader Loader
        {
            get { return this._loader; }
            internal set { this._loader = value; }
        }

        public Type RefEntityType => typeof(TRefEntity);

        public bool Nullable { get; internal set; }

        internal void ResetNullable(bool? isNullable)
        {
            if (isNullable.HasValue)
            {
                this.Nullable = isNullable.Value;
            }
            else
            {
                this.Nullable = this.ReferenceType != ReferenceType.Parent &&
                    TypeHelper.IsNullableOrClass(_refKeyProperty.PropertyType);
            }
        }

        Entity IRefPropertyInternal.Load(object keyValue, Entity owner)
        {
            //通过自定义 Loader 获取实体。
            if (_loader != null)
            {
                return _loader(keyValue, _keyPropertyOfRefEntity, owner);
            }

            //默认加载器就是实体的仓库。
            if (_defaultLoader == null)
            {
                _defaultLoader = RepositoryFactoryHost.Factory.FindByEntity(this.RefEntityType, true);
            }

            //如果是 Id，则通过默认的 CacheById 方法获取实体。
            if (_keyPropertyOfRefEntity == Entity.IdProperty)
            {
                return _defaultLoader.CacheById(keyValue);
            }

            return _defaultLoader.GetByKey(_keyPropertyOfRefEntity.Name, keyValue);
        }
    }

    /// <summary>
    /// 引用实体加载器方法。
    /// </summary>
    /// <param name="keyValue">引用实体的键的值。</param>
    /// <param name="refEntityKeyProperty">引用实体的键对应的属性。</param>
    /// <param name="owner">拥有该引用属性的实体。</param>
    /// <returns>返回对应的引用实体。</returns>
    public delegate Entity RefEntityLoader(object keyValue, IManagedProperty refEntityKeyProperty, Entity owner);

    internal interface IRefPropertyInternal
    {
        /// <summary>
        /// 加载某个 keyValue 对应的引用实体。
        /// </summary>
        /// <param name="keyValue"></param>
        /// <param name="owner"></param>
        /// <returns></returns>
        Entity Load(object keyValue, Entity owner);
    }
}