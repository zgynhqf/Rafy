/*******************************************************
 * 
 * 作者：佛山-程序缘
 * 创建日期：20160329
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 佛山-程序缘 20160329 21:10
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Rafy.Data;
using Rafy.DataPortal;
using Rafy.Reflection;

namespace Rafy.Domain
{
    /// <summary>
    /// 仓库类型、仓库扩展类型的查询方法的拦截器。
    /// 使得在调用仓库的查询方法时，不是直接执行方法代码，而是调用数据门户去间接调用此方法。
    /// </summary>
    class RepositoryQueryInterceptor : Castle.DynamicProxy.IInterceptor
    {
        internal static readonly RepositoryQueryInterceptor Instance = new RepositoryQueryInterceptor();

        private RepositoryQueryInterceptor() { }

        public void Intercept(IInvocation invocation)
        {
            var fetchType = JudgeFetchType(invocation);
            var ieqc = new IEQC
            {
                MethodName = invocation.Method.Name,
                Parameters = invocation.Arguments,
                QueryType = fetchType
            };

            using (IEQC.CurrentItem.UseScopeValue(ieqc))
            {
                //调用 DataPortalCallInterceptor
                invocation.Proceed();
            }

            //完成后，调用 Repository.SetRepo
            if (invocation.ReturnValue is IDomainComponent)
            {
                var target = invocation.InvocationTarget;
                var repo = target as EntityRepository ?? (target as IRepositoryExt).Repository as EntityRepository;

                switch (fetchType)
                {
                    case RepositoryQueryType.List:
                        var list = invocation.ReturnValue as IEntityList;
                        repo.SetRepo(list);
                        break;
                    case RepositoryQueryType.First:
                        var entity = invocation.ReturnValue as Entity;
                        repo.SetRepo(entity);
                        break;
                    case RepositoryQueryType.Count:
                    case RepositoryQueryType.Table:
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// 预处理，根据返回值的类型来判断 FetchType。
        /// </summary>
        /// <param name="invocation"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        private static RepositoryQueryType JudgeFetchType(IInvocation invocation)
        {
            //if (Attribute.IsDefined(invocation.TargetType, typeof(IgnoreProxyAttribute))
            //    || Attribute.IsDefined(invocation.Method, typeof(IgnoreProxyAttribute)))
            //{
            //    invocation.Proceed();
            //    return;
            //}

            var fetchType = RepositoryQueryType.List;
            var returnType = invocation.Method.ReturnType;
            if (!returnType.IsValueType)
            {
                if (typeof(IEntityList).IsAssignableFrom(returnType))
                {
                    fetchType = RepositoryQueryType.List;
                }
                else if (typeof(Entity).IsAssignableFrom(returnType))
                {
                    fetchType = RepositoryQueryType.First;
                }
                else if (returnType == typeof(LiteDataTable))
                {
                    fetchType = RepositoryQueryType.Table;
                }
                else
                {
                    throw new NotSupportedException("仓库查询不支持返回 {0} 类型。".FormatArgs(returnType));
                }
            }
            else
            {
                fetchType = RepositoryQueryType.Count;
            }

            return fetchType;
        }
    }
}