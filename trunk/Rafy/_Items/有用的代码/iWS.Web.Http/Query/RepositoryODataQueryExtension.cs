/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20141217
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20141217 16:21
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Rafy.Domain;
using Rafy.Domain.ORM;
using Rafy.Domain.ORM.Query;
using Rafy.ManagedProperty;

namespace iWS.Web.Http
{
    /// <summary>
    /// 在所有的实体仓库上扩展一个参数为 <see cref="ODataQueryCriteria"/> 的查询。
    /// </summary>
    public class RepositoryODataQueryExtension : EntityRepositoryExt<EntityRepository>
    {
        public static EntityList GetByOData(EntityRepository repository, ODataQueryCriteria criteria)
        {
            return FetchList(repository, criteria);
        }
        protected EntityList FetchBy(ODataQueryCriteria criteria)
        {
            var f = QueryFactory.Instance;
            var t = f.Table(this.Repository);

            var q = f.Query(from: t);

            var properties = this.Repository.EntityMeta.ManagedProperties.GetCompiledProperties();

            //filter
            if (!string.IsNullOrWhiteSpace(criteria.Filter))
            {
                var parser = new ODataFilterParser
                {
                    _mainTable = t,
                    _properties = properties
                };
                q.Where = parser.Parse(criteria.Filter);
            }

            //order by
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

            //expand
            if (!string.IsNullOrWhiteSpace(criteria.Expand))
            {
                criteria.EagerLoad = new EagerLoadOptions();

                var expandProperties = criteria.Expand.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var expand in expandProperties)
                {
                    var mp = properties.Find(expand, true);
                    if (mp != null)
                    {
                        if (mp is IListProperty)
                        {
                            criteria.EagerLoad.LoadWith(mp as IListProperty);
                        }
                        else if (mp is IRefEntityProperty)
                        {
                            criteria.EagerLoad.LoadWith(mp as IRefEntityProperty);
                        }
                    }
                }
            }

            return this.QueryList(q, criteria.PagingInfo, criteria.EagerLoad);
        }
    }
}