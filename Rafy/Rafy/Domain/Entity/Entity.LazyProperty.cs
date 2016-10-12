/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110320
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100320
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.Domain.ORM;
using Rafy.Utils;
using Rafy.Reflection;

namespace Rafy.Domain
{
    //懒加载引用实体的相关实现
    public partial class Entity
    {
        #region 延迟加载 - 子集合

        /// <summary>
        /// 延迟加载子对象的集合。
        /// </summary>
        /// <typeparam name="TCollection">子对象集合类型</typeparam>
        /// <param name="propertyInfo">当前属性的元信息</param>
        /// <returns></returns>
        protected TCollection GetLazyList<TCollection>(ListProperty<TCollection> propertyInfo)
            where TCollection : EntityList
        {
            return this.LoadLazyList(propertyInfo) as TCollection;
        }

        /// <summary>
        /// 延迟加载子对象的集合。
        /// </summary>
        /// <param name="listProperty">The list property.</param>
        /// <returns></returns>
        public EntityList GetLazyList(IListProperty listProperty)
        {
            return this.LoadLazyList(listProperty);
        }

        /// <summary>
        /// 执行懒加载操作。
        /// </summary>
        /// <param name="listProperty">The list property.</param>
        /// <returns></returns>
        private EntityList LoadLazyList(IListProperty listProperty)
        {
            EntityList data = null;

            if (this.FieldExists(listProperty))
            {
                data = this.GetProperty(listProperty) as EntityList;
                if (data != null) return data;
            }

            if (this.IsNew)
            {
                var listRepository = RepositoryFactoryHost.Factory.FindByEntity(listProperty.ListEntityType);
                data = listRepository.NewList();
            }
            else
            {
                var meta = listProperty.GetMeta(this) as IListPropertyMetadata;
                var dataProvider = meta.DataProvider;
                if (dataProvider != null)
                {
                    data = dataProvider(this);
                }
                else
                {
                    var listRepository = RepositoryFactoryHost.Factory.FindByEntity(listProperty.ListEntityType) as IRepositoryInternal;
                    data = listRepository.GetLazyListByParent(this);
                    data.SetParentEntity(this);
                }
            }

            this.LoadProperty(listProperty, data);

            return data;
        }

        #endregion

        #region 延迟加载 - 引用实体

        /// <summary>
        /// 获取指定引用 id 属性对应的 id 的可空类型返回值。
        /// </summary>
        /// <param name="property"></param>
        /// <returns>本方法为兼容值类型而使用。不论 Id 是值类型、还是引用类型，都可能返回 null。</returns>
        public object GetRefNullableId(IRefIdProperty property)
        {
            var value = this.GetProperty(property);
            return property.KeyProvider.ToNullableValue(value);
        }

        /// <summary>
        /// 设置指定引用 id 属性对应的 id 的可空类型值。
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value">本方法为兼容值类型而使用。不论外键是否为值类型，都可以传入 null。</param>
        /// <param name="source"></param>
        /// <returns></returns>
        public object SetRefNullableId(IRefIdProperty property, object value, ManagedPropertyChangedSource source = ManagedPropertyChangedSource.FromProperty)
        {
            if (value == null) { value = GetEmptyIdForRefIdProperty(property); }
            var finalValue = this.SetRefId(property, value, source);
            return property.KeyProvider.ToNullableValue(finalValue);
        }

        /// <summary>
        /// 获取指定引用 id 属性对应的 id 的返回值。
        /// </summary>
        /// <param name="property"></param>
        /// <returns>如果 Id 是值类型，则这个函数的返回值不会是 null；如果是引用类型，则可能返回 null。</returns>
        public object GetRefId(IRefIdProperty property)
        {
            return this.GetProperty(property);
        }

        /// <summary>
        /// 设置指定引用 id 属性对应的 id 的值。
        /// 
        /// 在引用 id 变化时，会同步相应的引用实体属性。
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value">外键如果是值类型，则不能传入 null。</param>
        /// <param name="source"></param>
        /// <returns></returns>
        public object SetRefId(IRefIdProperty property, object value, ManagedPropertyChangedSource source = ManagedPropertyChangedSource.FromProperty)
        {
            //引用属性的托管属性类型是 object，这里需要强制为指定的主键类型。
            value = TypeHelper.CoerceValue(property.KeyProvider.KeyType, value);

            var id = this.GetRefId(property);

            //设置 id 完成后的值。
            object finalId = id;

