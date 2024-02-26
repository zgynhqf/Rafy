/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20150314
 * 运行环境：.NET 4.5
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20150314 00:17
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Rafy.Data;
using Rafy.Domain.ORM;
using Rafy.Domain.ORM.Query;
using Rafy.ManagedProperty;
using Rafy.MetaModel;

namespace Rafy.Domain
{
    partial class RepositoryDataProvider
    {
        /// <summary>
        /// 子类可以重写这个方法，用于实现 GetAll 的数据层查询逻辑。
        /// </summary>
        /// <param name="paging">The paging information.</param>
        /// <param name="loadOptions">数据加载时选项（贪婪加载等）。</param>
        /// <returns></returns>
        public virtual object GetAll(PagingInfo paging, LoadOptions loadOptions)
        {
            var query = f.Query(_repository);

            return this.QueryData(query, paging, loadOptions, true);
        }

        /// <summary>
        /// 子类可以重写这个方法，用于实现 GetTreeRoots 的数据层查询逻辑。
        /// </summary>
        /// <param name="loadOptions">数据加载时选项（贪婪加载等）。</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public virtual object GetTreeRoots(LoadOptions loadOptions)
        {
            var query = f.Query(_repository);
            query.AddConstraint(Entity.TreePIdProperty, PropertyOperator.Equal, null);

            return this.QueryData(query, null, loadOptions, false);
        }

        /// <summary>
        /// 子类可以重写这个方法，用于实现 GetById 的数据层查询逻辑。
        /// </summary>
        /// <param name="id">The unique identifier.</param>
        /// <param name="loadOptions">数据加载时选项（贪婪加载等）。</param>
        /// <returns></returns>
        public virtual Entity GetById(object id, LoadOptions loadOptions)
        {
            var table = f.Table(_repository);
            var q = f.Query(
                table,
                where: f.Constraint(table.IdColumn, id)
            );

            return (Entity)this.QueryData(q, null, loadOptions, false);
        }

        /// <summary>
        /// 子类可以重写这个方法，用于实现 GetByKey 的数据层查询逻辑。
        /// </summary>
        /// <param name="keyProperty">The unique identifier.</param>
        /// <param name="key">The unique identifier.</param>
        /// <param name="loadOptions">数据加载时选项（贪婪加载等）。</param>
        /// <returns></returns>
        public virtual Entity GetByKey(string keyProperty, object key, LoadOptions loadOptions)
        {
            var allProperties = _repository.EntityMeta.ManagedProperties.GetNonReadOnlyCompiledProperties();
            var mp = allProperties.Find(keyProperty);

            var table = f.Table(_repository);
            var q = f.Query(
                table,
                where: f.Constraint(table.Column(mp), key)
            );

            return (Entity)this.QueryData(q, null, loadOptions, false);
        }

        /// <summary>
        /// 子类重写此方法，来实现自己的 GetByIdList 方法的数据层代码。
        /// </summary>
        /// <param name="idList"></param>
        /// <param name="loadOptions">数据加载时选项（贪婪加载等）。</param>
        /// <returns></returns>
        public virtual IEntityList GetByIdList(object[] idList, LoadOptions loadOptions)
        {
            var table = f.Table(_repository);
            var q = f.Query(
                table,
                where: f.Constraint(table.IdColumn, PropertyOperator.In, idList)
            );

            return (IEntityList)this.QueryData(q, null, loadOptions, false);
        }

        /// <summary>
        /// 子类重写此方法，来实现自己的 GetByParentId 方法的数据层代码。
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="paging">分页信息。</param>
        /// <param name="loadOptions">数据加载时选项（贪婪加载等）。</param>
        /// <returns></returns>
        public virtual object GetByParentId(object parentId, PagingInfo paging, LoadOptions loadOptions)
        {
            var parentProperty = _repository.EntityMeta.FindParentReferenceProperty(true);
            var mp = (parentProperty.ManagedProperty as IRefProperty).RefKeyProperty;

            var table = f.Table(_repository);
            var q = f.Query(
                table,
                where: f.Constraint(table.Column(mp), parentId)
            );

            var list = this.QueryData(q, paging, loadOptions, true);

            return list;
        }

