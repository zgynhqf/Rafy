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
using OEA.ManagedProperty;
using OEA.MetaModel;
using OEA.MetaModel.Attributes;
using OEA.Utils;

namespace OEA.Library
{
    /// <summary>
    /// 懒加载引用实体的相关实现
    /// </summary>
    public partial class Entity : IReferenceOwner
    {
        #region 延迟加载 - 子集合

        /// <summary>
        /// 延迟加载子对象的集合。
        /// </summary>
        /// <typeparam name="TCollection">
        /// 子对象集合类型
        /// </typeparam>
        /// <param name="propertyInfo">当前属性的元信息</param>
        /// <param name="creator">构造一个新的子集合</param>
        /// <param name="childrenLoader">根据当前对象，查询出一个子集合</param>
        /// <returns></returns>
        protected TCollection GetLazyList<TCollection>(ListProperty<TCollection> propertyInfo)
            where TCollection : EntityList
        {
            return this.LoadLazyList(propertyInfo) as TCollection;
        }

        /// <summary>
        /// 延迟加载子对象的集合。
        /// </summary>
        /// <typeparam name="TCollection">
        /// 子对象集合类型
        /// </typeparam>
        /// <param name="propertyInfo">当前属性的元信息</param>
        /// <param name="creator">构造一个新的子集合</param>
        /// <param name="childrenLoader">根据当前对象，查询出一个子集合</param>
        /// <returns></returns>
        public EntityList GetLazyList(IListProperty listProperty)
        {
            return this.LoadLazyList(listProperty);
        }

