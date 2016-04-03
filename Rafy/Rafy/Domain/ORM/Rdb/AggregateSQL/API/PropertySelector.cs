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
    /// 属性选择器
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class PropertySelector<TEntity> : LoadOptionSelector
        where TEntity : Entity
    {
        internal PropertySelector(AggregateDescriptor descriptor) : base(descriptor) { }

        /// <summary>
        /// 需要同时加载外键
        /// </summary>
        /// <typeparam name="TFKEntity"></typeparam>
        /// <param name="fkEntityExp">
        /// 需要加载的外键实体属性表达式
        /// </param>
        /// <returns></returns>
        public PropertySelector<TFKEntity> LoadFK<TFKEntity>(Expression<Func<TEntity, TFKEntity>> fkEntityExp)
            where TFKEntity : Entity
        {
            var entityPropertyName = GetPropertyName(fkEntityExp);
            var propertyName = entityPropertyName + Entity.IdProperty.Name;

            var entityInfo = CommonModel.Entities.Get(typeof(TEntity));
            var propertyInfo = entityInfo.EntityProperties.FirstOrDefault(p => p.Name == propertyName);

            //构造一个临时代理方法，实现：TEntity.EntityProperty = TFKEntity
            var pE = Expression.Parameter(typeof(TEntity), "e");
            var pEFK = Expression.Parameter(typeof(TFKEntity), "efk");
            var propertyExp = Expression.Property(pE, entityPropertyName);
            var body = Expression.Assign(propertyExp, pEFK);
            var result = Expression.Lambda<Action<TEntity, TFKEntity>>(body, pE, pEFK);
            var fkSetter = result.Compile();

            var option = new LoadOptionItem(propertyInfo, (e, eFK) => fkSetter(e as TEntity, eFK as TFKEntity));

            //避免循环
            if (this.InnerDescriptor.Items.Any(i => i.OwnerType == option.PropertyEntityType))
            {
                throw new InvalidOperationException("有循环的实体设置。");
            }

            this.InnerDescriptor.AddItem(option);

            return new PropertySelector<TFKEntity>(this.InnerDescriptor);
        }

        /// <summary>
        /// 需要同时加载孩子
        /// </summary>
        /// <typeparam name="TChildren"></typeparam>
        /// <param name="propExp">
        /// 需要加载的孩子属性表达式
        /// </param>
        /// <returns></returns>
        public ChildrenSelector LoadChildren<TChildren>(Expression<Func<TEntity, TChildren>> propExp)
            where TChildren : EntityList
        {
            var propertyName = GetPropertyName(propExp);
            var entityInfo = CommonModel.Entities.Get(typeof(TEntity));
            var propertyInfo = entityInfo.ChildrenProperties.FirstOrDefault(p => p.Name == propertyName);

            this.InnerDescriptor.AddItem(new LoadOptionItem(propertyInfo));

            return new ChildrenSelector(this.InnerDescriptor);
        }

        private static string GetPropertyName<TProperty>(Expression<Func<TEntity, TProperty>> propExp)
        {
            var member = propExp.Body as MemberExpression;
            var property = member.Member as PropertyInfo;
            if (property == null) throw new ArgumentNullException("property");
            var propertyName = property.Name;

            return propertyName;
        }
    }
}