/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20150314
 * 运行环境：.NET 4.5
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20150314 00:35
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rafy.Domain.Caching;
using Rafy.Domain.ORM.Query;
using Rafy.ManagedProperty;

namespace Rafy.Domain
{
    /// <summary>
    /// 仓库数据提供程序中的数据保存器。
    /// </summary>
    public abstract class DataSaver
    {
        private EntityRepository _repository;

        private RepositoryDataProvider _dataProvider;

        private SubmitInterceptorList _submitter;

        private static IList<Type> _submitInterceptors = new List<Type>();

        /// <summary>
        /// 提交功能的拦截器类型列表。
        /// </summary>
        public static IList<Type> SubmitInterceptors
        {
            get { return _submitInterceptors; }
            internal set { _submitInterceptors = value; }
        }

        /// <summary>
        /// Initializes the specified data provider.
        /// </summary>
        /// <param name="dataProvider">The data provider.</param>
        internal protected virtual void Init(RepositoryDataProvider dataProvider)
        {
            _dataProvider = dataProvider;
            _repository = dataProvider.Repository;

            //创建提交的拦截器列表。
            _submitter = new SubmitInterceptorList();
            _submitter.Add(dataProvider);
            foreach (var type in _submitInterceptors)
            {
                _submitter.Add(type);
            }
        }

        #region DeletingChildrenInMemory

        private bool _enableDeletingChildrenInMemory;

        /// <summary>
        /// 是否需要在内存中进行删除。
        /// 
        /// SqlCe 的数据库，常常需要打开这个选项。
        /// 因为 SqlCe 的级联删除在遇到组合子对象是 TreeEntity 时，会出现无法成功级联删除的问题。
        /// 
        /// 默认情况下，对象使用级联删除，所以不需要在内存中更新组合子，本值返回 false。
        /// 
        /// 此功能只能打开，打开后不能再关闭。
        /// </summary>
        public void EnableDeletingChildrenInMemory()
        {
            _enableDeletingChildrenInMemory = true;
        }

        #endregion

        /// <summary>
        /// 对应的数据提供程序。
        /// </summary>
        public RepositoryDataProvider DataProvider
        {
            get { return _dataProvider; }
        }

        #region 冗余属性更新处理

        /// <summary>
        /// 子类重写此方法实现冗余属性更新器的实现。
        /// </summary>
        /// <returns></returns>
        public abstract RedundanciesUpdater CreateRedundanciesUpdater();

        /// <summary>
        /// 尝试更新冗余属性值。
        /// </summary>
        internal void UpdateRedundanciesIf(Entity entity)
        {
            if (entity.UpdateRedundancies)
            {
                var redundanciesUpdater = this.CreateRedundanciesUpdater();
                redundanciesUpdater.UpdateRedundancies(entity, _repository);
            }
        }

        #endregion

        #region 数据层 - 提交接口

        /// <summary>
        /// 数据门户调用本接口来保存数据。
        /// </summary>
        /// <param name="component"></param>
        internal protected virtual void SubmitComposition(IDomainComponent component)
        {
            //在更新时，通知服务器更新数据版本号，并使用批量更新来提升更新的性能。
            using (VersionSyncMgr.BatchSaveScope())
            {
                //从数据门户过来的更新时，一般都是根实体时，这时需要同时更新整张表的服务端缓存版本号。
                //如果不是根实体，那也无法获取这个数据的版本号范围，所以也简单地更新整张表的版本号。
                _repository.ClientCache.UpdateServerVersion();

                this.SubmitComponent(component, true);
            }
        }

        /// <summary>
        /// 提交更新指定实体的组合子列表。
        /// </summary>
        /// <param name="child">The child.</param>
        /// <param name="parent">The parent.</param>
        private void SubmitChildrenComponent(IDomainComponent child, Entity parent)
        {
            if (child.IsDirty)
            {
                _repository.ClientCache.UpdateServerVersion(parent);

                this.SubmitComponent(child, false);
            }
        }