            //确实需要改变 Id 时，才进行以下逻辑。
            if (!object.Equals(id, value))
            {
                var entityProperty = property.RefEntityProperty;

                //在设置 RefId 前先清空实体值，这样在回调 RefId 的外部 Changed 事件处理函数时，
                //外部看到的 RefEntity 也已经改变了，外部可以获得一致的 Entity 和 Id 值。
                var entity = base.GetProperty(entityProperty) as Entity;
                if (entity != null && !object.Equals(entity.Id, value)) { base.ResetProperty(entityProperty); }
                try
                {
                    //此时发生 OnIdChanged 事件。
                    finalId = base.SetProperty(property, value, source);
                }
                finally
                {
                    //还原实体的值。
                    if (entity != null) { base.LoadProperty(entityProperty, entity); }
                }

                //如果二者相等，表示 Id 成功设置，没有被 cancel。
                if (object.Equals(finalId, value))
                {
                    //如果之前的实体已经存在值，则需要设置为 null，并引发外部事件。
                    if (entity != null)
                    {
                        //重新设置 Entity 的值，此时发生 OnEntityChanged 事件。
                        var finalEntity = base.SetProperty(entityProperty, null, source) as Entity;

                        //如果外部事件取消了属性的设置，那么实际使用的实体将不会为 null，
                        if (finalEntity != null)
                        {
                            //此时，需要重设 Id 的值。
                            finalId = base.SetProperty(property, finalEntity.Id, source);
                            if (!object.Equals(finalId, finalEntity.Id))
                            {
                                ThrowRefPropertyChangingConflict(property);
                            }
                        }
                    }
                }
            }

            return finalId;
        }

        /// <summary>
        /// 以懒加载的方式获取某个引用实体的值。
        /// </summary>
        /// <typeparam name="TRefEntity"></typeparam>
        /// <param name="entityProperty"></param>
        /// <returns></returns>
        public TRefEntity GetRefEntity<TRefEntity>(RefEntityProperty<TRefEntity> entityProperty)
            where TRefEntity : Entity
        {
            return this.GetRefEntity(entityProperty as IRefEntityProperty) as TRefEntity;
        }

        /// <summary>
        /// 以懒加载的方式获取某个引用实体的值。
        /// </summary>
        /// <param name="entityProperty"></param>
        /// <returns></returns>
        public Entity GetRefEntity(IRefEntityProperty entityProperty)
        {
            var value = base.GetProperty(entityProperty) as Entity;

            if (!this._settingEntity && value == null)
            {
                var idProperty = entityProperty.RefIdProperty;
                var id = this.GetRefId(idProperty);
                if (HasRefId(idProperty, id))
                {
                    value = (entityProperty as IRefEntityPropertyInternal).Load(id, this);
                    if (value != null)
                    {
                        base.LoadProperty(entityProperty, value);
                    }
                }
            }

            return value;
        }

        /// <summary>
        /// 这个值用于判断是否正在处于设置Entity的状态中。
        /// 
        /// 当在外界设置Entity属性时，如果获取Entity属性，不需要引起延迟加载。
        /// </summary>
        [NonSerialized]
        private bool _settingEntity;

