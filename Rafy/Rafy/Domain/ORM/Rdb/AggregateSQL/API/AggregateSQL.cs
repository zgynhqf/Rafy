/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20101230
 * 说明：聚合SQL的简单API
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20101230
 * 
*******************************************************/

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Rafy.MetaModel;
using Linq = System.Linq.Expressions;

namespace Rafy.Domain.ORM
{
    /// <summary>
    /// 聚合SQL的简单API。
    /// “Facade”
    /// </summary>
    public class AggregateSQL
    {
        #region 使用方法

        //var loadOptions = AggregateSQL.Instance
        //    .Begin<PBS>().LoadChildren(pbs => pbs.PBSPropertys)
        //    .Continue<PBSProperty>().LoadChildren(p => p.PBSPropertyOptionalValues)
        //    .Continue<PBSPropertyOptionalValue>().LoadChildren(p => p.PBSPropertyOptionalValueFilters);

        //var sql = AggregateSQL.Instance.GenerateQuerySQL(loadOptions, Guid.NewGuid());
        //var sql2 = AggregateSQL.Instance.GenerateQuerySQL(loadOptions, "PBS.ProjectId = '82E799D0-6CC0-4EC6-BEAA-A088D7462868'"); 

        #endregion

        public static readonly AggregateSQL Instance = new AggregateSQL();

        private AggregateSQL() { }

        #region 更简单的API

        public void LoadEntities<TEntity>(EntityList list, Action<PropertySelector<TEntity>> loader, object parentId)
            where TEntity : Entity
        {
            var loadOptions = this.BeginLoadOptions<TEntity>();
            loader(loadOptions);
            var sql = GenerateQuerySQL(loadOptions, parentId);
            LoadEntities(list, sql, loadOptions);
        }

        public void LoadEntities<TEntity>(EntityList list, Action<PropertySelector<TEntity>> loader, string whereCondition = null, string joinCondition = null)
            where TEntity : Entity
        {
            var loadOptions = this.BeginLoadOptions<TEntity>();
            loader(loadOptions);
            var sql = GenerateQuerySQL(loadOptions, whereCondition, joinCondition);
            LoadEntities(list, sql, loadOptions);
        }

        /// <summary>
        /// 生成指定加载选项的聚合SQL。
        /// 此方法使用父对象的Id作为查询条件。
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="loader">The loader.</param>
        /// <param name="parentId">The parent unique identifier.</param>
        /// <returns></returns>
        public string GenerateQuerySQL<TEntity>(Action<PropertySelector<TEntity>> loader, object parentId)
            where TEntity : Entity
        {
            var loadOptions = this.BeginLoadOptions<TEntity>();
            loader(loadOptions);
            return GenerateQuerySQL(loadOptions, parentId);
        }

        /// <summary>
        /// 生成指定加载选项的聚合SQL。
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="loader">The loader.</param>
        /// <param name="whereCondition">简单的过滤条件，如：
        /// PBS.PBSTypeId = '...'</param>
        /// <param name="joinCondition">The join condition.</param>
        /// <returns></returns>
        public string GenerateQuerySQL<TEntity>(Action<PropertySelector<TEntity>> loader, string whereCondition = null, string joinCondition = null)
            where TEntity : Entity
        {
            var loadOptions = this.BeginLoadOptions<TEntity>();
            loader(loadOptions);
            return GenerateQuerySQL(loadOptions, whereCondition, joinCondition);
        }

        #endregion

        #region 基本的API

        /// <summary>
        /// 开始为TEntity进行加载。
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public PropertySelector<TEntity> BeginLoadOptions<TEntity>()
            where TEntity : Entity
        {
            return new PropertySelector<TEntity>(new AggregateDescriptor());
        }

        /// <summary>
        /// 生成指定加载选项的聚合SQL。
        /// 
        /// 此方法使用父对象的Id作为查询条件。
        /// </summary>
        /// <param name="loadOptions"></param>
        /// <param name="parentId"></param>
        /// <returns></returns>
        public string GenerateQuerySQL(LoadOptionSelector loadOptions, object parentId)
        {
            var aggregateSQL = new AggregateSQLGenerator(loadOptions.InnerDescriptor, null);
            var result = aggregateSQL.Generate();
            result = string.Format(result, parentId);
            return result;
        }

        /// <summary>
        /// 生成指定加载选项的聚合SQL。
        /// </summary>
        /// <param name="loadOptions">The load options.</param>
        /// <param name="whereCondition">简单的过滤条件，如：
        /// PBS.PBSTypeId = '...'</param>
        /// <param name="joinCondition">The join condition.</param>
        /// <returns></returns>
        public string GenerateQuerySQL(LoadOptionSelector loadOptions, string whereCondition = null, string joinCondition = null)
        {
            var aggregateSQL = new AggregateSQLGenerator(loadOptions.InnerDescriptor, whereCondition, joinCondition);
            var result = aggregateSQL.Generate();
            return result;
        }

        /// <summary>
        /// 通过聚合SQL加载整个聚合对象列表。
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="sql">聚合SQL</param>
        /// <param name="loadOptions">聚合加载选项</param>
        public void LoadEntities(EntityList list, string sql, LoadOptionSelector loadOptions)
        {
            var loader = new AggregateEntityLoader(loadOptions.InnerDescriptor);
            loader.Query(list, sql);
        }

        #endregion
    }
}