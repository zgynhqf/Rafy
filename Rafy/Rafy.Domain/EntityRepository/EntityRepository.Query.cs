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
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime;
using System.Text;
using Rafy;
using Rafy.Data;
using Rafy.Reflection;
using Rafy.Domain.DataPortal;
using Rafy.Domain.Caching;
using Rafy.Domain.ORM;
using Rafy.Domain.ORM.Linq;
using Rafy.Domain.ORM.Query;
using Rafy.Domain.Validation;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.Utils;
using Rafy.Utils.Caching;

namespace Rafy.Domain
{
    partial class EntityRepository
    {
        protected EntityRepository()
        {
            this.DataPortalLocation = DataPortalLocation.Dynamic;
            _clientCache = new ClientCache(this);
            _serverCache = new ServerCache(this);
        }

        /// <summary>
        /// 获取或设置本仓库数据门户所在位置。
        /// </summary>
        public DataPortalLocation DataPortalLocation { get; protected set; }

        internal override IRepositoryInternal Repo
        {
            get { return this; }
        }

        #region 缓存

        private ClientCache _clientCache;
        private ServerCache _serverCache;

        /// <summary>
        /// 基于版本号更新的客户端缓存 API
        /// </summary>
        public ClientCache ClientCache
        {
            get { return _clientCache; }
        }

        /// <summary>
        /// 服务端内存缓存 API
        /// </summary>
        public ServerCache ServerCache
        {
            get { return _serverCache; }
        }

        #endregion

        #region 公有查询接口

        /// <summary>
        /// 优先使用缓存中的数据来查询所有的实体类。
        /// 
        /// 如果该实体的缓存没有打开，则本方法会直接调用 GetAll 并返回结果。
        /// 如果缓存中没有这些数据，则本方法同时会把数据缓存起来。
        /// </summary>
        /// <returns></returns>
        public EntityList CacheAll()
        {
            return this.DoCacheAll();
        }

        /// <summary>
        /// 优先使用缓存中的数据来通过 Id 获取指定的实体对象
        /// 
        /// 如果该实体的缓存没有启用，则本方法会直接调用 GetById 并返回结果。
        /// 如果缓存中没有这些数据，则本方法同时会把数据缓存起来。
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Entity CacheById(object id)
        {
            return this.DoCacheById(id);
        }

        /// <summary>
        /// 分页查询所有的实体类
        /// </summary>
        /// <param name="paging">分页信息。</param>
        /// <param name="eagerLoad">需要贪婪加载的属性。</param>
        /// <returns></returns>
        public EntityList GetAll(PagingInfo paging = null, EagerLoadOptions eagerLoad = null)
        {
            return this.DoGetAll(paging, eagerLoad);
        }

        /// <summary>
        /// 查询第一个实体。
        /// </summary>
        /// <param name="eagerLoad">需要贪婪加载的属性。</param>
        /// <returns></returns>
        public Entity GetFirst(EagerLoadOptions eagerLoad = null)
        {
            return this.DoGetFirst(eagerLoad);
        }

        /// <summary>
        /// 统计仓库中所有的实体数量
        /// </summary>
        /// <returns></returns>
        public int CountAll()
        {
            return this.DoCountAll();
        }

        /// <summary>
        /// 统计某个父对象下的子对象条数
        /// </summary>
        /// <returns></returns>
        public int CountByParentId(object parentId)
        {
            return this.DoCountByParentId(parentId);
        }

        /// <summary>
        /// 通过Id在数据层中查询指定的对象
        /// </summary>
        /// <param name="id">The unique identifier.</param>
        /// <param name="eagerLoad">需要贪婪加载的属性。</param>
        /// <returns></returns>
        public Entity GetById(object id, EagerLoadOptions eagerLoad = null)
        {
            return this.DoGetById(id, eagerLoad);
        }

        /// <summary>
        /// 获取指定 id 集合的实体列表。
        /// </summary>
        /// <param name="idList"></param>
        /// <param name="eagerLoad">需要贪婪加载的属性。</param>
        /// <returns></returns>
        public EntityList GetByIdList(object[] idList, EagerLoadOptions eagerLoad = null)
        {
            return this.DoGetByIdList(idList, eagerLoad);
        }

        /// <summary>
        /// 通过组合父对象的 Id 列表，查找所有的组合子对象的集合。
        /// </summary>
        /// <param name="parentIdList">The parent identifier list.</param>
        /// <param name="paging">分页信息。</param>
        /// <param name="eagerLoad">需要贪婪加载的属性。</param>
        /// <returns></returns>
        public EntityList GetByParentIdList(object[] parentIdList, PagingInfo paging = null, EagerLoadOptions eagerLoad = null)
        {
            return this.DoGetByParentIdList(parentIdList, paging, eagerLoad);
        }

