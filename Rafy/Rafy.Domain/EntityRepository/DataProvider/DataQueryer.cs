/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20150114
 * 运行环境：.NET 4.5
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20150114 12:05
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rafy.Data;
using Rafy.Domain.DataPortal;
using Rafy.Domain.ORM.Linq;
using Rafy.Domain.ORM.Query;
using Rafy.Domain.ORM.SqlTree;
using Rafy.ManagedProperty;

namespace Rafy.Domain
{
    /// <summary>
    /// 数据查询器
    /// </summary>
    public abstract class DataQueryer
    {
        private EntityRepository _repository;

        private RepositoryDataProvider _dataProvider;

        /// <summary>
        /// Initializes the specified data provider.
        /// </summary>
        /// <param name="dataProvider">The data provider.</param>
        internal protected virtual void Init(RepositoryDataProvider dataProvider)
        {
            _dataProvider = dataProvider;
            _repository = dataProvider.Repository;
        }

        /// <summary>
        /// 对应的仓库
        /// </summary>
        internal EntityRepository Repo
        {
            get { return _repository; }
        }

        /// <summary>
        /// 对应的仓库数据提供程序
        /// </summary>
        public RepositoryDataProvider DataProvider
        {
            get { return _dataProvider; }
        }

        #region 数据层查询接口 - Linq

        /// <summary>
        /// 创建一个实体 Linq 查询对象。
        /// 只能在服务端调用此方法。
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public IQueryable<TEntity> CreateLinqQuery<TEntity>()
        {
            return new EntityQueryable<TEntity>(Repo);
        }

        /// <summary>
        /// 通过 linq 来查询实体。
        /// </summary>
        /// <param name="queryable">linq 查询对象。</param>
        /// <param name="paging">分页信息。</param>
        /// <param name="eagerLoad">需要贪婪加载的属性。</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidProgramException"></exception>
        public EntityList QueryList(IQueryable queryable, PagingInfo paging = null, EagerLoadOptions eagerLoad = null)
        {
            var query = ConvertToQuery(queryable);

            return this.QueryList(query, paging, eagerLoad);
        }

        /// <summary>
        /// 把一个 Linq 查询转换为 IQuery 查询。
        /// </summary>
        /// <param name="queryable"></param>
        /// <returns></returns>
        public IQuery ConvertToQuery(IQueryable queryable)
        {
            if (queryable.Provider != Repo.LinqProvider)
            {
                throw new InvalidProgramException(string.Format("查询所属的仓库类型应该是 {0}。", Repo.GetType()));
            }

            var expression = queryable.Expression;
            expression = Evaluator.PartialEval(expression);
            var builder = new EntityQueryBuilder(this.Repo);
            var query = builder.BuildQuery(expression);

            return query;
        }

        #endregion

        #region 数据层查询接口 - IQuery

        /// <summary>
        /// 通过 IQuery 对象来查询实体。
        /// </summary>
        /// <param name="query">查询对象。</param>
        /// <param name="paging">分页信息。</param>
        /// <param name="eagerLoad">需要贪婪加载的属性。</param>
        /// <param name="markTreeFullLoaded">如果某次查询结果是一棵完整的子树，那么必须设置此参数为 true ，才可以把整个树标记为完整加载。</param>
        /// <returns></returns>
        public EntityList QueryList(IQuery query, PagingInfo paging = null, EagerLoadOptions eagerLoad = null, bool markTreeFullLoaded = false)
        {
            var args = new EntityQueryArgs(query);
            args.MarkTreeFullLoaded = markTreeFullLoaded;
            args.SetDataLoadOptions(paging, eagerLoad);

            return this.QueryList(args);
        }

        /// <summary>
        /// 通过 IQuery 对象来查询实体。
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        /// <exception cref="System.NotSupportedException">使用内存过滤器的同时，不支持提供分页参数。</exception>
        /// <exception cref="System.InvalidProgramException"></exception>
        public EntityList QueryList(EntityQueryArgs args)
        {
            if (args.Query == null) throw new ArgumentException("EntityQueryArgs.Query 属性不能为空。");

            this.PrepareArgs(args);

            this.BuildDefaultQuerying(args);

            _dataProvider.OnQuerying(args);

            var entityList = args.EntityList;
            var oldCount = entityList.Count;

            bool autoIndexEnabled = entityList.AutoTreeIndexEnabled;
            try
            {
                //在加载数据时，自动索引功能都不可用。
                entityList.AutoTreeIndexEnabled = false;

                QueryListCore(args, entityList);
            }
            finally
            {
                entityList.AutoTreeIndexEnabled = autoIndexEnabled;
            }

            this.EagerLoadOnCompleted(args, entityList, oldCount);

            return entityList;
        }

