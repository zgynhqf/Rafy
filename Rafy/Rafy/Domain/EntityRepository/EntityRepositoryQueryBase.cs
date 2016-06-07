/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130307 09:37
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130307 09:37
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Rafy;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.Domain.ORM;
using Rafy.Domain.ORM.Linq;
using Rafy.Data;
using Rafy.Domain.DataPortal;
using Rafy.Domain.ORM.SqlTree;
using Rafy.Domain.ORM.Query;
using Rafy.Domain.ORM.Query.Impl;

namespace Rafy.Domain
{
    /// <summary>
    /// 数据仓库查询基类。
    /// 作为 EntityRepository、EntityRepositoryExt 两个类的基类，本类提取了所有数据访问的公共方法。
    /// </summary>
    public abstract class EntityRepositoryQueryBase
    {
        internal abstract IRepositoryInternal Repo { get; }

        public EntityRepositoryQueryBase()
        {
            _linqProvider = new EntityQueryProvider(this);
        }

        /// <summary>
        /// 本仓库使用的数据查询器。
        /// 如果在仓库中直接实现数据层代码，则可以使用该查询器来查询数据。
        /// </summary>
        protected DataQueryer DataQueryer
        {
            get { return (Repo.DataProvider as IRepositoryDataProviderInternal).DataQueryer as DataQueryer; }
        }

        #region 数据层查询接口 - Linq

        private EntityQueryProvider _linqProvider;

        internal EntityQueryProvider LinqProvider
        {
            get { return _linqProvider; }
        }

        /// <summary>
        /// 创建一个实体 Linq 查询对象。
        /// 只能在服务端调用此方法。
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        protected IQueryable<TEntity> CreateLinqQuery<TEntity>()
        {
            return DataQueryer.CreateLinqQuery<TEntity>();
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
        protected object QueryData(IQueryable queryable, PagingInfo paging = null, EagerLoadOptions eagerLoad = null)
        {
            return this.DataQueryer.QueryData(queryable, paging, eagerLoad);
        }

        /// <summary>
        /// 把一个 Linq 查询转换为 IQuery 查询。
        /// </summary>
        /// <param name="queryable"></param>
        /// <returns></returns>
        protected IQuery ConvertToQuery(IQueryable queryable)
        {
            return DataQueryer.ConvertToQuery(queryable);
        }

        internal object QueryListByLinq(IQueryable queryable)
        {
            return DataQueryer.QueryData(queryable);
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
        protected object QueryData(IQuery query, PagingInfo paging = null, EagerLoadOptions eagerLoad = null, bool markTreeFullLoaded = false)
        {
            return this.DataQueryer.QueryData(query, paging, eagerLoad, markTreeFullLoaded);
        }

        /// <summary>
        /// 通过 IQuery 对象从持久层中查询数据。
        /// 本方法只能由仓库中的方法来调用。本方法的返回值的类型将与仓库中方法的返回值保持一致。
        /// 支持的返回值：EntityList、Entity、int。
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        protected object QueryData(EntityQueryArgs args)
        {
            return this.DataQueryer.QueryData(args);
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

        #endregion

        #region 数据层查询接口 - FormattedSql

        ///// <summary>
        ///// 使用 sql 语句来查询实体。
        ///// </summary>
        ///// <param name="sql">sql 语句，返回的结果集的字段，需要保证与属性映射的字段名相同。</param>
        ///// <param name="paging">分页信息。</param>
        ///// <param name="eagerLoad">需要贪婪加载的属性。</param>
        ///// <returns></returns>
        //protected EntityList QueryList(FormattedSql sql, PagingInfo paging = null, EagerLoadOptions eagerLoad = null)
        //{
        //    return Queryer.QueryList(sql, paging, eagerLoad);
        //}

        ///// <summary>
        ///// 使用 sql 语句来查询实体。
        ///// </summary>
        ///// <param name="args">The arguments.</param>
        ///// <returns></returns>
        ///// <exception cref="System.NotSupportedException">使用内存过滤器的同时，不支持提供分页参数。</exception>
        //protected EntityList QueryList(SqlQueryArgs args)
        //{
        //    return Queryer.QueryList(args);
        //}

        ///// <summary>
        ///// 使用 sql 语句来查询数据表。
        ///// </summary>
        ///// <param name="sql">Sql 语句.</param>
        ///// <param name="paging">分页信息。</param>
        ///// <returns></returns>
        //protected LiteDataTable QueryTable(FormattedSql sql, PagingInfo paging = null)
        //{
        //    return Queryer.QueryTable(sql, paging);
        //}

        ///// <summary>
        ///// 使用 sql 语句查询数据表。
        ///// </summary>
        ///// <param name="args"></param>
        ///// <returns></returns>
        //protected LiteDataTable QueryTable(TableQueryArgs args)
        //{
        //    return Queryer.QueryTable(args);
        //}

        #endregion

        /// <summary>
        /// 由于使用数据库的 In 语句有个数的限制。所以当 In 的参数个数比较多时，需要进行分批查询并汇总最后的列表。
        /// 本方法用于帮助实现这种场景。
        /// </summary>
        /// <param name="inParameters">In 语句的参数列表。</param>
        /// <param name="batchQueryer">分批进行查询的查询实现。</param>
        /// <param name="batchSize">每一个批次的大小。</param>
        /// <returns></returns>
        public EntityList QueryInBatches<TParamType>(TParamType[] inParameters, Func<TParamType[], EntityList> batchQueryer, int batchSize = 1000)
        {
            var all = inParameters.Length;

            if (all <= batchSize)
            {
                return batchQueryer(inParameters);
            }

            //分批进行查询。
            var res = this.Repo.NewList();
            for (int i = 0; i < all; i += batchSize)
            {
                var length = i + batchSize > all ? all - i : batchSize;
                var array = new TParamType[length];
                Array.Copy(inParameters, i, array, 0, length);

                var batchList = batchQueryer(array);

                res.AddRange(batchList);
            }

            return res;
        }

        internal static QueryFactory qf
        {
            get { return QueryFactory.Instance; }
        }
    }
}