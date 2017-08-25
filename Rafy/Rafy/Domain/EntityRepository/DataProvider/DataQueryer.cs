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
        /// 从持久层中查询数据。
        /// 本方法只能由仓库中的方法来调用。本方法的返回值的类型将与仓库中方法的返回值保持一致。
        /// 支持的返回值：EntityList、Entity、int、LiteDataTable。
        /// </summary>
        /// <param name="queryable"></param>
        /// <param name="paging"></param>
        /// <param name="eagerLoad"></param>
        /// <returns></returns>
        public object QueryData(IQueryable queryable, PagingInfo paging = null, EagerLoadOptions eagerLoad = null)
        {
            var query = ConvertToQuery(queryable);

            return this.QueryData(query, paging, eagerLoad);
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
        /// 通过 IQuery 对象从持久层中查询数据。
        /// 本方法只能由仓库中的方法来调用。本方法的返回值的类型将与仓库中方法的返回值保持一致。
        /// 支持的返回值：EntityList、Entity、int、LiteDataTable。
        /// </summary>
        /// <param name="query">查询对象。</param>
        /// <param name="paging">分页信息。</param>
        /// <param name="eagerLoad">需要贪婪加载的属性。</param>
        /// <param name="markTreeFullLoaded">如果某次查询结果是一棵完整的子树，那么必须设置此参数为 true ，才可以把整个树标记为完整加载。</param>
        /// <returns></returns>
        public object QueryData(IQuery query, PagingInfo paging = null, EagerLoadOptions eagerLoad = null, bool markTreeFullLoaded = false)
        {
            var queryType = FinalDataPortal.CurrentIEQC.QueryType;
            if (queryType == RepositoryQueryType.Table)
            {
                return this.QueryTable(query, paging);
            }

            var args = new EntityQueryArgs(query);
            args.MarkTreeFullLoaded = markTreeFullLoaded;
            args.SetDataLoadOptions(paging, eagerLoad);

            return this.QueryData(args);
        }

        /// <summary>
        /// 通过 IQuery 对象从持久层中查询数据。
        /// 本方法只能由仓库中的方法来调用。本方法的返回值的类型将与仓库中方法的返回值保持一致。
        /// 支持的返回值：EntityList、Entity、int。
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        public object QueryData(EntityQueryArgs args)
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

                QueryDataCore(args, entityList);
            }
            finally
            {
                entityList.AutoTreeIndexEnabled = autoIndexEnabled;
            }

            this.EagerLoadOnCompleted(args, entityList, oldCount);

            return ReturnForRepository(entityList, args.QueryType);
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
        protected abstract void QueryDataCore(EntityQueryArgs args, EntityList entityList);

        private void BuildDefaultQuerying(EntityQueryArgs args)
        {
            var query = args.Query;

            //树型实体不支持修改排序规则！此逻辑不能放到 OnQueryBuilt 虚方法中，以免被重写。
            if (Repo.SupportTree)
            {
                //if (query.OrderBy.Count > 0)
                //{
                //    throw new InvalidOperationException(string.Format("树状实体 {0} 只不支持自定义排序，必须使用索引排序。", Repo.EntityType));
                //}
                var c = query.OrderBy.Count;
                if (c > 0)
                {
                    bool error = true;
                    //如果只有一个排序时，允许使用聚合父属性进行排序。
                    if (c == 1)
                    {
                        var parentProperty = _repository.FindParentPropertyInfo(false);
                        if (parentProperty != null)
                        {
                            var property = query.OrderBy[0].Column.Property;
                            var pProperty = (parentProperty.ManagedProperty as IRefProperty).RefIdProperty;
                            error = property != pProperty;
                        }
                    }
                    if (error)
                    {
                        throw new InvalidOperationException(string.Format("树状实体 {0} 只不支持自定义排序，必须使用索引排序。", Repo.EntityType));
                    }
                }

                /*********************** 代码块解释 *********************************
                 * 以下，默认使用 TreeIndex 进行排序。
                 * 同时，在使用 TreeIndex 排序的基础上，还需要使用 Id 进行排序。
                 * TreeIndexHelper.ResetTreeIndex 在整理数据时，会把 TreeIndex 清空，此时数据可能无序。
                 * 而 Oracle 中查询时，返回的结果中 Id 可能是乱的，这会影响数据的加载。
                **********************************************************************/
                var f = QueryFactory.Instance;
                var table = query.From.FindTable(Repo);
                query.OrderBy.Add(
                    f.OrderBy(table.Column(Entity.TreeIndexProperty))
                    );
                query.OrderBy.Add(
                    f.OrderBy(table.IdColumn)
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
            //默认对分页进行处理。
            var pList = FinalDataPortal.CurrentIEQC.Parameters;
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
                if (args.QueryType == RepositoryQueryType.First || args.QueryType == RepositoryQueryType.List)
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

            args.SetQueryType(FinalDataPortal.CurrentIEQC.QueryType);
        }

        internal void LoadByFilter(EntityQueryArgsBase args)
        {
            var entityList = args.EntityList;
            var filter = args.Filter;

            //如果存储过滤器，则说明 list 是一个内存中的临时对象。这时需要重新把数据加载进 List 中来。
            if (args.QueryType == RepositoryQueryType.Count)
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

                var allChildren = targetRepo.GetByParentIdList(idList.ToArray());

                //继续递归加载它的贪婪属性。
                this.EagerLoad(allChildren, eagerLoadProperties);

                //把大的实体集合，根据父实体 Id，分拆到每一个父实体的子集合中。
                var parentProperty = targetRepo.FindParentPropertyInfo(true).ManagedProperty as IRefProperty;
                var parentIdProperty = parentProperty.RefIdProperty;

                #region 把父实体全部放到排序列表中

                //由于数据量可能较大，所以需要进行排序后再顺序加载。
                IList<Entity> sortedList = null;

                if (_repository.SupportTree)
                {
                    var sortedParents = new List<Entity>(list.Count);
                    list.EachNode(p =>
                    {
                        sortedParents.Add(p);
                        return false;
                    });
                    sortedList = sortedParents.OrderBy(e => e.Id).ToList();
                }
                else
                {
                    sortedList = list.OrderBy(e => e.Id).ToList();
                }

                #endregion

                #region 使用一次主循环就把所有的子实体都加载到父实体中。
                //一次循环就能完全加载的前提是因为父集合按照 Id 排序，子集合按照父 Id 排序。

                int pIndex = 0, pLength = sortedList.Count;
                var parent = sortedList[pIndex];
                var children = targetRepo.NewList();
                for (int i = 0, c = allChildren.Count; i < c; i++)
                {
                    var child = allChildren[i];
                    var childPId = child.GetRefId(parentIdProperty);

                    //必须把该子对象处理完成后，才能跳出下面的循环。
                    while (true)
                    {
                        if (object.Equals(childPId, parent.Id))
                        {
                            children.Add(child);
                            break;
                        }
                        else
                        {
                            //检测下一个父实体。
                            pIndex++;

                            //所有父集合已经加载完毕，退出整个循环。
                            if (pIndex >= pLength)
                            {
                                i = c;
                                break;
                            }

                            //把整理好的子集合，加载到父实体中。
                            children.SetParentEntity(parent);
                            parent.LoadProperty(listProperty, children);

                            //并同时更新变量。
                            parent = sortedList[pIndex];
                            children = targetRepo.NewList();
                        }
                    }
                }
                if (children.Count > 0)
                {
                    children.SetParentEntity(parent);
                }
                parent.LoadProperty(listProperty, children);

                //如果子集合处理完了，父集合还没有循环到最后，那么需要把余下的父实体的子集合都加载好。
                pIndex++;
                while (pIndex < pLength)
                {
                    parent = sortedList[pIndex];
                    parent.LoadProperty(listProperty, targetRepo.NewList());
                    pIndex++;
                }

                #endregion
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
                var refList = targetRepo.GetByIdList(idList.ToArray());

                //继续递归加载它的贪婪属性。
                this.EagerLoad(refList, eagerLoadProperties);

                #region 把实体全部放到排序列表中

                //由于数据量可能较大，所以需要进行排序后再顺序加载。
                IList<Entity> sortedList = null;

                if (_repository.SupportTree)
                {
                    var tmp = new List<Entity>(list.Count);
                    list.EachNode(p =>
                    {
                        tmp.Add(p);
                        return false;
                    });
                    sortedList = tmp.OrderBy(e => e.GetRefNullableId(refIdProperty)).ToList();
                }
                else
                {
                    sortedList = list.OrderBy(e => e.GetRefNullableId(refIdProperty)).ToList();
                }

                #endregion

                #region 使用一次主循环就把所有的子实体都加载到父实体中。
                //一次循环就能完全加载的前提是因为父集合按照 Id 排序，子集合按照父 Id 排序。

                //把大的实体集合，根据 Id，设置到每一个实体上。
                var refEntityProperty = refProperty.RefEntityProperty;
                int refListIndex = 0, refListCount = refList.Count;
                var refEntity = refList[refListIndex];
                for (int i = 0, c = sortedList.Count; i < c; i++)
                {
                    var entity = sortedList[i];

                    var refId = entity.GetRefNullableId(refIdProperty);
                    if (refId != null)
                    {
                        //必须把该对象处理完成后，才能跳出下面的循环。
                        while (true)
                        {
                            if (object.Equals(refId, refEntity.Id))
                            {
                                entity.LoadProperty(refEntityProperty, refEntity);
                                break;
                            }
                            else
                            {
                                //检测下一个引用实体。
                                refListIndex++;

                                //所有父集合已经加载完毕，退出整个循环。
                                if (refListIndex >= refListCount)
                                {
                                    i = c;
                                    break;
                                }

                                refEntity = refList[refListIndex];
                            }
                        }
                    }
                }

                #endregion
            }
        }

        #endregion

        /// <summary>
        /// 将查询出来的实体列表类型转换为与仓库查询方法返回值一致的类型。
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        protected static object ReturnForRepository(EntityList list)
        {
            return ReturnForRepository(list, FinalDataPortal.CurrentIEQC.QueryType);
        }

        private static object ReturnForRepository(EntityList list, RepositoryQueryType queryType)
        {
            switch (queryType)
            {
                case RepositoryQueryType.First:
                    return DisconnectFirst(list);
                case RepositoryQueryType.Count:
                    return list.TotalCount;
                case RepositoryQueryType.Table:
                    throw new InvalidOperationException();
                case RepositoryQueryType.List:
                default:
                    return list;
            }
        }

        /// <summary>
        /// 只返回列表中的唯一实体时，使用此方法。
        /// 断开 Entity 与 EntityList 之间的关系，可防止 EntityList 内存泄漏。
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
    }
}
