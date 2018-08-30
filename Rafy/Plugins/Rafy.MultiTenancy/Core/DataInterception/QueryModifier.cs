/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20141223
 * 运行环境：.NET 4.5
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20141223 11:11
 * 
*******************************************************/

using Rafy.Domain.ORM.Query;

namespace Rafy.MultiTenancy.Core.DataInterception
{
    /// <summary>
    /// 自动为 SQL 添加 TenantId = XXX 的条件。
    /// </summary>
    class QueryModifier : QueryNodeVisitor
    {
        public string TenantId { get; set; }

        public void Modify(IQuery node)
        {
            this.Visit(node);
        }

        /// <summary>
        /// 为所有的 IQuery 对象都添加相应的多租户查询。
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override IQuery VisitQuery(IQuery node)
        {
            var query = base.VisitQuery(node);

            var meta = query.MainTable.EntityRepository.EntityMeta;
            var property = meta.ManagedProperties.GetCompiledProperties().Find(TenantAwareEntityExtension.TenantIdProperty);
            var tenantIdColumn = query.MainTable.FindColumn(property);
            if (tenantIdColumn != null)
            {
                var condition = tenantIdColumn.Equal(this.TenantId);
                query.Where = QueryFactory.Instance.And(condition, query.Where);
            }

            return query;
        }
    }
}
