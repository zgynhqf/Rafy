/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130426
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130426 15:15
 * 
*******************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Rafy.Domain;

namespace Rafy.Domain.ORM.Linq
{
    /// <summary>
    /// 实体查询器。
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    internal class EntityQueryable<TEntity> : IOrderedQueryable<TEntity>
    {
        /// <summary>
        /// 为指定的仓库进行查询
        /// </summary>
        /// <param name="repo"></param>
        public EntityQueryable(EntityRepository repo)
        {
            this.Provider = repo.LinqProvider;
            this.Expression = Expression.Constant(this);
        }

        /// <summary>
        /// 为反射提供，见：EntityQueryProvider.CreateQuery(Expression)
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="exp"></param>
        public EntityQueryable(EntityQueryProvider provider, Expression exp)
        {
            this.Provider = provider;
            this.Expression = exp;
        }

        /// <summary>
        /// 实体类型
        /// </summary>
        public Type ElementType
        {
            get { return typeof(TEntity); }
        }

        /// <summary>
        /// 表达式
        /// </summary>
        public Expression Expression { get; private set; }

        /// <summary>
        /// 提供程序
        /// </summary>
        public IQueryProvider Provider { get; private set; }

        IEnumerator IEnumerable.GetEnumerator()
        {
            var res = Provider.Execute(this.Expression) as EntityList;
            return res.GetEnumerator();
        }

        public IEnumerator<TEntity> GetEnumerator()
        {
            var res = Provider.Execute(this.Expression) as EntityList;
            return new EntityListEnumerator<TEntity>(res);
        }
    }
}