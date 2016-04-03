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
    /// 孩子选择器
    /// </summary>
    public class ChildrenSelector : LoadOptionSelector
    {
        internal ChildrenSelector(AggregateDescriptor descriptor) : base(descriptor) { }

        public OrderByLoadOption<TEntity> Order<TEntity>()
            where TEntity : Entity
        {
            return new OrderByLoadOption<TEntity>(this.InnerDescriptor);
        }

        /// <summary>
        /// 把孩子集合转换为实体对象，需要继续加载它的子对象
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public PropertySelector<TEntity> Continue<TEntity>()
            where TEntity : Entity
        {
            return new PropertySelector<TEntity>(this.InnerDescriptor);
        }
    }
}