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
        /// 通过 linq 来查询实体。
        /// </summary>
        /// <param name="queryable">linq 查询对象。</param>
        /// <param name="paging">分页信息。</param>
        /// <param name="eagerLoad">需要贪婪加载的属性。</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidProgramException"></exception>
        protected EntityList QueryList(IQueryable queryable, PagingInfo paging = null, EagerLoadOptions eagerLoad = null)
        {
            return DataQueryer.QueryList(queryable, paging, eagerLoad);
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

        internal EntityList QueryListByLinq(IQueryable queryable)
        {
            return DataQueryer.QueryList(queryable);
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
        protected EntityList QueryList(IQuery query, PagingInfo paging = null, EagerLoadOptions eagerLoad = null, bool markTreeFullLoaded = false)
        {
            return DataQueryer.QueryList(query, paging, eagerLoad, markTreeFullLoaded);
        }

        /// <summary>
        /// 通过 IQuery 对象来查询实体。
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        /// <exception cref="System.NotSupportedException">使用内存过滤器的同时，不支持提供分页参数。</exception>
        /// <exception cref="System.InvalidProgramException"></exception>
        protected EntityList QueryList(EntityQueryArgs args)
        {
            return DataQueryer.QueryList(args);
        }

        /// <summary>
        /// 通过 IQuery 对象来查询数据表。
        /// </summary>
        /// <param name="query">查询条件。</param>
        /// <param name="paging">分页信息。</param>
        /// <returns></returns>
        protected LiteDataTable QueryTable(IQuery query, PagingInfo paging = null)
        {
            return DataQueryer.QueryTable(query, paging);
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

        internal static QueryFactory qf
        {
            get { return QueryFactory.Instance; }
        }
    }
}