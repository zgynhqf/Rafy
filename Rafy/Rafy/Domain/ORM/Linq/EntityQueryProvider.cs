/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130426
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130426 15:17
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Rafy.Domain.DataPortal;
using Rafy.Domain;
using Rafy.Reflection;

namespace Rafy.Domain.ORM.Linq
{
    /// <summary>
    /// 实体 Linq 查询提供器。
    /// 
    /// 作为 EntityRepository 的一个字段，单例。
    /// </summary>
    class EntityQueryProvider : IQueryProvider
    {
        public EntityQueryProvider(EntityRepositoryQueryBase queryBase)
        {
            _queryBase = queryBase;
            _repository = queryBase.Repo;
        }

        private IRepositoryInternal _repository;
        private EntityRepositoryQueryBase _queryBase;

        #region 兼容一般性遍历的接口

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new EntityQueryable<TElement>(this, expression);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            var res = this.Execute(expression);
            res = TypeHelper.CoerceValue(typeof(TResult), res);
            return (TResult)res;
        }

        public IQueryable CreateQuery(Expression expression)
        {
            Type elementType = _repository.EntityType;
            //Type elementType = TypeSystem.GetElementType(expression.Type);
            try
            {
                var queryableType = typeof(EntityQueryable<>).MakeGenericType(elementType);
                return (IQueryable)Activator.CreateInstance(queryableType, new object[] { this, expression });
            }
            catch (System.Reflection.TargetInvocationException tie)
            {
                throw tie.InnerException;
            }
        }

        /// <summary>
        /// 通过查询表达式来查询实体。
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public object Execute(Expression expression)
        {
            bool forceCounting = false;

            //如果最后调用了 Call 方法，则返回总行数。
            if (expression.NodeType == ExpressionType.Call)
            {
                var call = expression as MethodCallExpression;
                if (call.Method.DeclaringType == typeof(Queryable) &&
                    (call.Method.Name == LinqConsts.QueryableMethod_Count || call.Method.Name == LinqConsts.QueryableMethod_LongCount))
                {
                    if (call.Arguments.Count > 1) throw EntityQueryBuilder.OperationNotSupported(call.Method);

                    forceCounting = true;
                    expression = call.Arguments[0];
                }
            }

            var res = this.QueryEntityList(expression, forceCounting);

            //if (forceCounting) { return list.TotalCount; }

            return res;
        }

        #endregion

        /// <summary>
        /// 通过查询表达式来查询实体。
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="counting">if set to <c>true</c> [counting].</param>
        /// <returns></returns>
        private object QueryEntityList(Expression expression, bool counting)
        {
            //一般来说，直接使用 Linq 查询，都是在数据层，这时 FinalDataPortal.CurrentCriteria 已经被设置为当前的查询的条件了；
            //这时，如果接下来调用 _repository.QueryDbByLinq 方法，会导致这个查询使用外部的条件来检测是否需要分页、统计等。
            //所以，这里需要把这个条件先暂时清空。
            var iqec =new IEQC
            {
                QueryType = counting ? RepositoryQueryType.Count : RepositoryQueryType.List,
                Parameters = new object[0]
            };
            using (FinalDataPortal.CurrentQueryCriteriaItem.UseScopeValue(iqec))
            {
                var queryable = this.CreateQuery(expression);

                return _queryBase.QueryListByLinq(queryable);
            }
        }
    }
}