        /// <summary>
        /// 通过父对象 Id 分页查询子对象的集合。
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="paging">分页信息。</param>
        /// <param name="eagerLoad">需要贪婪加载的属性。</param>
        /// <returns></returns>
        public EntityList GetByParentId(object parentId, PagingInfo paging = null, EagerLoadOptions eagerLoad = null)
        {
            return this.DoGetByParentId(parentId, paging, eagerLoad);
        }

        /// <summary>
        /// 通过 <see cref="CommonQueryCriteria"/> 来查询实体列表。
        /// </summary>
        /// <param name="criteria">常用查询条件。</param>
        /// <returns></returns>
        public EntityList GetBy(CommonQueryCriteria criteria)
        {
            return this.DoGetBy(criteria);
        }

        /// <summary>
        /// 通过 <see cref="ODataQueryCriteria"/> 来查询实体列表。
        /// </summary>
        /// <param name="criteria">常用查询条件。</param>
        /// <returns></returns>
        public EntityList GetBy(ODataQueryCriteria criteria)
        {
            return this.DoGetBy(criteria);
        }

        /// <summary>
        /// 通过 CommonQueryCriteria 来查询实体列表。
        /// </summary>
        /// <param name="criteria">常用查询条件。</param>
        /// <returns></returns>
        public Entity GetFirstBy(CommonQueryCriteria criteria)
        {
            return this.DoGetFirstBy(criteria);
        }

        /// <summary>
        /// 通过 CommonQueryCriteria 来查询实体的个数。
        /// </summary>
        /// <param name="criteria">常用查询条件。</param>
        /// <returns></returns>
        public int CountBy(CommonQueryCriteria criteria)
        {
            return this.DoCountBy(criteria);
        }

        /// <summary>
        /// 递归查找指定父索引号的节点下的所有子节点。
        /// </summary>
        /// <param name="treeIndex"></param>
        /// <param name="eagerLoad">需要贪婪加载的属性。</param>
        /// <returns></returns>
        public EntityList GetByTreeParentIndex(string treeIndex, EagerLoadOptions eagerLoad = null)
        {
            return this.DoGetByTreeParentIndex(treeIndex, eagerLoad);
        }

        /// <summary>
        /// 获取指定索引对应的树节点的所有父节点。
        /// 查询出的父节点同样以一个部分树的形式返回。
        /// </summary>
        /// <param name="treeIndex"></param>
        /// <param name="eagerLoad">需要贪婪加载的属性。</param>
        /// <returns></returns>
        public EntityList GetAllTreeParents(string treeIndex, EagerLoadOptions eagerLoad = null)
        {
            return this.FetchList<EntityRepository>(r => r.__FetchAllTreeParents(treeIndex, eagerLoad));
        }

        /// <summary>
        /// 查找指定树节点的直接子节点。
        /// </summary>
        /// <param name="treePId">需要查找的树节点的Id.</param>
        /// <param name="eagerLoad">需要贪婪加载的属性。</param>
        /// <returns></returns>
        public EntityList GetByTreePId(object treePId, EagerLoadOptions eagerLoad = null)
        {
            return this.DoGetByTreePId(treePId, eagerLoad);
        }

        /// <summary>
        /// 查询所有的根节点。
        /// </summary>
        /// <param name="eagerLoad">需要贪婪加载的属性。</param>
        /// <returns></returns>
        public EntityList GetTreeRoots(EagerLoadOptions eagerLoad = null)
        {
            return this.DoGetTreeRoots(eagerLoad);
        }

        internal string GetMaxTreeIndex()
        {
            var table  = this.FetchTable<EntityRepository>(r => r.DA_GetMaxTreeIndex());
            if (table.Rows.Count > 0)
            {
                return table.Rows[0].GetString(0);
            }
            return null;
        }
        [Obfuscation]
        private LiteDataTable DA_GetMaxTreeIndex()
        {
            var f = QueryFactory.Instance;
            var t = f.Table(this);
            var q = f.Query(
                selection: t.Column(Entity.TreeIndexProperty),
                from: t,
                orderBy: new List<IOrderBy> { f.OrderBy(t.Column(Entity.TreeIndexProperty), OrderDirection.Descending) }
            );
            return this.QueryTable(q);
        }

        /// <summary>
        /// 查询所有的根节点数量。
        /// </summary>
        /// <returns></returns>
        public int CountTreeRoots()
        {
            return this.DoCountTreeRoots();
        }

