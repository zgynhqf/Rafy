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
using Rafy.Domain.ORM;
using Rafy.Domain.ORM.Linq;
using Rafy.Domain.ORM.Query;
using Rafy.Domain.ORM.SqlTree;
using Rafy.ManagedProperty;

namespace Rafy.Domain.ORM
{
    /// <summary>
    /// 关系数据库的查询器。
    /// 对接 ORM 查询(<see cref="RdbTable"/> )的实现。
    /// </summary>
    public class RdbDataQueryer : DataQueryer
    {
        /// <summary>
        /// 子类重写此方法，查询从持久层加载列表的具体实现。
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="entityList">The entity list.</param>
        /// <exception cref="System.NotSupportedException">使用内存过滤器的同时，不支持提供分页参数。</exception>
        protected override void QueryDataCore(EntityQueryArgs args, EntityList entityList)
        {
            var dp = RdbDataProvider.Get(this.Repo);
            using (var dba = dp.CreateDbAccesser())
            {
                //以下代码，开始访问数据库查询数据。
                var dbTable = dp.DbTable;
                if (args.Filter != null)
                {
                    #region 内存过滤式加载

                    if (!PagingInfo.IsNullOrEmpty(args.PagingInfo)) { throw new NotSupportedException("使用内存过滤器的同时，不支持提供分页参数。"); }

                    args.MemoryList = new List<Entity>();
                    dbTable.QueryList(dba, args);
                    this.LoadByFilter(args);

                    #endregion
                }
                else
                {
                    if (args.QueryType == RepositoryQueryType.Count)
                    {
                        #region 查询 Count

                        var count = dbTable.Count(dba, args.Query);
                        entityList.SetTotalCount(count);

                        #endregion
                    }
                    else
                    {
                        //是否需要为 PagingInfo 设置统计值。
                        var pi = args.PagingInfo;
                        var pagingInfoCount = !PagingInfo.IsNullOrEmpty(pi) && pi.IsNeedCount;

                        //如果 pagingInfoCount 为真，则在访问数据库时，会设置好 PagingInfo 的总行数。
                        dbTable.QueryList(dba, args);

                        //最后，还需要设置列表的 TotalCount。
                        if (pagingInfoCount) { entityList.SetTotalCount(pi.TotalCount); }
                    }
                }
            }
        }

        /// <summary>
        /// 子类重写此方法，查询从持久层加载列表的具体实现。
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="entityList">The entity list.</param>
        protected virtual void QueryDataCore(SqlQueryArgs args, EntityList entityList)
        {
            var dataProvider = RdbDataProvider.Get(Repo);
            using (var dba = dataProvider.CreateDbAccesser())
            {
                //访问数据库
                if (args.Filter != null)
                {
                    #region 内存过滤式加载

                    if (!PagingInfo.IsNullOrEmpty(args.PagingInfo)) { throw new NotSupportedException("使用内存过滤器的同时，不支持提供分页参数。"); }

                    args.EntityType = Repo.EntityType;
                    args.MemoryList = new List<Entity>();
                    dataProvider.DbTable.QueryList(dba, args);
                    this.LoadByFilter(args);

                    #endregion
                }
                else
                {
                    if (args.QueryType == RepositoryQueryType.Count)
                    {
                        #region 查询 Count

                        var value = dba.QueryValue(args.FormattedSql, args.Parameters);
                        var count = RdbTable.ConvertCount(value);
                        entityList.SetTotalCount(count);

                        #endregion
                    }
                    else
                    {
                        //是否需要为 PagingInfo 设置统计值。
                        var pagingInfoCount = !PagingInfo.IsNullOrEmpty(args.PagingInfo) && args.PagingInfo.IsNeedCount;

                        //如果 pagingInfoCount 为真，则在访问数据库时，会设置好 PagingInfo 的总行数。
                        args.EntityType = Repo.EntityType;
                        dataProvider.DbTable.QueryList(dba, args);

                        //最后，还需要设置列表的 TotalCount。
                        if (pagingInfoCount) { entityList.SetTotalCount(args.PagingInfo.TotalCount); }
                    }
                }
            }
        }

        /// <summary>
        /// 子类重写此方法，查询从持久层加载表格的具体实现。
        /// </summary>
        /// <param name="args">The arguments.</param>
        protected virtual void QueryTableCore(TableQueryArgs args)
        {
            var dp = RdbDataProvider.Get(this.Repo);
            using (var dba = dp.CreateDbAccesser())
            {
                dp.DbTable.QueryTable(dba, args);
            }
        }

        /// <summary>
        /// 通过 IQuery 对象来查询数据表。
        /// </summary>
        /// <param name="query">查询条件。</param>
        /// <param name="paging">分页信息。</param>
        /// <returns></returns>
        public override LiteDataTable QueryTable(IQuery query, PagingInfo paging = null)
        {
            var generator = RdbDataProvider.Get(this.Repo).DbTable.CreateSqlGenerator();
            generator.Generate(query as SqlNode);
            var sql = generator.Sql;

            return this.QueryTable(sql, paging);
        }

        /// <summary>
        /// 从持久层中查询数据。
        /// 本方法只能由仓库中的方法来调用。本方法的返回值的类型将与仓库中方法的返回值保持一致。
        /// 支持的返回值：EntityList、Entity、int、LiteDataTable。
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="paging"></param>
        /// <param name="eagerLoad"></param>
        /// <returns></returns>
        public object QueryData(FormattedSql sql, PagingInfo paging = null, EagerLoadOptions eagerLoad = null)
        {
            var args = new SqlQueryArgs(sql);
            args.SetDataLoadOptions(paging, eagerLoad);
            return this.QueryData(args);
        }

        /// <summary>
        /// 从持久层中查询数据。
        /// 本方法只能由仓库中的方法来调用。本方法的返回值的类型将与仓库中方法的返回值保持一致。
        /// 支持的返回值：EntityList、Entity、int、LiteDataTable。
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public object QueryData(SqlQueryArgs args)
        {
            this.PrepareArgs(args);

            var entityList = args.EntityList;
            var oldCount = entityList.Count;

            bool autoIndexEnabled = entityList.AutoTreeIndexEnabled;
            try
            {
                //在加载数据时，自动索引功能都不可用。
                entityList.AutoTreeIndexEnabled = false;

                this.QueryDataCore(args, entityList);
            }
            finally
            {
                entityList.AutoTreeIndexEnabled = autoIndexEnabled;
            }

            this.EagerLoadOnCompleted(args, entityList, oldCount);

            return ReturnForRepository(entityList);
        }

        /// <summary>
        /// 使用 sql 语句来查询数据表。
        /// </summary>
        /// <param name="sql">Sql 语句.</param>
        /// <param name="paging">分页信息。</param>
        /// <returns></returns>
        public LiteDataTable QueryTable(FormattedSql sql, PagingInfo paging = null)
        {
            return this.QueryTable(new TableQueryArgs(sql, paging));
        }

        /// <summary>
        /// 使用 sql 语句查询数据表。
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public LiteDataTable QueryTable(TableQueryArgs args)
        {
            args.EntityType = this.Repo.EntityType;

            this.QueryTableCore(args);

            var rdp = this.DataProvider as RdbDataProvider;
            rdp.OnTableQueryed(args);

            return args.ResultTable;
        }

        /// <summary>
        /// QueryTable 方法完成后调用。
        /// 
        /// 子类可重写此方法来实现查询完成后的数据修整工具。
        /// </summary>
        /// <param name="args"></param>
        internal protected virtual void OnTableQueryed(TableQueryArgs args) { }
    }
}
