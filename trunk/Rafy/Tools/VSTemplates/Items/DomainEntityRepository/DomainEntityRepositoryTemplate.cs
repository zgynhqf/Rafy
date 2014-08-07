

    partial class $domainEntityName$Repository
    {
        #region 私有方法，本类内部使用

        /// <summary>
        /// 在查询接口方法中，调用此方法来向服务端执行对应参数的 FetchBy 数据层方法，并返回第一个实体。
        /// </summary>
        /// <param name="parameters">可传递多个参数。</param>
        /// <returns>返回服务端返回的列表中的第一个实体。</returns>
        [DebuggerStepThrough]
        private new $domainEntityName$ FetchFirst(params object[] parameters)
        {
            return base.FetchFirst(parameters) as $domainEntityName$;
        }

        /// <summary>
        /// 在查询接口方法中，调用此方法来向服务端执行对应参数的 FetchBy 数据层方法，并返回满足条件的实体列表。
        /// </summary>
        /// <param name="parameters">可传递多个参数。</param>
        /// <returns>返回满足条件的实体列表。</returns>
        [DebuggerStepThrough]
        private new $domainEntityName$List FetchList(params object[] parameters)
        {
            return base.FetchList(parameters) as $domainEntityName$List;
        }

        /// <summary>
        /// 在查询接口方法中，调用此方法来导向服务端执行指定的数据层查询方法，并返回统计的行数。
        /// </summary>
        /// <param name="dataQueryExp">调用子仓库类中定义的数据查询方法的表达式。</param>
        /// <returns>返回统计的行数。</returns>
        [DebuggerStepThrough]
        protected int FetchCount(Expression<Func<$domainEntityName$Repository, EntityList>> dataQueryExp)
        {
            return this.FetchCount<$domainEntityName$Repository>(dataQueryExp);
        }

        /// <summary>
        /// 在查询接口方法中，调用此方法来导向服务端执行指定的数据层查询方法，并返回第一个满足条件的实体。
        /// </summary>
        /// <param name="dataQueryExp">调用仓库类中定义的数据查询方法的表达式。</param>
        /// <returns>返回第一个满足条件的实体。</returns>
        [DebuggerStepThrough]
        private $domainEntityName$ FetchFirst(Expression<Func<$domainEntityName$Repository, EntityList>> dataQueryExp)
        {
            return this.FetchFirst<$domainEntityName$Repository>(dataQueryExp) as $domainEntityName$;
        }

        /// <summary>
        /// 在查询接口方法中，调用此方法来向服务端执行指定的数据层查询方法，并返回满足条件的实体列表。
        /// </summary>
        /// <param name="dataQueryExp">调用仓库类中定义的数据查询方法的表达式。</param>
        /// <returns>返回满足条件的实体列表。</returns>
        [DebuggerStepThrough]
        private $domainEntityName$List FetchList(Expression<Func<$domainEntityName$Repository, EntityList>> dataQueryExp)
        {
            return this.FetchList<$domainEntityName$Repository>(dataQueryExp) as $domainEntityName$List;
        }

        /// <summary>
        /// 在查询接口方法中，调用此方法来向服务端执行指定的数据层查询方法，并返回满足条件的数据表格。
        /// </summary>
        /// <param name="dataQueryExp">调用仓库类中定义的数据查询方法的表达式。</param>
        /// <returns>返回满足条件的数据表格。</returns>
        [DebuggerStepThrough]
        private LiteDataTable FetchTable(Expression<Func<$domainEntityName$Repository, LiteDataTable>> dataQueryExp)
        {
            return this.FetchTable<$domainEntityName$Repository>(dataQueryExp);
        }

        /// <summary>
        /// 创建一个实体类的 Linq 查询器
        /// </summary>
        /// <returns></returns>
        [DebuggerStepThrough]
        private IQueryable<$domainEntityName$> CreateLinqQuery()
        {
            return base.CreateLinqQuery<$domainEntityName$>();
        }

        #endregion

        #region 强类型公有接口

        /// <summary>
        /// 创建一个新的实体。
        /// </summary>
        /// <returns></returns>
        [DebuggerStepThrough]
        public new $domainEntityName$ New()
        {
            return base.New() as $domainEntityName$;
        }

        /// <summary>
        /// 创建一个全新的列表
        /// </summary>
        /// <returns></returns>
        [DebuggerStepThrough]
        public new $domainEntityName$List NewList()
        {
            return base.NewList() as $domainEntityName$List;
        }

        /// <summary>
        /// 优先使用缓存中的数据来通过 Id 获取指定的实体对象
        /// 
        /// 如果该实体的缓存没有打开，则本方法会直接调用 GetById 并返回结果。
        /// 如果缓存中没有这些数据，则本方法同时会把数据缓存起来。
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public new $domainEntityName$ CacheById(object id)
        {
            return base.CacheById(id) as $domainEntityName$;
        }

        /// <summary>
        /// 优先使用缓存中的数据来查询所有的实体类
        /// 
        /// 如果该实体的缓存没有打开，则本方法会直接调用 GetAll 并返回结果。
        /// 如果缓存中没有这些数据，则本方法同时会把数据缓存起来。
        /// </summary>
        /// <returns></returns>
        [DebuggerStepThrough]
        public new $domainEntityName$List CacheAll()
        {
            return base.CacheAll() as $domainEntityName$List;
        }

        /// <summary>
        /// 通过Id在数据层中查询指定的对象
        /// </summary>
        /// <param name="id"></param>
        /// <param name="eagerLoad">需要贪婪加载的属性。</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public new $domainEntityName$ GetById(object id, EagerLoadOptions eagerLoad = null)
        {
            return base.GetById(id, eagerLoad) as $domainEntityName$;
        }

        /// <summary>
        /// 查询第一个实体类
        /// </summary>
        /// <param name="eagerLoad">需要贪婪加载的属性。</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public new $domainEntityName$ GetFirst(EagerLoadOptions eagerLoad = null)
        {
            return base.GetFirst(eagerLoad) as $domainEntityName$;
        }

        /// <summary>
        /// 分页查询所有的实体类
        /// </summary>
        /// <param name="paging"></param>
        /// <param name="eagerLoad">需要贪婪加载的属性。</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public new $domainEntityName$List GetAll(PagingInfo paging= null, EagerLoadOptions eagerLoad = null)
        {
            return base.GetAll(paging, eagerLoad) as $domainEntityName$List;
        }

        /// <summary>
        /// 获取指定 id 集合的实体列表。
        /// </summary>
        /// <param name="idList"></param>
        /// <param name="eagerLoad">需要贪婪加载的属性。</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public new $domainEntityName$List GetByIdList(object[] idList, EagerLoadOptions eagerLoad = null)
        {
            return base.GetByIdList(idList, eagerLoad) as $domainEntityName$List;
        }

        /// <summary>
        /// 通过组合父对象的 Id 列表，查找所有的组合子对象的集合。
        /// </summary>
        /// <param name="parentIdList"></param>
        /// <param name="paging">分页信息。</param>
        /// <param name="eagerLoad">需要贪婪加载的属性。</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public new $domainEntityName$List GetByParentIdList(object[] parentIdList, PagingInfo paging = null, EagerLoadOptions eagerLoad = null)
        {
            return base.GetByParentIdList(parentIdList, paging, eagerLoad) as $domainEntityName$List;
        }

        /// <summary>
        /// 通过父对象 Id 分页查询子对象的集合。
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="paging">分页信息。</param>
        /// <param name="eagerLoad">需要贪婪加载的属性。</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public new $domainEntityName$List GetByParentId(object parentId, PagingInfo paging = null, EagerLoadOptions eagerLoad = null)
        {
            return base.GetByParentId(parentId, paging, eagerLoad) as $domainEntityName$List;
        }
    
        /// <summary>
        /// 通过 CommonQueryCriteria 来查询实体列表。
        /// </summary>
        /// <param name="criteria">常用查询条件。</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public new $domainEntityName$List GetBy(CommonQueryCriteria criteria)
        {
            return base.GetBy(criteria) as $domainEntityName$List;
        }
    
        /// <summary>
        /// 通过 CommonQueryCriteria 来查询单一实体。
        /// </summary>
        /// <param name="criteria">常用查询条件。</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public new $domainEntityName$ GetFirstBy(CommonQueryCriteria criteria)
        {
            return base.GetFirstBy(criteria) as $domainEntityName$;
        }

        /// <summary>
        /// 递归查找所有树型子
        /// </summary>
        /// <param name="treeIndex"></param>
        /// <param name="eagerLoad">需要贪婪加载的属性。</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public new $domainEntityName$List GetByTreeParentIndex(string treeIndex, EagerLoadOptions eagerLoad = null)
        {
            return base.GetByTreeParentIndex(treeIndex, eagerLoad) as $domainEntityName$List;
        }

        /// <summary>
        /// 查找指定树节点的直接子节点。
        /// </summary>
        /// <param name="treePId">需要查找的树节点的Id.</param>
        /// <param name="eagerLoad">需要贪婪加载的属性。</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public new $domainEntityName$List GetByTreePId(object treePId, EagerLoadOptions eagerLoad = null)
        {
            return base.GetByTreePId(treePId, eagerLoad) as $domainEntityName$List;
        }

        #endregion
    }