        /// <summary>
        /// 子类重写此方法来实现通过父 Id 列表来获取所有组合子对象的列表
        /// </summary>
        /// <param name="parentIdList">The parent identifier list.</param>
        /// <param name="paging">分页信息。</param>
        /// <param name="loadOptions">数据加载时选项（贪婪加载等）。</param>
        /// <returns></returns>
        public virtual IEntityList GetByParentIdList(object[] parentIdList, PagingInfo paging, LoadOptions loadOptions)
        {
            var parentProperty = _repository.EntityMeta.FindParentReferenceProperty(true);
            var mp = RefPropertyHelper.Find(parentProperty.ManagedProperty).RefKeyProperty;

            var table = f.Table(_repository);
            var parentColumn = table.Column(mp);
            var q = f.Query(
                table,
                where: f.Constraint(parentColumn, PropertyOperator.In, parentIdList),
                orderBy: new List<IOrderBy> { f.OrderBy(parentColumn) }
                //orderBy: _repository.SupportTree ? null : new List<IOrderBy> { f.OrderBy(parentColumn) }
            );

            var list = (IEntityList)this.QueryData(q, paging, loadOptions, true);

            return list;
        }

        /// <summary>
        /// 通过树型编码，找到所有对应的子节点。
        /// </summary>
        /// <param name="treeIndex"></param>
        /// <param name="loadOptions">数据加载时选项（贪婪加载等）。</param>
        /// <returns></returns>
        public virtual IEntityList GetByTreeParentIndex(string treeIndex, LoadOptions loadOptions)
        {
            if (string.IsNullOrEmpty(treeIndex)) throw new ArgumentNullException(nameof(treeIndex));

            //递归查找所有树型子
            var childCode = treeIndex + SqlGenerator.WILDCARD_ALL + _repository.TreeIndexOption.Seperator;
            var table = f.Table(_repository);
            var q = f.Query(
                table,
                where: f.Constraint(table.Column(Entity.TreeIndexProperty), PropertyOperator.Like, childCode)
            );

            var list = (IEntityList)this.QueryData(q, null, loadOptions, true);

            return list;
        }

        /// <summary>
        /// 查找指定树节点的直接子节点。
        /// </summary>
        /// <param name="treePId">需要查找的树节点的Id.</param>
        /// <param name="loadOptions">数据加载时选项（贪婪加载等）。</param>
        /// <returns></returns>
        public virtual IEntityList GetByTreePId(object treePId, LoadOptions loadOptions)
        {
            var table = f.Table(_repository);
            var q = f.Query(
                table,
                where: table.Column(Entity.TreePIdProperty).Equal(treePId)
            );

            return (IEntityList)this.QueryData(q, null, loadOptions, false);
        }

        /// <summary>
        /// 获取指定树节点的所有父节点。
        /// </summary>
        /// <param name="treeIndex">Index of the tree.</param>
        /// <param name="loadOptions">数据加载时选项（贪婪加载等）。</param>
        /// <returns></returns>
        public virtual IEntityList GetAllTreeParents(string treeIndex, LoadOptions loadOptions)
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

            var table = f.Table(_repository);
            var q = f.Query(
                table,
                where: table.Column(Entity.TreeIndexProperty).In(parentIndeces)
            );

            return (IEntityList)this.QueryData(q, null, loadOptions, false);
        }

