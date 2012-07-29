/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20101101
 * 说明：所有仓库类的基类
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20101101
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using OEA.Library.Caching;
using OEA.ManagedProperty;
using OEA.MetaModel;
using OEA.ORM;
using OEA.Utils;
using OEA;
using hxy;
using OEA.Library.Validation;
using OEA.Utils.Caching;
using hxy.Common.Data;

namespace OEA.Library
{
    /// <summary>
    /// 仓库类
    /// 用于某个实体类型及其实体列表类的管理
    /// 
    /// 注意：
    /// 1. 其子类必须是线程安全的！
    /// 2. 子类的构建函数建议使用protected，不要向外界暴露，全部通过仓库工厂获取。
    /// </summary>
    public abstract class EntityRepository : IRepository, IDbFactory, IEntityInfoHost, ITypeValidationsHost
    {
        private Entity _delegate;

        public EntityRepository()
        {
            this._sqlColumnsGenerator = new SQLColumnsGenerator(this);
        }

        private Entity Delegate
        {
            get
            {
                //这个字段必须使用懒加载的方式，否则如果在构造函数中执行时，
                //对于系统自动生成的 Repository，会无法在动态程序集中找到对应的实体类而出现异常。
                if (this._delegate == null)
                {
                    //这里创建 delegate 时不能使用 this.New 方法，因为这样会发生 NotifyLoaded 事件。
                    this._delegate = Entity.New(this.EntityType);
                }

                return this._delegate;
            }
        }

        #region Merged API

        /// <summary>
        /// 通过数据层获取指定父对象下的子对象集合。
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        public virtual EntityList GetByParent(Entity parent, bool withCache = true)
        {
            if (withCache) { return this.CacheByParent(parent); }

            var list = this.GetByParentId(parent.Id);
            list.SetParentEntity(parent);
            return list;
        }

        /// <summary>
        /// 查询所有的实体类
        /// </summary>
        /// <returns></returns>
        public EntityList GetAll(bool withCache = true)
        {
            if (withCache) { return this.CacheAll(); }

            return this.GetAllCore();
        }

        /// <summary>
        /// 统计仓库中所有的实体数量
        /// </summary>
        /// <returns></returns>
        public int CountAll()
        {
            var svc = new CountAllEntityService { EntityType = this.EntityType };
            svc.Invoke();
            return svc.Count;
        }

        /// <summary>
        /// 通过Id在数据层中查询指定的对象
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual Entity GetById(int id, bool withCache = true)
        {
            if (withCache) { return this.CacheById(id); }

            return this.GetByIdCore(id);
        }

        /// <summary>
        /// 外界不要使用，OEA 框架自身使用。
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public EntityList __GetListImplicitly(object criteria)
        {
            return this.GetBy(criteria);
        }

        /// <summary>
        /// 子类重写此方法来实现隐式调用的自定义逻辑。
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        protected virtual EntityList GetBy(object criteria)
        {
            return this.FetchList(criteria);
        }

        ///// <summary>
        ///// 此方法用于显式实现原有的隐式GetList调用
        ///// 
        ///// 子类重写以显式调用。
        ///// </summary>
        ///// <param name="parameters"></param>
        ///// <returns></returns>
        //protected virtual EntityList GetListImplicitly(params object[] parameters)
        //{
        //    if (parameters.Length == 0)
        //    {
        //        return this.GetAll();
        //    }

        //    if (parameters.Length == 1)
        //    {
        //        var result = this.GetListImplicitly(parameters[0]);
        //        if (result != null) return result;
        //    }

        //    throw new NotSupportedException("请重写此方法或者相应的重载，以支持隐式调用。");
        //}

        #endregion

        #region List

        internal protected virtual EntityList GetAllCore()
        {
            return FetchList(new GetAllCriteria());
        }

        /// <summary>
        /// 查询某个父对象下的子对象
        /// </summary>
        /// <param name="parentId"></param>
        /// <returns></returns>
        public virtual EntityList GetByParentId(int parentId)
        {
            return this.FetchList(new GetByParentIdCriteria() { Id = parentId });
        }

