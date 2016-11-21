/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20131213
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20131213 17:45
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.Domain.ORM.Query
{
    public static partial class QueryExtensions
    {
        /// <summary>
        /// 从当前数据源中查找指定仓库对应的表。
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="entityType">要查找这个仓库对应的表。
        /// 如果这个参数传入 null，则表示查找主表（最左边的表）。</param>
        /// <param name="alias">
        /// 要查找表的别名。
        /// 如果仓库在本数据源中匹配多个实体源，那么将使用别名来进行精确匹配。
        /// 如果仓库在本数据源中只匹配一个实体源，那么忽略本参数。
        /// </param>
        /// <returns></returns>
        public static ITableSource FindTable(this ISource source, Type entityType, string alias = null)
        {
            var repo = RepositoryFactoryHost.Factory.FindByEntity(entityType);
            return source.FindTable(repo, alias);
        }

        /// <summary>
        /// 从当前数据源中查找指定仓库对应的表。
        /// </summary>
        /// <typeparam name="TRepository">要查找这个仓库对应的表。
        /// 如果这个参数传入 null，则表示查找主表（最左边的表）。</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="alias">
        /// 要查找表的别名。
        /// 如果仓库在本数据源中匹配多个实体源，那么将使用别名来进行精确匹配。
        /// 如果仓库在本数据源中只匹配一个实体源，那么忽略本参数。
        /// </param>
        /// <returns></returns>
        public static ITableSource FindTable<TRepository>(this ISource source, string alias = null)
            where TRepository : EntityRepository
        {
            var repo = RF.ResolveInstance<TRepository>();
            return source.FindTable(repo, alias);
        }

        /// <summary>
        /// 构造一个排序节点并添加到当前集合中。。
        /// </summary>
        /// <param name="orderByList">实例.</param>
        /// <param name="property">使用这个属性进行排序。</param>
        /// <param name="direction">使用这个方向进行排序。</param>
        /// <returns></returns>
        public static IOrderBy Add(this ICollection<IOrderBy> orderByList, IColumnNode property, OrderDirection direction = OrderDirection.Ascending)
        {
            var item = QueryFactory.Instance.OrderBy(property, direction);
            orderByList.Add(item);
            return item;
        }

        ///// <summary>
        ///// 从当前查询中查找指定仓库对应的表。
        ///// </summary>
        ///// <param name="repo">要查找这个仓库对应的表。
        ///// 如果这个参数传入 null，则表示查找主表（最左边的表）。</param>
        ///// <param name="alias">
        ///// 要查找表的别名。
        ///// 如果仓库在本数据源中匹配多个表，那么将使用别名来进行精确匹配。
        ///// 如果仓库在本数据源中只匹配一个表，那么忽略本参数。
        ///// </param>
        ///// <returns></returns>
        //public static ITableSource FindTable(this IQuery query, IRepository repo = null, string alias = null)
        //{
        //    return query.From.FindTable(repo, alias);
        //}
    }
}