        /// <summary>
        /// 通过某个具体的参数来调用数据层查询。
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        public EntityList GetBy(object criteria)
        {
            return this.DoGetBy(criteria);
        }

        /// <summary>
        /// 查询某个实体的某个属性的值。
        /// </summary>
        /// <param name="id"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public object GetEntityValue(object id, string property)
        {
            return this.DoGetEntityValue(id, property);
        }

        #endregion

        #region 可重写的 Do 接口

        //这类接口必须保留，不能直接把公有方法标记为 virtual，否则子类重写时，会跟 .g.cs 文件中的方法冲突。

        /// <summary>
        /// 使用Cache获取所有对象。
        /// 
        /// 如果Cache中不存在时，则会主动查询数据层，并加入到缓存中。
        /// 如果缓存没有被启用，则直接查询数据层，返回数据。
        /// </summary>
        /// <returns></returns>
        protected virtual EntityList DoCacheAll()
        {
            EntityList result = null;

            if (RafyEnvironment.IsOnServer())
            {
                if (this._serverCache.IsEnabled)
                {
                    result = this._serverCache.FindAll();
                }
            }
            else
            {
                if (this._clientCache.IsEnabled)
                {
                    result = this._clientCache.FindAll();
                }
            }

            if (result != null)
            {
                this.NotifyLoaded(result);
            }
            else
            {
                result = this.DoGetAll(null, null);
            }

            return result;
        }

        /// <summary>
        /// 使用Cache获取某个父对象下的所有子对象。
        /// 
        /// 如果Cache中不存在时，则会主动查询数据层，并加入到缓存中。
        /// 如果缓存没有被启用，则直接查询数据层，返回数据。
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        protected virtual EntityList DoCacheByParent(Entity parent)
        {
            EntityList result = null;

            if (RafyEnvironment.IsOnServer())
            {
                if (this._serverCache.IsEnabled)
                {
                    result = this._serverCache.FindByParent(parent);
                }
            }
            else
            {
                if (this._clientCache.IsEnabled)
                {
                    result = this._clientCache.FindByParent(parent);
                }
            }

            if (result != null)
            {
                this.NotifyLoaded(result);
            }
            else
            {
                result = this.GetByParentId(parent.Id);
            }

            return result;
        }

        /// <summary>
        /// 使用Cache获取某个指定的对象。
        /// 
        /// 如果Cache中不存在时，则会主动查询数据层，并加入到缓存中。
        /// 如果缓存没有被启用，则直接查询数据层，返回数据。
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        protected virtual Entity DoCacheById(object id)
        {
            Entity result = null;

            if (RafyEnvironment.IsOnServer())
            {
                if (this._serverCache.IsEnabled)
                {
                    result = this._serverCache.FindById(id);
                }
            }
            else
            {
                if (this._clientCache.IsEnabled)
                {
                    result = this._clientCache.FindById(id);
                }
            }

            if (result != null)
            {
                this.NotifyLoaded(result);
            }
            else
            {
                result = this.DoGetById(id, null);
            }

            return result;
        }

        /// <summary>
        /// 获取指定父对象下的子对象集合。
        /// 
        /// 此方法会被组合父实体的 GetLazyList 方法的默认逻辑所调用。
        /// 子类重写此方法来实现 GetLazyList 的默认查询逻辑。
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        protected virtual EntityList DoGetByParent(Entity parent)
        {
            //默认逻辑为，先尝试使用缓存中的对象。
            return this.DoCacheByParent(parent);
        }

        /// <summary>
        /// 分页查询所有实体
        /// </summary>
        /// <param name="paging">分页信息。</param>
        /// <param name="eagerLoad">需要贪婪加载的属性。</param>
        /// <returns></returns>
        protected virtual EntityList DoGetAll(PagingInfo paging, EagerLoadOptions eagerLoad)
        {
            return this.FetchList(new GetAllCriteria
            {
                PagingInfo = paging,
                EagerLoad = eagerLoad
            });
        }

        /// <summary>
        /// 查询第一个实体
        /// </summary>
        /// <param name="eagerLoad">需要贪婪加载的属性。</param>
        /// <returns></returns>
        protected virtual Entity DoGetFirst(EagerLoadOptions eagerLoad)
        {
            return this.FetchFirst(new GetAllCriteria { EagerLoad = eagerLoad });
        }

        /// <summary>
        /// 通过 Id 查询某个实体
        /// </summary>
        /// <param name="id"></param>
        /// <param name="eagerLoad">需要贪婪加载的属性。</param>
        /// <returns></returns>
        protected virtual Entity DoGetById(object id, EagerLoadOptions eagerLoad)
        {
            var list = FetchList(new GetByIdCriteria()
            {
                IdValue = id,
                EagerLoad = eagerLoad
            });
            return list.Count == 1 ? list[0] : null;
        }

