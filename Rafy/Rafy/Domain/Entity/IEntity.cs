using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Rafy.MetaModel;
using System.Collections;
using Rafy.ManagedProperty;

namespace Rafy.Domain
{
    /// <summary>
    /// 实体
    /// </summary>
    public interface IEntity : IManagedPropertyObject, IDomainComponent, IEntityWithId, IEntityWithStatus, IEntityWithManagedProperties, IClonableEntity
    {
        /// <summary>
        /// 实体所在的当前列表对象。 虽然一个实体可以存在于多个集合中，但是，它只保留一个主要集合的引用，见：<see cref="EntityList.ResetItemParent"/>。
        /// </summary>
        EntityList ParentList { get; }

        /// <summary>
        /// 获取组合关系中父对象的引用。
        /// </summary>
        Entity FindParentEntity();

        /// <summary>
        /// 获取所有已经加载的组合子的字段集合。
        /// 
        /// 返回的字段的值必须是 IEntityOrList 类型。
        /// 子有可能是集合、也有可能只是一个单一的实体。只会是这两种情况。
        /// </summary>
        /// <returns></returns>
        Entity.LoadedChildrenEnumerator GetLoadedChildren();

        /// <summary>
        /// 根据孩子类型，直接获取孩子列表。
        /// </summary>
        /// <typeparam name="TChild"></typeparam>
        /// <returns></returns>
        EntityList GetChildProperty<TChild>();
    }

    /// <summary>
    /// 实体列表
    /// </summary>
    public interface IEntityList : IList<Entity>, IList, IDomainComponent
    {
        #region 解决 IList<Entity> 与 IList 的冲突

        new int Count { get; }

        new Entity this[int index] { get; set; }

        #endregion
    }

    /// <summary>
    /// 一个能控制状态的实体。
    /// </summary>
    public interface IEntityWithStatus : IDirtyAware
    {
        /// <summary>
        /// 获取或设置实体当前的持久化状态。
        /// 保存实体时，会根据这个状态来进行对应的增、删、改的操作。
        /// </summary>
        PersistenceStatus PersistenceStatus { get; set; }

        /// <summary>
        /// 标记当前对象为需要保存的状态。
        /// 
        /// 只有实体的状态是 Unchanged 状态时（其它状态已经算是 Dirty 了），调用本方法才会把实体的状态改为 Modified。
        /// </summary>
        void MarkModifiedIfUnchanged();

        /// <summary>
        /// 清空实体的 IsDeleted 状态，使其还原到删除之前的状态。
        /// </summary>
        void RevertDeletedStatus();

        /// <summary>
        /// 清空实体的 IsNew 状态，使其还原到之前的状态。
        /// </summary>
        void RevertNewStatus();
    }

    /// <summary>
    /// 一个拥有许多自定义托管属性的实体
    /// </summary>
    public interface IEntityWithManagedProperties : IManagedPropertyObject
    {
        #region 延迟加载 - 引用实体

        /// <summary>
        /// 获取指定引用 id 属性对应的 id 的可空类型返回值。
        /// </summary>
        /// <param name="property"></param>
        /// <returns>本方法为兼容值类型而使用。不论 Id 是值类型、还是引用类型，都可能返回 null。</returns>
        object GetRefNullableId(IRefIdProperty property);

        /// <summary>
        /// 设置指定引用 id 属性对应的 id 的可空类型值。
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value">本方法为兼容值类型而使用。不论外键是否为值类型，都可以传入 null。</param>
        /// <param name="source"></param>
        /// <returns></returns>
        object SetRefNullableId(IRefIdProperty property, object value, ManagedPropertyChangedSource source = ManagedPropertyChangedSource.FromProperty);

        /// <summary>
        /// 获取指定引用 id 属性对应的 id 的返回值。
        /// </summary>
        /// <param name="property"></param>
        /// <returns>如果 Id 是值类型，则这个函数的返回值不会是 null；如果是引用类型，则可能返回 null。</returns>
        object GetRefId(IRefIdProperty property);

        /// <summary>
        /// 设置指定引用 id 属性对应的 id 的值。
        /// 
        /// 在引用 id 变化时，会同步相应的引用实体属性。
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value">外键如果是值类型，则不能传入 null。</param>
        /// <param name="source"></param>
        /// <returns></returns>
        object SetRefId(IRefIdProperty property, object value, ManagedPropertyChangedSource source = ManagedPropertyChangedSource.FromProperty);

        /// <summary>
        /// 以懒加载的方式获取某个引用实体的值。
        /// </summary>
        /// <typeparam name="TRefEntity"></typeparam>
        /// <param name="entityProperty"></param>
        /// <returns></returns>
        TRefEntity GetRefEntity<TRefEntity>(RefEntityProperty<TRefEntity> entityProperty) where TRefEntity : Entity;

        /// <summary>
        /// 以懒加载的方式获取某个引用实体的值。
        /// </summary>
        /// <param name="entityProperty"></param>
        /// <returns></returns>
        Entity GetRefEntity(IRefEntityProperty entityProperty);

        /// <summary>
        /// 设置指定引用实体属性的值。
        /// 在实体属性变化时，会同步相应的引用 Id 属性。
        /// </summary>
        /// <param name="entityProperty">The entity property.</param>
        /// <param name="value">The value.</param>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        Entity SetRefEntity(IRefEntityProperty entityProperty, Entity value, ManagedPropertyChangedSource source = ManagedPropertyChangedSource.FromProperty);

        #endregion

        #region 延迟加载 - LOB属性

        /// <summary>
        /// 获取指定的 LOB 属性的值。（懒加载）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="property"></param>
        /// <returns></returns>
        T GetLOBProperty<T>(LOBProperty<T> property)
            where T : class;

        /// <summary>
        /// 获取指定的 LOB 属性的值。（懒加载）
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        object GetLOBProperty(ILOBProperty property);

        /// <summary>
        /// 设置 LOB 属性的值。
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        /// <param name="source"></param>
        void SetLOBProperty(ILOBProperty property, object value, ManagedPropertyChangedSource source = ManagedPropertyChangedSource.FromProperty);

        #endregion

        /// <summary>
        /// 延迟加载子对象的集合。
        /// </summary>
        /// <param name="listProperty">The list property.</param>
        /// <returns></returns>
        EntityList GetLazyList(IListProperty listProperty);
    }

    /// <summary>
    /// 一个可克隆的实体。
    /// </summary>
    public interface IClonableEntity
    {
        /// <summary>
        /// 使用 <see cref="CloneOptions.NewSingleEntity"/> 来复制目标对象。
        /// </summary>
        /// <param name="source"></param>
        void Clone(Entity source);

        /// <summary>
        /// 使用指定的复制条件来复制源对象的值。
        /// </summary>
        /// <param name="source"></param>
        /// <param name="options"></param>
        void Clone(Entity source, CloneOptions options);
    }
}