/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20101230
 * 说明：生成聚合SQL的加载项中的某一项
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20101230
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Rafy.MetaModel;
using Rafy.ManagedProperty;

namespace Rafy.Domain.ORM
{
    /// <summary>
    /// 生成聚合SQL的加载项中的某一项
    /// </summary>
    [DebuggerDisplay("{OwnerType.Name}.{PropertyEntityType.Name}")]
    internal class LoadOptionItem
    {
        private Action<Entity, Entity> _fkSetter;

        internal LoadOptionItem(PropertyMeta fkPropertyInfo, Action<Entity, Entity> fkSetter)
        {
            if (fkPropertyInfo == null) throw new ArgumentNullException("propertyInfo");
            if (fkSetter == null) throw new ArgumentNullException("fkSetter");

            this.PropertyMeta = fkPropertyInfo;
            this._fkSetter = fkSetter;
        }

        internal LoadOptionItem(PropertyMeta childrenPropertyInfo)
        {
            if (childrenPropertyInfo == null) throw new ArgumentNullException("propertyInfo");

            this.PropertyMeta = childrenPropertyInfo;
        }

        internal void SetReferenceEntity(Entity owner, Entity referenceEntity)
        {
            this._fkSetter(owner, referenceEntity);
        }

        /// <summary>
        /// 加载这个属性。
        /// </summary>
        internal PropertyMeta PropertyMeta { get; private set; }

        internal Func<Entity, object> OrderBy { get; set; }

        /// <summary>
        /// 指标这个属性是一般的实体
        /// </summary>
        internal AggregateLoadType LoadType
        {
            get
            {
                return this._fkSetter == null ? AggregateLoadType.Children : AggregateLoadType.ReferenceEntity;
            }
        }

        /// <summary>
        /// 拥有这个属性的实体类型。
        /// </summary>
        internal Type OwnerType
        {
            get
            {
                return this.PropertyMeta.Owner.EntityType;
            }
        }

        #region 临时存储的不可变属性，不用多次查询，提升性能

        private Type _propertyEntityType;
        /// <summary>
        /// 这个属性对应的实体类型
        /// </summary>
        internal Type PropertyEntityType
        {
            get
            {
                if (this._propertyEntityType == null)
                {
                    var meta = this.PropertyMeta;
                    var propertyType = meta.Runtime.PropertyType;
                    if (this.LoadType == AggregateLoadType.Children)
                    {
                        _propertyEntityType = EntityMatrix.FindByList(propertyType).EntityType;
                    }
                    else
                    {
                        //如果是外键，则需要找到对应的实体属性的属性类型，即为引用的实体类型。
                        var refProperty = (meta as EntityPropertyMeta).ManagedProperty as IRefProperty;
                        _propertyEntityType = refProperty.RefEntityType;
                    }
                }
                return this._propertyEntityType;
            }
        }

        private EntityRepository _ownerRepository;
        public EntityRepository OwnerRepository
        {
            get
            {
                if (this._ownerRepository == null)
                {
                    this._ownerRepository = RF.Find(this.OwnerType);
                }
                return this._ownerRepository;
            }
        }

        private EntityRepository _propertyEntityRepository;
        public EntityRepository PropertyEntityRepository
        {
            get
            {
                if (this._propertyEntityRepository == null)
                {
                    this._propertyEntityRepository = RF.Find(this.PropertyEntityType);
                }
                return this._propertyEntityRepository;
            }
        }

        #endregion
    }

    /// <summary>
    /// 属性的加载类型
    /// </summary>
    internal enum AggregateLoadType
    {
        /// <summary>
        /// 加载子对象集合属性
        /// </summary>
        Children,

        /// <summary>
        /// 加载外键引用实体。
        /// </summary>
        ReferenceEntity
    }
}