        /// <summary>
        /// 查询所有的根节点。
        /// </summary>
        /// <param name="eagerLoad">需要贪婪加载的属性。</param>
        /// <returns></returns>
        protected virtual EntityList DoGetTreeRoots(EagerLoadOptions eagerLoad)
        {
            return this.FetchList<EntityRepository>(r => r.__FetchTreeRoots(eagerLoad));
        }

        /// <summary>
        /// 查询所有的根节点数量。
        /// </summary>
        /// <returns></returns>
        protected virtual int DoCountTreeRoots()
        {
            return this.FetchCount<EntityRepository>(r => r.__FetchTreeRoots(null));
        }

        /// <summary>
        /// 通过 Id 列表查询实体列表。
        /// </summary>
        /// <param name="idList"></param>
        /// <param name="eagerLoad">需要贪婪加载的属性。</param>
        /// <returns></returns>
        protected virtual EntityList DoGetByIdList(object[] idList, EagerLoadOptions eagerLoad)
        {
            return this.FetchList(new GetByIdListCriteria
            {
                IdList = idList,
                EagerLoad = eagerLoad
            });
        }

        /// <summary>
        /// 通过组合父对象的 Id 列表，查找所有的组合子对象的集合。
        /// </summary>
        /// <param name="parentIdList">The parent identifier list.</param>
        /// <param name="paging">The paging information.</param>
        /// <param name="eagerLoad">需要贪婪加载的属性。</param>
        /// <returns></returns>
        protected virtual EntityList DoGetByParentIdList(object[] parentIdList, PagingInfo paging, EagerLoadOptions eagerLoad)
        {
            return this.FetchList(new GetByParentIdListCriteria
            {
                ParentIdList = parentIdList,
                PagingInfo = paging,
                EagerLoad = eagerLoad
            });
        }

        /// <summary>
        /// 通过父对象 Id 分页查询子对象的集合。
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="paging"></param>
        /// <param name="eagerLoad">需要贪婪加载的属性。</param>
        /// <returns></returns>
        protected virtual EntityList DoGetByParentId(object parentId, PagingInfo paging, EagerLoadOptions eagerLoad)
        {
            return this.FetchList(new GetByParentIdCriteria()
            {
                ParentId = parentId,
                PagingInfo = paging,
                EagerLoad = eagerLoad
            });
        }

        /// <summary>
        /// 通过 <see cref="CommonQueryCriteria"/> 来查询实体列表。
        /// </summary>
        /// <param name="criteria">常用查询条件。</param>
        /// <returns></returns>
        protected virtual EntityList DoGetBy(CommonQueryCriteria criteria)
        {
            return this.FetchList(criteria);
        }

        /// <summary>
        /// 通过 <see cref="ODataQueryCriteria"/> 来查询实体列表。
        /// </summary>
        /// <param name="criteria">常用查询条件。</param>
        /// <returns></returns>
        protected virtual EntityList DoGetBy(ODataQueryCriteria criteria)
        {
            return this.FetchList(criteria);
        }

        /// <summary>
        /// 通过 CommonQueryCriteria 来查询单一实体。
        /// </summary>
        /// <param name="criteria">常用查询条件。</param>
        /// <returns></returns>
        protected virtual Entity DoGetFirstBy(CommonQueryCriteria criteria)
        {
            return this.FetchFirst(criteria);
        }

        /// <summary>
        /// 通过 CommonQueryCriteria 来查询实体个数。
        /// </summary>
        /// <param name="criteria">常用查询条件。</param>
        /// <returns></returns>
        protected virtual int DoCountBy(CommonQueryCriteria criteria)
        {
            return this.FetchCount(criteria);
        }

        /// <summary>
        /// 递归查找所有树型子
        /// </summary>
        /// <param name="treeIndex"></param>
        /// <param name="eagerLoad">需要贪婪加载的属性。</param>
        /// <returns></returns>
        protected virtual EntityList DoGetByTreeParentIndex(string treeIndex, EagerLoadOptions eagerLoad)
        {
            return this.FetchList(new GetByTreeParentIndexCriteria() { TreeIndex = treeIndex, EagerLoad = eagerLoad });
        }

        /// <summary>
        /// 查找指定树节点的直接子节点。
        /// </summary>
        /// <param name="treePId">需要查找的树节点的Id.</param>
        /// <param name="eagerLoad">需要贪婪加载的属性。</param>
        /// <returns></returns>
        protected virtual EntityList DoGetByTreePId(object treePId, EagerLoadOptions eagerLoad)
        {
            return this.FetchList<EntityRepository>(r => r.__FetchByTreePId(treePId, eagerLoad));
        }

