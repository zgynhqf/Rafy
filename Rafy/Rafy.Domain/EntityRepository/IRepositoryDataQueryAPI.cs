/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20150114
 * 运行环境：.NET 4.5
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20150114 12:03
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rafy.Data;
using Rafy.Domain.ORM.Query;

namespace Rafy.Domain
{
    /// <summary>
    /// 仓库中数据层实现的查询 API
    /// </summary>
    internal interface IRepositoryDataQueryAPI
    {
        #region 数据层查询接口 - Linq

        /// <summary>
        /// 创建一个实体 Linq 查询对象。
        /// 只能在服务端调用此方法。
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        IQueryable<TEntity> CreateLinqQuery<TEntity>();

        /// <summary>
        /// 通过 linq 来查询实体。
        /// </summary>
        /// <param name="queryable">linq 查询对象。</param>
        /// <param name="paging">分页信息。</param>
        /// <param name="eagerLoad">需要贪婪加载的属性。</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidProgramException"></exception>
        EntityList QueryList(IQueryable queryable, PagingInfo paging = null, EagerLoadOptions eagerLoad = null);

        /// <summary>
        /// 把一个 Linq 查询转换为 IQuery 查询。
        /// </summary>
        /// <param name="queryable"></param>
        /// <returns></returns>
        IQuery ConvertToQuery(IQueryable queryable);

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
        EntityList QueryList(IQuery query, PagingInfo paging = null, EagerLoadOptions eagerLoad = null, bool markTreeFullLoaded = false);

        /// <summary>
        /// 通过 IQuery 对象来查询实体。
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        /// <exception cref="System.NotSupportedException">使用内存过滤器的同时，不支持提供分页参数。</exception>
        /// <exception cref="System.InvalidProgramException"></exception>
        EntityList QueryList(EntityQueryArgs args);

        #endregion

        #region 数据层查询接口 - FormattedSql

        /// <summary>
        /// 使用 sql 语句来查询实体。
        /// </summary>
        /// <param name="sql">sql 语句，返回的结果集的字段，需要保证与属性映射的字段名相同。</param>
        /// <param name="paging">分页信息。</param>
        /// <param name="eagerLoad">需要贪婪加载的属性。</param>
        /// <returns></returns>
        EntityList QueryList(FormattedSql sql, PagingInfo paging = null, EagerLoadOptions eagerLoad = null);

        /// <summary>
        /// 使用 sql 语句来查询实体。
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        /// <exception cref="System.NotSupportedException">使用内存过滤器的同时，不支持提供分页参数。</exception>
        EntityList QueryList(SqlQueryArgs args);

        /// <summary>
        /// 通过 IQuery 对象来查询数据表。
        /// </summary>
        /// <param name="query">查询条件。</param>
        /// <param name="paging">分页信息。</param>
        /// <returns></returns>
        LiteDataTable QueryTable(IQuery query, PagingInfo paging = null);

        /// <summary>
        /// 使用 sql 语句来查询数据表。
        /// </summary>
        /// <param name="sql">Sql 语句.</param>
        /// <param name="paging">分页信息。</param>
        /// <returns></returns>
        LiteDataTable QueryTable(FormattedSql sql, PagingInfo paging = null);

        /// <summary>
        /// 使用 sql 语句查询数据表。
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        LiteDataTable QueryTable(TableQueryArgs args);

        #endregion
    }
}
