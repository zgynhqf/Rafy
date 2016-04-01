using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.Domain;
using Rafy.Domain.ORM;
using Rafy.Domain.ORM.Query;
using Rafy.ManagedProperty;

namespace UT
{
    public class EntityRepositoryExtension : EntityRepositoryExt<EntityRepository>
    {
        /// <summary>
        /// 通过一个属性与它对应的属性值，来查询一个实体。
        /// </summary>
        /// <param name="property">需要查询的属性。</param>
        /// <param name="value">对应的属性值，如果是字符串属性，会使用包含查询。</param>
        /// <returns></returns>
        [RepositoryQuery]
        public virtual EntityList GetBySingleProperty(IManagedProperty property, object value)
        {
            var q = QueryFactory.Instance.Query(this.Repository);
            var op = property.PropertyType == typeof(string) ? PropertyOperator.Contains : PropertyOperator.Equal;
            q.AddConstraintIf(property, op, value);

            return (EntityList)this.QueryData(q);
        }

        [RepositoryQuery]
        public virtual EntityList GetBy(SinglePropertyCriteira criteria)
        {
            var property = Repository.EntityMeta.ManagedProperties
                .GetCompiledProperties()
                .Find(criteria.PropertyName);
            if (property != null)
            {
                var q = QueryFactory.Instance.Query(this.Repository);
                var op = property.PropertyType == typeof(string) ? PropertyOperator.Contains : PropertyOperator.Equal;
                q.AddConstraintIf(property, op, criteria.Value);

                return (EntityList)this.QueryData(q);
            }

            return Repository.NewList();
        }
    }

    [Serializable]
    public class SinglePropertyCriteira
    {
        public string PropertyName;
        public object Value;
    }
}