        /// <summary>
        /// 统计仓库中所有的实体数量
        /// </summary>
        /// <returns></returns>
        protected virtual int DoCountAll()
        {
            return this.FetchCount(new CountAllCriteria());
        }

        /// <summary>
        /// 查询某个父对象下的子对象
        /// </summary>
        /// <param name="parentId"></param>
        /// <returns></returns>
        protected virtual int DoCountByParentId(object parentId)
        {
            return this.FetchCount(new GetByParentIdCriteria() { ParentId = parentId });
        }

        /// <summary>
        /// 通过某个具体的参数来调用数据层查询。
        /// 
        /// 子类重写此方法来实现 <see cref="GetBy(object)"/> 的接口层逻辑。
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        protected virtual EntityList DoGetBy(object criteria)
        {
            return this.FetchList(criteria);
        }

        /// <summary>
        /// 通过某个具体的参数来调用数据层查询。
        /// 子类重写此方法来实现 <see cref="GetEntityValue(object,string)" /> 的接口层逻辑。
        /// </summary>
        /// <param name="id">The unique identifier.</param>
        /// <param name="property">The property.</param>
        /// <returns></returns>
        protected virtual object DoGetEntityValue(object id, string property)
        {
            var table = this.FetchTable(new GetEntityValueCriteria()
            {
                EntityId = id,
                Property = property
            });

            return table[0, 0];
        }

        #endregion

        #region FetchBy 一些通用数据层实现

        private RepositoryDataProvider _dataProvider;

        /// <summary>
        /// 数据层提供程序。
        /// </summary>
        internal protected RepositoryDataProvider DataProvider
        {
            get { return _dataProvider; }
        }

        internal void InitDataProvider(RepositoryDataProvider value)
        {
            _dataProvider = value;
        }

        /// <summary>
        /// 来自数据门户的数据查询
        /// </summary>
        /// <param name="criteria">The criteria.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidProgramException">
        /// </exception>
        internal object PortalFetch(IEQC criteria)
        {
            var methodName = criteria.MethodName;
            var parameters = criteria.Parameters;

            //对表格类查询进行处理。
            if (criteria.FetchType == FetchType.Table)
            {
                //如果方法名为空，则使用约定的方法名。
                if (methodName == null) methodName = EntityConvention.QueryMethod;

                var table = ProvideData(methodName, parameters) as LiteDataTable;
                if (table == null) { throw new InvalidProgramException(string.Format("仓库 {0} 的数据层方法：'{1}'，必须返回一个 LiteDataTable 对象。", this.GetType(), methodName)); }
                return table;
            }

            //最常用的一些查询，不使用反射：
            if (methodName == null && parameters.Length == 1)
            {
                var p = parameters[0];
                if (p is GetByIdCriteria) { return this.FetchBy(p as GetByIdCriteria); }
                if (p is GetByParentIdCriteria) { return this.FetchBy(p as GetByParentIdCriteria); }
                if (p is GetAllCriteria) { return this.FetchBy(p as GetAllCriteria); }
                if (p is GetByTreeParentIndexCriteria) { return this.FetchBy(p as GetByTreeParentIndexCriteria); }
            }

            //如果方法名为空，则使用约定的方法名。
            if (methodName == null) methodName = EntityConvention.QueryMethod;

            var list = ProvideData(methodName, parameters) as EntityList;
            if (list == null) { throw new InvalidProgramException(string.Format("仓库 {0} 的数据层方法：'{1}'，必须返回一个 EntityList 或者其子类的对象。", this.GetType(), methodName)); }

            return list;
        }

        private object ProvideData(string methodName, object[] parameters)
        {
            object result = null;

            //先尝试在 DataProvider 中调用指定的方法，如果没有找到，才会调用 Repository 中的方法。
            if (!MethodCaller.CallMethodIfImplemented(
                _dataProvider, methodName, parameters, out result
                ))
            {
                MethodCaller.CallMethodIfImplemented(
                    this, methodName, parameters, out result
                    );
            }

            return result;
        }

        #region FetchBy 接口

        [Obfuscation]
        private EntityList FetchBy(GetByIdCriteria criteria)
        {
            return _dataProvider.GetById(criteria.IdValue, null);
        }

        [Obfuscation]
        private EntityList FetchBy(GetByParentIdListCriteria criteria)
        {
            return _dataProvider.GetByParentIdList(criteria.ParentIdList, criteria.PagingInfo, criteria.EagerLoad);
        }

