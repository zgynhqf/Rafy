/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20140503
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20140503 19:30
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Text;
using Rafy;
using Rafy.Data;
using Rafy.Domain.Caching;
using Rafy.Domain.ORM;
using Rafy.Domain.ORM.Linq;
using Rafy.Domain.ORM.Query;
using Rafy.ManagedProperty;

namespace Rafy.Domain
{
    /// <summary>
    /// 本类是为关系型数据库设计的数据提供器。
    /// IRepositoryDataProvider 则是更通用的接口。
    /// </summary>
    public class RepositoryDataProvider : EntityRepositoryQueryBase,
        IDbConnector,
        IRepositoryDataProvider
    {
        private EntityRepository _repository;

        internal void InitRepository(EntityRepository repository)
        {
            _repository = repository;
            _linqProvider = new EntityQueryProvider { _repository = repository };
        }

        internal override EntityRepository Repo
        {
            get { return _repository; }
        }

        /// <summary>
        /// 为此仓库提供数据。
        /// </summary>
        public EntityRepository Repository
        {
            get { return _repository; }
        }

        #region 数据库配置

        private DbSetting _dbSetting;
        /// <summary>
        /// 这个字段用于存储运行时解析出来的 ORM 信息。
        /// </summary>
        private DbTable _ormTable;

        /// <summary>
        /// 创建数据库操作对象
        /// </summary>
        /// <returns></returns>
        public IDbAccesser CreateDbAccesser()
        {
            return DbAccesserFactory.Create(this.DbSetting);
        }

        /// <summary>
        /// 数据库配置名称（每个库有一个唯一的配置名）
        /// 
        /// 默认使用 ConnectionStringNames.RafyPlugins 中配置的数据库。
        /// </summary>
        protected virtual string ConnectionStringSettingName
        {
            get { return ConnectionStringNames.RafyPlugins; }
        }

        /// <summary>
        /// 数据库配置（每个库有一个唯一的配置名）
        /// </summary>
        public DbSetting DbSetting
        {
            get
            {
                if (this._dbSetting == null)
                {
                    var conSetting = this.ConnectionStringSettingName;
                    if (conSetting == null) throw new InvalidProgramException("数据库配置属性重写有误，不能返回 null。");
                    this._dbSetting = DbSetting.FindOrCreate(conSetting);
                }
                return this._dbSetting;
            }
        }

        ///// <summary>
        ///// 获取该实体对应的 ORM 运行时对象。
        ///// 
        ///// 如果该实体没有对应的实体元数据或者该实体没有被配置为映射数据库，
        ///// 则本方法则无法创建对应的 ORM 运行时，此时会返回 null。
        ///// </summary>
        ///// <returns></returns>
        //public ITable GetDbTable()
        //{
        //    return this.DbTable;
        //}

        internal DbTable DbTable
        {
            get
            {
                if (_ormTable == null && _repository.EntityMeta != null)
                {
                    _ormTable = DbTableFactory.CreateORMTable(_repository);
                }

                return _ormTable;
            }
        }

        #endregion

        #region 增删改

        #region 配置

        /// <summary>
        /// 是否需要在内在中进行删除。
        /// 
        /// SqlCe 的数据库，常常需要打开这个选项。
        /// 因为 SqlCe 的级联删除在遇到组合子对象是 TreeEntity 时，会出现无法成功级联删除的问题。
        /// 
        /// 默认情况下，对象使用级联删除，所以不需要在内存中更新组合子，本值返回 false。
        /// </summary>
        protected virtual bool EnableDeletingChildrenInMemory
        {
            get { return false; }
        }

        #endregion

        #region 数据层 - 提交接口

        /// <summary>
        /// 数据门户调用本接口来保存数据。
        /// </summary>
        /// <param name="component"></param>
        public virtual void SubmitComposition(IDomainComponent component)
        {
            //以下事务代码，不需要区分是否使用分布式缓存的情况来做事务处理，
            //而是直接使用 SingleConnectionTransactionScope 类来管理不同数据库的事务，
            //因为这个类会保证不同的库使用不同的事务。
            using (var tran = new SingleConnectionTrasactionScope(this.DbSetting))
            {
                //在更新时，通知服务器更新数据版本号，并使用批量更新来提升更新的性能。
                using (VersionSyncMgr.BatchSaveScope())
                {
                    //从数据门户过来的更新时，一般都是根实体时，这时需要同时更新整张表的服务端缓存版本号。
                    //如果不是根实体，那也无法获取这个数据的版本号范围，所以也简单地更新整张表的版本号。
                    _repository.ClientCache.UpdateServerVersion();

                    this.SubmitComponent(component, true);
                }

                //最后提交事务。前面的代码，如果出现异常，则会回滚整个事务。
                tran.Complete();
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

            //先保存所有添加、变更的节点。
            //这里的 markSaved 传入的应该是 false，否则会把待删除列表中的元素清空。
            if (!tree.IsDeleted)
            {
                this.SubmitItem(tree, false, true);

                //然后再保存所有删除的节点。
                this.SubmitTreeDeletedItems(tree, markSaved);
            }
            else
            {
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
                Entity = entity,
                WithTreeChildren = withTreeChildren,
                Action = GetAction(entity)
            };

            //提交更改。
            this.Submit(args);

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
                        entity.MarkSaved();
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
        /// </summary>
        /// <param name="e"></param>
        protected virtual void Submit(SubmitArgs e)
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
            this.Insert(entity);

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

            this.Update(entity);

            this.SubmitChildren(entity);

            if (withTreeChildren && _repository.SupportTree)
            {
                this.SubmitTreeChildren(entity);
            }
        }

        /// <summary>
        /// 插入这个实体到仓库中。
        /// 
        /// 子类重写此方法来实现非关系型数据库的插入逻辑。
        /// 重写时，注意：
        /// 在插入完成后，把为实体新生成的 Id 赋值到实体中。否则组合子将插入失败。
        /// </summary>
        /// <param name="entity"></param>
        protected virtual void Insert(Entity entity)
        {
            using (var dba = this.CreateDbAccesser())
            {
                var table = this.DbTable;

                table.Insert(dba, entity);

                //放到 Insert 语句之后，否则 Id 不会有值。
                table.NotifyLoaded(entity);
            }
        }

        /// <summary>
        /// 更新这个实体到仓库中。
        /// 
        /// 子类重写此方法来实现非关系型数据库的更新逻辑。
        /// </summary>
        /// <param name="entity"></param>
        protected virtual void Update(Entity entity)
        {
            using (var dba = this.CreateDbAccesser())
            {
                var table = this.DbTable;
                table.Update(dba, entity);
                table.NotifyLoaded(entity);
            }
        }

        /// <summary>
        /// 提交更新指定实体的组合子列表。
        /// </summary>
        protected virtual void SubmitChildren(Entity entity)
        {
            var enumerator = entity.GetLoadedChildren();
            while (enumerator.MoveNext())
            {
                var child = enumerator.Current.Value;

                //使用组合子自己的仓库来进行提交。
                var repo = child.GetRepository() as EntityRepository;
                var repoData = repo.RdbDataProvider;
                repoData.SubmitChildrenComponent(child, entity);
            }
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
            if (this.EnableDeletingChildrenInMemory)
            {
                this.DeleteChildren(entity);
            }

            //所有节点的删除，都在最外层完成。
            //if (withTreeChildren && _repository.SupportTree)
            //{
            //    this.DeleteTreeChildren(entity);
            //}

            this.Delete(entity);
        }

        /// <summary>
        /// 从仓库中删除这个实体。
        /// 
        /// 子类重写此方法来实现非关系型数据库的删除逻辑。
        /// </summary>
        /// <param name="entity"></param>
        protected virtual void Delete(Entity entity)
        {
            using (var dba = this.CreateDbAccesser())
            {
                this.DbTable.Delete(dba, entity);
            }
        }

        /// <summary>
        /// 删除所有组合子。
        /// 
        /// 子类重写此方法来实现新的删除逻辑。
        /// 注意，此方法只会在指定了 <see cref="EnableDeletingChildrenInMemory"/> 时，才会调用。
        /// </summary>
        protected virtual void DeleteChildren(Entity entity)
        {
            foreach (var mp in _repository.EntityMeta.ManagedProperties.GetNonReadOnlyCompiledProperties())
            {
                var listProperty = mp as IListProperty;
                if (listProperty != null && listProperty.HasManyType == HasManyType.Composition)
                {
                    //不论这个列表属性是否已经加载，都必须获取其所有的数据行，并标记为删除。
                    var list = entity.GetLazyList(listProperty);
                    if (list.Count > 0)
                    {
                        list.Clear();

                        //删除所有子。
                        var childRepo = list.GetRepository() as EntityRepository;
                        childRepo.RdbDataProvider.SubmitChildrenComponent(list, entity);
                    }
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
        protected void DeleteRefInDb(Entity entity, IRefProperty refProperty, Type propertyOwner = null)
        {
            if (propertyOwner == null) propertyOwner = refProperty.OwnerType;

            var refRepo = RepositoryFactoryHost.Factory.FindByEntity(propertyOwner)
                as EntityRepository;
            refRepo.RdbDataProvider.DeleteRefInDbCore(entity, refProperty);
        }

        private void DeleteRefInDbCore(Entity entity, IRefProperty refProperty)
        {
            var f = QueryFactory.Instance;
            var table = f.Table(_repository);
            var where = f.Constraint(table.Column(refProperty.RefIdProperty), entity.Id);

            using (var dba = this.CreateDbAccesser())
            {
                this.DbTable.Delete(dba, where);
            }
        }

        #endregion

        #region 冗余属性更新处理

        /// <summary>
        /// 尝试更新冗余属性值。
        /// </summary>
        private void UpdateRedundanciesIf(Entity entity)
        {
            if (!entity.UpdateRedundancies) return;

            //如果有一些在冗余属性路径中的属性的值改变了，则开始更新数据库的中的所有冗余字段的值。
            Entity dbEntity = null;
            var propertiesInPath = _repository.GetPropertiesInRedundancyPath();
            for (int i = 0, c = propertiesInPath.Count; i < c; i++)
            {
                var property = propertiesInPath[i];

                //如果只有一个属性，那么就是它变更引起的更新
                //否则，需要从数据库获取原始值来对比检测具体哪些属性值变更，然后再发起冗余更新。
                bool isChanged = c == 1;

                var refProperty = property as IRefIdProperty;
                if (refProperty != null)
                {
                    if (!isChanged)
                    {
                        if (dbEntity == null) { dbEntity = ForceGetById(entity); }
                        var dbId = dbEntity.GetRefId(refProperty);
                        var newId = entity.GetRefId(refProperty);
                        isChanged = !object.Equals(dbId, newId);
                    }

                    if (isChanged)
                    {
                        foreach (var path in property.InRedundantPathes)
                        {
                            //如果这条路径中是直接把引用属性的值作为值属性进行冗余，那么同样要进行值属性更新操作。
                            if (path.ValueProperty.Property == property)
                            {
                                this.UpdateRedundancyByRefValue(entity, path, refProperty);
                            }
                            //如果是引用变更了，并且只有一个 RefPath，则不需要处理。
                            //因为这个已经在属性刚变更时的处理函数中实时处理过了。
                            else if (path.RefPathes.Count > 1)
                            {
                                this.UpdateRedundancyByIntermidateRef(entity, path, refProperty);
                            }
                        }
                    }
                }
                else
                {
                    var newValue = entity.GetProperty(property);

                    if (!isChanged)
                    {
                        if (dbEntity == null) { dbEntity = ForceGetById(entity); }
                        var dbValue = dbEntity.GetProperty(property);
                        isChanged = !object.Equals(dbValue, newValue);
                    }

                    if (isChanged)
                    {
                        foreach (var path in property.InRedundantPathes)
                        {
                            UpdateRedundancyByValue(entity, path, newValue);
                        }
                    }
                }
            }

            entity.UpdateRedundancies = false;
        }

        private Entity ForceGetById(Entity entity)
        {
            var dbEntity = _repository.GetById(entity.Id);
            if (dbEntity == null)
            {
                throw new InvalidOperationException(string.Format(@"{1} 类型对应的仓库中不存在 Id 为 {0} 的实体，更新冗余属性失败！", entity.Id, entity.GetType()));
            }
            return dbEntity;
        }

        /// <summary>
        /// 值改变时引发的冗余值更新操作。
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="path">The path.</param>
        /// <param name="newValue">The new value.</param>
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        private void UpdateRedundancyByValue(Entity entity, RedundantPath path, object newValue)
        {
            UpdateRedundancy(entity, path.Redundancy, newValue, path.RefPathes, entity.Id);
        }

        /// <summary>
        /// 冗余路径中非首位的引用属性变化时引发的冗余值更新操作。
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="path">The path.</param>
        /// <param name="refChanged">该引用属性值变化了</param>
        private void UpdateRedundancyByIntermidateRef(Entity entity, RedundantPath path, IRefIdProperty refChanged)
        {
            var newValue = entity.GetRedundancyValue(path, refChanged);

            //只要从开始到 refChanged 前一个
            var refPathes = new List<ConcreteProperty>(5);
            foreach (var refProperty in path.RefPathes)
            {
                if (refProperty.Property == refChanged) break;
                refPathes.Add(refProperty);
            }

            this.UpdateRedundancy(entity, path.Redundancy, newValue, refPathes, entity.Id);
        }

        /// <summary>
        /// 冗余路径中非首位的引用属的值作为值属性进行冗余，那么同样要进行值属性更新操作。
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="path">The path.</param>
        /// <param name="refChanged">该引用属性值变化了</param>
        private void UpdateRedundancyByRefValue(Entity entity, RedundantPath path, IRefIdProperty refChanged)
        {
            var newValue = entity.GetProperty(refChanged);

            this.UpdateRedundancy(entity, path.Redundancy, newValue, path.RefPathes, entity.Id);
        }

        /// <summary>
        /// 更新某个冗余属性
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="redundancy">更新指定的冗余属性</param>
        /// <param name="newValue">冗余属性的新值</param>
        /// <param name="refPathes">从冗余属性声明类型开始的一个引用属性集合，
        /// 将会为这个集合路径生成更新的 Where 条件。</param>
        /// <param name="lastRefId">引用路径中最后一个引用属性对应的值。这个值将会作为 Where 条件的值。</param>
        private void UpdateRedundancy(Entity entity, ConcreteProperty redundancy, object newValue, IList<ConcreteProperty> refPathes, object lastRefId)
        {
            /*********************** 代码块解释 *********************************
             * 
             * 假定场景是：
             * refPathes: D(CRef,AName) -> C(BRef) -> B(ARef)，
             * lastRefId: AId。（B.ARef 是最后一个引用属性）
             * 则对应生成的 SQL 是：
             * update D set AName = @AName where CId in (
             *     select id from C where BId in (
             *          select id from B where AId = @AId
             *     )
             * )
             * 如果 B、C 表也有冗余属性，对应的 SQL 则是：
             * update B Set AName = @AName where AId = @BId
             * update C set AName = @AName where BId in (
             *     select id from B where AId = @AId
             * )
             * 
            **********************************************************************/

            //准备所有用到的 DbTable
            var table = DbTableFinder.TableFor(redundancy.Owner);
            var refTables = new RefPropertyTable[refPathes.Count];
            for (int i = 0, c = refPathes.Count; i < c; i++)
            {
                var refProperty = refPathes[i];
                var refTable = DbTableFinder.TableFor(refProperty.Owner);
                if (refTable == null)
                {
                    ORMHelper.ThrowBasePropertyNotMappedException(refProperty.Name, refProperty.Owner);
                }

                refTables[i] = new RefPropertyTable
                {
                    RefProperty = refProperty,
                    OwnerTable = refTable
                };
            }

            var sql = new ConditionalSql();
            //SQL: UPDATE D SET AName = {0} WHERE
            sql.Append("UPDATE ").AppendQuoteName(table)
                .Append(" SET ").AppendQuote(table, table.Translate(redundancy.Property))
                .Append(" = ").AppendParameter(newValue).Append(" WHERE ");

            int quoteNeeded = 0;
            if (refTables.Length > 1)
            {
                //中间的都生成 Where XX in
                var inWherePathes = refTables.Take(refTables.Length - 1).ToArray();
                for (int i = 0; i < inWherePathes.Length; i++)
                {
                    var inRef = inWherePathes[i];

                    //SQL: CId In (
                    sql.AppendQuote(table, inRef.OwnerTable.Translate(inRef.RefProperty.Property))
                        .Append(" IN (").AppendLine();
                    quoteNeeded++;

                    var nextRef = refTables[i + 1];

                    //SQL: SELECT Id FROM C WHERE 
                    var nextRefOwnerTable = nextRef.OwnerTable;
                    sql.Append(" SELECT ").AppendQuote(nextRefOwnerTable, nextRefOwnerTable.PKColumn.Name)
                        .Append(" FROM ").AppendQuoteName(nextRefOwnerTable)
                        .Append(" WHERE ");
                }
            }

            //最后一个，生成SQL: BId = {1}
            var lastRef = refTables[refTables.Length - 1];
            sql.AppendQuote(table, lastRef.OwnerTable.Translate(lastRef.RefProperty.Property))
                .Append(" = ").AppendParameter(lastRefId);

            while (quoteNeeded > 0)
            {
                sql.AppendLine(")");
                quoteNeeded--;
            }

            //执行最终的 SQL 语句
            using (var dba = this.CreateDbAccesser())
            {
                dba.ExecuteText(sql, sql.Parameters);
            }
        }

        /// <summary>
        /// 某个引用属性与其所在类对应的表元数据
        /// </summary>
        private struct RefPropertyTable
        {
            public ConcreteProperty RefProperty;
            public DbTable OwnerTable;
        }

        #endregion

        #endregion

        #region 查询

        /// <summary>
        /// 子类可以重写这个方法，用于实现 GetAll 的数据层查询逻辑。
        /// </summary>
        /// <param name="paging">The paging information.</param>
        /// <param name="eagerLoad">需要贪婪加载的属性。</param>
        /// <returns></returns>
        public virtual EntityList GetAll(PagingInfo paging, EagerLoadOptions eagerLoad)
        {
            var query = qf.Query(_repository);

            var list = this.QueryList(query, paging, eagerLoad, true);

            return list;
        }

        /// <summary>
        /// 子类可以重写这个方法，用于实现 GetTreeRoots 的数据层查询逻辑。
        /// </summary>
        /// <param name="eagerLoad">需要贪婪加载的属性。</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public virtual EntityList GetTreeRoots(EagerLoadOptions eagerLoad)
        {
            var query = qf.Query(_repository);
            query.AddConstraint(Entity.TreePIdProperty, PropertyOperator.Equal, null);

            return this.QueryList(query, null, eagerLoad);
        }

        /// <summary>
        /// 子类可以重写这个方法，用于实现 GetById 的数据层查询逻辑。
        /// </summary>
        /// <param name="id">The unique identifier.</param>
        /// <param name="eagerLoad">需要贪婪加载的属性。</param>
        /// <returns></returns>
        public virtual EntityList GetById(object id, EagerLoadOptions eagerLoad)
        {
            var table = qf.Table(_repository);
            var q = qf.Query(
                table,
                where: qf.Constraint(table.IdColumn, id)
            );

            return this.QueryList(q, null, eagerLoad);
        }

        /// <summary>
        /// 子类重写此方法，来实现自己的 GetByIdList 方法的数据层代码。
        /// </summary>
        /// <param name="idList"></param>
        /// <param name="eagerLoad">需要贪婪加载的属性。</param>
        /// <returns></returns>
        public virtual EntityList GetByIdList(object[] idList, EagerLoadOptions eagerLoad)
        {
            var table = qf.Table(_repository);
            var q = qf.Query(
                table,
                where: qf.Constraint(table.IdColumn, PropertyOperator.In, idList)
            );

            return this.QueryList(q, null, eagerLoad);
        }

        /// <summary>
        /// 子类重写此方法，来实现自己的 GetByParentId 方法的数据层代码。
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="paging">分页信息。</param>
        /// <param name="eagerLoad">需要贪婪加载的属性。</param>
        /// <returns></returns>
        public virtual EntityList GetByParentId(object parentId, PagingInfo paging, EagerLoadOptions eagerLoad)
        {
            var parentProperty = _repository.FindParentPropertyInfo(true);
            var mp = (parentProperty.ManagedProperty as IRefEntityProperty).RefIdProperty;

            var table = qf.Table(_repository);
            var q = qf.Query(
                table,
                where: qf.Constraint(table.Column(mp), parentId)
            );

            var list = this.QueryList(q, paging, eagerLoad, true);

            return list;
        }

        /// <summary>
        /// 子类重写此方法来实现通过父 Id 列表来获取所有组合子对象的列表
        /// </summary>
        /// <param name="parentIdList">The parent identifier list.</param>
        /// <param name="paging">分页信息。</param>
        /// <param name="eagerLoad">需要贪婪加载的属性。</param>
        /// <returns></returns>
        public virtual EntityList GetByParentIdList(object[] parentIdList, PagingInfo paging, EagerLoadOptions eagerLoad)
        {
            var parentProperty = _repository.FindParentPropertyInfo(true);
            var mp = (parentProperty.ManagedProperty as IRefEntityProperty).RefIdProperty;

            var table = qf.Table(_repository);
            var q = qf.Query(
                table,
                where: qf.Constraint(table.Column(mp), PropertyOperator.In, parentIdList)
            );

            var list = this.QueryList(q, paging, eagerLoad, true);

            return list;
        }

        /// <summary>
        /// 通过树型编码，找到所有对应的子节点。
        /// </summary>
        /// <param name="treeIndex"></param>
        /// <param name="eagerLoad">需要贪婪加载的属性。</param>
        /// <returns></returns>
        public virtual EntityList GetByTreeParentIndex(string treeIndex, EagerLoadOptions eagerLoad)
        {
            //递归查找所有树型子
            var childCode = treeIndex + "%" + _repository.TreeIndexOption.Seperator + "%";

            var table = qf.Table(_repository);
            var q = qf.Query(
                table,
                where: qf.Constraint(table.Column(Entity.TreeIndexProperty), PropertyOperator.Like, childCode)
            );

            var list = this.QueryList(q, null, eagerLoad, true);

            return list;
        }

        /// <summary>
        /// 查找指定树节点的直接子节点。
        /// </summary>
        /// <param name="treePId">需要查找的树节点的Id.</param>
        /// <param name="eagerLoad">需要贪婪加载的属性。</param>
        /// <returns></returns>
        public virtual EntityList GetByTreePId(object treePId, EagerLoadOptions eagerLoad)
        {
            var table = qf.Table(_repository);
            var q = qf.Query(
                table,
                where: table.Column(Entity.TreePIdProperty).Equal(treePId)
            );

            return this.QueryList(q, null, eagerLoad);
        }

        /// <summary>
        /// 获取指定树节点的所有父节点。
        /// </summary>
        /// <param name="treeIndex">Index of the tree.</param>
        /// <param name="eagerLoad">需要贪婪加载的属性。</param>
        /// <returns></returns>
        public virtual EntityList GetAllTreeParents(string treeIndex, EagerLoadOptions eagerLoad)
        {
            var parentIndeces = new List<string>();
            var option = _repository.TreeIndexOption;
            var parentIndex = treeIndex;
            while (true)
            {
                parentIndex = option.CalculateParentIndex(parentIndex);
                if (parentIndex != null)
                {
                    parentIndeces.Add(parentIndex);
                }
                else
                {
                    break;
                }
            }

            var table = qf.Table(_repository);
            var q = qf.Query(
                table,
                where: table.Column(Entity.TreeIndexProperty).In(parentIndeces)
            );

            return this.QueryList(q, null, eagerLoad);
        }

        [Obfuscation]
        private EntityList FetchBy(CommonQueryCriteria criteria)
        {
            return this.GetBy(criteria);
        }

        /// <summary>
        /// 常用查询的数据层实现。
        /// </summary>
        /// <param name="criteria"></param>
        public virtual EntityList GetBy(CommonQueryCriteria criteria)
        {
            var table = qf.Table(_repository);
            var q = qf.Query(table);

            var allProperties = _repository.EntityMeta.ManagedProperties.GetNonReadOnlyCompiledProperties();

            //拼装所有 Where 条件。
            bool ignoreNull = criteria.IgnoreNull;
            foreach (var group in criteria.Groups)
            {
                IConstraint groupRes = null;
                foreach (var pm in group)
                {
                    var property = allProperties.Find(pm.PropertyName);
                    if (property != null)
                    {
                        var op = pm.Operator;
                        var value = pm.Value;
                        bool ignored = false;
                        if (ignoreNull)
                        {
                            ignored = !ConditionalSql.IsNotEmpty(value);
                        }
                        else
                        {
                            if (value is string || (value == null && property.PropertyType == typeof(string)))
                            {
                                #region 如果是对空字符串进行模糊匹配，那么这个条件需要被忽略。

                                var strValue = value as string;
                                if (string.IsNullOrEmpty(strValue))
                                {
                                    switch (op)
                                    {
                                        case PropertyOperator.Like:
                                        case PropertyOperator.Contains:
                                        case PropertyOperator.StartWith:
                                        case PropertyOperator.EndWith:
                                            ignored = true;
                                            break;
                                        default:
                                            break;
                                    }
                                }

                                #endregion
                            }
                        }
                        if (!ignored)
                        {
                            var propertyRes = qf.Constraint(table.Column(property), op, value);
                            groupRes = qf.Binary(groupRes, group.Concat, propertyRes);
                        }
                    }
                }

                q.Where = qf.Binary(groupRes, criteria.Concat, q.Where);
            }

            //OrderBy
            if (!string.IsNullOrWhiteSpace(criteria.OrderBy))
            {
                var orderBy = allProperties.Find(criteria.OrderBy);
                if (orderBy != null)
                {
                    var dir = criteria.OrderByAscending ? OrderDirection.Ascending : OrderDirection.Descending;
                    q.OrderBy.Add(table.Column(orderBy), dir);
                }
            }

            return this.QueryList(q, criteria.PagingInfo, criteria.EagerLoad);
        }

        /// <summary>
        /// 子类重写此方法，来实现自己的 CountAll 方法的数据层代码。
        /// </summary>
        public virtual EntityList CountAll()
        {
            var q = qf.Query(_repository);
            return this.QueryList(q);
        }

        /// <summary>
        /// 子类重写此方法，来实现自己的 GetEntityValue 方法的数据层代码。
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public virtual LiteDataTable GetEntityValue(object entityId, string property)
        {
            var table = this.DbTable;
            var idColumn = table.PKColumn.Name;
            var column = table.Translate(property);

            var sql = new StringWriter();
            sql.Write("SELECT ");
            sql.AppendQuote(table, column);
            sql.Write(" FROM ");
            sql.AppendQuoteName(table);
            sql.Write(" WHERE ");
            sql.AppendQuote(table, idColumn);
            sql.Write(" = {0}");

            return this.QueryTable(new TableQueryArgs
            {
                FormattedSql = sql.ToString(),
                Parameters = new object[] { entityId },
            });
        }

        #endregion

        #region 其它

        internal void NotifyDbLoaded(Entity entity)
        {
            this.OnDbLoaded(entity);
        }

        /// <summary>
        /// 子类重写这个方法，用于在从数据库获取出来时，及时地加载一些额外的属性。
        /// 
        /// 注意：这个方法中只应该为一般属性计算值，不能有其它的数据访问。
        /// </summary>
        /// <param name="entity"></param>
        protected virtual void OnDbLoaded(Entity entity) { }

        private SQLColumnsGenerator _sqlColumnsGenerator;

        internal SQLColumnsGenerator SQLColumnsGenerator
        {
            get
            {
                if (_sqlColumnsGenerator == null)
                {
                    _sqlColumnsGenerator = new SQLColumnsGenerator(_repository);
                }
                return _sqlColumnsGenerator;
            }
        }

        private EntityQueryProvider _linqProvider;

        internal EntityQueryProvider LinqProvider
        {
            get { return _linqProvider; }
        }

        internal EntityList QueryListByLinq(IQueryable queryable)
        {
            return base.QueryList(queryable);
            //var q = this.CreatePropertyQuery();
            //q.CombineLinq(queryable);

            //return this.QueryList(q);
        }

        #endregion
    }
}