        /// <summary>
        /// 通过 IQuery 对象来查询数据表。
        /// </summary>
        /// <param name="query">查询条件。</param>
        /// <param name="paging">分页信息。</param>
        /// <returns></returns>
        public abstract LiteDataTable QueryTable(IQuery query, PagingInfo paging = null);

        /// <summary>
        /// 子类重写此方法，查询从持久层加载列表的具体实现。
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="entityList">The entity list.</param>
        protected abstract void QueryListCore(EntityQueryArgs args, EntityList entityList);

        private void BuildDefaultQuerying(EntityQueryArgs args)
        {
            var query = args.Query;

            //树型实体不支持修改排序规则！此逻辑不能放到 OnQueryBuilt 虚方法中，以免被重写。
            if (Repo.SupportTree)
            {
                if (query.OrderBy.Count > 0)
                {
                    throw new InvalidOperationException(string.Format("树状实体 {0} 只不支持自定义排序，必须使用索引排序。", Repo.EntityType));
                }

                var f = QueryFactory.Instance;
                query.OrderBy.Add(
                    f.OrderBy(query.From.FindTable(Repo).Column(Entity.TreeIndexProperty))
                    );
            }
        }

        /// <summary>
        /// 所有使用 IQuery 的数据查询，在调用完应 queryBuilder 之后，都会执行此此方法。
        /// 所以子类可以重写此方法实现统一的查询条件逻辑。
        /// （例如，对于映射同一张表的几个子类的查询，可以使用此方法统一对所有方法都过滤）。
        /// 
        /// 默认实现为：
        /// * 如果还没有进行排序，则进行默认的排序。
        /// * 如果单一参数实现了 IPagingCriteria 接口，则使用其中的分页信息进行分页。
        /// </summary>
        /// <param name="args"></param>
        internal protected virtual void OnQuerying(EntityQueryArgs args)
        {
            //如果没有指定 OrderNo 字段，则按照Id 排序。
            if (args.FetchType != FetchType.Count && !(args.Query as SqlSelect).HasOrdered())
            {
                args.Query.OrderBy.Add(
                    QueryFactory.Instance.OrderBy(args.Query.From.FindTable(Repo).Column(Entity.IdProperty))
                    );
            }

            //默认对分页进行处理。
            var pList = CurrentIEQC.Parameters;
            if (pList.Length == 1)
            {
                var userCriteria = pList[0] as ILoadOptionsCriteria;
                if (userCriteria != null)
                {
                    if (args.Filter == null)
                    {
                        args.PagingInfo = userCriteria.PagingInfo;
                    }
                    args.EagerLoadOptions = userCriteria.EagerLoad;
                }
            }
        }

        /// <summary>
        /// 在数据加载完成后，完成其它的贪婪加载。
        /// </summary>
        /// <param name="args"></param>
        /// <param name="entityList"></param>
        /// <param name="oldCount"></param>
        protected void EagerLoadOnCompleted(EntityQueryArgsBase args, EntityList entityList, int oldCount)
        {
            if (entityList.Count > 0)
            {
                if (args.FetchType == FetchType.First || args.FetchType == FetchType.List)
                {
                    var elOptions = args.EagerLoadOptions;
                    if (elOptions != null)
                    {
                        //先加载树子节点。
                        if (elOptions.LoadTreeChildren)
                        {
                            /*********************** 代码块解释 *********************************
                             * 加载树时，EntityList.LoadAllNodes 方法只加载根节点的子节点。
                             * 如果使用了 GetAll 方法，那么默认是已经加载了子节点的，不需要再调用 EntityList.LoadAllNodes。
                             * 所以下面只能对于每一个节点，
                            **********************************************************************/
                            var tree = entityList as ITreeComponent;
                            if (!tree.IsFullLoaded)
                            {
                                for (int i = 0, c = entityList.Count; i < c; i++)
                                {
                                    var item = entityList[i] as ITreeComponent;
                                    item.LoadAllNodes();
                                }
                            }
                        }

                        //再加载实体的属性。
                        this.EagerLoad(entityList, elOptions.CoreList);
                    }

                    //如果 entityList 列表中已经有数据，那么只能对新添加的实体进行 OnDbLoaded操作通知加载完成。
                    this.OnDbLoaded(entityList, oldCount);
                }
            }

            _dataProvider.OnEntityQueryed(args);
        }