        [Obfuscation]
        private EntityList FetchBy(GetByParentIdCriteria criteria)
        {
            return _dataProvider.GetByParentId(criteria.ParentId, criteria.PagingInfo, criteria.EagerLoad);
        }

        [Obfuscation]
        private EntityList FetchBy(GetAllCriteria criteria)
        {
            return _dataProvider.GetAll(criteria.PagingInfo, criteria.EagerLoad);
        }

        [Obfuscation]
        private EntityList FetchBy(GetByIdListCriteria criteria)
        {
            return _dataProvider.GetByIdList(criteria.IdList, criteria.EagerLoad);
        }

        [Obfuscation]
        private EntityList FetchBy(CountAllCriteria criteria)
        {
            return _dataProvider.CountAll();
        }

        [Obfuscation]
        private EntityList FetchBy(GetByTreeParentIndexCriteria criteria)
        {
            return _dataProvider.GetByTreeParentIndex(criteria.TreeIndex, criteria.EagerLoad);
        }

        [Obfuscation]
        private LiteDataTable FetchBy(GetEntityValueCriteria criteria)
        {
            return _dataProvider.GetEntityValue(criteria.EntityId, criteria.Property);
        }

        [Obfuscation]
        private EntityList __FetchByTreePId(object treePId, EagerLoadOptions eagerLoad)
        {
            return _dataProvider.GetByTreePId(treePId, eagerLoad);
        }

        [Obfuscation]
        private EntityList __FetchAllTreeParents(string treeIndex, EagerLoadOptions eagerLoad)
        {
            return _dataProvider.GetAllTreeParents(treeIndex, eagerLoad);
        }

        [Obfuscation]
        private EntityList __FetchTreeRoots(EagerLoadOptions eagerLoad)
        {
            return _dataProvider.GetTreeRoots(eagerLoad);
        }

        [Obfuscation]
        private EntityList __FetchByIdOrTreePId(object id)
        {
            var table = qf.Table(this);
            var query = qf.Query(
                from: table,
                where: qf.Or(
                    table.Column(Entity.IdProperty).Equal(id),
                    table.Column(Entity.TreePIdProperty).Equal(id)
                ));

            return this.QueryList(query);
        }

        #endregion

        /// <summary>
        /// 所有无法找到对应单一参数的查询，都会调用此方法。
        /// 此方法会尝试使用仓库扩展类中编写的查询来响应本次查询。
        /// 
        /// 子类重写此方法来实现对所有未实现的查询。
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        protected EntityList FetchBy(object criteria)
        {
            //先尝试使用仓库扩展来满足提供查询结果。
            var result = this.FetchByExtensions(criteria);
            if (result != null) return result;

            //所有扩展检查完毕，直接抛出异常。
            throw new NotImplementedException(string.Format(
                "{1} 类需要编写一个单一参数类型为 {0} 的 FetchBy 方法并返回 EntityList 以实现数据层查询。格式如下：protected EntityList FetchBy({0} criteira) {{...}}。",
                criteria.GetType().Name,
                this.GetType().FullName
                ));
        }

        IRepositoryDataProvider IRepository.DataProvider
        {
            get { return _dataProvider; }
        }

        #endregion

        #region Fetch 接口

        /// <summary>
        /// 子类调用此方法来导向服务端执行仓库中对应参数的 FetchBy 数据层方法。
        /// </summary>
        /// <param name="parameters">对应数据层 FetchBy 方法的多个参数。</param>
        /// <returns>返回统计的行数。</returns>
        internal protected int FetchCount(params object[] parameters)
        {
            var list = DataPortalFetchList(new IEQC { FetchType = FetchType.Count, Parameters = parameters });

            var count = list.TotalCount;

            //由于自动调用 QueryAsCounting 方法，所以不需要再检测了。
            //if (count == EntityList.TotalCountInitValue)
            //{
            //    throw new InvalidProgramException(string.Format(
            //        "{0}.FetchBy({1}) 数据层方法统计行数无效！必须在调用 QueryDb 方法前先调用 QueryAsCounting 方法。",
            //        list.GetType().Name,
            //        string.Join(",", parameters.Select(c => c.GetType().Name))
            //        ));
            //}

            return count;
        }

        /// <summary>
        /// 子类在查询接口方法中，调用此方法来向服务端执行对应参数的 FetchBy 数据层方法，并返回第一个实体。
        /// </summary>
        /// <param name="parameters">对应数据层 FetchBy 方法的多个参数。</param>
        /// <returns>返回第一个满足条件的实体。</returns>
        internal protected Entity FetchFirst(params object[] parameters)
        {
            var list = DataPortalFetchList(new IEQC { FetchType = FetchType.First, Parameters = parameters });

            return DisconnectFirst(list);
        }

