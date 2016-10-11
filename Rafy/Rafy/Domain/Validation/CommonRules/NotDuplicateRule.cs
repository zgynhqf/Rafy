using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rafy.Domain.ORM;
using Rafy.ManagedProperty;
using Rafy.MetaModel;

namespace Rafy.Domain.Validation
{
    /// <summary>
    /// 限制实体的某一个或几个属性的值在数据库中不重复的规则。
    /// </summary>
    public class NotDuplicateRule : ValidationRule
    {
        public NotDuplicateRule()
        {
            this.Properties = new List<IManagedProperty>();
        }

        /// <summary>
        /// 本规则需要连接数据源。
        /// </summary>
        protected override bool ConnectToDataSource
        {
            get { return true; }
        }

        /// <summary>
        /// 可以设置多个属性进行验证。
        /// </summary>
        public List<IManagedProperty> Properties { get; private set; }

        /// <summary>
        /// 设置此属性可以自定义要显示的错误信息。
        /// </summary>
        public Func<Entity, string> MessageBuilder { get; set; }

        /// <summary>
        /// 限制实体的某一个或几个属性的值在数据库中不存在的规则。
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="e"></param>
        protected override void Validate(Entity entity, RuleArgs e)
        {
            if (entity.PersistenceStatus == PersistenceStatus.New ||
                entity.PersistenceStatus == PersistenceStatus.Modified)
            {
                this.InitProperties(e);

                var criteria = new CommonQueryCriteria();
                bool hasValue = this.AddToCriteria(entity, criteria);

                if (hasValue)
                {
                    //查询实体的个数，如果已经存在，则构造错误信息
                    var repo = entity.GetRepository();
                    var count = repo.CountBy(criteria);
                    if (count > 0)
                    {
                        e.BrokenDescription = BuildError(entity);
                    }
                }
            }
        }

        /// <summary>
        /// 获取传入的要验证的属性列表
        /// </summary>
        /// <param name="e"></param>
        private void InitProperties(RuleArgs e)
        {
            if (this.Properties.Count == 0)
            {
                var property = e.Property as IProperty;
                if (property == null)
                {
                    throw new InvalidProgramException("使用 PropertyNotExists 验证方法需要传入名称为 Properties 的参数。");
                }
                this.Properties.Add(property);
            }
        }

        /// <summary>
        /// 根据传入的属性列表，来构造 CommonQueryCriteria
        /// 返回是否有非空属性需要验证。
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="criteria"></param>
        /// <returns></returns>
        private bool AddToCriteria(Entity entity, CommonQueryCriteria criteria)
        {
            bool hasValue = false;

            foreach (IProperty property in this.Properties)
            {
                EnsurePropertyCategory(property);

                var value = entity.GetProperty(property);
                if (DomainHelper.IsNotEmpty(value))
                {
                    hasValue = true;
                    criteria.Add(property, value);
                }
            }
            if ((entity as IEntityWithId).IdProvider.IsAvailable(entity.Id))
            {
                criteria.Add(Entity.IdProperty, PropertyOperator.NotEqual, entity.Id);
            }
            return hasValue;
        }

        private string BuildError(Entity entity)
        {
            if (this.MessageBuilder != null)
            {
                return this.MessageBuilder(entity);
            }

            var propertyFormat = "属性 {0} 的值是 {1}".Translate();
            var error = new StringBuilder("已经存在");
            bool first = true;
            foreach (IProperty property in this.Properties)
            {
                if (!first) error.Append("、".Translate());
                first = false;

                var value = entity.GetProperty(property);
                error.AppendFormat(propertyFormat, Display(property), value);
            }
            error.Append(" 的实体 ".Translate())
                .Append(Display(entity.GetType()));

            return error.ToString();
        }

        private static void EnsurePropertyCategory(IProperty property)
        {
            switch (property.Category)
            {
                case PropertyCategory.Normal:
                case PropertyCategory.ReferenceId:
                case PropertyCategory.Redundancy:
                    break;
                default:
                    throw new InvalidProgramException(string.Format(
@"PropertyNotExists 不能验证类型为 {1} 的属性 {0}，该方法只接受三种类型的属性：Normal、ReferenceId、Redundancy。",
property, property.Category
));
            }
        }
    }
}