        /// <summary>
        /// <see cref="CommonQueryCriteria"/> 查询的数据层实现。
        /// </summary>
        /// <param name="criteria"></param>
        public virtual object GetBy(CommonQueryCriteria criteria)
        {
            var table = f.Table(_repository);
            var q = f.Query(table);

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
                            ignored = !DomainHelper.IsNotEmpty(value);
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
                                        case PropertyOperator.StartsWith:
                                        case PropertyOperator.EndsWith:
                                        case PropertyOperator.NotLike:
                                        case PropertyOperator.NotContains:
                                        case PropertyOperator.NotStartsWith:
                                        case PropertyOperator.NotEndsWith:
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
                            var propertyRes = f.Constraint(table.Column(property), op, value);
                            groupRes = f.Binary(groupRes, group.Concat, propertyRes);
                        }
                    }
                }

                q.Where = f.Binary(groupRes, criteria.Concat, q.Where);
            }

            //OrderBy
            if (!string.IsNullOrWhiteSpace(criteria.OrderBy))
            {
                var orderBy = allProperties.Find(criteria.OrderBy, true);
                if (orderBy == null) throw new InvalidProgramException(string.Format("在实体 {0} 中没有找到名为 {1} 的属性。", _repository.EntityType.FullName, criteria.OrderBy));

                var dir = criteria.OrderByAscending ? OrderDirection.Ascending : OrderDirection.Descending;
                q.OrderBy.Add(table.Column(orderBy), dir);
            }

            return this.QueryData(q, criteria.PagingInfo, criteria.LoadOptions, true);
        }

        /// <summary>
        /// <see cref="ODataQueryCriteria"/> 查询的数据层实现。
        /// </summary>
        /// <param name="criteria"></param>
        public virtual object GetBy(ODataQueryCriteria criteria)
        {
            var t = f.Table(this.Repository);

            var q = f.Query(from: t);

            var properties = this.Repository.EntityMeta.ManagedProperties.GetNonReadOnlyCompiledProperties();

            #region Filter

            if (!string.IsNullOrWhiteSpace(criteria.Filter))
            {
                var parser = new ODataFilterParser
                {
                    _properties = properties
                };
                parser.Parse(criteria.Filter, q);
            }

            #endregion

            #region OrderBy

            if (!string.IsNullOrWhiteSpace(criteria.OrderBy))
            {
                var orderByProperties = criteria.OrderBy.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var orderByExp in orderByProperties)
                {
                    var values = orderByExp.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    var property = values[0];
                    var orderBy = properties.Find(property, true);
                    if (orderBy != null)
                    {
                        var dir = values.Length == 1 || values[1].ToLower() == "asc" ? OrderDirection.Ascending : OrderDirection.Descending;
                        q.OrderBy.Add(f.OrderBy(t.Column(orderBy), dir));
                    }
                }
            }

            #endregion

            #region Expand

            if (!string.IsNullOrWhiteSpace(criteria.Expand))
            {
                var loadOptions = new LoadOptions();

                var expandProperties = criteria.Expand.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                var splitter = new char[] { '.' };
                foreach (var expand in expandProperties)
                {
                    //如果有'.'，表示类似于 Section.Chapter.Book 这种表达式。
                    if (expand.Contains('.'))
                    {
                        Type nextEntityType = null;//下一个需要使用的实体类型

                        var cascadeProperties = expand.Split(splitter, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var property in cascadeProperties)
                        {
                            var container = properties;
                            if (nextEntityType != null)
                            {
                                var meta = CommonModel.Entities.Find(nextEntityType);
                                container = meta.ManagedProperties.GetNonReadOnlyCompiledProperties();
                            }

                            var mp = container.Find(property, true);
                            if (mp != null)
                            {
                                if (mp is IRefProperty)
                                {
                                    var refProperty = mp as IRefProperty;
                                    loadOptions.LoadWith(refProperty);
                                    nextEntityType = refProperty.RefEntityType;
                                }
                                else if (mp is IListProperty)
                                {
                                    loadOptions.LoadWith(mp as IListProperty);
                                    nextEntityType = (mp as IListProperty).ListEntityType;
                                }
                            }
                        }
                    }
                    else
                    {
                        var mp = properties.Find(expand, true);
                        if (mp != null)
                        {
                            if (mp is IListProperty)
                            {
                                loadOptions.LoadWith(mp as IListProperty);
                            }
                            else if (mp is IRefProperty)
                            {
                                loadOptions.LoadWith(mp as IRefProperty);
                            }
                        }
                        else if (expand == EntityConvention.TreeChildrenPropertyName)
                        {
                            loadOptions.LoadWithTreeChildren();
                        }
                    }

                    criteria.LoadOptions = loadOptions;
                }
            }

            #endregion

            return this.QueryData(q, criteria.PagingInfo, criteria.LoadOptions, criteria.MarkTreeFullLoaded);
        }

        /// <summary>
        /// 子类重写此方法，来实现自己的 GetEntityValue 方法的数据层代码。
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public virtual LiteDataTable GetEntityValue(object entityId, string property)
        {
            var mp = _repository.EntityMeta.ManagedProperties.GetNonReadOnlyCompiledProperties().Find(property, true);

            var source = f.Table(_repository);
            var selection = source.Column(mp);
            var where = f.Constraint(source.IdColumn, entityId);
            var query = f.Query(source, selection, where);

            return this.QueryTable(query);
        }

        #region 提供给子类的查询接口

        /// <summary>
        /// 通过 IQuery 对象从持久层中查询数据。
        /// 本方法只能由仓库中的方法来调用。本方法的返回值的类型将与仓库中方法的返回值保持一致。
        /// 支持的返回值：EntityList、Entity、int、LiteDataTable。
        /// </summary>
        /// <param name="query">查询对象。</param>
        /// <param name="paging">分页信息。</param>
        /// <param name="loadOptions">数据加载时选项（贪婪加载等）。</param>
        /// <param name="markTreeFullLoaded">如果某次查询结果是一棵完整的子树，那么必须设置此参数为 true，才可以把整个树标记为完整加载。</param>
        /// <returns></returns>
        protected object QueryData(IQuery query, PagingInfo paging = null, LoadOptions loadOptions = null, bool markTreeFullLoaded = true)
        {
            return this.DataQueryer.QueryData(query, paging, loadOptions, markTreeFullLoaded);
        }

        /// <summary>
        /// 通过 IQuery 对象从持久层中查询数据。
        /// 本方法只能由仓库中的方法来调用。本方法的返回值的类型将与仓库中方法的返回值保持一致。
        /// 支持的返回值：EntityList、Entity、int。
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        protected object QueryData(ORMQueryArgs args)
        {
            return this.DataQueryer.QueryData(args);
        }

        /// <summary>
        /// 从持久层中查询数据。
        /// 本方法只能由仓库中的方法来调用。本方法的返回值的类型将与仓库中方法的返回值保持一致。
        /// 支持的返回值：EntityList、Entity、int。
        /// </summary>
        /// <param name="queryable"></param>
        /// <param name="paging"></param>
        /// <param name="loadOptions"></param>
        /// <returns></returns>
        protected object QueryData(IQueryable queryable, PagingInfo paging = null, LoadOptions loadOptions = null)
        {
            return this.DataQueryer.QueryData(queryable, paging, loadOptions);
        }

        /// <summary>
        /// 通过 IQuery 对象来查询数据表。
        /// </summary>
        /// <param name="query">查询条件。</param>
        /// <param name="paging">分页信息。</param>
        /// <returns></returns>
        protected LiteDataTable QueryTable(IQuery query, PagingInfo paging = null)
        {
            return this.DataQueryer.QueryTable(query, paging);
        }

        /// <summary>
        /// 创建一个实体 Linq 查询对象。
        /// 只能在服务端调用此方法。
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        protected IQueryable<TEntity> CreateLinqQuery<TEntity>()
        {
            return this.DataQueryer.CreateLinqQuery<TEntity>();
        }

        /// <summary>
        /// 把一个 Linq 查询转换为 IQuery 查询。
        /// </summary>
        /// <param name="queryable"></param>
        /// <returns></returns>
        protected IQuery ConvertToQuery(IQueryable queryable)
        {
            return this.DataQueryer.ConvertToQuery(queryable);
        }

        #endregion

        private static QueryFactory f
        {
            get { return QueryFactory.Instance; }
        }
    }
}