        /// <summary>
        /// 子类在查询接口方法中，调用此方法来向服务端执行对应参数的 FetchBy 数据层方法，并返回满足条件的实体列表。
        /// </summary>
        /// <param name="parameters">对应数据层 FetchBy 方法的多个参数。</param>
        /// <returns>返回满足条件的实体列表。</returns>
        internal protected EntityList FetchList(params object[] parameters)
        {
            return DataPortalFetchList(new IEQC { Parameters = parameters });
        }

        /// <summary>
        /// 子类在查询接口方法中，调用此方法来向服务端执行对应参数的 FetchBy 数据层方法，并返回满足条件的数据表格。
        /// </summary>
        /// <param name="parameters">对应数据层 FetchBy 方法的多个参数。</param>
        /// <returns>返回满足条件的数据表格。</returns>
        internal protected LiteDataTable FetchTable(params object[] parameters)
        {
            var ieqc = new IEQC { Parameters = parameters };

            return DataPortalFetchTable(ieqc);
        }

        /// <summary>
        /// 子类在查询接口方法中，调用此方法来导向服务端执行指定的数据层查询方法，并返回统计的行数。
        /// </summary>
        /// <typeparam name="TRepository">子仓库的类型</typeparam>
        /// <param name="dataQueryExp">调用子仓库类中定义的数据查询方法的表达式。</param>
        /// <returns>返回统计的行数。</returns>
        protected int FetchCount<TRepository>(Expression<Func<TRepository, EntityList>> dataQueryExp)
            where TRepository : EntityRepository
        {
            var ieqc = new IEQC { FetchType = FetchType.Count };

            ParseExpToIEQC(dataQueryExp, ieqc);

            return DataPortalFetchList(ieqc).TotalCount;
        }

        /// <summary>
        /// 子类在查询接口方法中，调用此方法来导向服务端执行指定的数据层查询方法，并返回第一个满足条件的实体。
        /// </summary>
        /// <typeparam name="TRepository">子仓库的类型</typeparam>
        /// <param name="dataQueryExp">调用子仓库类中定义的数据查询方法的表达式。</param>
        /// <returns>返回第一个满足条件的实体。</returns>
        protected Entity FetchFirst<TRepository>(Expression<Func<TRepository, EntityList>> dataQueryExp)
            where TRepository : EntityRepository
        {
            var ieqc = new IEQC { FetchType = FetchType.First };

            ParseExpToIEQC(dataQueryExp, ieqc);

            var list = DataPortalFetchList(ieqc);

            return DisconnectFirst(list);
        }

        /// <summary>
        /// 子类在查询接口方法中，调用此方法来向服务端执行指定的数据层查询方法，并返回满足条件的实体列表。
        /// </summary>
        /// <typeparam name="TRepository">子仓库的类型</typeparam>
        /// <param name="dataQueryExp">调用子仓库类中定义的数据查询方法的表达式。</param>
        /// <returns>返回满足条件的实体列表。</returns>
        protected EntityList FetchList<TRepository>(Expression<Func<TRepository, EntityList>> dataQueryExp)
            where TRepository : EntityRepository
        {
            var ieqc = new IEQC();

            ParseExpToIEQC(dataQueryExp, ieqc);

            return DataPortalFetchList(ieqc);
        }

        /// <summary>
        /// 子类在查询接口方法中，调用此方法来向服务端执行指定的数据层查询方法，并返回满足条件的数据表格。
        /// </summary>
        /// <typeparam name="TRepository">子仓库的类型</typeparam>
        /// <param name="dataQueryExp">调用子仓库类中定义的数据查询方法的表达式。</param>
        /// <returns>返回满足条件的数据表格。</returns>
        protected LiteDataTable FetchTable<TRepository>(Expression<Func<TRepository, LiteDataTable>> dataQueryExp)
            where TRepository : EntityRepository
        {
            var ieqc = new IEQC();

            ParseExpToIEQC(dataQueryExp, ieqc);

            return DataPortalFetchTable(ieqc);
        }

        private EntityList DataPortalFetchList(IEQC ieqc)
        {
            this.OnFetching(ieqc);

            var list = DataPortalApi.Fetch(this.GetType(), ieqc, this.DataPortalLocation) as EntityList;

            this.NotifyLoaded(list);

            return list;
        }