        /// <summary>
        /// 递归查找所有树型子
        /// </summary>
        /// <param name="treeCode"></param>
        /// <returns></returns>
        public virtual EntityList GetByTreeParentCode(string treeCode)
        {
            return this.FetchList(new GetByTreeParentCodeCriteria() { TreeCode = treeCode });
        }

        /// <summary>
        /// 把一个 table 转换为新的实体列表
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        protected EntityList ConvertTable(IList<Entity> table)
        {
            var newList = this.NewList();

            newList.RaiseListChangedEvents = false;
            for (int i = 0, c = table.Count; i < c; i++)
            {
                var item = this.ConvertRow(table[i]);
                newList.Add(item);
            }
            newList.RaiseListChangedEvents = true;

            return newList;
        }

        /// <summary>
        /// 创建一个全新的列表
        /// </summary>
        /// <returns></returns>
        public EntityList NewList()
        {
            var list = Activator.CreateInstance(this.ListType) as EntityList;

            this.NotifyLoaded(list);

            return list;
        }

        /// <summary>
        /// 把旧的实体列表中的实体按照一定的排序规则，排序后组装一个新的列表返回
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="oldList"></param>
        /// <param name="keySelector"></param>
        /// <returns></returns>
        public EntityList NewListOrderBy<TKey>(IEnumerable<Entity> oldList, Func<Entity, TKey> keySelector)
        {
            if (oldList == null) throw new ArgumentNullException("oldList");

            var newList = this.NewList();

            newList.RaiseListChangedEvents = false;
            newList.AddRange(oldList.OrderBy(keySelector));
            newList.RaiseListChangedEvents = true;

            return newList;
        }

        #endregion

        #region Entity

        internal protected virtual Entity GetByIdCore(int id)
        {
            var list = FetchList(new GetByIdCriteria() { Id = id });
            return list.Count == 1 ? list[0] : null;
        }

        /// <summary>
        /// 把一行数据转换为一个实体。
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public Entity ConvertRow(Entity row)
        {
            var entity = Entity.New(row.GetType());
            entity.Status = PersistenceStatus.Unchanged;

            //返回的子对象的属性只是简单的完全Copy参数data的数据。
            entity.Clone(row, CloneOptions.ReadDbRow());

            this.NotifyLoaded(entity);

            return entity;
        }

        /// <summary>
        /// 创建一个新的实体。
        /// 
        /// 如果在已经获取 Repository 的场景下，使用本方法返回的实体会设置好内部的 Repository 属性，
        /// 这会使得 FindRepositor 方法更加快速。
        /// </summary>
        /// <returns></returns>
        public Entity New()
        {
            var entity = Entity.New(this.EntityType);

            this.NotifyLoaded(entity);

            return entity;
        }

        #endregion

        /// <summary>
        /// 把这个组件中的所有改动保存到仓库中。
        /// 
        /// 一般场景下，主要使用该方法保存聚合根对象
        /// </summary>
        /// <param name="component">
        /// 传入参数：需要保存的实体/实体列表。
        /// 传出结果：保存完成后的实体/实体列表。注意，它与传入的对象并不是同一个对象。
        /// </param>
        public void Save<T>(ref T component)
            where T : class, IEntityOrList
        {
            component = this.Save(component, false) as T;
        }

        /// <summary>
        /// 把这个组件中的所有改动保存到仓库中。
        /// 
        /// 方法返回保存结束的结果对象（保存可能会涉及到跨网络，所以服务端传输回来的值并不是之前传入的对象。）
        /// </summary>
        /// <param name="component"></param>
        /// <param name="markOld">是否需要把参数对象保存为未修改状态</param>
        /// <returns></returns>
        public IEntityOrList Save(IEntityOrList component, bool markOld = true)
        {
            IEntityOrList result = null;

            if (component is Entity)
            {
                var entity = component as Entity;
                bool isNewEntity = entity != null && entity.IsNew;

                var entity2 = component.IsDirty ? DataPortal.Update(component) as Entity : entity;

                if (markOld)
                {
                    if (isNewEntity) { this.MergeIdRecur(entity, entity2 as Entity); }

                    component.MarkOld();
                }

                result = entity2;
            }
            else
            {
                var list = component as EntityList;

                //保存实体列表时，需要把所有新加的实体的 Id 都设置好。
                List<int> newEntitiesIndeces = null;
                if (markOld)
                {
                    newEntitiesIndeces = new List<int>();
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (list[i].IsNew) { newEntitiesIndeces.Add(i); }
                    }
                }

                var list2 = component.IsDirty ? DataPortal.Update(component) as EntityList : list;

                if (markOld)
                {
                    foreach (var i in newEntitiesIndeces)
                    {
                        this.MergeIdRecur(list[i], list2[i]);
                    }

                    component.MarkOld();
                }

                result = list2;
            }

