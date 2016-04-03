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
    public sealed class RefEntityProperty<TRefEntity> : Property<Entity>, IRefEntityProperty, IRefEntityPropertyInternal
        where TRefEntity : Entity
    {
        private IRefIdProperty _refIdProperty;

        /// <summary>
        /// 自定义加载器。
        /// </summary>
        private RefEntityLoader _loader;

        /// <summary>
        /// 为了提高性能，在这个属性上添加一个 IRepository 的缓存字段。
        /// </summary>
        private IRepository _defaultLoader;

        internal RefEntityProperty(Type ownerType, Type declareType, string propertyName, ManagedPropertyMetadata<Entity> defaultMeta) : base(ownerType, declareType, propertyName, defaultMeta) { }

        internal RefEntityProperty(Type ownerType, string propertyName, ManagedPropertyMetadata<Entity> defaultMeta) : base(ownerType, propertyName, defaultMeta) { }

        public override PropertyCategory Category
        {
            get { return PropertyCategory.ReferenceEntity; }
        }

        public ReferenceType ReferenceType
        {
            get { return this._refIdProperty.ReferenceType; }
        }

        public IRefIdProperty RefIdProperty
        {
            get { return this._refIdProperty; }
            internal set
            {
                this._refIdProperty = value;
                if (value != null)
                {
                    (value as IRefIdPropertyInternal).RefEntityProperty = this;
                }
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

        public Type RefEntityType
        {
            get { return typeof(TRefEntity); }
        }

        public bool Nullable
        {
            get { return this._refIdProperty.Nullable; }
        }

        IRefEntityProperty IRefProperty.RefEntityProperty
        {
            get { return this; }
        }

        Entity IRefEntityPropertyInternal.Load(object id, Entity owner)
        {
            //通过自定义 Loader 获取实体。
            if (this._loader != null)
            {
                return this._loader(id, owner);
            }

            //通过默认的 CacheById 方法获取实体。
            if (this._defaultLoader == null)
            {
                this._defaultLoader = RepositoryFactoryHost.Factory.FindByEntity(this.RefEntityType);
            }
            return this._defaultLoader.CacheById(id);
        }
    }

    /// <summary>
    /// 引用实体加载器方法。
    /// </summary>
    /// <param name="id">引用实体的 id。</param>
    /// <param name="owner">拥有该引用属性的实体。</param>
    /// <returns>返回对应的引用实体。</returns>
    public delegate Entity RefEntityLoader(object id, Entity owner);

    internal interface IRefEntityPropertyInternal
    {
        /// <summary>
        /// 加载某个 id 对应的引用实体。
        /// </summary>
        /// <param name="id"></param>
        /// <param name="owner"></param>
        /// <returns></returns>
        Entity Load(object id, Entity owner);
    }
}