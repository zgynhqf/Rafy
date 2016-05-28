/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20140725
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20140725 15:54
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rafy.ManagedProperty;
using Rafy.MetaModel;

namespace Rafy.Domain.Validation
{
    /// <summary>
    /// 限制规则：实体的键必须没有被指定的引用属性对应的主表中的行所使用。
    /// </summary>
    public class NotUsedByReferenceRule : ValidationRule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotUsedByReferenceRule"/> class.
        /// </summary>
        public NotUsedByReferenceRule() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotUsedByReferenceRule"/> class.
        /// </summary>
        /// <param name="refProperty">可以是引用属性，也可以是冗余属性。.</param>
        public NotUsedByReferenceRule(IManagedProperty refProperty)
        {
            this.ReferenceProperty = new ConcreteProperty(refProperty);
        }

        /// <summary>
        /// 本规则需要连接数据源。
        /// </summary>
        protected override bool ConnectToDataSource
        {
            get { return true; }
        }

        /// <summary>
        /// 需要检查的
        /// </summary>
        public ConcreteProperty ReferenceProperty { get; set; }

        /// <summary>
        /// 设置此属性可以自定义要显示的错误信息。
        /// long 参数表示已经被使用的次数。
        /// </summary>
        public Func<Entity, long, string> MessageBuilder { get; set; }

        protected override void Validate(Entity entity, RuleArgs e)
        {
            var id = entity.Id;

            var repo = RF.Find(ReferenceProperty.Owner);
            if (repo != null)
            {
                var criteria = new CommonQueryCriteria();
                criteria.Add(ReferenceProperty.Property, id);

                var count = repo.CountBy(criteria);
                if (count > 0)
                {
                    e.BrokenDescription = this.BuildError(entity, count);
                }
            }
        }

        private string BuildError(Entity entity, long usedCount)
        {
            if (this.MessageBuilder != null)
            {
                return this.MessageBuilder(entity, usedCount);
            }

            return string.Format(
                "类型为 {0}、键为 {1} 的实体，已经被类型为 {2} 的实体使用了 {3} 次。",
                entity.GetType().Name, entity.Id,
                this.ReferenceProperty.Owner.Name, usedCount
                );
        }
    }
}