        /// <summary>
        /// 执行懒加载操作。
        /// </summary>
        /// <param name="listProperty"></param>
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
                var listRepository = RepositoryFactoryHost.Factory.Create(listProperty.ListEntityType);
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
                    var listRepository = RepositoryFactoryHost.Factory.Create(listProperty.ListEntityType);
                    data = listRepository.GetByParent(this);
                }
            }

            this.LoadProperty(listProperty, data);

            return data;
        }

        #endregion

        #region 延迟加载 - 引用实体

        protected int GetRefId<TRefEntity>(RefProperty<TRefEntity> propertyIndicator)
            where TRefEntity : Entity
        {
            return this.GetLazyRef(propertyIndicator).Id;
        }

        protected int? GetRefNullableId<TRefEntity>(RefProperty<TRefEntity> propertyIndicator)
            where TRefEntity : Entity
        {
            return this.GetLazyRef(propertyIndicator).NullableId;
        }

        protected TRefEntity GetRefEntity<TRefEntity>(RefProperty<TRefEntity> propertyIndicator)
            where TRefEntity : Entity
        {
            return this.GetLazyRef(propertyIndicator).Entity;
        }

        protected void SetRefId<TRefEntity>(RefProperty<TRefEntity> propertyIndicator, int value)
            where TRefEntity : Entity
        {
            this.GetLazyRef(propertyIndicator).Id = value;
        }

        protected void SetRefNullableId<TRefEntity>(RefProperty<TRefEntity> propertyIndicator, int? value)
            where TRefEntity : Entity
        {
            this.GetLazyRef(propertyIndicator).NullableId = value;
        }

        protected void SetRefEntity<TRefEntity>(RefProperty<TRefEntity> propertyIndicator, TRefEntity value)
            where TRefEntity : Entity
        {
            this.GetLazyRef(propertyIndicator).Entity = value;
        }

        protected ILazyEntityRef<TRefEntity> GetLazyRef<TRefEntity>(RefProperty<TRefEntity> propertyIndicator)
            where TRefEntity : Entity
        {
            var myRef = this.GetProperty(propertyIndicator);

            if (myRef == null)
            {
                myRef = propertyIndicator.CreateRef(this);
                this.LoadProperty(propertyIndicator, myRef);
            }

            return myRef;
        }

        public ILazyEntityRef GetLazyRef(IRefProperty propertyIndicator)
        {
            var myRef = this.GetProperty(propertyIndicator) as ILazyEntityRef;

            if (myRef == null)
            {
                myRef = propertyIndicator.CreateRef(this);
                this.LoadProperty(propertyIndicator, myRef);
            }

            return myRef;
        }

        #endregion

        #region IReferenceOwner Members

        bool IReferenceOwner.NotifyIdChanging(LazyEntityRefPropertyInfo refInfo, int newId)
        {
            var meta = refInfo.CorrespondingRefPropertyMeta(this);
            if (meta != null)
            {
                var callBack = meta.IdChangingCallBack;
                if (callBack != null)
                {
                    var e = new RefIdChangingEventArgs(newId);
                    callBack(this, e);
                    return e.Cancel;
                };
            }

            return false;
        }

        bool IReferenceOwner.NotifyEntityChanging(LazyEntityRefPropertyInfo refInfo, Entity newEntity)
        {
            var meta = refInfo.CorrespondingRefPropertyMeta(this);
            if (meta != null)
            {
                var callBack = meta.EntityChangingCallBack;
                if (callBack != null)
                {
                    var e = new RefEntityChangingEventArgs(newEntity);
                    callBack(this, e);
                    return e.Cancel;
                };
            }

            return false;
        }

        /// <summary>
        /// “私有”实现此接口
        /// 外键Id改变时，需要设置Dirty、引发事件 等。
        /// </summary>
        /// <param name="refType"></param>
        /// <param name="refIdProperty"></param>
        void IReferenceOwner.NotifyIdChanged(LazyEntityRefPropertyInfo refInfo, int oldId, int newId)
        {
            this.MarkModified();

            var mp = refInfo.CorrespondingRefProperty(this);
            if (mp != null)
            {
                var meta = mp.GetMeta(this) as IRefPropertyMetadata;
                var callBack = meta.IdChangedCallBack;
                if (callBack != null) callBack(this, new RefIdChangedEventArgs(oldId, newId));

                this.OnPropertyChanged(refInfo.IdProperty);

                //id 变化的时候，直接看作是 RefProperty 在改变。
                this.OnPropertyChanged(mp);
            }
            else
            {
                this.OnPropertyChanged(refInfo.IdProperty);
            }
        }

        /// <summary>
        /// “私有”实现此接口
        /// 外键实体改变时，只需要引发事件即可。
        /// </summary>
        /// <param name="refType"></param>
        /// <param name="refEntityProperty"></param>
        void IReferenceOwner.NotifyEntityChanged(LazyEntityRefPropertyInfo refInfo, Entity oldEntity, Entity newEntity)
        {
            if (refInfo.NotifyRefEntityChanged)
            {
                var meta = refInfo.CorrespondingRefPropertyMeta(this);
                if (meta != null)
                {
                    var callBack = meta.EntityChangedCallBack;
                    if (callBack != null) callBack(this, new RefEntityChangedEventArgs(oldEntity, newEntity));
                }

                this.OnPropertyChanged(refInfo.RefEntityProperty);
            }
        }

        #endregion

        /// <summary>
        /// 设置聚合关系中父对象的引用。
        /// </summary>
        /// <param name="parent"></param>
        public void SetParentEntity(Entity parent)
        {
            var property = this.GetRepository().FindParentPropertyInfo(true).ManagedProperty as IRefProperty;
            this.GetLazyRef(property).Entity = parent;
        }

        /// <summary>
        /// 获取聚合关系中父对象的引用。
        /// </summary>
        public Entity FindParentEntity()
        {
            var pMeta = this.GetRepository().FindParentPropertyInfo(false);
            if (pMeta != null) { return this.GetLazyRef(pMeta.ManagedProperty as IRefProperty).Entity; }
            return null;
        }

        /// <summary>
        /// 复制某个属性
        /// </summary>
        /// <param name="target">从这个对象拷贝</param>
        /// <param name="property">拷贝这个属性</param>
        private void CopyProperty(Entity target, IManagedProperty property, CloneOptions options)
        {
            if (property.IsReadOnly) { return; }

            var refIndicator = property as IRefProperty;
            if (refIndicator != null)
            {
                var targetRef = target.GetProperty(property) as ILazyEntityRef;
                if (targetRef == null) return;

                var myRef = this.GetLazyRef(refIndicator);
                if (myRef == null) return;

                var meta = refIndicator.GetMeta(this) as IRefPropertyMetadata;

                bool copyEntity = meta.ReferenceType == ReferenceType.Parent ?
                    options.HasAction(CloneActions.ParentRefEntity) :
                    options.HasAction(CloneActions.RefEntities);

                myRef.Clone(targetRef, copyEntity);
            }
            else
            {
                this.LoadProperty(property, target.GetProperty(property));
            }
        }

        protected void CopyProperty(Entity target, IManagedProperty property)
        {
            if (property.IsReadOnly) { return; }

            this.LoadProperty(property, target.GetProperty(property));
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