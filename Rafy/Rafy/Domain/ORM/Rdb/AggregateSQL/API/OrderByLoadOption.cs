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
    public class OrderByLoadOption<TEntity> : LoadOptionSelector
        where TEntity : Entity
    {
        internal OrderByLoadOption(AggregateDescriptor descriptor) : base(descriptor) { }

        public PropertySelector<TEntity> By<TKey>(Func<TEntity, TKey> keySelector)
        {
            this.InnerDescriptor.Items.Last.Value
                .OrderBy = e => keySelector(e as TEntity);

            return new PropertySelector<TEntity>(this.InnerDescriptor);
        }
    }
}