        /// <summary>
        /// 通知所有的实体都已经被加载。
        /// </summary>
        /// <param name="allEntities"></param>
        /// <param name="fromIndex">从这个索引号开始的实体，才会被通知加载。</param>
        private void OnDbLoaded(IList<Entity> allEntities, int fromIndex = 0)
        {
            for (int i = fromIndex, c = allEntities.Count; i < c; i++)
            {
                var item = allEntities[i];

                item.PersistenceStatus = PersistenceStatus.Unchanged;

                //由于 OnDbLoaded 中可能会使用到关系，导致再次进行数据访问，所以不能放在 Reader 中。
                _dataProvider.OnDbLoaded(item);
            }
        }

        /// <summary>
        /// 子类重写这个方法，用于在从数据库获取出来时，及时地加载一些额外的属性。
        /// 
        /// 注意：这个方法中只应该为一般属性计算值，不能有其它的数据访问。
        /// </summary>
        /// <param name="entity"></param>
        internal protected virtual void OnDbLoaded(Entity entity) { }

        #endregion

        #region 通用查询接口

        /// <summary>
        /// QueryList 方法完成后调用。
        /// 
        /// 子类可重写此方法来实现查询完成后的数据修整工具。
        /// </summary>
        /// <param name="args"></param>
        internal protected virtual void OnEntityQueryed(EntityQueryArgsBase args) { }

        protected void PrepareArgs(EntityQueryArgsBase args)
        {
            if (args.EntityList == null)
            {
                //必须使用 NewListFast，否则使用 NewList 会导致调用了 NotifyLoaded，
                //这样，不但不符合设计（这个列表需要在客户端才调用 NotifyLoaded），还会引发树型实体列表的多次关系重建。
                args.EntityList = Repo.NewListFast();
            }

            args.SetFetchType(CurrentIEQC.FetchType);
        }

        internal void LoadByFilter(EntityQueryArgsBase args)
        {
            var entityList = args.EntityList;
            var filter = args.Filter;

            //如果存储过滤器，则说明 list 是一个内存中的临时对象。这时需要重新把数据加载进 List 中来。
            if (args.FetchType == FetchType.Count)
            {
                entityList.SetTotalCount(args.MemoryList.Count(e => filter(e)));
            }
            else
            {
                foreach (var item in args.MemoryList)
                {
                    if (filter(item)) { entityList.Add(item); }
                }
            }
        }

        #endregion

        #region 贪婪加载

        /*********************** 代码块解释 *********************************
         * 
         * 贪婪加载使用简单的内存级别中的贪婪加载。
         * 每使用 IQuery.EagerLoad 的来声明一个贪婪属性，则会多一次查询。
         * 该次查询会查询出所有这个引用的实体类型的实体，然后分配到结果列表的每一个实体中。
         * 
        **********************************************************************/