        /// <summary>
        /// 完整提交指定的领域实体或领域实体列表。
        /// </summary>
        /// <param name="component"></param>
        /// <param name="markSaved">如果是最外层调用此方法，则在最终标记整个组件为保存完毕。</param>
        private void SubmitComponent(IDomainComponent component, bool markSaved)
        {
            //对于树的列表的提交，需要单独处理
            if (_repository.SupportTree)
            {
                if (component is Entity)
                {
                    this.SubmitTree(component as Entity, markSaved);
                }
                else
                {
                    this.SubmitTreeList(component as EntityList, markSaved);
                }
            }
            else
            {
                if (component is Entity)
                {
                    this.SubmitItem(component as Entity, markSaved, false);
                }
                else
                {
                    this.SubmitList(component as EntityList, markSaved);
                }
            }
        }

        #endregion

        #region 数据层 - 提交主逻辑

        /// <summary>
        /// 提交以实体为根的一个树。
        /// </summary>
        /// <param name="tree"></param>
        /// <param name="markSaved"></param>
        private void SubmitTree(Entity tree, bool markSaved)
        {
            //先保存所有添加、变更的节点。
            //这里的 markSaved 传入的应该是 false，否则会把待删除列表中的元素清空。
            if (!tree.IsDeleted)
            {
                //如果要提交的树节点是一个根节点，而且它的索引还没有生成，则需要主动为其生成索引。
                if (tree.TreePId == null && string.IsNullOrEmpty(tree.TreeIndex))
                {
                    var maxIndex = _repository.GetMaxTreeIndex();
                    if (!string.IsNullOrEmpty(maxIndex))
                    {
                        tree.TreeIndex = _repository.TreeIndexOption.GetNextRootTreeIndex(maxIndex);
                    }
                    else
                    {
                        tree.TreeIndex = _repository.TreeIndexOption.CalculateChildIndex(null, 0);
                    }

                    var treeChildren = tree.TreeChildren;
                    treeChildren.LoadAllNodes();
                    treeChildren.ResetTreeIndex();
                }

                this.SubmitItem(tree, false, true);

                //然后再保存所有删除的节点。
                this.SubmitTreeDeletedItems(tree, markSaved);
            }
            else
            {
                if ((tree as IEntityWithId).IdProvider.IsAvailable(tree.TreePId))
                {
                    throw new NotSupportedException("删除树形子实体，请用父实体的TreeChildren.Remove()方法。");
                }

                this.DeleteTreeChildren(tree);

                this.SubmitItem(tree, false, true);
            }
        }

        /// <summary>
        /// 数据层 - 提交树节点
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="markSaved">if set to <c>true</c> [mark saved].</param>
        private void SubmitTreeList(EntityList list, bool markSaved)
        {
            //先保存所有添加、变更的节点。
            for (int i = 0, c = list.Count; i < c; i++)
            {
                var item = list[i];
                if (item.IsDirty)
                {
                    //这里的 markSaved 传入的应该是 false，否则会把待删除列表中的元素清空。
                    this.SubmitItem(item, false, true);
                }
            }

            //然后再保存所有删除的节点。
            this.SubmitTreeDeletedItems(list, markSaved);
        }