        /// <summary>
        /// 设置指定引用实体属性的值。
        /// 在实体属性变化时，会同步相应的引用 Id 属性。
        /// </summary>
        /// <param name="entityProperty">The entity property.</param>
        /// <param name="value">The value.</param>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public Entity SetRefEntity(IRefEntityProperty entityProperty, Entity value, ManagedPropertyChangedSource source = ManagedPropertyChangedSource.FromProperty)
        {
            var oldEntity = base.GetProperty(entityProperty) as Entity;
            Entity finalEntity = oldEntity;

            try
            {
                _settingEntity = true;

                var idProperty = entityProperty.RefIdProperty;
                var oldId = this.GetProperty(idProperty);

                //如果 实体变更 或者 （设置实体为 null 并且 id 不为 null），都需要设置值改变。
                if (oldEntity != value || (value == null && HasRefId(idProperty, oldId)))
                {
                    var newId = value == null ? GetEmptyIdForRefIdProperty(idProperty) : value.Id;

                    //在触发外界事件处理函数之前，先设置好 Id 的值
                    base.LoadProperty(idProperty, newId);
                    try
                    {
                        //此时再发生 OnEntityChanged 事件，外界可以获取到一致的 id 和 entity 值。
                        finalEntity = base.SetProperty(entityProperty, value, source) as Entity;
                    }
                    finally
                    {
                        //还原 id 的值。
                        base.LoadProperty(idProperty, oldId);
                    }

                    //如果设置实体成功，则需要开始变更 Id 的值。
                    if (finalEntity == value)
                    {
                        //如果 id 发生了变化，则需要设置 id 的值。
                        if (!object.Equals(oldId, newId))
                        {
                            //尝试设置 id 值，如果成功，则同时会发生 OnIdChanged 事件。
                            var finalId = base.SetProperty(idProperty, newId, source);

                            //如果设置 id 值失败，则应该还原 entity 的值。
                            if (!object.Equals(finalId, newId))
                            {
                                finalEntity = base.SetProperty(entityProperty, oldEntity, source) as Entity;

                                //还原 entity 值失败，向个界抛出冲突的异常。
                                if (finalEntity != oldEntity)
                                {
                                    ThrowRefPropertyChangingConflict(idProperty);
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                _settingEntity = false;
            }

            return finalEntity;
        }

        internal bool IsRefLoadedOrAssigned(IRefProperty refProperty)
        {
            var value = this.GetProperty(refProperty.RefEntityProperty);
            return value != null;
        }

        //protected void LoadRefProperty(IRefIdProperty refProperty)
        //{
        //    //暂时只是获取 Entity，以后可以使用其它方法优化此实现。
        //    var load = this.GetLazyRef(refProperty).Entity;
        //}

        /// <summary>
        /// 设置指定属性的值。
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public override object SetProperty(IManagedProperty property, object value, ManagedPropertyChangedSource source = ManagedPropertyChangedSource.FromProperty)
        {
            //防止外界使用 SetProperty 方法来操作引用属性。
            if (property is IRefProperty)
            {
                if (property is IRefIdProperty)
                {
                    return this.SetRefId(property as IRefIdProperty, value, source);
                }
                else
                {
                    return this.SetRefEntity(property as IRefEntityProperty, value as Entity, source);
                }
            }
            //防止外界使用 SetProperty 方法来操作列表属性。
            else if (property is IListProperty)
            {
                throw new InvalidOperationException(string.Format("{0} 是列表属性，不能使用 SetProperty 方法直接设置。请使用 GetLazyList 方法获取，或使用 LoadProperty 方法进行加载。", property));
            }
            else if (property == IdProperty || property == TreePIdProperty && value != null)
            {
                //由于 Id 属性的托管属性类型是 object，这里需要强制为具体的主键类型。
                value = TypeHelper.CoerceValue(this.IdProvider.KeyType, value);
            }

            return base.SetProperty(property, value, source);
        }

        private static void ThrowRefPropertyChangingConflict(IRefIdProperty property)
        {
            throw new InvalidOperationException(
                string.Format(@"{0} 属性的变更前事件与引用实体属性 {1} 的变更前事件设置的值冲突！",
                property.Name, property.RefEntityProperty.Name)
                );
        }

        #endregion

        #region 延迟加载 - LOB属性

        /// <summary>
        /// 获取指定的 LOB 属性的值。（懒加载）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="property"></param>
        /// <returns></returns>
        public T GetLOBProperty<T>(LOBProperty<T> property)
            where T : class
        {
            return this.GetLOBProperty(property as ILOBProperty) as T;
        }

        /// <summary>
        /// 获取指定的 LOB 属性的值。（懒加载）
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public object GetLOBProperty(ILOBProperty property)
        {
            if (!this.IsNew && !this.FieldExists(property))
            {
                var value = (property as ILOBPropertyInternal).LoadLOBValue(this.Id);
                base.LoadProperty(property, value);
                return value;
            }

            return base.GetProperty(property);
        }

        /// <summary>
        /// 设置 LOB 属性的值。
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        /// <param name="source"></param>
        public void SetLOBProperty(ILOBProperty property, object value, ManagedPropertyChangedSource source = ManagedPropertyChangedSource.FromProperty)
        {
            //直接调用父类的 SetProperty 方法，而不是本类重写的 SetProperty 方法。
            base.SetProperty(property, value, source);
        }

        #endregion

        /// <summary>
        /// 设置聚合关系中父对象的引用。
        /// </summary>
        /// <param name="parent"></param>
        public void SetParentEntity(Entity parent)
        {
            var property = this.GetRepository().FindParentPropertyInfo(true).ManagedProperty as IRefEntityProperty;
            this.SetRefEntity(property, parent);

            //由于新的父实体可以还没有 Id，这时需要主动通知冗余属性变更。
            //见测试：MPT_Redundancy_AddNewAggt
            if (parent != null && parent._status == PersistenceStatus.New)
            {
                this.NotifyIfInRedundancyPath(property.RefIdProperty as IProperty);
            }
        }

        /// <summary>
        /// 获取组合关系中父对象的引用。
        /// </summary>
        Entity IEntity.FindParentEntity()
        {
            var pMeta = this.GetRepository().FindParentPropertyInfo(false);
            if (pMeta != null) { return this.GetRefEntity(pMeta.ManagedProperty as IRefEntityProperty); }
            return null;
        }

        private static object GetEmptyIdForRefIdProperty(IRefIdProperty refIdProperty)
        {
            return refIdProperty.KeyProvider.GetEmptyIdForRefIdProperty();
        }

        private static bool HasRefId(IRefIdProperty refIdProperty, object id)
        {
            return refIdProperty.KeyProvider.IsAvailable(id);
        }

        ///// <summary>
        ///// 创建默认值
        ///// </summary>
        ///// <param name="property"></param>
        ///// <returns></returns>
        //private object GetDefaultValue(IManagedProperty property)
        //{
        //    var meta = property.GetMeta(this);

        //    var value = meta.DefaultValue;

        //    if (value == null)
        //    {
        //        var refIndicator = property as IRefProperty;
        //        if (refIndicator != null) value = refIndicator.CreateRef(this);
        //    }

        //    return value;
        //}
    }
}