            (component as IEntityOrListInternal).NotifySaved();

            return result;
        }

        /// <summary>
        /// 把整个聚合对象的 Id 设置完整。
        /// </summary>
        /// <param name="oldEntity"></param>
        /// <param name="newEntity"></param>
        private void MergeIdRecur(Entity oldEntity, Entity newEntity)
        {
            oldEntity.LoadProperty(Entity.IdProperty, newEntity.GetProperty(Entity.IdProperty));

            foreach (var field in oldEntity.GetLoadedChildren())
            {
                var oldChildren = field.Value as EntityList;
                var newChildren = newEntity.GetLazyList(field.Property as IListProperty) as EntityList;

                //两个集合应该是一样的数据、顺序？如果后期出现 bug，则修改此处的逻辑为查找到对应项再修改。
                for (int i = 0, c = oldChildren.Count; i < c; i++)
                {
                    this.MergeIdRecur(oldChildren[i], newChildren[i]);
                }

                oldChildren.SyncParentEntityId(oldEntity);
            }
        }

        protected Entity NotifyLoaded(Entity entity)
        {
            if (entity != null)
            {
                entity.NotifyLoaded(this);
                this.NotifyLoadedIfMemory(entity);
            }

            return entity;
        }

        internal virtual void NotifyLoadedIfMemory(Entity entity) { }

        protected EntityList NotifyLoaded(EntityList list)
        {
            if (list != null)
            {
                list.NotifyLoaded(this);

                if (list.Count > 0)
                {
                    foreach (var entity in list)
                    {
                        this.NotifyLoaded(entity);
                    }
                }
            }

            return list;
        }

        #region Caching

        /// <summary>
        /// 指定当前的仓库是否支持Cache
        /// </summary>
        private bool IsCacheEnabled
        {
            get
            {
                return EntityListVersion.Repository != null &&
                    OEAEnvironment.Location.IsOnClient() &&
                    this.EntityMeta.CacheDefinition != null;
            }
        }

        /// <summary>
        /// 使用Cache获取所有对象。
        /// （如果Cache中不存在时，则会主动查询数据层。）
        /// </summary>
        /// <returns></returns>
        protected virtual EntityList CacheAll()
        {
            if (this.IsCacheEnabled)
            {
                var table = GetCachedTable();

                if (table != null)
                {
                    var list = this.ConvertTable(table);
                    this.NotifyLoaded(list);
                    return list;
                }
            }

            return this.GetAllCore();
        }

        /// <summary>
        /// 使用Cache获取某个父对象下的所有子对象。
        /// （如果Cache中不存在时，则会主动查询数据层。）
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        protected virtual EntityList CacheByParent(Entity parent)
        {
            EntityList children = null;

            if (this.IsCacheEnabled)
            {
                //如果是在客户端，则先检测缓存。
                if (OEAEnvironment.Location.IsOnClient())
                {
                    var smallTable = this.GetCachedTable(parent);
                    if (smallTable != null)
                    {
                        children = this.ConvertTable(smallTable);
                    }
                }
            }

            //如果缓存中没有，则直接获取数据库
            if (children == null)
            {
                children = this.GetByParentId(parent.Id);
            }
            else
            {
                this.NotifyLoaded(children);
            }

            children.SetParentEntity(parent);

            return children;
        }