        /// <summary>
        /// 删除整棵树中所有需要删除的节点。
        /// </summary>
        /// <param name="tree">The tree.</param>
        /// <param name="markSaved">if set to <c>true</c> [mark saved].</param>
        private void SubmitTreeDeletedItems(ITreeComponent tree, bool markSaved)
        {
            //整合所有要删除的节点。
            var mergedDeletedList = new List<Entity>();

            #region 加入列表的待删除节点。

            var list = tree as EntityList;
            if (list != null)
            {
                var toDeletedList = list.DeletedListField;
                if (toDeletedList != null)
                {
                    var isTreeRootList = list.IsTreeRootList;
                    for (int i = 0, c = toDeletedList.Count; i < c; i++)
                    {
                        var child = toDeletedList[i];
                        //如果在删除之后，又变换了 TreePId，那么可能是该节点已经加入到别的子树中，
                        //却无法修改根列表的 DeletedList 而导致该节点还在其中。
                        //所以需要把这些节点过滤掉，根列表只处理根节点的删除。
                        if (!isTreeRootList || child.TreePId == null)
                        {
                            //由于自关联外键没有级联删除，所以节点必须完整加载后，再全部加入到列表中。
                            TreeHelper.FullAddIntoList(child, mergedDeletedList);
                        }
                    }
                }
            }

            #endregion

            #region 加入所有节点的待删除列表节点。

            tree.EachNode(node =>
            {
                var tc = node.TreeChildrenField;
                if (tc != null)
                {
                    var tcDeleted = tc.DeletedListField;
                    if (tcDeleted != null)
                    {
                        for (int i = 0, c = tcDeleted.Count; i < c; i++)
                        {
                            var deleted = tcDeleted[i];
                            //与列表一样，必须检测删除的节点，当前是不是还属于 node。
                            if (node.Id.Equals(deleted.TreePId))
                            {
                                TreeHelper.FullAddIntoList(deleted, mergedDeletedList);
                            }
                        }
                    }
                }
                return false;
            });

            #endregion

            //反转顺序后再保存。
            mergedDeletedList.Reverse();
            for (int i = 0, c = mergedDeletedList.Count; i < c; i++)
            {
                var item = mergedDeletedList[i];
                item.PersistenceStatus = PersistenceStatus.Deleted;
                this.SubmitItem(item, false, false);
            }

            if (markSaved)
            {
                tree.MarkSaved();
            }
        }

        /// <summary>
        /// 保存根对象列表
        /// </summary>
        private void SubmitList(EntityList list, bool markSaved)
        {
            var toDelete = list.DeletedListField;
            if (toDelete != null)
            {
                for (int i = 0, c = toDelete.Count; i < c; i++)
                {
                    var child = toDelete[i];
                    this.SubmitItem(child, markSaved, false);
                }

                list.DeletedListField = null;
            }

            for (int i = 0, c = list.Count; i < c; i++)
            {
                var item = list[i];
                if (item.IsDirty)
                {
                    this.SubmitItem(item, markSaved, false);
                }
            }
        }

