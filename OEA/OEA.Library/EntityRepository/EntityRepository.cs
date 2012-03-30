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
using SimpleCsla;
using SimpleCsla.Core;
using hxy;

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
    public abstract class EntityRepository : IRepository, IDbFactory, ITableInfoHost, IEntityInfoHost
    {
        private Entity _delegate;

        private Entity Delegate
        {
            get
            {
                if (this._delegate == null)
                {
                    //这里创建 delegate 时不能使用 this.New 方法，因为这样会发生 NotifyLoaded 事件。
                    this._delegate = Entity.New(this.EntityType);
                }

                return _delegate;
            }
        }

        public EntityRepository()
        {
            this._sqlColumnsGenerator = new SQLColumnsGenerator(this);
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
        /// 子类重写此方法来显式实现单参数的GetList调用
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        protected virtual EntityList GetListImplicitly(object criteria)
        {
            throw new NotSupportedException("请重写此方法，以支持隐式调用。");
        }

        /// <summary>
        /// 外界不要使用，OEA 框架自身使用。
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public EntityList __GetListImplicitly(object parameter)
        {
            return this.GetListImplicitly(parameter);
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

        protected virtual EntityList GetAllCore()
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
            var newList = this.OldList();

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
        /// 创建一个空列表。
        /// 这个空列表的IsNew标识为false，它可能正准备存入旧数据。
        /// </summary>
        /// <returns></returns>
        public EntityList OldList()
        {
            var list = this.IsRootType() ?
                DataPortal.Fetch(this.ListType, new GetRootsCriteria()) as EntityList :
                DataPortal.FetchChild(this.ListType) as EntityList;

            this.NotifyLoaded(list);

            return list;
        }

        /// <summary>
        /// 创建一个全新的列表
        /// </summary>
        /// <returns></returns>
        public EntityList NewList()
        {
            var list = this.IsRootType() ?
                DataPortal.Create(this.ListType) as EntityList :
                DataPortal.CreateChild(this.ListType) as EntityList;

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
        public EntityList NewListOrderBy<TKey>(EntityList oldList, Func<Entity, TKey> keySelector)
        {
            if (oldList == null) throw new ArgumentNullException("oldList");

            var newList = this.OldList();

            newList.RaiseListChangedEvents = false;
            newList.AddRange(oldList.OrderBy(keySelector));
            newList.RaiseListChangedEvents = true;

            return newList;
        }

        #endregion

        #region Entity

        protected virtual Entity GetByIdCore(int id)
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
        /// 把这个列表中的所有改动保存到仓库中。
        /// </summary>
        /// <param name="component"></param>
        public IEntityOrList Save(IEntityOrList component)
        {
            IEntityOrList result = component.IsDirty ? DataPortal.Update(component) : component;

            (component as IEntityOrListInternal).NotifySaved();

            component.MarkOld();

            return result;
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

        private bool _parentPropertyIndicatorLoaded;

        private IRefProperty _parentPropertyIndicator;

        public IRefProperty ParentPropertyIndicator
        {
            get
            {
                if (!this._parentPropertyIndicatorLoaded)
                {
                    var parentsIndicators = this.GetAvailableIndicators()
                        .OfType<IRefProperty>()
                        .Where(p => p.GetMeta(this.EntityType).ReferenceType == ReferenceType.Parent)
                        .ToArray();

                    if (parentsIndicators.Length > 1) throw new InvalidOperationException("一个类中只能定义一个父外键。");
                    if (parentsIndicators.Length == 1) this._parentPropertyIndicator = parentsIndicators[0];

                    this._parentPropertyIndicatorLoaded = true;
                }

                return this._parentPropertyIndicator;
            }
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
                    CacheDefinition.Instance.IsEnabled(this.EntityType);
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
                    var smallTable = EntityRowCache.Instance.CacheByParent(this.EntityType, parent, this.GetByParentId);
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
                if (this.IsRootType())
                {
                    result = AggregateRootCache.Instance.CacheById(this.EntityType, id, this.GetByIdCore);
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
                AggregateRootCache.Instance.ModifyRootEntity(entity);
            }
        }

        /// <summary>
        /// 从缓存中获取整个列表。
        /// </summary>
        /// <returns></returns>
        private IList<Entity> GetCachedTable()
        {
            return EntityRowCache.Instance.CacheAll(this.EntityType, this.GetAllCore);
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// 判断当前的实体类型是否是根对象
        /// </summary>
        /// <returns></returns>
        public bool IsRootType()
        {
            return this.EntityMeta.EntityCategory == EntityCategory.Root;
        }

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

            var connection = this.CreateDb().Connection;
            var tableInfo = this.GetTableInfo();

            var sqlCon = connection as SqlConnection;
            if (sqlCon == null) throw new ArgumentNullException("只支持 SqlServer");
            new BatchInsert(entityList, sqlCon, tableInfo).Execute();
        }

        #endregion

        #region 帮助方法

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

        EntityPropertyMeta IRepository.GetParentPropertyInfo()
        {
            return this.GetParentPropertyInfo();
        }

        #endregion

        #region IDbFactory Members

        public IDb CreateDb()
        {
            return DBHelper.CreateDb(this.ConnectionStringSettingName);
        }

        public string ConnectionStringSettingName
        {
            get { return this.Delegate.ConnectionStringSettingName; }
        }

        #endregion

        #region ITableInfoHost Members

        private ITable _tableCache;

        public ITable GetTableInfo()
        {
            if (this._tableCache == null)
            {
                var tiRepo = new TableInfoFinder(this);
                ITable fromDb = tiRepo.GetTableInfo(this.EntityType);

                lock (this)
                {
                    if (this._tableCache == null)
                    {
                        this._tableCache = fromDb;
                    }
                }
            }

            return this._tableCache;
        }

        #endregion

        #region IEntityInfoHost Members

        private EntityPropertyMeta _parentPropertyCache;

        /// <summary>
        /// 找到本对象上层父聚合对象的外键
        /// </summary>
        /// <returns></returns>
        public EntityPropertyMeta GetParentPropertyInfo()
        {
            if (this._parentPropertyCache == null)
            {
                var result = this.EntityMeta.FindParentReferenceProperty();
                if (result == null) throw new ArgumentNullException(this.EntityMeta.Name + "类还没有标记IsParent=true的外键属性。");
                this._parentPropertyCache = result;
            }

            return this._parentPropertyCache;
        }

        private ConsolidatedTypePropertiesContainer _propertiesContainer;

        /// <summary>
        /// 获取所有的托管属性信息
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public IList<IManagedProperty> GetAvailableIndicators()
        {
            if (this._propertiesContainer == null)
            {
                this._propertiesContainer = ManagedPropertyRepository.Instance
                    .GetTypePropertiesContainer(this.EntityType);
                if (this._propertiesContainer == null) throw new InvalidOperationException();
            }

            return this._propertiesContainer.GetAvailableProperties();
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
            get { return this.Delegate.SupportTree; }
        }

        public TreeCodeOption TreeCodeOption
        {
            get { return this.Delegate.TreeCodeOption; }
        }
    }
}