        private LiteDataTable DataPortalFetchTable(IEQC ieqc)
        {
            ieqc.FetchType = FetchType.Table;

            this.OnFetching(ieqc);

            return DataPortalApi.Fetch(this.GetType(), ieqc, this.DataPortalLocation) as LiteDataTable;
        }

        /// <summary>
        /// 在所有接口调用数据门户查询时调用此方法。
        /// <remarks>
        /// 子类可重写此方法实现一些查询门户方法的检查。
        /// 例如，一些仓库只允许在客户端进行调用时，可以在方法中判断，如果当前处在服务端，则抛出异常的逻辑。
        /// </remarks>
        /// </summary>
        /// <param name="criteria">当前查询的条件。</param>
        protected virtual void OnFetching(IEntityQueryCriteria criteria) { }

        /// <summary>
        /// 解析方法调用表达式，获取方法名及参数列表，存入到 IEQC 对象中。
        /// </summary>
        /// <param name="dataQueryExp"></param>
        /// <param name="ieqc"></param>
        private void ParseExpToIEQC(LambdaExpression dataQueryExp, IEQC ieqc)
        {
            dataQueryExp = Evaluator.PartialEval(dataQueryExp) as LambdaExpression;

            var methodCallExp = dataQueryExp.Body as MethodCallExpression;
            if (methodCallExp == null) ExpressionNotSupported(dataQueryExp);

            //repoExp 可以是仓库本身，也可以是 DataProvider，所以以下检测不再需要。
            //var repoExp = methodCallExp.Object as ParameterExpression;
            //if (repoExp == null || !repoExp.Type.IsInstanceOfType(this)) ExpressionNotSupported(dataQueryExp);

            //参数转换
            var arguments = methodCallExp.Arguments;
            ieqc.Parameters = new object[arguments.Count];
            for (int i = 0, c = arguments.Count; i < c; i++)
            {
                var argumentExp = arguments[i] as ConstantExpression;
                if (argumentExp == null) ExpressionNotSupported(dataQueryExp);

                //把参数的值设置到数组中，如果值是 null，则需要使用参数的类型。
                ieqc.Parameters[i] = argumentExp.Value ??
                    new MethodCaller.NullParameter { ParameterType = argumentExp.Type };
            }

            //方法转换
            ieqc.MethodName = methodCallExp.Method.Name;
        }

        private static void ExpressionNotSupported(LambdaExpression dataQueryExp)
        {
            throw new NotSupportedException(string.Format("表达式 {0} 的格式不满足规范，只支持对仓库实例方法的简单调用。", dataQueryExp));
        }

        /// <summary>
        /// 只返回列表中的唯一实体时，使用此方法。可防止 EntityList 内存泄漏。
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private static Entity DisconnectFirst(EntityList list)
        {
            Entity res = null;

            if (list.Count > 0)
            {
                res = list[0];
                res.DisconnectFromParent();
            }

            return res;
        }

        #endregion

        #region NotifyLoaded

        /// <summary>
        /// 当一个实体最终要出仓库时，才调用此方法完成加载。
        /// </summary>
        /// <param name="entity">The entity.</param>
        internal protected void NotifyLoaded(Entity entity)
        {
            if (entity != null)
            {
                entity.NotifyLoaded(this);

                this.NotifyLoadedIfMemory(entity);
            }
        }

        internal virtual void NotifyLoadedIfMemory(Entity entity) { }

        /// <summary>
        /// 当一个实体列表最终要出仓库时，才调用此方法完成加载。
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        internal protected void NotifyLoaded(EntityList list)
        {
            if (list != null)
            {
                list.NotifyLoaded(this);

                list.EachNode(e =>
                {
                    this.NotifyLoaded(e);
                    return false;
                });
            }
        }

        #endregion

        #region IRepositoryInternal

        EntityList IRepositoryInternal.GetLazyListByParent(Entity parent)
        {
            return this.DoGetByParent(parent);
        }

        /// <summary>
        /// 把一行数据转换为一个实体。
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        Entity IRepositoryInternal.ConvertRow(Entity row)
        {
            var entity = Entity.New(row.GetType());
            entity.PersistenceStatus = PersistenceStatus.Unchanged;

            //返回的子对象的属性只是简单的完全Copy参数data的数据。
            entity.Clone(row, CloneOptions.ReadDbRow());

            this.NotifyLoaded(entity);

            return entity;
        }

        EntityList IRepositoryInternal.GetByIdOrTreePId(object id)
        {
            var list =  this.FetchList<EntityRepository>(r => r.__FetchByIdOrTreePId(id));
            if (list.Count > 0)
            {
                list[0].TreeChildren.MarkLoaded();
            }

            return list;
        }

        #endregion
    }
}