        /// <summary>
        /// 根据实体状态来选择保存方法。
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="markSaved">是否当前正在保存最外层组合根对象。如果是，则应该在保存完成后，把整个组合对象状态清空。</param>
        /// <param name="withTreeChildren">是否需要同时处理树的子节点。</param>
        /// <exception cref="System.NotSupportedException"></exception>
        private void SubmitItem(Entity entity, bool markSaved, bool withTreeChildren)
        {
            //创建提交数据的参数。
            var args = new SubmitArgs
            {
                DataProvider = _dataProvider,
                Entity = entity,
                WithTreeChildren = withTreeChildren,
                Action = GetAction(entity)
            };

            //提交更改。
            _submitter.Submit(args);

            //保存完毕，修改实体的状态
            switch (args.Action)
            {
                case SubmitAction.Delete:
                    //在删除后，标记对象的状态到“新对象”。
                    entity.PersistenceStatus = PersistenceStatus.New;
                    break;
                case SubmitAction.Update:
                case SubmitAction.Insert:
                case SubmitAction.ChildrenOnly:
                    if (markSaved)
                    {
                        (entity as IDirtyAware).MarkSaved();
                    }
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        /// 提交聚合对象到数据库中。
        /// 
        /// 子类重写此方法实现整个聚合对象保存到非关系型数据库的逻辑。
        /// 如果只想重写单个对象的 CUD 逻辑，请重写 Insert、Update、Delete 方法。
        /// 
        /// 注意，不论是聚合父对象，还是聚合子对象，还是没有聚合子的对象，都会执行该方法。
        /// 它与 Insert、Update、Delete 等方法的区别在于，重写此方法可以同时阻止对聚合子对象的默认保存逻辑。
        /// </summary>
        /// <param name="e"></param>
        internal protected virtual void Submit(SubmitArgs e)
        {
            var entity = e.Entity;
            switch (e.Action)
            {
                case SubmitAction.Delete:
                    this.DoDelete(entity);
                    break;
                case SubmitAction.Insert:
                    this.DoInsert(entity, e.WithTreeChildren);
                    break;
                case SubmitAction.Update:
                    this.DoUpdate(entity, e.WithTreeChildren);
                    break;
                case SubmitAction.ChildrenOnly:
                    this.SubmitChildren(entity);
                    if (e.WithTreeChildren && _repository.SupportTree)
                    {
                        this.SubmitTreeChildren(entity);
                    }
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        private static SubmitAction GetAction(Entity entity)
        {
            switch (entity.PersistenceStatus)
            {
                case PersistenceStatus.Deleted:
                    return SubmitAction.Delete;
                case PersistenceStatus.New:
                    return SubmitAction.Insert;
                case PersistenceStatus.Modified:
                    return SubmitAction.Update;
                case PersistenceStatus.Unchanged:
                default:
                    return SubmitAction.ChildrenOnly;
            }
        }

        #endregion

        #region 数据层 - 插入/更新

        private void DoInsert(Entity entity, bool withTreeChildren)
        {
            _dataProvider.Insert(entity);

            this.SubmitChildren(entity);

            if (withTreeChildren && _repository.SupportTree)
            {
                this.SubmitTreeChildren(entity);
            }
        }

        private void DoUpdate(Entity entity, bool withTreeChildren)
        {
            //如果是聚合子对象发生改变，而当前对象没有改变时，则不需要更新当前对象。
            this.UpdateRedundanciesIf(entity);

            _dataProvider.Update(entity);

            this.SubmitChildren(entity);

            if (withTreeChildren && _repository.SupportTree)
            {
                this.SubmitTreeChildren(entity);
            }
        }

        /// <summary>
        /// 插入这个数据到持久层中。
        /// 
        /// 子类重写此方法来实现持久层的插入逻辑。
        /// 重写时，注意：
        /// 在插入完成后，把为实体新生成的 Id 赋值到实体中。否则组合子将插入失败。
        /// </summary>
        /// <param name="data"></param>
        public abstract void InsertToPersistence(Entity data);

        /// <summary>
        /// 更新这个这个数据到持久层中。
        /// 
        /// 子类重写此方法来实现持久层的更新逻辑。
        /// </summary>
        /// <param name="data"></param>
        public abstract void UpdateToPersistence(Entity data);

        /// <summary>
        /// 提交更新指定实体的组合子列表。
        /// </summary>
        protected virtual void SubmitChildren(Entity entity)
        {
            var enumerator = entity.GetLoadedChildren();
            while (enumerator.MoveNext())
            {
                var child = enumerator.Current.Value;

                SaveRecur(child, entity);
            }
        }

        private static void SaveRecur(IDomainComponent child, Entity parent)
        {
            //使用组合子自己的仓库来进行提交。
            var childDataProvider = child.GetRepository().DataProvider as IRepositoryDataProviderInternal;
            var childSaver = childDataProvider.DataSaver;
            childSaver.SubmitChildrenComponent(child, parent);
        }

        /// <summary>
        /// 提交指定树节点的所有树子节点。
        /// </summary>
        /// <param name="entity"></param>
        protected virtual void SubmitTreeChildren(Entity entity)
        {
            var treeChildren = entity.TreeChildrenField;
            if (treeChildren != null)
            {
                //所有节点的删除，都在最外层完成。
                ////先保存删除的子节点。
                //var deletedList = treeChildren.DeletedListField;
                //if (deletedList != null && deletedList.Count > 0)
                //{
                //    for (int i = 0, c = deletedList.Count; i < c; i++)
                //    {
                //        var child = deletedList[i];
                //        //检测的原因见单元测试：TET_Save_Combine_DeleteByTreeParentAndReAddIt
                //        if (entity.Id.Equals(child.TreePId))
                //        {
                //            this.SubmitChild(child, true);
                //        }
                //    }

                //    treeChildren.DeletedListField = null;
                //}

                //保存子节点。
                for (int i = 0, c = treeChildren.Count; i < c; i++)
                {
                    var treeChild = treeChildren[i];
                    if (treeChild.IsDirty)
                    {
                        this.SubmitItem(treeChild, false, true);
                    }
                }
            }
        }

        #endregion

        #region 数据层 - 删除

        private void DoDelete(Entity entity)
        {
            //如果需要在内存中删除组合子，则应该先删除这些子对象。
            if (_enableDeletingChildrenInMemory || _repository.EntityMeta.DeletingChildrenInMemory)
            {
                this.DeleteChildren(entity);
            }

            //所有节点的删除，都在最外层完成。而不是使用递归。
            //if (withTreeChildren && _repository.SupportTree)
            //{
            //    this.DeleteTreeChildren(entity);
            //}

            _dataProvider.Delete(entity);
        }

        /// <summary>
        /// 从持久层中删除这个数据。
        /// 
        /// 子类重写此方法来实现持久层的删除逻辑。
        /// </summary>
        /// <param name="data"></param>
        public abstract void DeleteFromPersistence(Entity data);

        /// <summary>
        /// 删除所有组合子。
        /// 
        /// 子类重写此方法来实现新的删除逻辑。
        /// 注意，此方法只会在指定了 <see cref="EnableDeletingChildrenInMemory"/> 时，才会调用。
        /// </summary>
        protected virtual void DeleteChildren(Entity entity)
        {
            _repository.LoadAllChildren(entity);

            foreach (var childField in entity.GetLoadedChildren())
            {
                var children = childField.Value as EntityList;
                if (children != null && children.Count > 0)
                {
                    //删除所有子。
                    children.Clear();
                    SaveRecur(children, entity);
                }
            }
        }

        /// <summary>
        /// 删除所有的子节点。
        /// 
        /// 子类重写此方法来实现新的删除逻辑。
        /// </summary>
        protected virtual void DeleteTreeChildren(Entity entity)
        {
            //删除节点时，需要先保证所有的结点都已经加到了集合中。
            var treeChildren = entity.TreeChildren;
            treeChildren.LoadAllNodes();

            //整合所有要删除的节点。
            var mergedDeletedList = new List<Entity>();
            treeChildren.EachNode(e =>
            {
                mergedDeletedList.Add(e);
                return false;
            });

            //反转顺序后再保存。
            mergedDeletedList.Reverse();
            for (int i = 0, c = mergedDeletedList.Count; i < c; i++)
            {
                var item = mergedDeletedList[i];
                item.PersistenceStatus = PersistenceStatus.Deleted;
                this.SubmitItem(item, false, false);
            }
        }

        /// <summary>
        /// 通过引用关系，来删除引用表中引用本对象的所有对象。
        /// 
        /// 一般情况下，子类可以在自己重写的 Submit 方法中调用此方法来先删除其它非级联有关系。
        /// 
        /// 注意，此方法暂时只会生成 SQL 删除引用表的对象，不主动处理更多的级联关系。（以后再实现。）
        /// </summary>
        /// <param name="entity">正在删除这个实体。</param>
        /// <param name="refProperty">这个引用属性表示了需要删除的另一个实体到 <c>entity</c> 的引用关系。</param>
        /// <param name="propertyOwner"><c>refProperty</c> 属性对应的拥有者实体类型。</param>
        protected void DeleteRef(Entity entity, IRefProperty refProperty, Type propertyOwner = null)
        {
            if (propertyOwner == null) propertyOwner = refProperty.OwnerType;

            var refRepo = RepositoryFactoryHost.Factory.FindByEntity(propertyOwner);
            var saver = (refRepo.DataProvider as IRepositoryDataProviderInternal).DataSaver;
            saver.DeleteRefCore(entity, refProperty);
        }

        /// <summary>
        /// 子类重写此方法，实现 <see cref="DeleteRef(Entity, IRefProperty, Type)" /> 的具体逻辑。
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="refProperty"></param>
        protected abstract void DeleteRefCore(Entity entity, IRefProperty refProperty);

        #endregion
    }
}