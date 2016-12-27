/*******************************************************
 * 
 * 作者：吴中坡
 * 创建日期：20161214
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 吴中坡 20161214 15:26
 * 
*******************************************************/

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Rafy.Accounts;
using Rafy.ComponentModel;
using Rafy.Domain;
using Rafy.RBAC.RoleManagement;

namespace Rafy.RBAC.DataPermissionManagement
{
    /// <summary>
    /// 数据权限过滤拦截器
    /// 默认关闭过滤
    /// 开启过滤,需要设置上下文信息如下
    /// using (DataPermissionInterceptor.FilterEnabled.UseScopeValue(true))
    /// {
    ///    dp.GetBy(xxxxx);
    /// }
    /// </summary>
    internal class DataPermissionInterceptor
    {
        internal static readonly AppContextItem<Resource> FilterResource =
            new AppContextItem<Resource>("Rafy.RBAC.DataPermissionManagement.DataPermissionInterceptor.Resource");

        /// <summary>
        /// 注册拦截器
        /// </summary>
        public static void Intercept()
        {
            RepositoryDataProvider.Querying += RepositoryDataProvider_Querying;
        }

        /// <summary>
        /// 数据权限过滤
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void RepositoryDataProvider_Querying(object sender, QueryingEventArgs e)
        {
            var resource = FilterResource.Value;
            if (resource != null)
            {
                var dp = sender as RepositoryDataProvider;
                if (resource.GeResourceEntityType() == dp.Repository.EntityType.FullName && resource.GetIsSupportDataPermission())
                {
                    var currentUser = AccountContext.CurrentUser;

                    var userRoleFilder = Composer.ObjectContainer.Resolve<IUserRoleFinder>();
                    var roles = userRoleFilder.FindByUser(currentUser);

                    var dataPermissions = CollectDataPermissions(resource.Id, roles);

                    var appender = new DataPermissionWhereAppender();
                    List<DataPermissionConstraintBuilder> duplicateList = new List<DataPermissionConstraintBuilder>();

                    foreach (var dataPermission in dataPermissions)
                    {
                        var constraintBuilder = dataPermission.CreateBuilder();
                        //去掉重复的builder
                        if (!duplicateList.Contains(constraintBuilder))
                        {
                            appender.ConstrainsBuilders.Add(constraintBuilder);
                            duplicateList.Add(constraintBuilder);
                        }
                    }
                    appender.Append(e.Args.Query);
                }
            }
        }

        /// <summary>
        /// 获取某个资源角色的数据权限列表
        /// </summary>
        /// <param name="resourceId"></param>
        /// <param name="roles"></param>
        /// <returns></returns>
        private static DataPermissionList CollectDataPermissions(long resourceId, RoleList roles)
        {
            return RepositoryFacade.ResolveInstance<DataPermissionRepository>()
              .GetDataPermissionList(resourceId, roles.Select(p => p.Id).Cast<long>().ToList());
        }
    }
}
