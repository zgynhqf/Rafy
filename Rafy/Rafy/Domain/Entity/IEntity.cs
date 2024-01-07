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
        /// 实体所在的当前列表对象。 虽然一个实体可以存在于多个集合中，但是，它只保留一个主要集合的引用，见：<see cref="EntityList{TEntity}.ResetItemParent"/>。
        /// </summary>
        IEntityList ParentList { get; }

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
        IEntityList GetChildProperty<TChild>();
    }

    /// <summary>
    /// 实体列表
    /// </summary>
    public interface IEntityList : IList<Entity>, IList, IDomainComponent, ITreeComponent
    {
        /// <summary>
        /// 是否：在添加每一项时，
        /// * 设置：实体的 <see cref="IEntity.ParentList"/> 为当前列表；
        /// * 设置它的父对象为本列表对象的父对象。
        /// 
        /// 此属性大部分情况下，应该都是返回 true。
        /// </summary>
        bool ResetItemParent { get; set; }

        /// <summary>
        /// 查询出来的当前列表在数据库中存在的总数据条数。
        /// 
        /// 一是用于统计数据条数查询的数据传输。
        /// 二是是分页时保存所有数据的行数。
        /// </summary>
        long TotalCount { get; }

        /// <summary>
        /// 本列表中已经被移除的所有对象。
        /// 这些对象将会从仓库中删除。
        /// </summary>
        IList<Entity> DeletedList { get; }

        /// <summary>
        /// 如果支持树型操作，需要重写 TreeId、OrderNo。
        /// </summary>
        bool SupportTree { get; }

        /// <summary>
        /// 是否启用在改变列表元素时，自动生成元素的树型索引功能。
        /// 默认为 true。
        /// </summary>
        bool AutoTreeIndexEnabled { get; set; }

        /// <summary>
        /// 返回当前的这个列表是否作为树结构中的根节点的集合。
        /// 注意，如果集合中没有元素时，同样会返回 false。
        /// 
        /// 如果本属性是 false 时，那么 IEntityList 中与树相关的功能都不再可用。
        /// </summary>
        bool IsTreeRootList { get; }

        /// <summary>
        /// 当查询 Count 时，调用此方法设置最终查询出的总条数。
        /// </summary>
        /// <param name="value"></param>
        void SetTotalCount(long value);

        /// <summary>
        /// 通过 Id 来查找某个实体。
        /// </summary>
        /// <param name="id">需要查找的实体的 id 值。</param>
        /// <param name="coreceType">
        /// 如果传入的 id 的类型与实体的类型不一致，则强制转换为一致的类型。
        /// 如果不确定传入的类型是否一致，则可以指定这个参数为 true。
        /// </param>
        /// <returns></returns>
        Entity Find(object id, bool coreceType = false);

        /// <summary>
        /// 复制目标集合中的所有对象。
        /// * 根据列表中的位置来进行拷贝；
        /// * 多余的会移除（不进入 DeletedList）；
        /// * 不够的会生成新的对象；（如果是新构建的实体，持久化状态也完全拷贝。）
        /// </summary>
        /// <param name="sourceList"></param>
        /// <param name="options"></param>
        void Clone(IEntityList sourceList, CloneOptions options);

        /// <summary>
        /// 添加一组实体到列表中。
        /// </summary>
        /// <param name="collection"></param>
        void AddRange(IEnumerable<Entity> collection);

        /// <summary>
        /// 如果当前集合是一个根节点的集合，那么可以使用此方法来重新生成树中所有节点的索引。
        /// </summary>
        void ResetTreeIndex();

        #region 解决 IList<Entity> 与 IList 的冲突

        new int Count { get; }

        new void Clear();

        new Entity this[int index] { get; set; }

        #endregion
    }

    internal interface IEntityListInternal : IEntityList
    {
        ManagedPropertyObjectList<Entity> DeletedListField { get; set; }
        void LoadData(IEnumerable srcList);
        void SetRepo(IRepository repository);
        void InitListProperty(IListProperty value);
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
        /// 清空实体的 IsDeleted 状态，使其还原到删除之前的状态。
        /// </summary>
        void RevertDeletedStatus();

        ///// <summary>
        ///// 清空实体的 IsNew 状态，使其还原到之前的状态。
        ///// </summary>
        //void RevertNewStatus();
    }

    /// <summary>
    /// 一个拥有许多自定义托管属性的实体
    /// </summary>
    public interface IEntityWithManagedProperties : IManagedPropertyObject
    {
        #region 延迟加载 - 引用实体

        /// <summary>
        /// 获取指定引用属性对应的键的值。
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        object GetRefKey(IManagedProperty property);

        /// <summary>
        /// 设置指定引用属性对应的键属性的值。
        /// 
        /// 在引用键变化时，会同步相应的引用实体属性。
        /// </summary>
        /// <param name="refProperty">引用属性。</param>
        /// <param name="value"></param>
        /// <returns></returns>
        object SetRefKey(IManagedProperty refProperty, object value);

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
        /// <param name="refProperty"></param>
        /// <returns></returns>
        Entity GetRefEntity(IRefProperty refProperty);

        /// <summary>
        /// 设置指定引用实体属性的值。
        /// 在实体属性变化时，会同步相应的引用 Id 属性。
        /// </summary>
        /// <param name="refProperty">The entity property.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        Entity SetRefEntity(IRefProperty refProperty, Entity value);

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
        /// 
        void SetLOBProperty(ILOBProperty property, object value);

        #endregion

        /// <summary>
        /// 延迟加载子对象的集合。
        /// </summary>
        /// <param name="listProperty">The list property.</param>
        /// <returns></returns>
        IEntityList GetLazyList(IListProperty listProperty);
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