/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20100330
 * 说明：所有实体集合类的基类
 * 运行环境：.NET 3.5 SP1
 * 版本号：1.0.3
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100330
 * 添加EntityTreeList和EntityList，并实现ReadFromTable方法 胡庆访 20100402
 * 添加两个非泛型的列表基类 胡庆访 20100920
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Rafy;
using Rafy.Domain.Caching;
using Rafy.ManagedProperty;
using Rafy.MetaModel;

namespace Rafy.Domain
{
    /// <summary>
    /// 所有实体集合类的基类。
    /// 
    /// 仓库的所有数据查询，都是通过 EntityList 来完成的。包括：FetchCount、FetchFirst、FetchList。
    /// </summary>
    [Serializable]
    public abstract partial class EntityList : ManagedPropertyObjectList<Entity>,
        IEntityList, IDirtyAware, IDomainComponent
    {
        #region FindRepository

        [NonSerialized]
        private bool _repositoryLoaded;

        [NonSerialized]
        private IRepository _repository;

        /// <summary>
        /// 尝试找到这个实体列表对应的仓库类。
        /// 
        /// 没有标记 RootEntity/ChildEntity 的类型是没有仓库类的，例如所有的条件类型。
        /// </summary>
        /// <returns></returns>
        public IRepository FindRepository()
        {
            if (!this._repositoryLoaded)
            {
                var entityType = EntityMatrix.FindByList(this.GetType()).EntityType;
                this._repository = RepositoryFactoryHost.Factory.FindByEntity(entityType);

                this._repositoryLoaded = true;
            }

            return this._repository;
        }

        /// <summary>
        /// 获取该实体列表对应的仓库类，如果没有找到，则抛出异常。
        /// </summary>
        /// <returns></returns>
        public IRepository GetRepository()
        {
            var repo = this.FindRepository();
            if (repo == null) throw new InvalidProgramException(string.Format("类型 {0} 没有对应的仓库类，无法使用此方法。", this.EntityType.Name));

            return repo;
        }

        #endregion

        #region FindListProperty

        [NonSerialized]
        private IListProperty _listProperty;
        [NonSerialized]
        private HasManyType? _listHasManyType;

        internal void InitListProperty(IListProperty value)
        {
            this._listProperty = value;
        }

        /// <summary>
        /// 查找本实体对应的列表属性
        /// 
        /// 以下情况下返回 null：
        /// * 这是一个根对象的集合。
        /// * 这是一个子对象的集合，但是这个集合不在根对象聚合树中。（没有 this.Parent 属性。）
        /// </summary>
        /// <returns></returns>
        private IListProperty FindListProperty()
        {
            if (this._listProperty == null)
            {
                var parentEntity = this.Parent;
                if (parentEntity != null)
                {
                    var enumerator = parentEntity.GetLoadedChildren();
                    while (enumerator.MoveNext())
                    {
                        var current = enumerator.Current;
                        if (current.Value == this)
                        {
                            _listProperty = current.Property as IListProperty;
                            break;
                        }
                    }
                }
            }

            return this._listProperty;
        }

        /// <summary>
        /// 当前集合的一对多类型
        /// </summary>
        internal HasManyType HasManyType
        {
            get
            {
                if (this._listHasManyType == null)
                {
                    var parentEntity = this.Parent;
                    if (parentEntity != null)
                    {
                        var listProperty = this.FindListProperty();
                        if (listProperty != null)
                        {
                            this._listHasManyType = listProperty.HasManyType;
                        }
                    }
                }

                return this._listHasManyType.GetValueOrDefault(HasManyType.Aggregation);
            }
        }

        #endregion

        /// <summary>
        /// 是否需要关闭此行为：
        /// 在添加每一项时，设置父列表为当前列表，并设置它的父对象为本列表对象的父对象。
        /// </summary>
        public bool SupressSetItemParent { get; set; }

        #region Insert, Remove, Clear

        #region public IDisposable MovingItems

        [NonSerialized]
        private bool _movingItems;

        /// <summary>
        /// 设置本列表是否正在调整元素的位置。
        /// 为使得排序性能更好，可使用此方法声明一个只变更元素顺序的代码块，
        /// 此代码块中，列表会认为所有的 Remove、Add、Set 操作，都只是在维护元素顺序，而不会添加新元素、删除旧元素。
        /// </summary>
        /// <returns></returns>
        public IDisposable MovingItems()
        {
            _movingItems = true;
            return new MovingItemsDisposable { Owner = this };
        }

        private class MovingItemsDisposable : IDisposable
        {
            internal EntityList Owner;
            public void Dispose()
            {
                Owner._movingItems = false;
                Owner.OnTreeItemsMoved();
            }
        }

        #endregion

        /// <summary>
        /// 清空所有项。
        /// </summary>
        protected override void ClearItems()
        {
            if (!_movingItems)
            {
                for (int i = 0, c = this.Count; i < c; i++)
                {
                    var item = this[i];
                    this.OnItemRemoving(i, item);
                }
            }

            base.ClearItems();
        }

        /// <summary>
        /// Sets the item.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="item">The item.</param>
        protected override void SetItem(int index, Entity item)
        {
            if (!_movingItems)
            {
                var toDelete = this[index];
                if (toDelete != item)
                {
                    this.OnItemRemoving(index, toDelete);
                    this.OnItemAdding(index, item);

                    base.SetItem(index, item);

                    this.OnItemRemoved(index, item);
                    this.OnItemAdded(index, item);
                }
            }
            else
            {
                base.SetItem(index, item);
            }
        }

        /// <summary>
        /// Removes the item.
        /// </summary>
        /// <param name="index">The index.</param>
        protected override void RemoveItem(int index)
        {
            var item = this[index];

            if (!_movingItems)
            {
                this.OnItemRemoving(index, item);
            }

            base.RemoveItem(index);

            if (!_movingItems)
            {
                this.OnItemRemoved(index, item);
            }
        }

        /// <summary>
        /// Inserts the item.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="item">The item.</param>
        /// <exception cref="System.InvalidOperationException">当前列表中已经存在这个实体，添加操作不可用。</exception>
        protected override void InsertItem(int index, Entity item)
        {
            if (item == null) throw new ArgumentNullException("item");

            if (!_movingItems)
            {
                this.OnItemAdding(index, item);
            }

            base.InsertItem(index, item);

            if (!_movingItems)
            {
                this.OnItemAdded(index, item);
            }
        }

        private void OnItemAdding(int index, Entity item)
        {
            if (this.SupportTree)
            {
                if (this.IsTreeRootList && item.TreePId != null && !item.IsDeleted)
                {
                    throw new InvalidOperationException(string.Format(
                        "树的根节点列表中不能添加非根节点：{0}。", item
                    ));
                }

                if (this.Contains(item)) throw new InvalidOperationException(string.Format(
                    "树的列表中已经存在实体 {0}，不能重复添加。", item
                    ));
            }

            //从已删除列表中删除。
            if (item.IsDeleted)
            {
                (item as ITreeComponent).EachNode(i =>
                {
                    i.RevertDeletedStatus();
                    return false;
                });

                if (_deletedList != null)
                {
                    _deletedList.Remove(item);
                }
            }

            this.SetItemParent(item);
        }

        private void OnItemAdded(int index, Entity item)
        {
            this.OnTreeItemInserted(index, item);
        }

        private void OnItemRemoving(int index, Entity item)
        {
            //如果是新的对象，则不需要加入到 DeletedList 列表中。
            if (!item.IsNew)
            {
                (item as ITreeComponent).EachNode(i =>
                {
                    i.PersistenceStatus = PersistenceStatus.Deleted;
                    return false;
                });

                this.DeletedList.Add(item);
            }

            this.ClearItemParent(item);

            this.OnTreeItemRemoving(item);
        }

        private void OnItemRemoved(int index, Entity item)
        {
            this.OnTreeItemRemoved(index, item);
        }

        private void SetItemParent(Entity item)
        {
            var needParent = item != null && !this.SupressSetItemParent;
            if (needParent)
            {
                (item as IDomainComponent).SetParent(this);

                if (this.HasManyType == HasManyType.Composition)
                {
                    item.SetParentEntity(this.Parent);
                }
            }
        }

        private void ClearItemParent(Entity item)
        {
            var needParent = item != null && !this.SupressSetItemParent;
            if (needParent)
            {
                (item as IDomainComponent).SetParent(null);

                if (this.HasManyType == HasManyType.Composition)
                {
                    item.SetParentEntity(null);
                }
            }
        }

        #endregion

        /// <summary>
        /// 对应的实体类型。
        /// </summary>
        public Type EntityType
        {
            get
            {
                var repo = this.FindRepository();
                if (repo != null) return repo.EntityType;

                return EntityMatrix.FindByList(this.GetType()).EntityType;
            }
        }

        /// <summary>
        /// 设置组合父对象。
        /// </summary>
        /// <param name="entity"></param>
        public void SetParentEntity(Entity entity)
        {
            var property = this.GetRepository().FindParentPropertyInfo(true).ManagedProperty as IRefEntityProperty;
            this.EachNode(child =>
            {
                child.SetRefEntity(property, entity);
                return false;
            });
        }

        /// <summary>
        /// 由于有时父引用实体没有发生改变，但是父引用实体的 Id 变了，此时需要调用此方法同步二者的 Id。
        /// </summary>
        /// <param name="parent">由于外部调用时，已经有 parent 的值了，所以直接传进来。</param>
        internal void SyncParentEntityId(Entity parent)
        {
            //调用此方法的方法，必须保证这个列表是指定实体的组合子集合。
            //if (this.HasManyType == HasManyType.Composition)
            {
                var property = this.GetRepository().FindParentPropertyInfo(true).ManagedProperty as IRefEntityProperty;
                this.EachNode(child =>
                {
                    //注意，由于实体可能并没有发生改变，而只是 Id 变了，
                    //所以在设置的时候，先设置 Id，然后设置 Entity。
                    child.SetRefId(property.RefIdProperty, parent.Id);
                    child.SetRefEntity(property, parent);

                    return false;
                });
            }
        }

        #region ReadRowDirectly

        #endregion

        #region 值的复制

        /// <summary>
        /// 复制目标集合中的所有对象。
        /// </summary>
        /// <param name="sourceList"></param>
        /// <param name="options"></param>
        public void Clone(EntityList sourceList, CloneOptions options)
        {
            this.Clear();

            var repo = this.FindRepository();
            var entityType = this.EntityType;

            for (int i = 0, c = sourceList.Count; i < c; i++)
            {
                var source = sourceList[i];

                Entity entity = null;
                if (repo != null)
                {
                    entity = repo.New();
                }
                else
                {
                    entity = Entity.New(entityType);
                }

                entity.Clone(source, options);
                this.Add(entity);
            }

            this.NotifyLoaded(sourceList._repository);
        }

        #endregion

        /// <summary>
        /// 触发某个路由事件
        /// </summary>
        /// <param name="indicator"></param>
        /// <param name="args"></param>
        protected void RaiseRoutedEvent(EntityRoutedEvent indicator, EventArgs args)
        {
            if (indicator.Type == EntityRoutedEventType.BubbleToTreeParent)
            {
                throw new InvalidOperationException("列表类上只支持 BubbleToParent 的实体路由事件。");
            }

            //如果没有父实体，则直接返回
            var parent = this.Parent;
            if (parent == null) return;

            var arg = new EntityRoutedEventArgs
            {
                Source = this,
                Event = indicator,
                Args = args
            };

            parent.RouteByList(this, arg);
        }

        /// <summary>
        /// 把指定的实体集合都回到本集合中来。
        /// </summary>
        /// <param name="list">The list.</param>
        public void AddRange(IEnumerable<Entity> list)
        {
            foreach (var item in list) { this.Add(item); }
        }
    }
}