        /// <summary>
        /// 使用Cache获取某个指定的对象。
        /// （如果Cache中不存在时，则会主动查询数据层。）
        /// 
        /// 注意：
        /// 根对象和子对象分别以不同的方式进行处理：
        /// 根对象：使用单个根对象的内存缓存。
        /// 子对象：在子对象的集合中查询。
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        protected virtual Entity CacheById(int id)
        {
            Entity result = null;

            if (this.IsCacheEnabled)
            {
                if (this.EntityMeta.EntityCategory == EntityCategory.Root)
                {
                    result = AggregateRootCache.Instance.CacheById(this, id);
                }
                else
                {
                    var table = this.GetCachedTable();

                    var row = table.FirstOrDefault(e => e.Id == id);

                    if (row != null)
                    {
                        result = this.ConvertRow(row);
                    }
                }
            }

            if (result == null)
            {
                result = this.GetByIdCore(id);
            }
            else
            {
                NotifyLoaded(result);
            }

            return result;
        }

        /// <summary>
        /// 直接设置根对象为缓存
        /// </summary>
        /// <param name="entity"></param>
        public void CacheRootEntity(Entity entity)
        {
            if (this.IsCacheEnabled)
            {
                AggregateRootCache.Instance.ModifyRootEntity(this, entity);
            }
        }