        /// <summary>
        /// 对列表加载指定的贪婪属性。
        /// </summary>
        /// <param name="list"></param>
        /// <param name="eagerLoadProperties">所有需要贪婪加载的属性。</param>
        private void EagerLoad(EntityList list, IList<ConcreteProperty> eagerLoadProperties)
        {
            if (list.Count > 0 && eagerLoadProperties.Count > 0)
            {
                //为了不修改外面传入的列表，这里缓存一个新的列表。
                var eagerCache = eagerLoadProperties.ToList();

                //找到这个列表需要加载的所有贪婪加载属性。
                var listEagerProperties = new List<ConcreteProperty>();
                for (int i = eagerCache.Count - 1; i >= 0; i--)
                {
                    var item = eagerCache[i];
                    if (item.Owner.IsAssignableFrom(list.EntityType))
                    {
                        listEagerProperties.Add(item);
                        eagerCache.RemoveAt(i);
                    }
                }

                //对于每一个属性，直接查询出该属性对应实体的所有实体对象。
                foreach (var property in listEagerProperties)
                {
                    var mp = property.Property;
                    var listProperty = mp as IListProperty;
                    if (listProperty != null)
                    {
                        this.EagerLoadChildren(list, listProperty, eagerCache);
                    }
                    else
                    {
                        var refProperty = mp as IRefProperty;
                        if (refProperty != null)
                        {
                            this.EagerLoadRef(list, refProperty, eagerCache);
                        }
                        else
                        {
                            throw new InvalidOperationException("贪婪加载属性只支持引用属性和列表属性两种。");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 对实体列表中每一个实体都贪婪加载出它的所有子实体。
        /// </summary>
        /// <param name="list"></param>
        /// <param name="listProperty">贪婪加载的列表子属性。</param>
        /// <param name="eagerLoadProperties">所有还需要贪婪加载的属性。</param>
        private void EagerLoadChildren(EntityList list, IListProperty listProperty, List<ConcreteProperty> eagerLoadProperties)
        {
            //查询一个大的实体集合，包含列表中所有实体所需要的所有子实体。
            var idList = new List<object>(10);
            list.EachNode(e =>
            {
                idList.Add(e.Id);
                return false;
            });
            if (idList.Count > 0)
            {
                var targetRepo = RepositoryFactoryHost.Factory.FindByEntity(listProperty.ListEntityType);

                var allChildren = targetRepo.GetByParentIdList(idList.ToArray(), PagingInfo.Empty);

                //继续递归加载它的贪婪属性。
                this.EagerLoad(allChildren, eagerLoadProperties);

                //把大的实体集合，根据父实体 Id，分拆到每一个父实体的子集合中。
                var parentProperty = targetRepo.FindParentPropertyInfo(true).ManagedProperty as IRefProperty;
                var parentIdProperty = parentProperty.RefIdProperty;
                list.EachNode(parent =>
                {
                    var children = targetRepo.NewList();
                    foreach (var child in allChildren)
                    {
                        var pId = child.GetRefId(parentIdProperty);
                        if (object.Equals(pId, parent.Id)) { children.Add(child); }
                    }
                    children.SetParentEntity(parent);

                    parent.LoadProperty(listProperty, children);
                    return false;
                });
            }
        }

        /// <summary>
        /// 对实体列表中每一个实体都贪婪加载出它的所有引用实体。
        /// </summary>
        /// <param name="list"></param>
        /// <param name="refProperty">贪婪加载的引用属性。</param>
        /// <param name="eagerLoadProperties">所有还需要贪婪加载的属性。</param>
        private void EagerLoadRef(EntityList list, IRefProperty refProperty, List<ConcreteProperty> eagerLoadProperties)
        {
            var refIdProperty = refProperty.RefIdProperty;
            //查询一个大的实体集合，包含列表中所有实体所需要的所有引用实体。
            var idList = new List<object>(10);
            list.EachNode(e =>
            {
                var refId = e.GetRefNullableId(refIdProperty);
                if (refId != null && idList.All(id => !id.Equals(refId)))
                {
                    idList.Add(refId);
                }
                return false;
            });
            if (idList.Count > 0)
            {
                var targetRepo = RepositoryFactoryHost.Factory.FindByEntity(refProperty.RefEntityType);
                var allRefList = targetRepo.GetByIdList(idList.ToArray());

                //继续递归加载它的贪婪属性。
                this.EagerLoad(allRefList, eagerLoadProperties);

                //把大的实体集合，根据 Id，设置到每一个实体上。
                var refEntityProperty = refProperty.RefEntityProperty;
                list.EachNode(entity =>
                {
                    var refId = entity.GetRefNullableId(refIdProperty);
                    if (refId != null)
                    {
                        var refEntity = allRefList.Find(refId);
                        entity.LoadProperty(refEntityProperty, refEntity);
                    }
                    return false;
                });
            }
        }

        #endregion

        /// <summary>
        /// 当前正在使用的查询参数
        /// </summary>
        private static IEQC CurrentIEQC
        {
            get
            {
                var ieqc = FinalDataPortal.CurrentCriteria as IEQC;
                if (ieqc == null) throw new NotSupportedException("实体查询必须使用 FetchCount、FetchFirst、FetchList、FetchTable 等方法。");
                return ieqc;
            }
        }
    }
}
