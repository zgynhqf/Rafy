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
using Rafy.Domain.DataPortal;
using Rafy.Reflection;

namespace Rafy.Domain
{
    /// <summary>
    /// 仓库类型、仓库扩展类型的查询方法的拦截器。
    /// 使得在调用仓库的查询方法时，不是直接执行方法代码，而是调用数据门户去间接调用此方法。
    /// </summary>
    class RepositoryInterceptor : Castle.DynamicProxy.IInterceptor
    {
        internal static readonly RepositoryInterceptor Instance = new RepositoryInterceptor();

        private RepositoryInterceptor() { }

        public void Intercept(IInvocation invocation)
        {
            #region 预处理，根据返回值的类型来判断 FetchType。

            //if (Attribute.IsDefined(invocation.TargetType, typeof(IgnoreProxyAttribute))
            //    || Attribute.IsDefined(invocation.Method, typeof(IgnoreProxyAttribute)))
            //{
            //    invocation.Proceed();
            //    return;
            //}

            var fetchType = RepositoryQueryType.List;
            var returnType = invocation.Method.ReturnType;
            if (returnType.IsClass)
            {
                if (typeof(EntityList).IsAssignableFrom(returnType))
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

            #endregion

            var ieqc = new IEQC
            {
                MethodName = invocation.Method.Name,
                Parameters = invocation.Arguments,
                QueryType = fetchType
            };

            var repoExt = invocation.InvocationTarget as IRepositoryExt;
            var repo = invocation.InvocationTarget as EntityRepository ?? repoExt.Repository as EntityRepository;

            //只是不要纯客户端，都直接使用本地访问
            if (repo.DataPortalLocation == DataPortalLocation.Local || RafyEnvironment.Location.ConnectDataDirectly)
            {
                using (FinalDataPortal.CurrentQueryCriteriaItem.UseScopeValue(ieqc))
                {
                    try
                    {
                        RafyEnvironment.ThreadPortalCount++;

                        if (repoExt != null)
                        {
                            //invoke repositoryExt
                            invocation.Proceed();
                        }
                        else
                        {
                            //先尝试在 DataProvider 中调用指定的方法，如果没有找到，才会调用 Repository 中的方法。
                            //这样可以使得 DataProvider 中定义同名方法即可直接重写仓库中的方法。
                            object result = null;
                            if (MethodCaller.CallMethodIfImplemented(
                                repo.DataProvider, invocation.Method.Name, invocation.Arguments, out result
                                ))
                            {
                                invocation.ReturnValue = result;
                            }
                            else
                            {
                                //invoke repository
                                invocation.Proceed();
                            }
                        }
                    }
                    finally
                    {
                        RafyEnvironment.ThreadPortalCount--;
                    }
                }
            }
            else
            {
                //调用数据门户，使得在服务端才执行真正的数据层方法。
                invocation.ReturnValue = DataPortalApi.Fetch(invocation.TargetType, ieqc, repo.DataPortalLocation);
            }

            #region Repository.NotifyLoaded

            switch (fetchType)
            {
                case RepositoryQueryType.List:
                    var list = invocation.ReturnValue as EntityList;
                    repo.NotifyLoaded(list);
                    break;
                case RepositoryQueryType.First:
                    var entity = invocation.ReturnValue as Entity;
                    repo.NotifyLoaded(entity);
                    break;
                case RepositoryQueryType.Count:
                case RepositoryQueryType.Table:
                default:
                    break;
            }

            #endregion
        }
    }
}