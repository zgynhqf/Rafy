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
        /// <param name="loadEmptyData">The list property.</param>
        /// <returns></returns>
        private EntityList LoadLazyList(IListProperty listProperty, bool loadEmptyData = false)
        {
            EntityList data = null;

            if (this.HasLocalValue(listProperty))
            {
                data = this.GetProperty(listProperty) as EntityList;
                if (data != null) return data;
            }

            if (this.IsNew || loadEmptyData)
            {
                var listRepository = RepositoryFactoryHost.Factory.FindByEntity(listProperty.ListEntityType, true);
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
                    var listRepository = RepositoryFactoryHost.Factory.FindByEntity(listProperty.ListEntityType, true) as IRepositoryInternal;
                    data = listRepository.GetLazyListByParent(this);
                }
            }

            this.LoadProperty(listProperty, data);

            return data;
        }

        #endregion

        #region 延迟加载 - 引用实体

        #region 考虑到未来可能会还需要获取可空值，这些方法暂时不能删除。

        /// <summary>
        /// 获取引用键属性的可空值。
        /// </summary>
        /// <param name="refProperty"></param>
        /// <returns></returns>
        internal object GetRefNullableKey(IManagedProperty refProperty)
        {
            var value = this.GetRefKey(refProperty);
            return value;
            //return refProperty.KeyProvider.ToNullableValue(value);
        }

        /// <summary>
        /// 设置引用键属性的可空值。
        /// </summary>
        /// <param name="refProperty"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        internal object SetRefNullableKey(IManagedProperty refProperty, object value)
        {
            return this.SetRefKey(refProperty, value, true);
            //允许
            //if (value == null) { value = GetEmptyKeyForRefKeyProperty(refProperty); }
            //var finalValue = this.SetRefKey(refProperty, value, true);
            //return refProperty.KeyProvider.ToNullableValue(finalValue);
        }

        #endregion

        /// <summary>
        /// 获取指定引用属性对应的键的值。
        /// </summary>
        /// <param name="refProperty"></param>
        /// <returns></returns>
        public object GetRefKey(IManagedProperty refProperty)
        {
            var value = this.GetProperty((refProperty as IRefProperty)?.RefKeyProperty ?? refProperty);
            return value;
        }

        /// <summary>
        /// 设置指定引用属性对应的键属性的值。
        /// 
        /// 在引用键变化时，会同步相应的引用实体属性。
        /// </summary>
        /// <param name="refProperty">引用属性。</param>
        /// <param name="value"></param>
        /// <returns></returns>
        public object SetRefKey(IManagedProperty refProperty, object value)
        {
            return this.SetRefKey(refProperty, value, true);
        }

        /// <summary>
        /// 设置指定引用属性对应的键属性的值。
        /// 
        /// 在引用键变化时，会同步相应的引用实体属性。
        /// </summary>
        /// <param name="refProperty">引用属性。</param>
        /// <param name="value"></param>
        /// <param name="resetDisabledStatus"></param>
        /// <returns></returns>
        public object SetRefKey(IManagedProperty refProperty, object value, bool resetDisabledStatus)
        {
            var keyP = (refProperty as IRefProperty)?.RefKeyProperty ?? refProperty;
            return this.SetRefKey(keyP as IManagedPropertyInternal, value, resetDisabledStatus);
        }

        private object SetRefKey(IManagedPropertyInternal keyProperty, object value, bool resetDisabledStatus)
        {
            //引用属性的托管属性类型是 object，这里需要强制为指定的主键类型。
            var entityProperty = keyProperty.RefEntityProperty;

            value = TypeHelper.CoerceValue(entityProperty.RefKeyProperty.PropertyType, value);
            if (entityProperty.Nullable)
            {
                value = entityProperty.KeyProvider.ToNullableValue(value);
            }

            var oldKey = this.GetProperty(keyProperty);

            //设置 id 完成后的值。
            object finalKey = oldKey;

            //确实需要改变 Id 时，才进行以下逻辑。
            if (!object.Equals(oldKey, value))
            {
                //在设置 RefId 前先清空实体值，这样在回调 RefId 的外部 Changed 事件处理函数时，
                //外部看到的 RefEntity 也已经改变了，外部可以获得一致的 Entity 和 Id 值。
                var oldEntity = base.GetProperty(entityProperty) as Entity;
                var needResetEntity = oldEntity != null && !object.Equals(oldEntity.GetProperty(entityProperty.KeyPropertyOfRefEntity), value);
                if (needResetEntity)
                {
                    base.ResetProperty(entityProperty);
                }

                try
                {
                    //此时发生 OnIdChanged 事件。
                    finalKey = base.SetProperty(keyProperty, value, resetDisabledStatus);
                }
                finally
                {
                    //还原实体的值。
                    if (oldEntity != null) { base.LoadProperty(entityProperty, oldEntity); }
                }

                //如果二者相等，表示 Id 成功设置，没有被 cancel。
                if (object.Equals(finalKey, value))
                {
                    //如果旧实体的键值不同，则需要设置为 null，并引发外部事件。
                    if (needResetEntity)
                    {
                        //重新设置 Entity 的值，此时发生 OnEntityChanged 事件。
                        var finalEntity = base.SetProperty(entityProperty, null) as Entity;

                        //如果外部事件取消了属性的设置，那么实际使用的实体将不会为 null，
                        if (finalEntity != null)
                        {
                            //此时，需要重设 Id 的值。
                            var finalEntityKey = finalEntity.GetProperty(entityProperty.KeyPropertyOfRefEntity);
                            finalKey = base.SetProperty(keyProperty, finalEntityKey);
                            if (!object.Equals(finalKey, finalEntityKey))
                            {
                                ThrowRefPropertyChangingConflict(keyProperty, entityProperty);
                            }
                        }
                    }
                }
            }

            return finalKey;
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
            return this.GetRefEntity(entityProperty as IRefProperty) as TRefEntity;
        }

        /// <summary>
        /// 以懒加载的方式获取某个引用实体的值。
        /// </summary>
        /// <param name="entityProperty"></param>
        /// <returns></returns>
        public Entity GetRefEntity(IRefProperty entityProperty)
        {
            var value = base.GetProperty(entityProperty) as Entity;

            if (!this._settingEntity && value == null)
            {
                var keyValue = this.GetProperty(entityProperty.RefKeyProperty);
                if (entityProperty.KeyProvider.IsAvailable(keyValue))
                {
                    value = (entityProperty as IRefEntityPropertyInternal).Load(keyValue, this);
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
        /// <returns></returns>
        public Entity SetRefEntity(IRefProperty entityProperty, Entity value)
        {
            var oldEntity = base.GetProperty(entityProperty) as Entity;
            Entity finalEntity = oldEntity;

            try
            {
                _settingEntity = true;

                var keyProperty = entityProperty.RefKeyProperty;
                var oldKey = this.GetProperty(keyProperty);

                //如果 实体变更 或者 （设置实体为 null 并且 key 不为 null），都需要设置值改变。
                if (oldEntity != value || (value == null && entityProperty.KeyProvider.IsAvailable(oldKey)))
                {
                    //从引用实体中加载引用键的值。
                    var newKey = value == null ? entityProperty.KeyProvider.GetEmptyId() : value.GetProperty(entityProperty.KeyPropertyOfRefEntity);
                    if (entityProperty.Nullable)
                    {
                        newKey = entityProperty.KeyProvider.ToNullableValue(newKey);
                    }

                    //在触发外界事件处理函数之前，先设置好 key 的值
                    base.LoadProperty(keyProperty, newKey);
                    try
                    {
                        //此时再发生 OnEntityChanged 事件，外界可以获取到一致的 key 和 entity 值。
                        finalEntity = base.SetProperty(entityProperty, value) as Entity;
                    }
                    finally
                    {
                        //还原 key 的值。
                        base.LoadProperty(keyProperty, oldKey);
                    }

                    //如果设置实体成功，则需要开始变更 Id 的值。
                    if (finalEntity == value)
                    {
                        //如果 id 发生了变化，则需要设置 id 的值。
                        if (!object.Equals(oldKey, newKey))
                        {
                            //尝试设置 id 值，如果成功，则同时会发生 OnIdChanged 事件。
                            var finalKey = base.SetProperty(keyProperty, newKey);

                            //如果设置 id 值失败，则应该还原 entity 的值。
                            if (!object.Equals(finalKey, newKey))
                            {
                                finalEntity = base.SetProperty(entityProperty, oldEntity) as Entity;

                                //还原 entity 值失败，向个界抛出冲突的异常。
                                if (finalEntity != oldEntity)
                                {
                                    ThrowRefPropertyChangingConflict(keyProperty, entityProperty);
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
        /// <param name="resetDisabledStatus">如果本字段处于禁用状态，那么是否在设置新值时，将禁用状态解除？</param>
        /// <returns></returns>
        public override object SetProperty(IManagedProperty property, object value, bool resetDisabledStatus)
        {
            //为了提升性能，这里面的判断不再需要。
            ////防止外界使用 SetProperty 方法来操作列表属性。
            //if (property is IListProperty)
            //{
            //    throw new InvalidOperationException($"{property} 是列表属性，不能使用 SetProperty 方法直接设置。请使用 GetLazyList 方法获取，或使用 LoadProperty 方法进行加载。");
            //}

            if ((property == IdProperty || property == TreePIdProperty) && value != null)
            {
                //由于 Id 属性的托管属性类型是 object，这里需要强制为具体的主键类型。
                value = TypeHelper.CoerceValue(this.IdProvider.KeyType, value);
            }

            //如果是引用值属性，则需要调用 SetRefKey 方法。
            var propertyInternal = property as IManagedPropertyInternal;
            if (propertyInternal.RefEntityProperty != null)
            {
                return this.SetRefKey(propertyInternal, value, resetDisabledStatus);
            }

            return base.SetProperty(property, value, resetDisabledStatus);
        }

        private static void ThrowRefPropertyChangingConflict(IManagedProperty keyProperty, IRefProperty entityProperty)
        {
            throw new InvalidOperationException(
                string.Format(@"{0} 属性的变更前事件与引用实体属性 {1} 的变更前事件设置的值冲突！",
                keyProperty.Name, entityProperty.Name)
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
            if (!this.IsNew && !this.HasLocalValue(property))
            {
                var id = this.Id;
                if (!this.IdProvider.IsAvailable(id)) return null;

                var repo = this.GetRepository();
                var value = repo.GetEntityValue(id, property);
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
        public void SetLOBProperty(ILOBProperty property, object value)
        {
            //直接调用父类的 SetProperty 方法，而不是本类重写的 SetProperty 方法。
            base.SetProperty(property, value);
        }

        internal override void DisableCore(IManagedProperty property, bool value = true)
        {
            switch ((property as IProperty).Category)
            {
                case PropertyCategory.ReferenceEntity:
                case PropertyCategory.List:
                case PropertyCategory.LOB:
                case PropertyCategory.Readonly:
                    //以上三类属性，在禁用时，都需要被忽略。
                    break;
                default:
                    base.DisableCore(property, value);
                    break;
            }
        }

        #endregion

        /// <summary>
        /// 设置聚合关系中父对象的引用。
        /// </summary>
        /// <param name="parent"></param>
        public virtual void SetParentEntity(Entity parent)
        {
            var property = this.GetRepository().EntityMeta
                .FindParentReferenceProperty(true).ManagedProperty as IRefProperty;
            this.SetParentEntity(parent, property);
        }

        internal void SetParentEntity(Entity parent, IRefProperty parentRefProperty)
        {
            //由于有时父引用实体没有发生改变，但是父引用实体的 Id 变了，此时也可以调用此方法同步二者的 Id。
            //例如：保存父实体后，它的 Id 生成了。这时会调用此方法来同步 Id。
            this.SetRefNullableKey(parentRefProperty, parent.GetProperty(parentRefProperty.KeyPropertyOfRefEntity));
            this.SetRefEntity(parentRefProperty, parent);

            //由于新的父实体可以还没有 Id，这时需要主动通知冗余属性变更。
            //见测试：MPT_Redundancy_AddNewAggt
            if (parent != null && parent.PersistenceStatus == PersistenceStatus.New)
            {
                this.NotifyIfInRedundancyPath(parentRefProperty.RefKeyProperty as IProperty);
            }
        }

        /// <summary>
        /// 获取组合关系中父对象的引用。
        /// </summary>
        Entity IEntity.FindParentEntity()
        {
            var pMeta = this.GetRepository().EntityMeta.FindParentReferenceProperty();
            if (pMeta != null) { return this.GetRefEntity(pMeta.ManagedProperty as IRefProperty); }
            return null;
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
        //        var refIndicator = RefPropertyHelper.Find(property);
        //        if (refIndicator != null) value = refIndicator.CreateRef(this);
        //    }

        //    return value;
        //}
    }
}