        /// <summary>
        /// 从缓存中获取整个列表。
        /// 
        /// 从缓存中读取指定实体类型的所有数据。
        /// 如果缓存中不存在，或者缓存数据已经过期，则调用ifNotExsits方法获取数据，并把最终数据加入到缓存中。
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="ifNotExsits"></param>
        /// <returns></returns>
        private IList<Entity> GetCachedTable()
        {
            IList<Entity> result = null;

            //目前只是在客户端使用了缓存。
            if (OEAEnvironment.Location.IsOnClient())
            {
                CacheScope sd = this.EntityMeta.CacheDefinition;
                if (sd != null)
                {
                    var className = sd.Class.Name;
                    var key = "All";

                    //如果内存不存在此数据，则尝试使用硬盘缓存获取
                    result = CacheInstance.MemoryDisk.Get(key, className) as IList<Entity>;
                    if (result == null)
                    {
                        result = this.GetAllCore();

                        var policy = new Policy()
                        {
                            Checker = new VersionChecker(sd.Class)
                        };
                        CacheInstance.MemoryDisk.Add(key, result, policy, className);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 从缓存中读取指定实体类型的某个父对象下的所有子对象。
        /// 如果缓存中不存在，或者缓存数据已经过期，则调用ifNotExsits方法获取数据，并把最终数据加入到缓存中。
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="parent"></param>
        /// <param name="ifNotExsits"></param>
        /// <returns></returns>
        private IList<Entity> GetCachedTable(Entity parent)
        {
            IList<Entity> result = null;

            if (OEAEnvironment.Location.IsOnClient())
            {
                CacheScope sd = this.EntityMeta.CacheDefinition;
                if (sd != null)
                {
                    var className = sd.Class.Name;
                    var key = string.Format("ByParentId_{0}", parent.Id);
                    result = CacheInstance.SqlCe.Get(key, className) as IList<Entity>;

                    if (result == null)
                    {
                        result = this.GetByParentId(parent.Id);
                        var scopeId = sd.ScopeIdGetter(parent);
                        var policy = new Policy()
                        {
                            Checker = new VersionChecker(sd.Class, sd.ScopeClass, scopeId)
                        };

                        CacheInstance.SqlCe.Add(key, result, policy, className);
                    }
                }
            }

            return result;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// 被本仓库管理的列表类型
        /// </summary>
        /// <returns></returns>
        public Type ListType
        {
            get
            {
                var item = this.GetConventionItem();
                return item.ListType;
            }
        }

        /// <summary>
        /// 被本仓库管理的实体类型
        /// </summary>
        /// <returns></returns>
        public Type EntityType
        {
            get
            {
                var item = this.GetConventionItem();
                return item.EntityType;
            }
        }

        private EntityMatrix _cacheConvention;

        /// <summary>
        /// 获取当前的实体类型组合
        /// </summary>
        /// <returns></returns>
        protected EntityMatrix GetConventionItem()
        {
            if (this._cacheConvention == null)
            {
                this._cacheConvention = GetConventionItemCore();
            }
            return this._cacheConvention;
        }

        /// <summary>
        /// 由于这个类是被多个实体类共用，所以需要带上这个字段以区分。
        /// </summary>
        internal Type RealEntityType;

        /// <summary>
        /// 判断当前的类型是否是默认的仓库
        /// </summary>
        /// <returns></returns>
        public bool IsDefalutRepository()
        {
            return this.RealEntityType != null;
        }

        /// <summary>
        /// 查询实体类型组合
        /// </summary>
        /// <returns></returns>
        internal virtual EntityMatrix GetConventionItemCore()
        {
            if (this.RealEntityType != null)
            {
                return EntityConvention.FindByEntity(this.RealEntityType);
            }

            //默认使用约定查询出实体类型组合
            return EntityConvention.FindByRepository(this.GetType());
        }

        #endregion

        #region 聚合SQL

        private SQLColumnsGenerator _sqlColumnsGenerator;

        public SQLColumnsGenerator SQLColumnsGenerator
        {
            get
            {
                return this._sqlColumnsGenerator;
            }
        }

        /// <summary>
        /// 数据行中的列名必须由 SQLColumnsGenerator 生成的列名对应。
        /// </summary>
        /// <param name="rowData"></param>
        /// <returns></returns>
        public Entity GetFromRow(DataRow rowData)
        {
            return this.SQLColumnsGenerator.ReadDataDirectly(rowData);
        }

        #endregion

        #region Batch SQL

        public void AddBatch(IList<Entity> entityList)
        {
            if (!OEAEnvironment.Location.IsOnServer()) throw new InvalidOperationException("!OEAEnvironment.IsOnServer() must be false.");
            if (entityList.Count < 1) throw new ArgumentOutOfRangeException();

            var connection = this.CreateDb().DBA.Connection;
            var tableInfo = this.GetORMTable();

            var sqlCon = connection as SqlConnection;
            if (sqlCon == null) throw new ArgumentNullException("只支持 SqlServer");
            new BatchInsert(entityList, sqlCon, tableInfo).Execute();
        }

        #endregion

        #region 获取对象接口方法

        protected TEntityList FetchListCast<TEntityList>(object criteria)
            where TEntityList : EntityList
        {
            return this.FetchList(criteria) as TEntityList;
        }

        protected TEntity FetchFirstAs<TEntity>(object criteria)
            where TEntity : Entity
        {
            return this.FetchFirst(criteria) as TEntity;
        }

        protected EntityList FetchList(object criteria)
        {
            var list = DataPortal.Fetch(this.ListType, criteria) as EntityList;

            this.NotifyLoaded(list);

            return list;
        }

        protected Entity FetchFirst(object criteria)
        {
            var list = this.FetchList(criteria);

            this.NotifyLoaded(list);

            return list.Count > 0 ? list[0] : null;
        }

        #endregion

        #region 获取对象接口方法 - CommonPropertiesCriteria

        protected TEntityList FetchListCast<TEntityList>(Dictionary<IManagedProperty, object> propertyValues)
            where TEntityList : EntityList
        {
            return this.FetchListCast<TEntityList>(PropertyCriteria(propertyValues));
        }

        protected TEntity FetchFirstAs<TEntity>(Dictionary<IManagedProperty, object> propertyValues)
            where TEntity : Entity
        {
            return this.FetchFirstAs<TEntity>(PropertyCriteria(propertyValues));
        }

        protected EntityList FetchList(Dictionary<IManagedProperty, object> propertyValues)
        {
            return this.FetchList(PropertyCriteria(propertyValues));
        }

        protected Entity FetchFirst(Dictionary<IManagedProperty, object> propertyValues)
        {
            return this.FetchFirst(PropertyCriteria(propertyValues));
        }

        private static object PropertyCriteria(Dictionary<IManagedProperty, object> propertyValues)
        {
            var c = new CommonPropertiesCriteria();
            foreach (var kv in propertyValues)
            {
                c.Values[kv.Key.Name] = kv.Value;
            }
            return c;
        }

        #endregion

        #region ILazyProvider Members
        //以下内定都是Entity中所要使用的懒加载查询，
        //默认都直接使用缓存进行查询

        Entity IRepository.GetById(int id)
        {
            return this.GetById(id);
        }

        EntityList IRepository.GetByParent(Entity parent)
        {
            return this.GetByParent(parent);
        }

        #endregion

        #region IDbFactory Members

        public IDb CreateDb()
        {
            return Db.Create(this.DbSetting);
        }

        private DbSetting _dbSetting;
        /// <summary>
        /// 数据库配置（每个库有一个唯一的配置名）
        /// </summary>
        public DbSetting DbSetting
        {
            get
            {
                if (this._dbSetting == null)
                {
                    var conSetting = this.Delegate.ConnectionStringSettingName;
                    this._dbSetting = DbSetting.FindOrCreate(conSetting);
                }
                return this._dbSetting;
            }
        }

        /// <summary>
        /// OEA 内部使用!!!
        /// 
        /// 这个字段用于存储运行时解析出来的 ORM 信息。
        /// 本字段只为提升性能。
        /// </summary>
        private OEA.ORM.DbTable _ormRuntime;

        /// <summary>
        /// 获取该实体对应的数据库 映射信息运行时对象
        /// </summary>
        /// <returns></returns>
        public ITable GetORMTable()
        {
            if (this._ormRuntime == null)
            {
                this._ormRuntime = DbTableFactory.CreateORMTable(this);
            }

            return this._ormRuntime;
        }

        #endregion

        #region IEntityInfoHost Members

        private bool _parentPropertyCacheLoaded;
        private EntityPropertyMeta _parentPropertyCache;
        public EntityPropertyMeta FindParentPropertyInfo(bool throwOnNotFound = false)
        {
            if (!this._parentPropertyCacheLoaded)
            {
                var result = this.EntityMeta.FindParentReferenceProperty();
                if (result != null)
                {
                    this._parentPropertyCache = result;
                    this._parentPropertyCacheLoaded = true;
                }
            }

            if (this._parentPropertyCache == null && throwOnNotFound)
            {
                throw new NotSupportedException(this.EntityType + " 类型中没有注册引用类型为 ReferenceType.Parent 的父引用属性。");
            }

            return this._parentPropertyCache;
        }

        private ConsolidatedTypePropertiesContainer _propertiesContainer;
        internal ConsolidatedTypePropertiesContainer PropertiesContainer
        {
            get
            {
                if (this._propertiesContainer == null)
                {
                    this._propertiesContainer = ManagedPropertyRepository.Instance
                        .GetTypePropertiesContainer(this.EntityType);
                    if (this._propertiesContainer == null) throw new InvalidOperationException();
                }

                return _propertiesContainer;
            }
        }
        /// <summary>
        /// 获取所有的托管属性信息
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public IList<IManagedProperty> GetAvailableIndicators()
        {
            return this.PropertiesContainer.GetAvailableProperties();
        }

        private EntityMeta _entityInfo;
        public EntityMeta EntityMeta
        {
            get
            {
                if (this._entityInfo == null)
                {
                    this._entityInfo = CommonModel.Entities.Get(this.EntityType);
                }

                return this._entityInfo;
            }
        }

        #endregion

        public bool SupportTree
        {
            get { return this.EntityMeta.IsTreeEntity; }
        }

        public TreeCodeOption TreeCodeOption
        {
            get { return this.EntityMeta.TreeCodeOption ?? TreeCodeOption.Default; }
        }

        #region ITypeValidationsHost Members

        private bool _typeRulesLoad = false;

        private ValidationRulesManager _typeRules;

        ValidationRulesManager ITypeValidationsHost.Rules
        {
            get
            {
                if (!this._typeRulesLoad)
                {
                    this._typeRulesLoad = true;

                    this._typeRules = new ValidationRulesManager();

                    //在第一次创建时，添加类型的业务规则
                    //注意，这个方法可能会调用到 Rules 属性获取刚才设置在 _typeRules 上的 ValidationRulesManager。
                    this.Delegate.AddValidations();

                    //如果没有一个规则，则把这个属性删除。
                    if (this._typeRules.PropertyRules.Count == 0 && this._typeRules.TypeRules.GetList(false).Count == 0)
                    {
                        this._typeRules = null;
                    }
                }

                return this._typeRules;
            }
        }

        #endregion
    }
}