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
using OEA.MetaModel;

namespace OEA.Library
{
    /// <summary>
    /// 引用属性标记
    /// </summary>
    /// <typeparam name="TRefEntity">引用实体的类型</typeparam>
    public class RefProperty<TRefEntity> : Property<ILazyEntityRef<TRefEntity>>, IRefProperty
        where TRefEntity : Entity
    {
        public RefProperty(Type ownerType, string refEntityProperty, RefPropertyMetadata<TRefEntity> meta)
            : base(ownerType, refEntityProperty, meta) { }

        /// <summary>
        /// 引用实体的类型
        /// </summary>
        public Type RefEntityType
        {
            get { return typeof(TRefEntity); }
        }

        public new IOEARefPropertyMetadata GetMeta(object owner)
        {
            return base.GetMeta(owner) as IOEARefPropertyMetadata;
        }

        public new IOEARefPropertyMetadata GetMeta(Type ownerType)
        {
            return base.GetMeta(ownerType) as IOEARefPropertyMetadata;
        }

        internal ILazyEntityRef<TRefEntity> CreateRef(Entity owner)
        {
            var typedMeta = this.GetMeta(owner) as RefPropertyMetadata<TRefEntity>;
            var meta = typedMeta.Core;

            var info = new LazyEntityRefPropertyInfo(meta.RefEntityProperty)
            {
                NotifyRefEntityChanged = meta.NotifyRefEntityChanged,
                IdProperty = meta.IdProperty
            };

            //根据不同的 Loader 进行不同的外键加载。
            if (meta.InstaceLoader != null) { return CreateReference(owner, meta.InstaceLoader, info, meta.SerializeEntity); }

            return CreateReference(owner, meta.StaticLoader, info, meta.SerializeEntity);
        }

        ILazyEntityRef IRefProperty.CreateRef(Entity owner)
        {
            return this.CreateRef(owner);
        }

        /// <summary>
        /// 创建一个使用延迟加载的外键的引用
        /// </summary>
        /// <typeparam name="TRefEntity"></typeparam>
        /// <param name="loader"></param>
        /// <param name="refPropertyInfo"></param>
        /// <param name="serializeEntity"></param>
        /// <returns></returns>
        private static ILazyEntityRef<TRefEntity> CreateReference(Entity owner, Func<int, Entity> loader, LazyEntityRefPropertyInfo refPropertyInfo, bool serializeEntity)
        {
            if (serializeEntity)
            {
                return new LazyEntityRef<TRefEntity>(loader, owner, refPropertyInfo);
            }
            else
            {
                return new NonSerializableEntityLazyEntityRef<TRefEntity>(loader, owner, refPropertyInfo);
            }
        }

        /// <summary>
        /// 创建一个使用延迟加载的外键的引用
        /// </summary>
        /// <typeparam name="TRefEntity"></typeparam>
        /// <param name="instaceLoader"></param>
        /// <param name="serializeEntity"></param>
        /// <param name="notifyRefEntityChanged"></param>
        /// <returns></returns>
        private static ILazyEntityRef<TRefEntity> CreateReference(Entity owner, Func<int, object, Entity> instaceLoader, LazyEntityRefPropertyInfo refPropertyInfo, bool serializeEntity)
        {
            if (serializeEntity)
            {
                return new LazyEntityRef<TRefEntity>(instaceLoader, owner, refPropertyInfo);
            }
            else
            {
                return new NonSerializableEntityLazyEntityRef<TRefEntity>(instaceLoader, owner, refPropertyInfo);
            }
        }
    }
}